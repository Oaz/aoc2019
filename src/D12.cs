namespace src12
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;
  using System.Text.RegularExpressions;

  public readonly struct Moon
  {
    public Moon(string def)
    {
      var match = Regex.Match(def, "<x=([0-9-]+), y=([0-9-]+), z=([0-9-]+)>");
      var coords = match.Groups.Cast<Group>().Skip(1).Select(g => int.Parse(g.Value)).ToArray();
      Position = Coords3.At(coords[0], coords[1], coords[2]);
      Velocity = Coords3.At(0, 0, 0);
    }
    public Moon(Coords3 p, Coords3 v)
    {
      Position = p;
      Velocity = v;
    }
    public readonly Coords3 Position;
    public readonly Coords3 Velocity;
    public override string ToString() => $"{Position},{Velocity}";
    public int PotentialEnergy => Position.Length;
    public int KineticEnergy => Velocity.Length;
    public int TotalEnergy => PotentialEnergy * KineticEnergy;
    public Coords3 Gravity(Moon other) => Coords3.At(
        Position.X > other.Position.X ? -1 : (Position.X < other.Position.X ? 1 : 0),
        Position.Y > other.Position.Y ? -1 : (Position.Y < other.Position.Y ? 1 : 0),
        Position.Z > other.Position.Z ? -1 : (Position.Z < other.Position.Z ? 1 : 0)
      );

    public Moon ApplyGravity(Coords3 g) => new Moon(Position,Velocity+g);

  }

  public readonly struct Universe
  {
    public Universe(IEnumerable<string> defs) => Moons = defs.Select(d => new Moon(d)).ToArray();      
    public Universe(Moon[] moons) => Moons = moons;

    public readonly Moon[] Moons;
    public Universe Step()
    {
      var s = Moons.ToArray();
      for (int i = 0; i < s.Length; i++)
      {
        var moon1 = s[i];
        for (int j = i + 1; j < s.Length; j++)
        {
          var moon2 = s[j];
          var g = moon2.Gravity(moon1);
          s[j] = s[j].ApplyGravity(g);
          s[i] = s[i].ApplyGravity(-g);
        }
      }
      var u = s.Select(m => new Moon(m.Position+m.Velocity,m.Velocity)).ToArray();
      return new Universe(u);
    }

    public Universe Steps(int steps) =>
      Enumerable.Range(0,steps).Aggregate(this, (u,i) => u.Step());

    public int TotalEnergy => Moons.Select(m => m.TotalEnergy).Sum();

    public Projection ProjectionX => new Projection(Moons,c => c.X);
    public Projection ProjectionY => new Projection(Moons,c => c.Y);
    public Projection ProjectionZ => new Projection(Moons,c => c.Z);
    public long NumberOfStepsUntilSame =>
      LCM(ProjectionX.NumberOfStepsUntilSame(),
        LCM(ProjectionY.NumberOfStepsUntilSame(),ProjectionZ.NumberOfStepsUntilSame()));

    static long LCM(long a,long b)
    {
      var p=a*b;
      while (b!=0) (a,b)=(b,a%b);
      return p/a;
    }
  }
  public readonly struct Coords3
  {
    public static Coords3 At(int x, int y, int z) => new Coords3(x, y, z);
    public Coords3(int x, int y, int z) { X = x; Y = y; Z = z; }
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public Coords3 Abs => Coords3.At(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    public int Length => Abs.X + Abs.Y + Abs.Z;

    public override string ToString() => $"({X},{Y},{Z})";

    public static Coords3 operator +(Coords3 a, Coords3 b) => At(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Coords3 operator -(Coords3 a, Coords3 b) => At(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Coords3 operator -(Coords3 a) => At(-a.X, -a.Y, -a.Z);
  }

  public static class CoordsExtensions
  {
    public static Coords3 Bulk(this IEnumerable<Coords3> l, Func<IEnumerable<int>, int> f) =>
      Coords3.At(f(l.Select(c => c.X)), f(l.Select(c => c.Y)), f(l.Select(c => c.Z)));
    public static Coords3 Min(this IEnumerable<Coords3> l) => l.Bulk(x => x.Min());
    public static Coords3 Max(this IEnumerable<Coords3> l) => l.Bulk(x => x.Max());
  }

  public readonly struct Projection
  {
    public Projection(IEnumerable<Moon> moons, Func<Coords3,int> p) =>
      values = moons.Select(m => (p(m.Position),p(m.Velocity))).ToArray();   
    public Projection((int,int)[] v) => values = v;   
    private readonly (int,int)[] values;
    public Projection Step()
    {
      var v=values.ToArray();
      for (int i = 0; i < v.Length; i++)
      {
        for (int j = i + 1; j < v.Length; j++)
        {
          var g = Math.Sign(v[i].Item1-v[j].Item1);
          v[i].Item2 -= g;
          v[j].Item2 += g;
        }
      }
      var newValues = v.Select(p => (p.Item1+p.Item2,p.Item2)).ToArray();
      return new Projection(newValues);
    }
    public int NumberOfStepsUntilSame()
    {
      var i = 1;
      var now = this;
      for(;;)
      {
        var next = now.Step();
        if(next.values.Zip(values).All(v => v.First == v.Second))
          return i;
        i++;
        now = next;
      }
    }
  }

}
