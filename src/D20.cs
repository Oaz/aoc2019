using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Heuristic.Linq;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;

namespace src20
{
    public class DonutMaze
  {
    public DonutMaze(string map)
    {
      var lines = map.Split('\n').Where(l => l.Trim().Length > 0).Select(l => l.Trim('\n','\r')).ToArray();
      var indexes = Enumerable.Range(0,lines[0].Length);
      Size = Coords.At(lines[0].Length-4,lines.Length-4);
      OutsidePortals =
        lines.Select((l,i) => (l.Substring(0,2),Coords.At(0,i-2)))
        .Concat(lines.Select((l,i) => (l.Substring(lines[0].Length-2,2),Coords.At(Size.X-1,i-2))))
        .Concat(indexes.Select(i => (lines[0].Substring(i,1)+lines[1].Substring(i,1),Coords.At(i-2,0))))
        .Concat(indexes.Select(i => (lines[^2].Substring(i,1)+lines[^1].Substring(i,1),Coords.At(i-2,Size.Y-1))))
        .Where(x => x.Item1 != "  ")
        .ToDictionary(x => x.Item1, x => x.Item2);
      var innerLines = lines.Skip(2).Take(Size.Y).Select(l => l.Substring(2,Size.X)).ToArray();
      var innerRows = innerLines.Select((l,i)=>l.Contains(' ')?i:0).Where(y => y!=0).ToArray();
      var topRow = innerRows.First();
      HoleTopLeft = Coords.At(innerLines[topRow].Select((c,i) => c==' '?i:0).First(x => x!=0),topRow);
      var bottomRow = innerRows.Last();
      HoleBottomRight = Coords.At(innerLines[bottomRow].Select((c,i) => c==' '?i:0).Last(x => x!=0),bottomRow);
      var innerIndexes = Enumerable.Range(0,innerLines[0].Length);
      InsidePortals =
        innerLines.Select((l,i) => (l.Substring(HoleTopLeft.X,2),Coords.At(HoleTopLeft.X-1,i)))
        .Concat(innerLines.Select((l,i) => (l.Substring(HoleBottomRight.X-1,2),Coords.At(HoleBottomRight.X+1,i))))
        .Concat(innerIndexes.Select(i => (innerLines[HoleTopLeft.Y].Substring(i,1)+innerLines[HoleTopLeft.Y+1].Substring(i,1),Coords.At(i,HoleTopLeft.Y-1))))
        .Concat(innerIndexes.Select(i => (innerLines[HoleBottomRight.Y-1].Substring(i,1)+innerLines[HoleBottomRight.Y].Substring(i,1),Coords.At(i,HoleBottomRight.Y+1))))
        .Where(x => x.Item1 != "  " && !x.Item1.Contains('#') && !x.Item1.Contains('.'))
        .ToDictionary(x => x.Item1, x => x.Item2);
      var coords = from x in Enumerable.Range(0, Size.X)
                   from y in Enumerable.Range(0, Size.Y)
                   where (x<HoleTopLeft.X) || (x>HoleBottomRight.X) || (y<HoleTopLeft.Y) || (y>HoleBottomRight.Y)
                   select Coords.At(x, y);
      Tunnels = coords.ToDictionary(c => c, c => innerLines[c.Y][c.X].ToString());
      foreach (var ip in OutsidePortals)
        Tunnels[ip.Value] = ip.Key;
      foreach (var ip in InsidePortals)
        Tunnels[ip.Value] = ip.Key;
    }
    public IEnumerable<Coords> CoordsOf(string s) => Tunnels.Where(t => t.Value == s).Select(t => t.Key);

    public int Path(string begin, string end)
    {
      var candidates = from coordsBegin in CoordsOf(begin)
                       from coordsEnd in  CoordsOf(end)
                       select (coordsBegin,coordsEnd);
      return candidates.Select(be =>
        HeuristicSearch.AStar( be.coordsBegin, be.coordsEnd,
                              (c, i) => Deltas.Select(d => c + d)
                                .Where(p => Tunnels.ContainsKey(p) && Tunnels[p]!="#")).Count()-1
      ).Max();
    }

    public Dictionary<ValueTuple<string,string>,int> Edges =>
      (from From in OutsidePortals.Keys
       from To in OutsidePortals.Keys
       where From != To
       let Length = Path(From, To)
       where Length > 0
       select (From, To, Length)).ToDictionary(x => (x.From, x.To), x => x.Length);

    public int ShortestGlobalPath()
    {
      var nodes = OutsidePortals.Keys;
      var edges = Edges;
      var graph = new AdjacencyGraph<string, Edge<string>>();
      graph.AddVertexRange(nodes);
      graph.AddEdgeRange(edges.Keys.Select(e => new Edge<string>(e.Item1, e.Item2)));
      var search = new AStarShortestPathAlgorithm<string, Edge<string>>(
                graph,
                e => edges[(e.Source, e.Target)] + 1,
                v => 0
      );
      search.Compute("AA");
      return search.TryGetDistance("ZZ", out double d) ? (int)(d - 1) : -1;
    }

    static Coords[] Deltas = new Coords[] {
        Coords.At(0, 1), Coords.At(0, -1), Coords.At(1, 0), Coords.At(-1, 0)
      };
    public Coords Size;
    public Coords HoleTopLeft;
    public Coords HoleBottomRight;
    public Dictionary<string,Coords> OutsidePortals;
    public Dictionary<string,Coords> InsidePortals;
    public Dictionary<Coords,string> Tunnels;
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
  public class LambdaComparer<T> : IEqualityComparer<T>
  {
    public static LambdaComparer<T> With(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash = null) =>
      new LambdaComparer<T>(lambdaComparer, lambdaHash);
    private readonly Func<T, T, bool> lambdaComparer;
    private readonly Func<T, int> lambdaHash;
    public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
    {
      this.lambdaComparer = lambdaComparer;
      this.lambdaHash = lambdaHash ?? (x => 0);
    }
    public bool Equals(T x, T y) => lambdaComparer(x, y);
    public int GetHashCode(T obj) => lambdaHash(obj);
  }
}
