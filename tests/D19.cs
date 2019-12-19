namespace tests19
{
  using NUnit.Framework;
  using NFluent;
  using System;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src19;

  public class Tests
  {
    [Test]
    public void Part1()
    {
      Check.That(Scanner.GetArea(MyProgram,Coords.At(50,50)).Count(x => x.Item2)).IsEqualTo(179);
    }

    [Test]
    public void Part2()
    {
      Check.That(Scanner.GetTopLeft(MyProgram,100)).IsEqualTo(Coords.At(976,485));
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D19.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }

    public TextWriter LocalTestConsole => new StreamWriter(System.Console.OpenStandardOutput());
  }
}