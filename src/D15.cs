namespace src15
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;
  using Heuristic.Linq;

  public static class Oxygen
  {
    public static int SpreadAndCountMinutes(Dictionary<Coords, char> tiles)
    {
      var oxygenSource = tiles.First(p => p.Value == 'o').Key;
      var targetCoords = tiles.Where(p => p.Value == '.'||p.Value == 'o').Select(p => p.Key);
      var targets = new HashSet<Coords>(targetCoords);
      var deltas = Enumerable.Range(1, 4).Select(d => Delta(d)).ToArray();
      var neighbours = targets.ToDictionary(
        t => t,
        t => deltas.Select(d => t+d).Where(p => tiles.GetValueOrDefault(p) == '.').ToArray()
      );
      var count = 0;
      while (targets.Any())
      {
        var target = targets.OrderByDescending(t => (t - oxygenSource).ManhattanDistance).First();
        var steps = HeuristicSearch.AStar(oxygenSource, target, (c, i) => neighbours[c]).ToList();
        steps.ForEach(s => targets.Remove(s));
        count = Math.Max(count, steps.Count);
      }
      return count - 1;
    }

    public static Dictionary<Coords, char> WholeMap(PathExplorer pathExplorer)
    {
      var (tiles, steps) = Explore(pathExplorer, 3);
      steps.Count();
      return tiles;
    }

    public static int FindShortestPath(PathExplorer pathExplorer)
    {
      var (_, steps) = Explore(pathExplorer, 2);
      return steps.Count() - 1;
    }

    public static (Dictionary<Coords, char>, HeuristicSearchBase<State, State>)
                  Explore(PathExplorer pathExplorer, int goalID)
    {
      var tiles = new Dictionary<Coords, char> { { Coords.At(0, 0), Display(1) } };
      var steps = HeuristicSearch.AStar(
        new State(1, Coords.At(0,0), pathExplorer),
        new State(goalID),
        (s, i) => s.AvailableDirections(tiles)
      );
      return (tiles, steps);
    }

    public static string Show(Dictionary<Coords, char> tiles)
    {
      var topLeft = tiles.Keys.Min();
      var bottomRight = tiles.Keys.Max();
      var size = bottomRight - topLeft;
      var rows = from y in Enumerable.Range(topLeft.Y, size.Y + 1)
                 from x in Enumerable.Range(topLeft.X, size.X + 1)
                 let c = tiles.GetValueOrDefault(Coords.At(x, y), ' ')
                 orderby y, x
                 group c by y into row
                 select row;
      return string.Join("\n", rows.Select(row => new string(row.ToArray())).ToArray());
    }

    public readonly struct State
    {
      public State(int identifier, Coords position, PathExplorer explorer)
      {
        ID = identifier;
        Position = position;
        Explorer = explorer;
      }
      public State(int identifier) : this(identifier, Coords.At(int.MaxValue, 0), null) { }

      public IEnumerable<State> AvailableDirections(Dictionary<Coords, char> tiles)
      {
        var my = this;
        var nextStates =
          (from direction in Enumerable.Range(1, 4)
          let newPosition = my.Position + Delta(direction)
          let explorer = my.Explorer.Clone()
          let statusCode = explorer.Move(direction)
          select new State(statusCode, newPosition, explorer)).ToArray();
        foreach (var state in nextStates)
          tiles[state.Position] = Display(state.ID);
        return nextStates.Where(s => s.ID != 0);
      }
      public readonly Coords Position;
      public readonly int ID;
      public readonly PathExplorer Explorer;

      public override bool Equals(object o) =>
        (ID == 1) ? (Position.Equals(((State)o).Position)) : (ID == ((State)o).ID);
      public override int GetHashCode() =>
        (ID == 1) ? Position.GetHashCode() : ID.GetHashCode();
    }
    public static Coords Delta(int direction) => direction switch
    {
      1 => Coords.At(0, -1),
      2 => Coords.At(0, 1),
      3 => Coords.At(-1, 0),
      4 => Coords.At(1, 0),
      _ => throw new ApplicationException("Unknown direction"),
    };
    public static char Display(int result) => result switch
    {
      0 => '#',
      1 => '.',
      2 => 'o',
      _ => throw new ApplicationException("Unknown status code"),
    };
  }
  public class PathExplorer : AbstractIntcodeComputer
  {
    public PathExplorer(BigInteger[] program) : base(program) { }

    public int Move(int direction)
    {
      movementCommand = direction;
      HasNewStatus = false;
      while (!HasHalted && !HasNewStatus)
        RunOne();
      return lastStatus;
    }

    int movementCommand;
    int lastStatus;
    bool HasNewStatus = false;

    private PathExplorer() { }
    public PathExplorer Clone()
    {
      var clone = new PathExplorer();
      base.CopyTo(clone);
      clone.movementCommand = movementCommand;
      clone.lastStatus = lastStatus;
      clone.HasNewStatus = HasNewStatus;
      return clone;
    }
    public override BigInteger ReadFromInput() => movementCommand;
    public override void WriteToOutput(BigInteger val)
    {
      lastStatus = (int)val;
      HasNewStatus = true;
    }
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

  public static class ExtensionsHelper
  {
    public static IEnumerable<BigInteger> AsBig(this IEnumerable<int> l) =>
      l.Select(i => new BigInteger(i));
    public static BigInteger AsBig(this int i) => new BigInteger(i);
  }
}
