namespace tests21
{
  using NUnit.Framework;
  using NFluent;
  using System;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src21;

  public class Tests
  {
    [TestCase("NOT B J\nWALK\n", 0)]
    [TestCase("OR A J\nAND B J\nNOT J J\nWALK\n", 0)]
    [TestCase("NOT C J\nNOT B T\nOR T J\nNOT A T\nOR T J\nAND D J\nWALK\n", 19354437)]
    [TestCase("NOT A J\nNOT C T\nAND D T\nOR T J\nWALK\n", 19354437)]
    [TestCase("OR A J\nAND B J\nAND C J\nNOT J J\nAND D J\nWALK\n", 19354437)]
    [TestCase("NOT T J\nAND A J\nAND B J\nAND C J\nNOT J J\nAND D J\nWALK\n", 19354437)]
    [TestCase("NOT A J\nNOT C T\nAND D T\nOR T J\nRUN\n", 0)]
    [TestCase("NOT C J\nAND D J\nAND H J\nNOT B T\nAND D T\nOR T J\nNOT A T\nOR T J\nRUN\n", 1145373084)]
    public void BlindTest(string springscript, long expectedResult)
    {
      var droid = new SpringDroid(MyProgram,springscript);
      var result = (long)droid.Result;
      Check.That(result).IsEqualTo(expectedResult);
      if(result == 0)
      {
        using var console = LocalTestConsole;
        console.WriteLine(droid.Output);
      }
    }

    [Test]
    public void Part1()
    {
      Check.That(new SpringDroid(MyProgram,"NOT A J\nNOT C T\nAND D T\nOR T J\nWALK\n").Result)
        .IsEqualTo(new BigInteger(19354437));
    }

    [Test]
    public void Part2()
    {
      Check.That(new SpringDroid(MyProgram,"NOT C J\nAND D J\nAND H J\nNOT B T\nAND D T\nOR T J\nNOT A T\nOR T J\nRUN\n").Result)
        .IsEqualTo(new BigInteger(1145373084));
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D21.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }

    public TextWriter LocalTestConsole => new StreamWriter(System.Console.OpenStandardOutput());
  }
}