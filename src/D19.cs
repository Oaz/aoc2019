namespace src19
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;

  public class Scanner : AbstractIntcodeComputer
  {
    public static Coords GetTopLeft(BigInteger[] program, int size)
    {
      var rows = new List<ValueTuple<int,int>>();
      var y = -1;
      foreach(var row in GetBigArea(program))
      {
        y++;
        rows.Add(row);
        if(y < size)
          continue;
        if(row.Item2-row.Item1<size)
          continue;
        var topLeft = Coords.At(row.Item1,y-size+1);
        var horizontalFit = rows[topLeft.Y].Item2-row.Item1;
        if(horizontalFit>=size)
          return topLeft;
      }
      throw new Exception();
    }

    public static IEnumerable<ValueTuple<int,int>> GetBigArea(BigInteger[] program)
    {
      var y = 0;
      var minX = 0;
      var maxX = 0;
      for(;;)
      {
        minX = ScanUntil(program, minX, y, true);
        maxX = ScanUntil(program, Math.Max(minX,maxX), y, false);
        yield return (minX,maxX);
        y++;
      }
    }

    private static int ScanUntil(BigInteger[] program, int x0, int y0, bool expected) =>
      LinqX.Generate(x0,x => x+1)
           .SkipWhile(x => new Scanner(program, x, y0).Affected != expected)
           .First();

    public static IEnumerable<ValueTuple<Coords,bool>> GetArea(BigInteger[] program, Coords bottomRight) =>
      from x in Enumerable.Range(0,bottomRight.X)
      from y in Enumerable.Range(0,bottomRight.Y)
      let scanner = new Scanner(program, x, y)
      select (Coords.At(x,y),scanner.Affected);

    public Scanner(BigInteger[] program, int x ,int y) : base(program)
    {
      Inputs = new Queue<int>();
      Inputs.Enqueue(x);
      Inputs.Enqueue(y);
      Run();
    }

    public Queue<int> Inputs;
    public bool Affected;

    public override BigInteger ReadFromInput() => Inputs.Dequeue();

    public override void WriteToOutput(BigInteger val) => Affected = val == 1;
  }

  public readonly struct Coords
  {
    public static Coords At(int x, int y) => new Coords(x, y);
    public Coords(int x, int y) { X = x; Y = y; }
    public readonly int X;
    public readonly int Y;

    public int ManhattanDistance => Math.Abs(X) + Math.Abs(Y);
    public override string ToString() => $"({X},{Y})";

    public static Coords operator +(Coords a, Coords b) => At(a.X + b.X, a.Y + b.Y);
    public static Coords operator -(Coords a, Coords b) => At(a.X - b.X, a.Y - b.Y);
  }

  public static class CoordsExtensions
  {
    public static Coords Bulk(this IEnumerable<Coords> l, Func<IEnumerable<int>, int> f) =>
      Coords.At(f(l.Select(c => c.X)), f(l.Select(c => c.Y)));
    public static Coords Min(this IEnumerable<Coords> l) => l.Bulk(x => x.Min());
    public static Coords Max(this IEnumerable<Coords> l) => l.Bulk(x => x.Max());
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
    public static void AddTo<K>(this IDictionary<K, long> d, K k, long v) =>
      Set(d, k, v, (a, b) => a + b);
  }

  public abstract class AbstractIntcodeComputer
  {
    public AbstractIntcodeComputer(IEnumerable<BigInteger> program) => Program = program.ToList();
    protected AbstractIntcodeComputer() { }
    public BigInteger ReadAt(BigInteger index) =>
      ResizeToIncludeAnd((int)index, () => Program[(int)index]);
    public BigInteger WriteAt(BigInteger index, BigInteger val) =>
      ResizeToIncludeAnd((int)index, () => Program[(int)index] = val);
    private BigInteger ResizeToIncludeAnd(int index, Func<BigInteger> f)
    {
      if (Program.Count - 1 < index)
        Program.AddRange(new BigInteger[index + 1 - Program.Count]);
      return f();
    }

    public List<BigInteger> Program;
    public int Counter = 0;
    public int RelativeBase = 0;

    protected void CopyTo(AbstractIntcodeComputer dst)
    {
      dst.Program = Program.ToList();
      dst.Counter = Counter;
      dst.RelativeBase = RelativeBase;
    }

    public abstract BigInteger ReadFromInput();
    public abstract void WriteToOutput(BigInteger val);
    public static Dictionary<int, Operation> Operations = new Dictionary<int, Operation>
      {
        { 1, new OperationBinary((a,b) => a+b) },
        { 2, new OperationBinary((a,b) => a*b) },
        { 3, new OperationInput() },
        { 4, new OperationOutput() },
        { 5, new OperationJumpIf(true) },
        { 6, new OperationJumpIf(false) },
        { 7, new OperationBinary((a,b) => a<b ? 1 : 0) },
        { 8, new OperationBinary((a,b) => a==b ? 1 : 0) },
        { 9, new OperationChangeRelativeBase() }
      };

    public AbstractIntcodeComputer Run()
    {
      Counter = 0;
      while (!HasHalted) RunOne();
      return this;
    }
    public bool HasHalted => Program[Counter] == 99;
    public AbstractIntcodeComputer MoveTo(int index)
    {
      Counter = index;
      return this;
    }
    public AbstractIntcodeComputer RunOne()
    {
      var opcode = Program[Counter];
      var operation = Operations[(int)(opcode % 100)];
      var modes = LinqX.Generate(opcode / 100, m => m / 10).Select(m => m % 10).Take(3);
      var args = Enumerable
                  .Range(Counter + 1, operation.Length - 1)
                  .Zip(modes).Select(ChooseMode).ToArray();
      operation.Execute(this, args);
      if (Program[Counter] == opcode)
        Counter += operation.Length;
      return this;
    }
    private BigInteger ChooseMode((int, BigInteger) x) =>
      x.Item2 == 1 ? x.Item1 : Program[x.Item1] + (x.Item2 == 0 ? 0 : RelativeBase);
  }

  public interface Operation
  {
    void Execute(AbstractIntcodeComputer computer, BigInteger[] args);
    int Length { get; }
  }

  public class OperationBinary : Operation
  {
    public OperationBinary(Func<BigInteger, BigInteger, BigInteger> o) { op = o; }
    readonly Func<BigInteger, BigInteger, BigInteger> op;
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args)
      => computer.WriteAt(args[2], op(computer.ReadAt(args[0]), computer.ReadAt(args[1])));
    public int Length => 4;
  }

  public class OperationInput : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args) =>
      computer.WriteAt(args[0], computer.ReadFromInput());
    public int Length => 2;
  }

  public class OperationOutput : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args) =>
      computer.WriteToOutput(computer.ReadAt(args[0]));
    public int Length => 2;
  }

  public class OperationChangeRelativeBase : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args) =>
      computer.RelativeBase += (int)computer.ReadAt(args[0]);
    public int Length => 2;
  }

  public class OperationJumpIf : Operation
  {
    public OperationJumpIf(bool c) { condition = c; }
    readonly bool condition;
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args)
    {
      if ((computer.ReadAt(args[0]) == 0) != condition)
        computer.Counter = (int)computer.ReadAt(args[1]);
    }
    public int Length => 3;
  }

  public static class LinqX
  {
    public static IEnumerable<BigInteger> AsBig(this IEnumerable<int> l) =>
      l.Select(i => new BigInteger(i));
    public static BigInteger AsBig(this int i) => new BigInteger(i);
    public static T Identity<T>(T x) => x;
    public static IEnumerable<T> Generate<T>(T x, Func<T, T> f)
    {
      for (; ; ) { yield return x; x = f(x); }
    }
    public static IEnumerable<T> RepeatInfinitely<T>(this IEnumerable<T> l) =>
      Generate(l, Identity).SelectMany(Identity);
  }
}
