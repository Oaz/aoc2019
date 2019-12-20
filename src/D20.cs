using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Heuristic.Linq;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;

namespace src20
{
  public class RecursiveMaze
  {
    public RecursiveMaze(string map, int maxLevel)
    {
      MaxLevel = maxLevel;
      donutMaze = new DonutMaze(map);
      Nodes = Enumerable.Range(0, 2*maxLevel).SelectMany(GetNodes).ToList();
      var nodesForEdges = Enumerable.Range(0, 2).SelectMany(level => GetPortals(level).Select(p => (p.Key, p.Value, level))).ToList();
      var edgeCandidates = 
        from begin in nodesForEdges
        from end in nodesForEdges
        where begin != end
        let length = HeuristicSearch.AStar(
                      begin.Value, end.Value,
                      (c, i) => DonutMaze
                                  .Deltas.Select(d => c + d)
                                  .Where(p => donutMaze.Tunnels.ContainsKey(p)
                                              && donutMaze.Tunnels[p] != "#")
                    ).Count()-1
        where length > 0
        select KeyValuePair.Create((new Node(begin.Key, begin.level), new Node(end.Key, end.level)), length);
      Edges = new Dictionary<(Node, Node), int>(edgeCandidates);
      Graph = new AdjacencyGraph<Node, Edge<Node>>();
      Graph.AddVertexRange(Nodes);
      Graph.AddEdgeRange(
        from node1 in Nodes
        from node2 in Nodes
        let baseLevel = 2*(Math.Min(node1.Level,node2.Level) / 2)
        let refLevel1 = node1.Level - baseLevel
        where refLevel1 < 2
        let refLevel2 = node2.Level - baseLevel
        where refLevel2 < 2
        let edgeKey = (new Node(node1.Name, refLevel1), new Node(node2.Name, refLevel2))
        let length = Edges.ContainsKey(edgeKey) ? Edges[edgeKey] : -1
        where length > 0
        select new TaggedEdge<Node,int>(node1, node2, length)
      );
      var recursionEdges = from nodeName in donutMaze.InsidePortals.Keys
                           from level in Enumerable.Range(1, maxLevel-1).Select(x => 2*x)
                           select new TaggedEdge<Node,int>(new Node(nodeName,level-1), new Node(nodeName, level), 1);
      Graph.AddEdgeRange(recursionEdges);
      Graph.AddEdgeRange(recursionEdges.Select(re => new TaggedEdge<Node,int>(re.Target, re.Source, 1)));
    }

    public int ShortestGlobalPath()
    {
      var search = new AStarShortestPathAlgorithm<Node, Edge<Node>>(Graph,e => ((TaggedEdge<Node,int>)e).Tag,v => 0);
      search.Compute(new Node("AA",0));
      return search.TryGetDistance(new Node("ZZ", 0), out double d) ? (int)d : -1;
    }

    public readonly int MaxLevel;
    readonly DonutMaze donutMaze;
    public readonly List<Node> Nodes;
    public readonly Dictionary<(Node,Node),int> Edges;
    public readonly AdjacencyGraph<Node, Edge<Node>> Graph;

    private Dictionary<string, Coords> GetPortals(int level) =>
      level == 0 ? donutMaze.OutsidePortals : donutMaze.InsidePortals;
    private IEnumerable<Node> GetNodes(int level) =>
      GetPortals(level).Keys.Select(n => new Node(n, level));

    public readonly struct Node
    {
      public Node(string name, int level)
      {
        Name = name;
        Level = level;
      }
      public readonly string Name;
      public readonly int Level;
      public override string ToString() => $"{Name}-{Level}";
    }
  }
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
        .Where(x => !x.Item1.Contains(' ') && !x.Item1.Contains('#') && !x.Item1.Contains('.'))
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

    public static Coords[] Deltas = new Coords[] {
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
    public static bool operator ==(Coords a, Coords b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Coords a, Coords b) => a.X != b.X || a.Y != b.Y;
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
