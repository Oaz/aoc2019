namespace tests24
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using src24;
  using System.Linq;

  public class Tests
  {
    readonly string example = X(@"
    ....#
    #..#.
    #..##
    ..#..
    #....");

    [Test]
    public void Evolution()
    {
      var bugs = new BugsLife(example);
      var evolution = bugs.Evolution.Take(5).Select(x => x.ToString()).ToArray();
      Check.That(evolution[0]).IsEqualTo(example);
      Check.That(evolution[1]).IsEqualTo(X(@"
        #..#.
        ####.
        ###.#
        ##.##
        .##.."));
      Check.That(evolution[2]).IsEqualTo(X(@"
        #####
        ....#
        ....#
        ...#.
        #.###"));
      Check.That(evolution[3]).IsEqualTo(X(@"
        #....
        ####.
        ...##
        #.##.
        .##.#"));
      Check.That(evolution[4]).IsEqualTo(X(@"
        ####.
        ....#
        ##..#
        .....
        ##..."));
    }

    [Test]
    public void BiodiversityRating()
    {
      var bugs = new BugsLife(@"
        .....
        .....
        .....
        #....
        .#...");
      Check.That(bugs.BiodiversityRating).IsEqualTo(2129920);
    }

    [Test]
    public void FirstMatchingAnyPreviousLayout()
    {
      var bugs = new BugsLife(example);
      Check.That(bugs.FirstAppearTwice.ToString()).IsEqualTo(X(@"
        .....
        .....
        .....
        #....
        .#..."));
    }

    [Test]
    public void Part1()
    {
      var bugs = new BugsLife(File.ReadAllText("D24.txt"));
      Check.That(bugs.FirstAppearTwice.BiodiversityRating).IsEqualTo(28903899);
    }

    [Test]
    public void RecursiveEvolution1()
    {
      var bugs = new RecursiveBugsLife(example);
      var after1 = bugs.Evolution.Skip(1).First();
      Check.That(after1.Levels[0].ToString()).IsEqualTo(X(@"
        #..#.
        ####.
        ##..#
        ##.##
        .##.."));
      Check.That(after1.Levels[-1].ToString()).IsEqualTo(X(@"
        .....
        ..#..
        ...#.
        ..#..
        ....."));
      Check.That(after1.Levels[1].ToString()).IsEqualTo(X(@"
        ....#
        ....#
        ....#
        ....#
        #####"));
    }

    [Test]
    public void RecursiveEvolution10()
    {
      var bugs = new RecursiveBugsLife(example);
      var after10 = bugs.Evolution.Skip(10).First();
      Check.That(after10.Levels[-5].ToString()).IsEqualTo(X(@"
        ..#..
        .#.#.
        ....#
        .#.#.
        ..#.."));
      Check.That(after10.Levels[-4].ToString()).IsEqualTo(X(@"
        ...#.
        ...##
        .....
        ...##
        ...#."));
      Check.That(after10.Levels[-3].ToString()).IsEqualTo(X(@"
        #.#..
        .#...
        .....
        .#...
        #.#.."));
      Check.That(after10.Levels[-2].ToString()).IsEqualTo(X(@"
        .#.##
        ....#
        ....#
        ...##
        .###."));
      Check.That(after10.Levels[-1].ToString()).IsEqualTo(X(@"
        #..##
        ...##
        .....
        ...#.
        .####"));
      Check.That(after10.Levels[0].ToString()).IsEqualTo(X(@"
        .#...
        .#.##
        .#...
        .....
        ....."));
      Check.That(after10.Levels[1].ToString()).IsEqualTo(X(@"
        .##..
        #..##
        ....#
        ##.##
        #####"));
      Check.That(after10.Levels[2].ToString()).IsEqualTo(X(@"
        ###..
        ##.#.
        #....
        .#.##
        #.#.."));
      Check.That(after10.Levels[3].ToString()).IsEqualTo(X(@"
        ..###
        .....
        #....
        #....
        #...#"));
      Check.That(after10.Levels[4].ToString()).IsEqualTo(X(@"
        .###.
        #..#.
        #....
        ##.#.
        ....."));
      Check.That(after10.Levels[5].ToString()).IsEqualTo(X(@"
        ####.
        #..#.
        #..#.
        ####.
        ....."));
      Check.That(after10.TotalBugsCount).IsEqualTo(99);
    }

    [Test]
    public void Part2()
    {
      var bugs = new RecursiveBugsLife(File.ReadAllText("D24.txt"));
      var after200 = bugs.Evolution.Skip(200).First();
      Check.That(after200.TotalBugsCount).IsEqualTo(1896);
    }

    static string X(string s) => string.Join('\n', s.Trim().Split('\n').Select(l => l.Trim()));
  }
}