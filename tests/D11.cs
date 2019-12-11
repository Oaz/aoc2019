namespace tests11
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src11;

  public class Tests
  {
    [Test]
    public void Part1()
    {
      var robot = new Robot(MyProgram);
      Check.That(robot.NumberOfPaintedPanelOnce()).IsEqualTo(2339);
    }

    [Test]
    public void Part2()
    {
      var robot = new Robot(MyProgram);
      robot.PaintIdentifier("/tmp/identifier.png");
      var actualIdentifier = File.ReadAllBytes("/tmp/identifier.png");
      var expectedIdentifier = File.ReadAllBytes("D11_identifier.png");
      Check.That(actualIdentifier).ContainsExactly(expectedIdentifier);
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D11.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }
  }
}