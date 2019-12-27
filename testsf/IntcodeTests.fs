namespace Tests

open NUnit.Framework
open NFluent
open Intcode

[<TestFixture>]
type IntcodeTests() =
    
    let exampleProgram = LoadIntProgram "1,9,10,3,2,3,11,0,99,30,40,50"

    [<Test>]
    member this.AddOperation() =
        Check.That(exampleProgram |> RunOne |> MemoryDump)
            .ContainsExactly([1;9;10;70;2;3;11;0;99;30;40;50])
            |> ignore

    [<Test>]
    member this.MulOperation() =
        Check.That(exampleProgram |> Goto 4 |> RunOne |> MemoryDump)
            .ContainsExactly([150;9;10;3;2;3;11;0;99;30;40;50])
            |> ignore

    [<Test>]
    member this.RunProgramUntilHalt() =
        Check.That(exampleProgram |> RunToHalt |> MemoryDump)
            .ContainsExactly([3500;9;10;70;2;3;11;0;99;30;40;50])
            |> ignore

    [<TestCase("1,0,0,0,99","2,0,0,0,99")>]
    [<TestCase("2,3,0,3,99","2,3,0,6,99")>]
    [<TestCase("2,4,4,5,99,0","2,4,4,5,99,9801")>]
    [<TestCase("1,1,1,4,99,5,6,0,99","30,1,1,4,2,5,6,0,99")>]
    member this.RunSomeOtherPrograms(input,expectedOutput) =
        Check.That(LoadIntProgram input |> RunToHalt |> MemoryDump)
            .ContainsExactly(LoadIntProgram expectedOutput |> MemoryDump)
            |> ignore
    
    [<Test>]
    member this.InputOperation() =
      let computer = LoadIntProgram "3,3,99,15" |> Input [28;42]
      let result = computer |> RunToHalt
      Check.That(result |> MemoryDump).ContainsExactly(LoadIntProgram "3,3,99,28" |> MemoryDump) |> ignore
      Check.That(Seq.ofList result.input).ContainsExactly([42]) |> ignore

    [<Test>]
    member this.OutputOperation() =
      let computer = LoadIntProgram "4,3,99,15"
      let result = computer |> RunToHalt
      Check.That(result |> MemoryDump).ContainsExactly(LoadIntProgram "4,3,99,15" |> MemoryDump) |> ignore
      Check.That(computer |> Output).ContainsExactly([]) |> ignore
      Check.That(result |> Output).ContainsExactly([15]) |> ignore

    [<Test>]
    member this.ImmediateMode() =
      let computer = LoadIntProgram "1002,4,3,4,33"
      Check.That(computer |> RunToHalt |> MemoryDump).ContainsExactly(LoadIntProgram "1002,4,3,4,99" |> MemoryDump) |> ignore

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
      let r = computer |> Run |> List.ofSeq
      Check.That(computer |> RunToHalt |> Output).IsEqualTo([expectedOutput]) |> ignore

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
      Check.That(computer |> RunToHalt |> Output).IsEqualTo([expectedOutput]) |> ignore
