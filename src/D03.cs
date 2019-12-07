namespace src03
{
  using System;
  using System.Linq;
  using System.Collections.Generic;

  public readonly struct Coords
  {
    public static Coords At(int x, int y) => new Coords(x,y);
    public Coords(int x, int y) { X=x; Y=y; }
    public readonly int X;
    public readonly int Y;

    public override string ToString() => $"({X},{Y})";
    public int DistanceToOrigin => Math.Abs(X)+Math.Abs(Y);

    public IEnumerable<Coords> Segment(string segment)
    {
      var len = int.Parse(segment.Substring(1));
      var range = Enumerable.Range(1,len);
      var builder = BuildSegment(segment[0]);
      var origin = this;
      return range.Select(i => builder(origin.X,origin.Y,i));
    }
    private static Func<int,int,int,Coords> BuildSegment(char type)
    {
      switch(type)
      {
        case 'U': return (x,y,i) => Coords.At(x,y+i);
        case 'D': return (x,y,i) => Coords.At(x,y-i);
        case 'L': return (x,y,i) => Coords.At(x-i,y);
        case 'R': return (x,y,i) => Coords.At(x+i,y);
      }
      throw new Exception("Unknown segment type");
    }
  }

  public static class Code
  {
    public static int ShortestSteps(string wire1, string wire2)
    {
      var coords1 = CoordsForWire(wire1);
      var coords2 = CoordsForWire(wire2);
      var intersections = coords1.Intersect(coords2);
      return intersections.Select(c => coords1.CountStepsTo(c)+coords2.CountStepsTo(c)).Min();
    }

    public static int CountStepsTo(this IList<Coords> coords, Coords destination)
     => coords.IndexOf(destination)+1;

    public static int ShortestDistance(string wire1, string wire2)
    {
      return Intersections(wire1,wire2).Select(c => c.DistanceToOrigin).Min();
    }
    public static IEnumerable<Coords> Intersections(string wire1, string wire2)
    {
      var coords1 = CoordsForWire(wire1);
      var coords2 = CoordsForWire(wire2);
      return coords1.Intersect(coords2);
    }

    public static IList<Coords> CoordsForWire(string wire)
    {
      var segments = wire.Split(',');
      var origin = Coords.At(0,0);
      var allCoords = new List<Coords>();
      foreach (var segment in segments)
      {
          allCoords.AddRange(origin.Segment(segment));
          origin = allCoords.Last();
      }
      return allCoords;
    }
  }
}
