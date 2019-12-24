namespace src24
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class RecursiveBugsLife
  {
    public RecursiveBugsLife(string definition)
      : this(new Dictionary<int, BugsLife> { [0] = new BugsLife(definition) }) { }
    private RecursiveBugsLife(Dictionary<int, BugsLife> levels)
    {
      Levels = levels;
      Levels[Levels.Keys.Min() - 1] = new BugsLife(Size);
      Levels[Levels.Keys.Max() + 1] = new BugsLife(Size);
    }
    public int TotalBugsCount => Levels.Values.Select(l => l.Bugs.Count).Sum();
    public readonly Dictionary<int, BugsLife> Levels;
    public static readonly Coords Size;
    private static readonly Coords Center;
    public IEnumerable<RecursiveBugsLife> Evolution =>
      LinqX.Generate(this, b => b.Evolve());
    public RecursiveBugsLife Evolve()
    {
      var newLevels = new Dictionary<int, BugsLife>();
      foreach (var level in Levels)
      {
        var newBugs = level.Value.NewBugs(p => CountNeighbors(level.Key, p));
        newLevels[level.Key] = new BugsLife(Size, new HashSet<Coords>(newBugs));
      }
      return new RecursiveBugsLife(newLevels);
    }
    private int CountNeighbors(int baseDepth, Coords position)
    {
      if (position == Center)
        return 0;
      var neighborCounts = from relativeDepth in Enumerable.Range(-1, 3)
                           let depth = baseDepth + relativeDepth
                           where Levels.ContainsKey(depth)
                           let level = Levels[depth]
                           let adj = adjacents[position][relativeDepth + 1]
                           let neighbors = level.Bugs.Intersect(adj).Count()
                           select neighbors;
      return neighborCounts.Sum();
    }

    static readonly Dictionary<Coords, Coords[][]> adjacents;
    static RecursiveBugsLife()
    {
      Size = Coords.At(5, 5);
      Center = Coords.At(2, 2);
      adjacents = new Dictionary<Coords, Coords[][]>();
      var empty = new Coords[] { };
      var outsideTop = new Coords[] { Coords.At(2, 1) };
      var outsideBottom = new Coords[] { Coords.At(2, 3) };
      var outsideLeft = new Coords[] { Coords.At(1, 2) };
      var outsideRight = new Coords[] { Coords.At(3, 2) };
      A(0, 0, outsideTop.Concat(outsideLeft).ToArray(), empty);
      A(1, 0, outsideTop, empty);
      A(2, 0, outsideTop, empty);
      A(3, 0, outsideTop, empty);
      A(4, 0, outsideTop.Concat(outsideRight).ToArray(), empty);
      A(0, 1, outsideLeft, empty);
      A(1, 1, empty, empty);
      A(2, 1, empty, Range(x => Coords.At(x, 0)));
      A(3, 1, empty, empty);
      A(4, 1, outsideRight, empty);
      A(0, 2, outsideLeft, empty);
      A(1, 2, empty, Range(y => Coords.At(0, y)));
      A(3, 2, empty, Range(y => Coords.At(4, y)));
      A(4, 2, outsideRight, empty);
      A(0, 3, outsideLeft, empty);
      A(1, 3, empty, empty);
      A(2, 3, empty, Range(x => Coords.At(x, 4)));
      A(3, 3, empty, empty);
      A(4, 3, outsideRight, empty);
      A(0, 4, outsideBottom.Concat(outsideLeft).ToArray(), empty);
      A(1, 4, outsideBottom, empty);
      A(2, 4, outsideBottom, empty);
      A(3, 4, outsideBottom, empty);
      A(4, 4, outsideBottom.Concat(outsideRight).ToArray(), empty);
      void A(int x, int y, Coords[] o, Coords[] i)
      {
        var p = Coords.At(x,y);
        var locals = BugsLife.Deltas.Select(d => d + p).Where(IsInside);
        adjacents[p] = new Coords[][] { o, locals.ToArray(), i };
      }
      static bool IsInside(Coords c) =>
        c != Center && c.X >= 0 && c.Y >= 0 && c.X <= Size.X && c.Y <= Size.Y;
      static Coords[] Range(Func<int,Coords> f) =>
        Enumerable.Range(0, 5).Select(f).ToArray();
    }
  }
  public class BugsLife
  {
    public BugsLife(string definition)
    {
      var tiles = definition.Trim().Split('\n').Select(l => l.Trim()).ToArray();
      Size = Coords.At(tiles[0].Length, tiles.Length);
      var bugs = from x in Enumerable.Range(0, Size.X)
                 from y in Enumerable.Range(0, Size.Y)
                 where tiles[y][x] == '#'
                 select Coords.At(x, y);
      Bugs = new HashSet<Coords>(bugs);
    }
    public BugsLife(Coords size) : this(size, new HashSet<Coords>()) { }
    public BugsLife(Coords size, HashSet<Coords> bugs) { Size = size; Bugs = bugs; }
    public HashSet<Coords> Bugs;
    public Coords Size;
    public long BiodiversityRating => Bugs.Select(b => 1 << (b.Y * Size.X + b.X)).Sum();
    public BugsLife FirstAppearTwice =>
      Evolution.FirstAppearingTwice(layout => layout.BiodiversityRating);
    public IEnumerable<BugsLife> Evolution => LinqX.Generate(this, b => b.Evolve);
    public BugsLife Evolve =>
      new BugsLife(Size, new HashSet<Coords>(NewBugs(CountNeighbors)));
    public IEnumerable<Coords> NewBugs(Func<Coords, int> countNeighbors) =>
      from position in AllPositions
      let neighbors = countNeighbors(position)
      let hasBug = Bugs.Contains(position)
      let infested = !hasBug && (neighbors == 1 || neighbors == 2)
      let survive = hasBug && neighbors == 1
      where infested || survive
      select position;
    private int CountNeighbors(Coords position)
    {
      var adjacents = Deltas.Select(d => d + position).Where(IsInside);
      return Bugs.Intersect(adjacents).Count();
    }
    private IEnumerable<Coords> AllPositions => from y in Enumerable.Range(0, Size.Y)
                                                from x in Enumerable.Range(0, Size.X)
                                                select Coords.At(x, y);
    public static Coords[] Deltas = new Coords[] {
      Coords.At(0, 1), Coords.At(0, -1), Coords.At(1, 0), Coords.At(-1, 0)
    };

    public bool IsInside(Coords c) => c.X >= 0 && c.Y >= 0 && c.X <= Size.X && c.Y <= Size.Y;
    public override string ToString() =>
      string.Join('\n', Enumerable.Range(0, Size.Y).Select(DisplayRow));
    private string DisplayRow(int y) => new string(Enumerable.Range(0, Size.X)
            .Select(x => Bugs.Contains(Coords.At(x, y)) ? '#' : '.').ToArray());
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
    public static bool operator ==(Coords a, Coords b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Coords a, Coords b) => a.X != b.X || a.Y != b.Y;
  }
  public static class LinqX
  {
    public static T Identity<T>(T x) => x;
    public static IEnumerable<T> Generate<T>(T x, Func<T, T> f)
    {
      for (; ; ) { yield return x; x = f(x); }
    }
    public static IEnumerable<T> RepeatInfinitely<T>(this IEnumerable<T> l) =>
      Generate(l, Identity).SelectMany(Identity);
    public static T FirstAppearingTwice<T, U>(this IEnumerable<T> l, Func<T, U> key)
      where T : class
    {
      var previous = new HashSet<U>();
      foreach (var x in l)
      {
        var k = key(x);
        if (previous.Contains(k))
          return x;
        previous.Add(k);
      }
      return null;
    }
  }
}
