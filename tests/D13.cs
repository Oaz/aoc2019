namespace tests13
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src13;

  public class Tests
  {
    [Test]
    public void Part1()
    {
      var arcade = new Arcade(MyProgram);
      Check.That(arcade.NumberOfBlockTiles()).IsEqualTo(301);
    }

    [Test]
    public void Part2()
    {
      var arcade = new Arcade(MyProgram);
      Check.That(arcade.ScoreAfterBreakingAllBlocks()).IsEqualTo(14096);
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D13.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }
  }
}