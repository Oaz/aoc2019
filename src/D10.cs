namespace src10
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public class Space
  {
    public Space(IEnumerable<string> lines)
    {
      asteroids = lines.SelectMany((l,y) => 
        l.SelectMany((c,x) =>
          (c=='#')? new Coords[]{Coords.At(x,y)}:new Coords[]{})
      ).ToList();
      XMax = asteroids.Select(a => a.X).Max();
      YMax = asteroids.Select(a => a.Y).Max();
    }
    public Space(IEnumerable<Coords> a)
    {
      asteroids = a.ToList();
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

    public bool IsVisible(Coords from, Coords to)
    {
      var sv = SightVector(from,to);
      var los = LineOfSight(from,sv.X,sv.Y).ToList();
      var ailos = los.Intersect(asteroids).ToList();
      var filos = ailos.First();
      return filos.Equals(to);
    }
    
    public int NumberOfVisibleFrom(Coords origin)
    {
      return asteroids.Where(a => !a.Equals(origin) && IsVisible(origin,a)).Count();
    }    
    public (Coords,int) Best()
    {
      var r = from a in asteroids
              let v = NumberOfVisibleFrom(a)
              orderby v descending
              select (a,v);
      return r.First();
    }
    
    public IEnumerable<Coords> KillsFrom(Coords origin)
    {
      var sightVectors =  SightVectorRotatingFrom(origin).ToList();
      var remaining = new Space(asteroids);
      for(;;)
      {
        foreach (var sv in sightVectors)
        {
          var candidates = remaining.LineOfSight(origin,sv.X,sv.Y).Intersect(remaining.asteroids);
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
    IList<Coords> asteroids;
  }

  public readonly struct Coords
  {
    public static Coords At(int x, int y) => new Coords(x,y);
    public Coords(int x, int y) { X=x; Y=y; }
    public readonly int X;
    public readonly int Y;

    public override string ToString() => $"({X},{Y})";
    public int DistanceToOrigin => Math.Abs(X)+Math.Abs(Y);
  }
}
