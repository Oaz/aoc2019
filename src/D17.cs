namespace src17
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public class Robot : AbstractIntcodeComputer
  {
    public Robot(BigInteger[] program, string orders) : base(program)
    {
      RemainingOrders = orders;
      Program[0] = 2;
      Run();
    }

    public string RemainingOrders;
    public long Result;

    public override BigInteger ReadFromInput()
    {
      var input = RemainingOrders.First();
      RemainingOrders = RemainingOrders.Substring(1);
      return input;
    }
    public override void WriteToOutput(BigInteger val)
    {
      Result = (long)val;
    }
  }

  public class Camera
  {
    public Camera(string input)
    {
      var lines = input.Trim().Split('\n').Select(s => s.Trim()).ToArray();
      Width = lines[0].Length;
      Height = lines.Length;
      var coords = from x in Enumerable.Range(0, Width)
                   from y in Enumerable.Range(0, Height)
                   select Coords.At(x, y);
      Points = coords.ToDictionary(c => c, c => lines[c.Y][c.X]);
    }

    public readonly int Width;
    public readonly int Height;
    public readonly Dictionary<Coords, char> Points;

    public IEnumerable<string> TextPath =>
      Path.Zip(Path.Skip(1))
        .SelectMany(p => Operations(p.Item1, p.Item2))
        .ToList().Prepend("R").Take(2 * Path.Count() - 2);
    public IEnumerable<string> Operations(Moving begin, Moving end) =>
      new string[] {
        (end.Position-begin.Position).ManhattanDistance.ToString(),
        (begin.RotationTo(end) == Moving.Rotation.Right ? "R" : "L").ToString(),
      };
    public IEnumerable<Moving> Path =>
      LinqX.Generate(Start, Segment)
        .Zip(LinqX.Generate(Start, Segment).Skip(1))
        .TakeWhile(p => !p.Item1.Position.Equals(p.Item2.Position))
        .Select(p => p.Item2)
        .ToList().Prepend(Start);
    public Moving Segment(Moving start) => RotateOnDeadEnd(GoToEndOfSegment(start));
    public Moving RotateOnDeadEnd(Moving m) => m.Rotate(SelectRotationOnDeadEnd(m));
    public Moving.Rotation SelectRotationOnDeadEnd(Moving m) =>
      CanRotate(m, Moving.Rotation.Left) ? Moving.Rotation.Left : Moving.Rotation.Right;
    public bool CanRotate(Moving m, Moving.Rotation r) =>
      IsScaffolding(m.Rotate(r).Next.Position);
    public Moving GoToEndOfSegment(Moving start) =>
      LinqX.Generate(start, m => m.Next)
           .TakeWhile(m => IsScaffolding(m.Position))
           .Last();
    public Moving Start =>
      new Moving(Points.First(p => p.Value == '^').Key, Coords.At(1, 0));
    public int Alignment => Intersections.Select(p => p.X * p.Y).Sum();
    public IEnumerable<Coords> Intersections => Points.Keys.Where(p => IsIntersection(p));
    public bool IsIntersection(Coords c) =>
      IsScaffolding(c) && Neighbors(c).Count() == 4 && Neighbors(c).All(IsScaffolding);
    public IEnumerable<Coords> Neighbors(Coords c) => Deltas.Select(d => c + d).Where(IsInside);
    public bool IsInside(Coords c) => Points.ContainsKey(c);
    public bool IsScaffolding(Coords c) => IsInside(c) && Points[c] != '.';
    static Coords[] Deltas = new Coords[] { Coords.At(0, 1), Coords.At(0, -1), Coords.At(1, 0), Coords.At(-1, 0) };
  }

  public class Scanner : AbstractIntcodeComputer
  {
    public Scanner(BigInteger[] program) : base(program) { Run(); }

    public string Image;

    public override BigInteger ReadFromInput() => 0;
    public override void WriteToOutput(BigInteger val)
    {
      Image += (char)val;
    }
  }

  public readonly struct Moving
  {
    public Moving(Coords position, Coords direction)
    {
      Position = position;
      Direction = direction;
    }
    public override string ToString() => $"[{Position} {Direction}]";
    public enum Rotation { Left, Right }
    public readonly Coords Position;
    public readonly Coords Direction;
    public Moving Next => new Moving(Position + Direction, Direction);
    public Moving Rotate(Rotation r) => new Moving(Position,
      r == Rotation.Left ? Coords.At(Direction.Y, -Direction.X) : Coords.At(-Direction.Y, Direction.X)
    );

    public Rotation RotationTo(Moving next) =>
      (Direction.X * next.Direction.Y - Direction.Y * next.Direction.X == 1)
        ? Rotation.Right : Rotation.Left;
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
      var modes = Observable.Generate(opcode / 100, _ => true, m => m / 10, m => m % 10).Take(3).ToEnumerable();
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
