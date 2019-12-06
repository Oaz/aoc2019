namespace src06
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public class OrbitMap
  {
    public OrbitMap(IEnumerable<string> definition)
    {
        directOrbits = definition.Select(x => x.Split(')')).ToDictionary(x=>x[1],x =>x[0]);
    }
    Dictionary<string,string> directOrbits;

    public int Count => directOrbits.Keys.Distinct().Select(x => ChainStartingFrom(x).Count()).Sum();
    
    public IEnumerable<string> ChainStartingFrom(string obj)
      => Observable.Generate(
          obj, o => directOrbits.Keys.Contains(o), o => directOrbits[o], o => directOrbits[o]
        ).ToEnumerable().Reverse();

    public string ClosestSharedObject(string a, string b)
      => ChainStartingFrom(a).Intersect(ChainStartingFrom(b)).Last();

    public int CountHopsFromTo(string a, string b)
    {
      var cso = ClosestSharedObject(a,b);
      return CountFrom(a)+CountFrom(b);
      int CountFrom(string o) => ChainStartingFrom(o).Reverse().TakeWhile(x => x!=cso).Count();
    }
  }
}
