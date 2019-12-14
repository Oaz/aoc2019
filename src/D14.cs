namespace src14
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;
  using System.Text.RegularExpressions;

  public readonly struct Reaction
  {
    public Reaction(string def)
    {
      var parse = Regex.Matches(def, "([0-9-]+) ([A-Z]+),?");
      Inputs = Enumerable
                .Range(0, parse.Count - 1)
                .ToDictionary(i => parse[i].Groups[2].Value, i => long.Parse(parse[i].Groups[1].Value));
      Output = parse.Last().Groups[2].Value;
      Quantity = int.Parse(parse.Last().Groups[1].Value);
    }
    public Reaction(string input, int inputQ, string output, int outputQ)
    {
      Inputs = new Dictionary<string, long> { {input, inputQ} };
      Output = output;
      Quantity = outputQ;
    }
    public readonly Dictionary<string, long> Inputs;
    public readonly string Output;
    public readonly long Quantity;
  }

  public static class Extensions
  {
    public static void Set<K, V>(this IDictionary<K, V> d, K k, V v, Func<V, V, V> f)
    {
      if (d.TryGetValue(k, out V v0))
      {
        var v1 = f(v0, v);
        if (v1.Equals(default(V))) d.Remove(k); else d[k] = v1;
      }
      else
        d[k] = v;
    }
    public static V Get<K, V>(this IDictionary<K, V> d, K k) =>
      d.TryGetValue(k, out V v) ? v : default;
    public static void AddTo<K>(this IDictionary<K, long> d, K k, long v) =>
      Set(d, k, v, (a, b) => a + b);
  }

  public class Factory
  {
    public Factory(string[] defs) =>
      Reactions = defs.Select(d => new Reaction(d)).ToList();
    Reaction FindProducer(KeyValuePair<string, long> production) =>
      Reactions.First(r => r.Output == production.Key);
    public long OreForSingleFuel() => OreForFuel(1);
    public long FuelWithTrillionOre() => FuelWithOre(1000000000000);
    public long FuelWithOre(long ore) =>
      DichotomicSearch( OreForFuel, ore, 1, 2*ore/OreForSingleFuel() );

    private static long DichotomicSearch(Func<long,long> f, long target, long low, long high)
    {
      while(high > low+1)
      {
        var mid = (low+high)/2;
        if(f(mid) > target) high = mid; else low = mid;
      }
      return low;
    }
    public long OreForFuel(long requested)
    {
      var needed = new Dictionary<string, long> { { "FUEL", requested } };
      var leftOver = new Dictionary<string, long>();
      for (; ; )
      {
        var allButORE = needed.Where(p => p.Key != "ORE").ToList();
        if (!allButORE.Any())
          return needed.First().Value;
        var nextProduction = allButORE.First();
        var nextQuantity = nextProduction.Value - leftOver.Get(nextProduction.Key);
        if (nextQuantity > 0)
          UseProducer(FindProducer(nextProduction), nextQuantity);
        leftOver.AddTo(nextProduction.Key, -nextProduction.Value);
        needed.AddTo(nextProduction.Key, -nextProduction.Value);
      }
      void UseProducer(Reaction producer, long quantity)
      {
        var units = quantity / producer.Quantity + ((quantity % producer.Quantity == 0) ? 0 : 1);
        foreach (var i in producer.Inputs)
          needed.AddTo(i.Key, i.Value * units);
        leftOver.AddTo(producer.Output, producer.Quantity * units);
      }
    }
    public List<Reaction> Reactions;
  }
}
