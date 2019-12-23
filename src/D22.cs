namespace src22
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class Deck
  {
    public Deck(long size, params string[] operations)
    {
      Size = size;
      Operations = operations;
    }
    public readonly long Size;
    public readonly string[] Operations;

    public IEnumerable<int> FullDeal =>
      Compute(Operations, Enumerable.Range(0, (int)Size), (d, op) => d.FullDeal(op, Size));

    public long PositionOf(long n) =>
      Compute(Operations, n, (d, op) => d.PositionOf(op, Size));

    public long AtPosition(long n) =>
      Compute(Operations.Reverse(), n, (d, op) => d.AtPosition(op, Size));

    public T Compute<T>(IEnumerable<string> operations, T v, Func<Deal, string, Func<T, T>> f)
    {
      foreach (var operation in CreateOperations(operations, f))
        v = operation(v);
      return v;
    }

    public ValueTuple<long, long> Coefs
    {
      get
      {
        var ops = Operations.Reverse()
          .Select(op => Deal.All.Select(d => d.Coefs(op, Size)).First(o => o.HasValue))
          .ToList();
        var coefs = (1L, 0L);
        foreach (var operation in ops)
          coefs = Arithmetic.MulAdd(coefs, operation.Value, Size);
        return coefs;
      }
    }

    public static IEnumerable<Func<T, T>> CreateOperations<T>(IEnumerable<string> operations, Func<Deal, string, Func<T, T>> f) =>
      operations.Select(op => Deal.All.Select(d => f(d, op)).First(o => o != null));
  }

  public abstract class Deal
  {
    static Deal()
    {
      All.Add(new DealIntoNewStack());
      All.Add(new Cut());
      All.Add(new DealWithIncrement());
    }
    public static readonly List<Deal> All = new List<Deal>();

    public Deal(string pattern) => Pattern = new Regex(pattern);
    public abstract Func<IEnumerable<int>, IEnumerable<int>> FullDeal(string operation, long size);
    public abstract Func<long, long> PositionOf(string operation, long size);
    public abstract Func<long, long> AtPosition(string operation, long size);
    public abstract ValueTuple<long, long>? Coefs(string operation, long size);
    public Func<T, T> CreateOperation<T>(string operation, Func<Match, Func<T, T>> create)
    {
      var use = Pattern.Match(operation);
      return use.Success ? create(use) : null;
    }
    public U? CreateOperation<U>(string operation, Func<Match, U> create) where U : struct
    {
      var use = Pattern.Match(operation);
      return use.Success ? create(use) : (U?)null;
    }
    public readonly Regex Pattern;
  }
  public class DealIntoNewStack : Deal
  {
    public DealIntoNewStack() : base("deal into new stack") { }
    public override Func<IEnumerable<int>, IEnumerable<int>> FullDeal(string operation, long size) =>
      CreateOperation<IEnumerable<int>>(operation, m => (x => x.Reverse()));
    public override Func<long, long> PositionOf(string operation, long size) =>
      CreateOperation<long>(operation, m => (x => size - x - 1));
    public override Func<long, long> AtPosition(string operation, long size) =>
      CreateOperation<long>(operation, m => (x => size - x - 1));
    public override ValueTuple<long, long>? Coefs(string operation, long size) =>
      CreateOperation(operation, m => (size - 1, size - 1));
  }
  public class Cut : Deal
  {
    public Cut() : base("cut ([0-9-]+)") { }
    public override Func<IEnumerable<int>, IEnumerable<int>> FullDeal(string operation, long size) =>
      CreateOperation<IEnumerable<int>>(operation, m =>
      {
        var n = int.Parse(m.Groups[1].Value);
        int s = (int)size;
        n = (n < 0) ? s + n : n;
        return x => x.Skip(n).Concat(x.Take(n));
      });
    public override Func<long, long> PositionOf(string operation, long size) =>
      CreateOperation<long>(operation, m =>
      {
        var cut = long.Parse(m.Groups[1].Value);
        var rcut = (cut < 0) ? size + cut : cut;
        return x => x - rcut + ((x < rcut) ? size : 0);
      });
    public override Func<long, long> AtPosition(string operation, long size) =>
      CreateOperation<long>(operation, m =>
      {
        var cut = long.Parse(m.Groups[1].Value);
        var rcut = (cut < 0) ? size + cut : cut;
        return x => (x + rcut) % size;
      });
    public override ValueTuple<long, long>? Coefs(string operation, long size) =>
      CreateOperation(operation, m =>
      {
        var cut = long.Parse(m.Groups[1].Value);
        var rcut = (cut < 0) ? size + cut : cut;
        return (1L, rcut);
      });
  }
  public class DealWithIncrement : Deal
  {
    public DealWithIncrement() : base("deal with increment ([0-9]+)") { }
    public override Func<IEnumerable<int>, IEnumerable<int>> FullDeal(string operation, long size) =>
      CreateOperation<IEnumerable<int>>(operation, m =>
      {
        var n = int.Parse(m.Groups[1].Value);
        return l =>
        {
          var p = 0;
          var r = new int[(int)size];
          foreach (var x in l)
          {
            r[p] = x;
            p = (p + n) % r.Length;
          }
          return r;
        };
      });
    public override Func<long, long> PositionOf(string operation, long size) =>
      CreateOperation<long>(operation, m =>
      {
        var inc = long.Parse(m.Groups[1].Value);
        return x => (x * inc) % size;
      });
    public override Func<long, long> AtPosition(string operation, long size) =>
      CreateOperation<long>(operation, m =>
      {
        var inc = long.Parse(m.Groups[1].Value);
        var ea = Arithmetic.ExtendedEuclidean(size, inc);
        var v = ea.Item3;
        return x => (Arithmetic.ModuloMultiply(v, x, size) + size) % size;
      });
    public override ValueTuple<long, long>? Coefs(string operation, long size) =>
      CreateOperation(operation, m =>
      {
        var inc = long.Parse(m.Groups[1].Value);
        var ea = Arithmetic.ExtendedEuclidean(size, inc);
        var v = ((ea.Item3%size)+size)%size;
        return (v, 0L);
      });
  }

  public static class Arithmetic
  {
    public static (long, long, long) ExtendedEuclidean(long a, long b)
    {
      long r = a, u = 1, v = 0, r2 = b, u2 = 0, v2 = 1;
      while (r2 != 0)
      {
        var q = r / r2;
        (r, u, v, r2, u2, v2) = (r2, u2, v2, r - q * r2, u - q * u2, v - q * v2);
      }
      // au+bv=r
      return (r, u, v);
    }

    public static long ModuloMultiply(long a, long b, long mod)
    {
      long res = 0;
      a %= mod;
      while (b > 0) // Assume initial b > 0
      {
        if ((b & 1) > 0)
          res = (res + a) % mod;
        a = (2 * a) % mod; // Assume 2*a doesn't cause overflow
        b >>= 1;
      }
      return res;
    }

    public static ValueTuple<long,long> MulAdd(ValueTuple<long,long> x, ValueTuple<long,long> y, long mod) =>
      (
        ModuloMultiply(y.Item1, x.Item1, mod),
        (ModuloMultiply(y.Item1, x.Item2, mod) + y.Item2) % mod
      );
  }

}
