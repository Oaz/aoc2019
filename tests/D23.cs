namespace tests23
{
  using NUnit.Framework;
  using NFluent;
  using System;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src23;
  using System.Collections.Generic;

  public class Tests
  {
    [Test]
    public void PingPong()
    {
      var program = IntcodeProgram.Load("3,60,1005,60,18,1101,0,1,61,4,61,104,1011,104,1,1105,1,22,1101,0,0,61,3,62,1007,62,0,64,1005,64,22,3,63,1002,63,2,63,1007,63,256,65,1005,65,48,1101,0,255,61,4,61,4,62,4,63,1105,1,22,99");
      var network = new Network(2, program);
      var packet = network.Run().First();
      Check.That(network.Bus.Count).IsEqualTo(0);
      Check.That(network.NICs.Select(n => n.Inputs.Count).Sum()).IsEqualTo(0);
      Check.That(network.NICs.Select(n => n.OutputBuffer.Count).Sum()).IsEqualTo(0);
      Check.That(packet.X).IsEqualTo(new BigInteger(1011));
      Check.That(packet.Y).IsEqualTo(new BigInteger(256));
    }

    [Test]
    public void Part1()
    {
      var network = new Network(50, MyProgram);
      var packet = network.Run().First();
      Check.That(network.Bus.Count).IsEqualTo(0);
      Check.That(network.NICs.Select(n => n.Inputs.Count).Sum()).IsEqualTo(0);
      Check.That(network.NICs.Select(n => n.OutputBuffer.Count).Sum()).IsEqualTo(0);
      Check.That(packet.X).IsEqualTo(new BigInteger(16979));
      Check.That(packet.Y).IsEqualTo(new BigInteger(26163));
    }

    [Test]
    public void Part2()
    {
      using var console = LocalTestConsole;
      var network = new Network(50, MyProgram);
      BigInteger? previousY = null;
      foreach (var y in network.Run().Select(p => p.Y))
      {
        console.WriteLine(previousY+" "+y);
        if (y == previousY)
          break;
        previousY = y;
      }
      Check.That(previousY).IsEqualTo(new BigInteger(18733));
    }

    public IEnumerable<BigInteger> MyProgram
    {
      get => IntcodeProgram.Load(File.ReadAllText("D23.txt"));
    }

    public TextWriter LocalTestConsole => new StreamWriter(System.Console.OpenStandardOutput());
  }

  public class OldTests
  {
        [Test]
    public void AddOperation()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50 }).RunOne().Program.AsInt()
      ).ContainsExactly(new int[] { 1, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 });
    }

    [Test]
    public void MulOperation()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 }).MoveTo(4).RunOne().Program.AsInt()
      ).ContainsExactly(new int[] { 3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 });
    }

    [Test]
    public void RunProgramUntilHalt()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50 }).Run().Program.AsInt()
      ).ContainsExactly(new int[] { 3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 });
    }

    [Test]
    public void InputOperation()
    {
      var computer = new IntcodeComputer(new int[] { 3, 3, 99, 15 });
      computer.Input.Enqueue(28);
      computer.Run();
      Check.That(computer.Program.AsInt()).ContainsExactly(new int[] { 3, 3, 99, 28 });
    }

    [Test]
    public void OutputOperation()
    {
      var computer = new IntcodeComputer(new int[] { 4, 3, 99, 15 });
      computer.Run();
      Check.That(computer.Program.AsInt()).ContainsExactly(new int[] { 4, 3, 99, 15 });
      Check.That(computer.Output.AsInt()).ContainsExactly(new int[] { 15 });
    }


    [TestCase(new int[] { 2, 3, 0, 3, 99 }, new int[] { 2, 3, 0, 6, 99 })]
    [TestCase(new int[] { 2, 4, 4, 5, 99, 0 }, new int[] { 2, 4, 4, 5, 99, 9801 })]
    [TestCase(new int[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 }, new int[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 })]
    public void RunSomeOtherPrograms(int[] input, int[] expectedOutput)
    {
      Check.That(new IntcodeComputer(input).Run().Program.AsInt()).ContainsExactly(expectedOutput);
    }

    [Test]
    public void ImmediateMode()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1002, 4, 3, 4, 33 }).Run().Program.AsInt()
      ).ContainsExactly(new int[] { 1002, 4, 3, 4, 99 });
    }

    [TestCase(new int[] { 3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8 }, 7, 0)]
    [TestCase(new int[] { 3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8 }, 8, 1)]
    [TestCase(new int[] { 3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8 }, 9, 0)]
    [TestCase(new int[] { 3, 9, 7, 9, 10, 9, 4, 9, 99, -1, 8 }, 7, 1)]
    [TestCase(new int[] { 3, 9, 7, 9, 10, 9, 4, 9, 99, -1, 8 }, 8, 0)]
    [TestCase(new int[] { 3, 9, 7, 9, 10, 9, 4, 9, 99, -1, 8 }, 9, 0)]
    [TestCase(new int[] { 3, 3, 1108, -1, 8, 3, 4, 3, 99 }, 7, 0)]
    [TestCase(new int[] { 3, 3, 1108, -1, 8, 3, 4, 3, 99 }, 8, 1)]
    [TestCase(new int[] { 3, 3, 1108, -1, 8, 3, 4, 3, 99 }, 9, 0)]
    [TestCase(new int[] { 3, 3, 1107, -1, 8, 3, 4, 3, 99 }, 7, 1)]
    [TestCase(new int[] { 3, 3, 1107, -1, 8, 3, 4, 3, 99 }, 8, 0)]
    [TestCase(new int[] { 3, 3, 1107, -1, 8, 3, 4, 3, 99 }, 9, 0)]
    [TestCase(new int[] { 3, 12, 6, 12, 15, 1, 13, 14, 13, 4, 13, 99, -1, 0, 1, 9 }, 0, 0)]
    [TestCase(new int[] { 3, 12, 6, 12, 15, 1, 13, 14, 13, 4, 13, 99, -1, 0, 1, 9 }, 283, 1)]
    [TestCase(new int[] { 3, 3, 1105, -1, 9, 1101, 0, 0, 12, 4, 12, 99, 1 }, 0, 0)]
    [TestCase(new int[] { 3, 3, 1105, -1, 9, 1101, 0, 0, 12, 4, 12, 99, 1 }, 283, 1)]
    public void ComparisonsAndJumps(int[] program, int input, int expectedOutput)
    {
      var computer = new IntcodeComputer(program);
      computer.Input.Enqueue(input);
      computer.Run();
      Check.That(computer.Output.Dequeue().AsInt()).IsEqualTo(expectedOutput);
    }

    [TestCase(-1, 999)]
    [TestCase(3, 999)]
    [TestCase(7, 999)]
    [TestCase(8, 1000)]
    [TestCase(9, 1001)]
    [TestCase(1234, 1001)]
    public void LargerExample(int input, int expectedOutput)
    {
      var computer = new IntcodeComputer(new int[] { 3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,
                                                      1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,
                                                      999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99 });
      computer.Input.Enqueue(input);
      computer.Run();
      Check.That(computer.Output.Dequeue().AsInt()).IsEqualTo(expectedOutput);
    }

    [Test]
    public void CopyOfItself()
    {
        var data = new int[] { 109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99 };
        var program = new IntcodeComputer(data);
        program.Run();
        Check.That(program.Output.AsInt()).ContainsExactly(data);
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
  }
}