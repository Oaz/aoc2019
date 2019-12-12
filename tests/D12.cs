namespace tests12
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src12;

  public class Tests
  {

    [TestCase(0, "(-1,0,2),(0,0,0)", "(2,-10,-7),(0,0,0)", "(4,-8,8),(0,0,0)", "(3,5,-1),(0,0,0)")]
    [TestCase(1, "(2,-1,1),(3,-1,-1)", "(3,-7,-4),(1,3,3)", "(1,-7,5),(-3,1,-3)", "(2,2,0),(-1,-3,1)")]
    public void Steps(int steps, string moon0, string moon1, string moon2, string moon3)
    {
      var newMoons = new Universe(example1).Steps(steps).Moons;
      Check.That(newMoons[0].ToString()).IsEqualTo(moon0);
      Check.That(newMoons[1].ToString()).IsEqualTo(moon1);
      Check.That(newMoons[2].ToString()).IsEqualTo(moon2);
      Check.That(newMoons[3].ToString()).IsEqualTo(moon3);
    }

    [TestCase(0, 6, 6, 36)]
    [TestCase(1, 9, 5, 45)]
    [TestCase(2, 10, 8, 80)]
    [TestCase(3, 6, 3, 18)]
    public void Energy(int i, int pot, int kin, int tot)
    {
      var newMoons = new Universe(example1).Steps(10).Moons;
      Check.That(newMoons[i].PotentialEnergy).IsEqualTo(pot);
      Check.That(newMoons[i].KineticEnergy).IsEqualTo(kin);
      Check.That(newMoons[i].TotalEnergy).IsEqualTo(tot);
    }

    [Test]
    public void TotalEnergy() =>
      Check.That(new Universe(example1).Steps(10).TotalEnergy).IsEqualTo(179);

    [Test]
    public void Part1() =>
      Check.That(new Universe(MySystem).Steps(1000).TotalEnergy).IsEqualTo(7928);

    public string[] MySystem => File.ReadAllLines("D12.txt");

    readonly string[] example1 = new string[] {
      "<x=-1, y=0, z=2>",
      "<x=2, y=-10, z=-7>",
      "<x=4, y=-8, z=8>",
      "<x=3, y=5, z=-1>"
      };
    readonly string[] example2 = new string[] {
      "<x=-8, y=-10, z=0>",
      "<x=5, y=5, z=10>",
      "<x=2, y=-7, z=3>",
      "<x=9, y=-8, z=-3>"
      };

    [Test]
    public void FindExample1()
    {
      var u = new Universe(example1);
      Check.That(u.ProjectionX.NumberOfStepsUntilSame()).IsEqualTo(18);
      Check.That(u.ProjectionY.NumberOfStepsUntilSame()).IsEqualTo(28);
      Check.That(u.ProjectionZ.NumberOfStepsUntilSame()).IsEqualTo(44);
      Check.That(u.NumberOfStepsUntilSame).IsEqualTo(2772);
    }

    [Test]
    public void FindExample2()
    {
      var u = new Universe(example2);
      Check.That(u.ProjectionX.NumberOfStepsUntilSame()).IsEqualTo(2028);
      Check.That(u.ProjectionY.NumberOfStepsUntilSame()).IsEqualTo(5898);
      Check.That(u.ProjectionZ.NumberOfStepsUntilSame()).IsEqualTo(4702);
      Check.That(u.NumberOfStepsUntilSame).IsEqualTo(4686774924);
    }

    [Test]
    public void Part2() =>
      Check.That(new Universe(MySystem).NumberOfStepsUntilSame).IsEqualTo(518311327635164);

  }
}