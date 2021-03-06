﻿namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Intcode

[<TestFixture>]
type IntcodeTests() =
  
  let exampleProgram = LoadIntProgram "1,9,10,3,2,3,11,0,99,30,40,50"

  [<Test>]
  member this.AddOperation() =
    let result = exampleProgram |> Step
    Check.That(result |> MemoryDump).ContainsExactly([1;9;10;70;2;3;11;0;99;30;40;50]) |> ignore
    Check.That(result |> IsHalted).IsFalse() |> ignore

  [<Test>]
  member this.MulOperation() =
    let result = exampleProgram |> Goto 4 |> Step
    Check.That(result |> MemoryDump).ContainsExactly([150;9;10;3;2;3;11;0;99;30;40;50]) |> ignore
    Check.That(result |> IsHalted).IsFalse() |> ignore

  [<Test>]
  member this.RunProgramUntilHalt() =
    let result = exampleProgram |> Run
    Check.That(result |> MemoryDump).ContainsExactly([3500;9;10;70;2;3;11;0;99;30;40;50]) |> ignore
    Check.That(result |> IsHalted).IsTrue() |> ignore

  [<TestCase("1,0,0,0,99","2,0,0,0,99")>]
  [<TestCase("2,3,0,3,99","2,3,0,6,99")>]
  [<TestCase("2,4,4,5,99,0","2,4,4,5,99,9801")>]
  [<TestCase("1,1,1,4,99,5,6,0,99","30,1,1,4,2,5,6,0,99")>]
  member this.RunSomeOtherPrograms(input,expectedOutput) =
      Check.That(LoadIntProgram input |> Run |> MemoryDump)
          .ContainsExactly(LoadIntProgram expectedOutput |> MemoryDump)
          |> ignore
  
  [<Test>]
  member this.InputOperation() =
    let computer = LoadIntProgram "3,3,99,15" |> Input [28;42]
    let result = computer |> Run
    Check.That(result |> MemoryDump).ContainsExactly(LoadIntProgram "3,3,99,28" |> MemoryDump) |> ignore
    Check.That(Seq.ofList result.input).ContainsExactly([42]) |> ignore

  [<Test>]
  member this.OutputOperation() =
    let computer = LoadIntProgram "4,3,99,15"
    let result = computer |> Run
    Check.That(result |> MemoryDump).ContainsExactly(LoadIntProgram "4,3,99,15" |> MemoryDump) |> ignore
    Check.That(computer |> Output |> Seq.ofList).ContainsExactly([]) |> ignore
    Check.That(result |> Output |> Seq.ofList).ContainsExactly([15]) |> ignore

  [<Test>]
  member this.ImmediateMode() =
    let computer = LoadIntProgram "1002,4,3,4,33"
    Check.That(computer |> Run |> MemoryDump).ContainsExactly(LoadIntProgram "1002,4,3,4,99" |> MemoryDump) |> ignore

  [<TestCase("3,9,8,9,10,9,4,9,99,-1,8",7,0)>]
  [<TestCase("3,9,8,9,10,9,4,9,99,-1,8",8,1)>]
  [<TestCase("3,9,8,9,10,9,4,9,99,-1,8",9,0)>]
  [<TestCase("3,9,7,9,10,9,4,9,99,-1,8",7,1)>]
  [<TestCase("3,9,7,9,10,9,4,9,99,-1,8",8,0)>]
  [<TestCase("3,9,7,9,10,9,4,9,99,-1,8",9,0)>]
  [<TestCase("3,3,1108,-1,8,3,4,3,99",7,0)>]
  [<TestCase("3,3,1108,-1,8,3,4,3,99",8,1)>]
  [<TestCase("3,3,1108,-1,8,3,4,3,99",9,0)>]
  [<TestCase("3,3,1107,-1,8,3,4,3,99",7,1)>]
  [<TestCase("3,3,1107,-1,8,3,4,3,99",8,0)>]
  [<TestCase("3,3,1107,-1,8,3,4,3,99",9,0)>]
  [<TestCase("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9",0,0)>]
  [<TestCase("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9",283,1)>]
  [<TestCase("3,3,1105,-1,9,1101,0,0,12,4,12,99,1",0,0)>]
  [<TestCase("3,3,1105,-1,9,1101,0,0,12,4,12,99,1",283,1)>]
  member this.ComparisonsAndJumps(program,input,expectedOutput) =
    let computer = LoadIntProgram program |> Input [input]
    let r = computer |> Steps |> List.ofSeq
    Check.That(computer |> Run |> Output).IsEqualTo([expectedOutput]) |> ignore

  [<TestCase(-1, 999)>]
  [<TestCase(3, 999)>]
  [<TestCase(7, 999)>]
  [<TestCase(8, 1000)>]
  [<TestCase(9, 1001)>]
  [<TestCase(1234, 1001)>]
  member this.LargerExample(input, expectedOutput) =
    let computer = LoadIntProgram "3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,
      1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,
      999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99" |> Input [input]
    Check.That(computer |> Run |> Output).IsEqualTo([expectedOutput]) |> ignore

  [<Test>]
  member this.CopyOfItself() =
    let computer =
      LoadIntProgram "109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99"
    Check.That(computer |> Run |> Output |> Seq.ofList)
      .ContainsExactly(computer |> MemoryDump) |> ignore

  [<Test>]
  member this.OutputLongNumber() =
    let computer = LoadLongProgram "1102,34915192,34915192,7,4,7,99,0"
    Check.That(computer |> Run |> Output |> List.head)
      .IsEqualTo(1219070632396864L) |> ignore

  [<Test>]
  member this.OutputInsideLongNumber() =
    let computer = LoadLongProgram "104,1125899906842624,99"
    Check.That(computer |> Run |> Output |> List.head)
      .IsEqualTo(1125899906842624L) |> ignore

  [<Test>]
  member this.OutputInsideBigNumber() =
    let computer = LoadBigProgram "104,1125899906842624597845631235698413,99"
    Check.That(computer |> Run |> Output |> List.head)
      .IsEqualTo(bigint.Parse("1125899906842624597845631235698413")) |> ignore

  [<Test>]
  member this.Hello() =
    let computer = LoadLongProgram "109,271,1101,0,0,268,1101,1,0,267,1006,267,100,3,267,1001,267,0,269,21001,269,0,0,109,1,1001,268,0,267,109,-1,1201,0,0,270,101,227,267,267,1001,267,0,46,1001,270,0,0,21001,269,0,0,109,1,1101,10,0,267,109,-1,1201,0,0,270,8,270,267,267,1006,267,73,1106,0,100,21001,268,0,0,109,1,1101,1,0,267,109,-1,1201,0,0,270,1,270,267,267,1001,267,0,268,1106,0,6,1101,72,0,267,4,267,1101,69,0,267,4,267,1101,76,0,267,4,267,1101,76,0,267,4,267,1101,79,0,267,4,267,1101,32,0,267,4,267,1101,0,0,268,1101,1,0,267,1006,267,226,1001,268,0,267,101,227,267,267,1001,267,0,160,1001,0,0,267,1001,267,0,269,1001,269,0,267,4,267,21001,269,0,0,109,1,1101,10,0,267,109,-1,1201,0,0,270,8,270,267,267,1006,267,199,1106,0,226,21001,268,0,0,109,1,1101,1,0,267,109,-1,1201,0,0,270,1,270,267,267,1001,267,0,268,1106,0,140,99"
    Check.That(RunASCII ("FOOBAR\n",computer) |> fst).IsEqualTo("HELLO FOOBAR\n") |> ignore
  
  [<Test>]
  member this.TestFileDump() =
    let computer = LoadIntProgram "3,3,99,15" |> Input [28]
    let result = computer |> FileDump "/tmp/TESTDUMP_" |> Run
    Check.That(File.ReadAllText "/tmp/TESTDUMP_0000000000").IsEqualTo("PC=0\nRB=0\n3\n3\n99\n15\n") |> ignore
    Check.That(File.ReadAllText "/tmp/TESTDUMP_0000000001").IsEqualTo("PC=2\nRB=0\n3\n3\n99\n28\n") |> ignore
  
  [<Test>]
  member this.TestFileDumpWithHoles() =
    let computer = LoadIntProgram "3,5,99,15" |> Input [28]
    let result = computer |> FileDump "/tmp/TESTDUMP_" |> Run
    Check.That(File.ReadAllText "/tmp/TESTDUMP_0000000000").IsEqualTo("PC=0\nRB=0\n3\n5\n99\n15\n") |> ignore
    Check.That(File.ReadAllText "/tmp/TESTDUMP_0000000001").IsEqualTo("PC=2\nRB=0\n3\n5\n99\n15\n0\n28\n") |> ignore

  [<Test>]
  member this.TestLoopUntil() =
    Check.That(loopUntil 5 (fun x -> x+3) (fun x -> x>12))
      .ContainsExactly([5;8;11;14]) |> ignore
    Check.That(loopUntil 5 (fun x -> if x=3 then 0 else 15/x) (fun x -> x=0))
      .ContainsExactly([5;3;0]) |> ignore
