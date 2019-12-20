namespace tests20
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using src20;

  public class Tests
  {
    string example1 = @"
         A           
         A           
  #######.#########  
  #######.........#  
  #######.#######.#  
  #######.#######.#  
  #######.#######.#  
  #####  B    ###.#  
BC...##  C    ###.#  
  ##.##       ###.#  
  ##...DE  F  ###.#  
  #####    G  ###.#  
  #########.#####.#  
DE..#######...###.#  
  #.#########.###.#  
FG..#########.....#  
  ###########.#####  
             Z       
             Z       ";

    [Test]
    public void Example1Basics()
    {
      var maze = new DonutMaze(example1);
      Check.That(maze.Size).IsEqualTo(Coords.At(17,15));
      Check.That(maze.HoleTopLeft).IsEqualTo(Coords.At(5,5));
      Check.That(maze.HoleBottomRight).IsEqualTo(Coords.At(11,9));
    }

    [TestCase("AA", 7, 0)]
    [TestCase("ZZ", 11, 14)]
    [TestCase("BC", 0, 6)]
    [TestCase("DE", 0, 11)]
    [TestCase("FG", 0, 13)]
    public void Example1OutsidePortals(string name, int x, int y)
    {
      var maze = new DonutMaze(example1);
      Check.That(maze.OutsidePortals[name]).IsEqualTo(Coords.At(x, y));
    }

    [TestCase("BC", 7, 4)]
    [TestCase("DE", 4, 8)]
    [TestCase("FG", 9, 10)]
    public void Example1InsidePortals(string name, int x, int y)
    {
      var maze = new DonutMaze(example1);
      Check.That(maze.InsidePortals[name]).IsEqualTo(Coords.At(x, y));
    }

    [TestCase("AA", "ZZ", 26)]
    [TestCase("ZZ", "AA", 26)]
    [TestCase("AA", "BC", 4)]
    [TestCase("AA", "DE", -1)]
    [TestCase("AA", "FG", 30)]
    [TestCase("BC", "DE", 6)]
    [TestCase("DE", "FG", 4)]
    [TestCase("FG", "ZZ", 6)]
    [TestCase("DE", "ZZ", -1)]
    public void Example1Paths(string begin, string end, int len)
    {
      var maze = new DonutMaze(example1);
      Check.That(maze.Path(begin,end)).IsEqualTo(len);
    }

    [Test]
    public void Example1ShortestGlobalPath()
    {
      var maze = new DonutMaze(example1);
      Check.That(maze.ShortestGlobalPath()).IsEqualTo(23);
    }

    [Test]
    public void Example1RecursivePath()
    {
      var maze = new RecursiveMaze(example1,2);
      Check.That(maze.Nodes.Count).IsEqualTo(14);
      Check.That(maze.ShortestGlobalPath()).IsEqualTo(26);
    }

    string example2 = @"
                   A               
                   A               
  #################.#############  
  #.#...#...................#.#.#  
  #.#.#.###.###.###.#########.#.#  
  #.#.#.......#...#.....#.#.#...#  
  #.#########.###.#####.#.#.###.#  
  #.............#.#.....#.......#  
  ###.###########.###.#####.#.#.#  
  #.....#        A   C    #.#.#.#  
  #######        S   P    #####.#  
  #.#...#                 #......VT
  #.#.#.#                 #.#####  
  #...#.#               YN....#.#  
  #.###.#                 #####.#  
DI....#.#                 #.....#  
  #####.#                 #.###.#  
ZZ......#               QG....#..AS
  ###.###                 #######  
JO..#.#.#                 #.....#  
  #.#.#.#                 ###.#.#  
  #...#..DI             BU....#..LF
  #####.#                 #.#####  
YN......#               VT..#....QG
  #.###.#                 #.###.#  
  #.#...#                 #.....#  
  ###.###    J L     J    #.#.###  
  #.....#    O F     P    #.#...#  
  #.###.#####.#.#####.#####.###.#  
  #...#.#.#...#.....#.....#.#...#  
  #.#####.###.###.#.#.#########.#  
  #...#.#.....#...#.#.#.#.....#.#  
  #.###.#####.###.###.#.#.#######  
  #.#.........#...#.............#  
  #########.###.###.#############  
           B   J   C               
           U   P   P               ";


    [Test]
    public void Example2ShortestGlobalPath()
    {
      var maze = new DonutMaze(example2);
      Check.That(maze.ShortestGlobalPath()).IsEqualTo(58);
    }

    [Test]
    public void Part1()
    {
      var maze = new DonutMaze(File.ReadAllText("D20.txt"));
      Check.That(maze.Size).IsEqualTo(Coords.At(113,121));
      Check.That(maze.ShortestGlobalPath()).IsEqualTo(588);
    }

    string example3 = @"
             Z L X W       C                 
             Z P Q B       K                 
  ###########.#.#.#.#######.###############  
  #...#.......#.#.......#.#.......#.#.#...#  
  ###.#.#.#.#.#.#.#.###.#.#.#######.#.#.###  
  #.#...#.#.#...#.#.#...#...#...#.#.......#  
  #.###.#######.###.###.#.###.###.#.#######  
  #...#.......#.#...#...#.............#...#  
  #.#########.#######.#.#######.#######.###  
  #...#.#    F       R I       Z    #.#.#.#  
  #.###.#    D       E C       H    #.#.#.#  
  #.#...#                           #...#.#  
  #.###.#                           #.###.#  
  #.#....OA                       WB..#.#..ZH
  #.###.#                           #.#.#.#  
CJ......#                           #.....#  
  #######                           #######  
  #.#....CK                         #......IC
  #.###.#                           #.###.#  
  #.....#                           #...#.#  
  ###.###                           #.#.#.#  
XF....#.#                         RF..#.#.#  
  #####.#                           #######  
  #......CJ                       NM..#...#  
  ###.#.#                           #.###.#  
RE....#.#                           #......RF
  ###.###        X   X       L      #.#.#.#  
  #.....#        F   Q       P      #.#.#.#  
  ###.###########.###.#######.#########.###  
  #.....#...#.....#.......#...#.....#.#...#  
  #####.#.###.#######.#######.###.###.#.#.#  
  #.......#.......#.#.#.#.#...#...#...#.#.#  
  #####.###.#####.#.#.#.#.###.###.#.###.###  
  #.......#.....#.#...#...............#...#  
  #############.#.#.###.###################  
               A O F   N                     
               A A D   M                     ";

    [Test]
    public void Example3RecursiveShortestGlobalPath()
    {
      var maze = new RecursiveMaze(example3, 11);
      Check.That(maze.ShortestGlobalPath()).IsEqualTo(396);
    }

    [Test]
    public void Part2()
    {
      var maze = new RecursiveMaze(File.ReadAllText("D20.txt"),26);
      Check.That(maze.ShortestGlobalPath()).IsEqualTo(6834);
    }

  }
}