namespace tests15
{
  using NUnit.Framework;
  using NFluent;
  using System;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src15;

  public class Tests
  {
    [Test]
    public void Part1()
    {
      var pathExplorer = new PathExplorer(MyProgram);
      Check.That(Oxygen.FindShortestPath(pathExplorer)).IsEqualTo(248);
    }

    [Test]
    public void Part2()
    {
      using var console = LocalTestConsole;
      var pathExplorer = new PathExplorer(MyProgram);
      var wholeMap = Oxygen.WholeMap(pathExplorer);
      console.WriteLine(Oxygen.Show(wholeMap));
      Check.That(Oxygen.SpreadAndCountMinutes(wholeMap)).IsEqualTo(382);
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D15.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }

    public TextWriter LocalTestConsole => new StreamWriter(System.Console.OpenStandardOutput());
  }
}