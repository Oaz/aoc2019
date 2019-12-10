namespace src10
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public class Space
  {
    public Space(IEnumerable<string> lines) :this(Read(lines)) {}

    private static IEnumerable<Coords> Read(IEnumerable<string> lines) =>
      lines.SelectMany((l,y) => 
        l.SelectMany((c,x) => (c=='#')? new Coords[]{Coords.At(x,y)}:new Coords[]{})
      );
    public Space(IEnumerable<Coords> a)
    {
      asteroids = new HashSet<Coords>(a);
      XMax = asteroids.Select(a => a.X).Max();
      YMax = asteroids.Select(a => a.Y).Max();
    }
    int XMax;
    int YMax;

    public IEnumerable<Coords> LineOfSight(Coords origin, int dx, int dy)
    {
      var x = origin.X + dx;
      var y = origin.Y + dy;
      while(x>=0 && y>=0 && x <=XMax && y<=YMax)
      {
        yield return Coords.At(x,y);
        x +=dx;
        y+=dy;
      }
    }
    public static Coords SightVector(Coords from, Coords to)
    {
      var dx = to.X-from.X;
      var dy = to.Y-from.Y;
      if(dx==0) return Coords.At(0,Math.Sign(dy));
      if(dy==0) return Coords.At(Math.Sign(dx),0);
      var pgcd = Math.Abs(GCD(dx,dy));
      return Coords.At(dx/pgcd,dy/pgcd);
    }
    static int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

    public IEnumerable<Coords> FindVisible(Coords origin,Coords direction) =>
      LineOfSight(origin,direction.X,direction.Y).Where(a => asteroids.Contains(a));
    
    public (Coords,int) Best()
    {
      var r = from a in asteroids
              let v = VisibleFrom(a).Count()
              orderby v descending
              select (a,v);
      return r.First();
    }
    
    public IEnumerable<Coords> VisibleFrom(Coords origin)
    {
      var sightVectors =  SightVectorRotatingFrom(origin).ToList();
        foreach (var sv in sightVectors)
        {
          var candidates = FindVisible(origin,sv);
          if(candidates.Any())
            yield return candidates.First();
        }
    }    
    public IEnumerable<Coords> KillsFrom(Coords origin)
    {
      var sightVectors =  SightVectorRotatingFrom(origin).ToList();
      var remaining = new Space(asteroids);
      for(;;)
      {
        foreach (var sv in sightVectors)
        {
          var candidates = remaining.FindVisible(origin,sv);
          if(candidates.Any())
          {
            var killed = candidates.First();
            yield return killed;
            remaining.asteroids.Remove(killed);
          }
        }
      }
    }
    
    public IEnumerable<Coords> SightVectorRotatingFrom(Coords origin)
    {
      var all = from x in Enumerable.Range(0,XMax+1)
                from y in Enumerable.Range(0,YMax+1)
                let c = Coords.At(x,y)
                where !c.Equals(origin)
                let v = SightVector(origin, c)
                let atn = Math.Atan2(v.X,v.Y)
                orderby atn descending
                select v;
      return all.Distinct();
    }
    HashSet<Coords> asteroids;
  }

  public readonly struct Coords
  {
    public static Coords At(int x, int y) => new Coords(x,y);
    public Coords(int x, int y) { X=x; Y=y; }
    public readonly int X;
    public readonly int Y;

    public override string ToString() => $"({X},{Y})";
  }
}
