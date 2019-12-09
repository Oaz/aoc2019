namespace tests09
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using System.Linq;
  using src09;
  using System.Numerics;


  public class Tests
  {
    [Test]
    public void CopyOfItself()
    {
        var data = new int[] { 109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99 };
        var program = new IntcodeComputer(data);
        program.Run();
        Check.That(program.Output).ContainsExactly(data.AsBig());
    }

    [Test]
    public void OutputBigNumber()
    {
        var program = new IntcodeComputer(new int[] { 1102,34915192,34915192,7,4,7,99,0 });
        program.Run();
        Check.That(program.Output.First()).IsEqualTo(BigInteger.Parse("1219070632396864"));
    }

    [Test]
    public void OutputInsideNumber()
    {
        var largeNumber = BigInteger.Parse("1219070632396864");
        var program = new IntcodeComputer(new BigInteger[] { 104.AsBig(),largeNumber,99.AsBig() });
        program.Run();
        Check.That(program.Output.First()).IsEqualTo(largeNumber);
    }

    [Test]
    public void Part1()
    {
      var computer = new IntcodeComputer(MyProgram);
      computer.Input.Enqueue(1);
      computer.Run();
      Check.That(computer.Output.First()).IsEqualTo(BigInteger.Parse("2745604242"));
    }

    [Test]
    public void Part2()
    {
      var computer = new IntcodeComputer(MyProgram);
      computer.Input.Enqueue(2);
      computer.Run();
      Check.That(computer.Output.First()).IsEqualTo(BigInteger.Parse("51135"));
    }

    public BigInteger[] MyProgram
    {
      get => File.ReadAllText("D09.txt").Split(',').Select(n => BigInteger.Parse(n)).ToArray();
    }
  }
}