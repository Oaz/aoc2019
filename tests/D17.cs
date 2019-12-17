namespace tests17
{
  using NUnit.Framework;
  using NFluent;
  using System;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src17;

  public class Tests
  {
    [Test]
    public void FindIntersectionsAndAlignment()
    {
      var camera = new Camera(@"
          ..#..........
          ..#..........
          #######...###
          #.#...#...#.#
          #############
          ..#...#...#..
          ..#####...^..
      ");
      Check.That(camera.Intersections).ContainsExactly(
        new Coords[] { Coords.At(2,2),Coords.At(2,4),Coords.At(6,4),Coords.At(10,4) }
      );
      Check.That(camera.Alignment).IsEqualTo(76);
    }

    [Test]
    public void Part1()
    {
      var scanner = new Scanner(MyProgram);
      var camera = new Camera(scanner.Image);
      Check.That(camera.Alignment).IsEqualTo(3920);
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D17.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }

    public TextWriter LocalTestConsole => new StreamWriter(System.Console.OpenStandardOutput());
  }
}