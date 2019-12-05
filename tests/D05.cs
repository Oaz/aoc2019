namespace tests05
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using System.Linq;
  using src05;

  public class Tests
  {
    [Test]
    public void AddOperation()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50 }).RunOne().Program
      ).ContainsExactly(new int[] { 1, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 });
    }

    [Test]
    public void MulOperation()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 }).MoveTo(4).RunOne().Program
      ).ContainsExactly(new int[] { 3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 });
    }

    [Test]
    public void RunProgramUntilHalt()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50 }).Run().Program
      ).ContainsExactly(new int[] { 3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 });
    }

    [Test]
    public void InputOperation()
    {
      var computer = new IntcodeComputer(new int[] { 3, 3, 99, 15 });
      computer.Input.Enqueue(28);
      computer.Run();
      Check.That(computer.Program).ContainsExactly(new int[] { 3, 3, 99, 28 });
    }

    [Test]
    public void OutputOperation()
    {
      var computer = new IntcodeComputer(new int[] { 4, 3, 99, 15 });
      computer.Run();
      Check.That(computer.Program).ContainsExactly(new int[] { 4, 3, 99, 15 });
      Check.That(computer.Output).ContainsExactly(new int[] { 15 });
    }


    [TestCase(new int[] { 2, 3, 0, 3, 99 }, new int[] { 2, 3, 0, 6, 99 })]
    [TestCase(new int[] { 2, 4, 4, 5, 99, 0 }, new int[] { 2, 4, 4, 5, 99, 9801 })]
    [TestCase(new int[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 }, new int[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 })]
    public void RunSomeOtherPrograms(int[] input, int[] expectedOutput)
    {
      Check.That(new IntcodeComputer(input).Run().Program).ContainsExactly(expectedOutput);
    }

    [Test]
    public void ImmediateMode()
    {
      Check.That(
          new IntcodeComputer(new int[] { 1002, 4, 3, 4, 33 }).Run().Program
      ).ContainsExactly(new int[] { 1002, 4, 3, 4, 99 });
    }

    [Test]
    public void Part1()
    {
      var computer = new IntcodeComputer(MyProgram);
      computer.Input.Enqueue(1);
      computer.Run();
      Check.That(computer.Output.Last()).IsEqualTo(13547311);
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
      Check.That(computer.Output.Dequeue()).IsEqualTo(expectedOutput);
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
      Check.That(computer.Output.Dequeue()).IsEqualTo(expectedOutput);
    }

    [Test]
    public void Part2()
    {
      var computer = new IntcodeComputer(MyProgram);
      computer.Input.Enqueue(5);
      computer.Run();
      Check.That(computer.Output.First()).IsEqualTo(236453);
    }

    public int[] MyProgram
    {
      get => File.ReadAllText("D05.txt").Split(',').Select(n => int.Parse(n)).ToArray();
    }
  }
}