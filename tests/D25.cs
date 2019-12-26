namespace tests25
{
  using NUnit.Framework;
  using NFluent;
  using System;
  using System.IO;
  using System.Numerics;
  using System.Linq;
  using src25;
  using System.Collections.Generic;

    public class Tests
  {
    [TestCase("", "Hull Breach", "NS", new string[] { })]
    [TestCase("N", "Holodeck", "ESW", new string[] { "giant electromagnet" })]
    [TestCase("NE", "Warp Drive Maintenance", "NW", new string[] { "ornament" })]
    [TestCase("NEN", "Kitchen", "NES", new string[] { "escape pod" })]
    [TestCase("NENN", "Sick Bay", "S", new string[] { "dark matter" })]
    [TestCase("NENE", "Hallway", "W", new string[] { })]
    [TestCase("NW", "Science Lab", "NEW", new string[] { })]
    [TestCase("NWN", "Crew Quarters", "ESW", new string[] { "astrolabe" })]
    [TestCase("NWNE", "Engineering", "ESW", new string[] { "hologram" })]
    [TestCase("NWNEE", "Corridor", "W", new string[] { "klein bottle" })]
    [TestCase("NWNES", "Arcade", "NW", new string[] { "molten lava" })]
    [TestCase("NWNESW", "Security Checkpoint", "NE", new string[] { })]
    [TestCase("NWNESWN", "Pressure-Sensitive Floor", "S", new string[] { })]
    [TestCase("NWNW", "Stables", "E", new string[] { })]
    [TestCase("NWW", "Gift Wrapping Center", "EW", new string[] { "candy cane" })]
    [TestCase("NWWW", "Passages", "ESW", new string[] { "photons" })]
    [TestCase("NWWWS", "Hot Chocolate Fountain", "N", new string[] { })]
    [TestCase("NWWWW", "Storage", "E", new string[] { "tambourine" })]
    [TestCase("S", "Observatory", "NE", new string[] { "infinite loop" })]
    [TestCase("SE", "Navigation", "W", new string[] { "whirled peas" })]
    public void Places(string actions, string title, string directions, string[] items)
    {
      var status = Explorer.Try(MyProgram, actions);
      Check.That(status.Title).IsEqualTo(title);
      Check.That(status.Directions.Select(d => char.ToUpper(d[0]))).ContainsExactly(directions);
      Check.That(status.Items).ContainsExactly(items);
    }

    const string gotoOrnament = "NE";
    const string gotoDarkMatter = "NENN";
    const string gotoAstrolabe = "NWN";
    const string gotoHologram = "NWNE";
    const string gotoKleinBottle = "NWNEE";
    const string gotoCandyCane = "NWW";
    const string gotoTambourine = "NWWWW";
    const string gotoWhirledPeas = "SE";
    const string gotoSecurity = "NWNESWN";

    [TestCase(gotoOrnament, "ornament")]
    [TestCase(gotoDarkMatter, "dark matter")]
    [TestCase(gotoAstrolabe, "astrolabe")]
    [TestCase(gotoHologram, "hologram")]
    [TestCase(gotoKleinBottle, "klein bottle")]
    [TestCase(gotoCandyCane, "candy cane")]
    [TestCase(gotoTambourine, "tambourine")]
    [TestCase(gotoWhirledPeas, "whirled peas")]
    public void TakeItems(string actions, string inventory)
    {
      var status = Explorer.Try(MyProgram, actions+"TI");
      Check.That(status.Inventory).Contains(inventory);
    }

    [TestCase(new string[] { gotoOrnament }, "lighter")]
    [TestCase(new string[] { gotoDarkMatter, gotoAstrolabe, gotoHologram }, "lighter")]
    [TestCase(new string[] { gotoDarkMatter, gotoHologram }, "lighter")]
    [TestCase(new string[] { gotoHologram, gotoAstrolabe, gotoKleinBottle, gotoCandyCane }, "lighter")]
    [TestCase(new string[] { gotoHologram, gotoAstrolabe, gotoKleinBottle }, "heavier")]
    [TestCase(new string[] { gotoHologram, gotoAstrolabe }, "heavier")]
    [TestCase(new string[] { gotoDarkMatter, gotoAstrolabe, gotoKleinBottle, gotoCandyCane, gotoTambourine, gotoWhirledPeas }, "heavier")]
    [TestCase(new string[] { gotoDarkMatter, gotoAstrolabe }, "heavier")]
    [TestCase(new string[] { gotoDarkMatter }, "heavier")]
    [TestCase(new string[] { gotoAstrolabe }, "heavier")]
    [TestCase(new string[] { gotoHologram }, "heavier")]
    [TestCase(new string[] { gotoKleinBottle }, "heavier")]
    [TestCase(new string[] { gotoCandyCane }, "heavier")]
    [TestCase(new string[] { gotoTambourine }, "heavier")]
    [TestCase(new string[] { gotoWhirledPeas }, "heavier")]
    public void TakeMultipleItemsAndGotoSecurityCheckpoint(string[] items, string result)
    {
      var status = Explorer.Try(MyProgram, TakeAll(items) + gotoSecurity);
      var expected = "A loud, robotic voice says \"Alert! Droids on this ship are "+ result
                    + " than the detected value!\" and you are ejected back to the checkpoint.";
      Check.That(status.Description).IsEqualTo(expected);
    }

    static string TakeAll(string[] ss) => string.Join("", ss.Select(a => a + "T" + R(a)));
    static string R(string s) => new string(s.Replace('N', 's').Replace('S', 'N').Replace('s', 'S')
                                    .Replace('E', 'w').Replace('W', 'E').Replace('w', 'W').Reverse().ToArray());

    [TestCase("N", "The giant electromagnet is stuck to you.  You can't move!!")]
    [TestCase("NEN", "You take the escape pod.You're launched into space! Bye!")]
    [TestCase("NWNES", "You take the molten lava.The molten lava is way too hot! You melt!")]
    [TestCase("NWWW", "You take the photons.It is suddenly completely dark! You are eaten by a Grue!")]
    [TestCase("S", "he infinite loop.You take the infinite loop.You take the infinite loop.You take the infinite lo")]
    public void WeirdItems(string actions, string result)
    {
      var status = Explorer.Try(MyProgram, actions + "TI");
      Check.That(status.Description).IsEqualTo(result);
    }

    [Test]
    public void Part1()
    {
      var status = Explorer.Try(MyProgram,
        TakeAll(new string[] { gotoHologram, gotoAstrolabe, gotoKleinBottle, gotoTambourine }) + gotoSecurity);
      Check.That(status.Title).IsEqualTo("Pressure-Sensitive Floor");
      var expected = "A loud, robotic voice says \"Analysis complete! You may proceed.\" and you enter the cockpit.\n"
      + "Santa notices your small droid, looks puzzled for a moment, realizes what has happened, and radios your ship directly.\n"
      + "\"Oh, hello! You should be able to get in by typing 0 on the keypad at the main airlock.\"";
      Check.That(status.Description).IsEqualTo(expected);
    }

    public IEnumerable<BigInteger> MyProgram =>
      IntcodeProgram.Load(File.ReadAllText("D25.txt"));

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