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

    [Test]
    public void FindPath()
    {
      var camera = new Camera(@"
          #######...#####
          #.....#...#...#
          #.....#...#...#
          ......#...#...#
          ......#...###.#
          ......#.....#.#
          ^########...#.#
          ......#.#...#.#
          ......#########
          ........#...#..
          ....#########..
          ....#...#......
          ....#...#......
          ....#...#......
          ....#####......
      ");
      Check.That(camera.Start.Position).IsEqualTo(Coords.At(0,6));
      Check.That(camera.Segment(camera.Start))
        .IsEqualTo(new Moving(Coords.At(8,6),Coords.At(0,1)));
      Check.That(camera.Path.First()).IsEqualTo(camera.Start);
      Check.That(camera.Path.Last().Position).IsEqualTo(Coords.At(0,2));
      Check.That(camera.TextPath).ContainsExactly(
        "R,8,R,8,R,4,R,4,R,8,L,6,L,2,R,4,R,4,R,8,R,8,R,8,L,6,L,2".Split(',')
      );
    }

    [Test]
    public void Part2()
    {
      var scanner = new Scanner(MyProgram);
      var camera = new Camera(scanner.Image);
      using var console = LocalTestConsole;
      console.WriteLine(string.Join(',',camera.TextPath));
      var orders = @"A,A,B,C,B,C,B,C,C,A
R,8,L,4,R,4,R,10,R,8
L,12,L,12,R,8,R,8
R,10,R,4,R,4
n
";
      var robot = new Robot(MyProgram,orders);
      Check.That(robot.Result).IsEqualTo(673996);
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D17.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }

    public TextWriter LocalTestConsole => new StreamWriter(System.Console.OpenStandardOutput());
  }
}