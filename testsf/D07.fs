namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Src07

[<TestFixture>]
type Tests07() =

  [<TestCase(43210, [|4;3;2;1;0|])>]
  [<TestCase(01234, [|0;1;2;3;4|])>]
  member this.TestPhaseSettings(input, expectedOutput) =
    Check.That(PhaseSettings input 5).ContainsExactly(expectedOutput) |> ignore

  [<Test>]
  member this.TestPermutations() =
    Check.That(AllPermutations [1;2] |> Seq.ofList).ContainsExactly([[1;2];[2;1]]) |> ignore
    Check.That(AllPermutations [3;2;1] |> Seq.ofList)
      .ContainsExactly([[3;2;1];[3;1;2];[2;3;1];[2;1;3];[1;3;2];[1;2;3]]) |> ignore

  [<TestCase("3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0", 43210, 43210)>]
  [<TestCase("3,23,3,24,1002,24,10,24,1002,23,-1,23,101,5,23,23,1,24,23,23,4,23,99,0,0", 1234, 54321)>]
  [<TestCase("3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,
              1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0", 10432, 65210)>]
  member this.CheckSimpleAmplifierSeries(program,phaseSettings,expectedOutputSignal) =
    let settings = PhaseSettings phaseSettings 5
    let amplifiers = MakeAmplifierSeries program 5
    Check.That(InitializeWithSettings settings amplifiers |> ComputeOutputSignal 0).IsEqualTo(expectedOutputSignal) |> ignore
    Check.That(FindMaxOutputSignal amplifiers 0 [0;1;2;3;4] |> fst).ContainsExactly(settings) |> ignore

  [<Test>]
  member this.Part1() =
    let amplifiers = MakeAmplifierSeries this.MyProgram 5
    Check.That(FindMaxOutputSignal amplifiers 0 [0;1;2;3;4] |> snd).IsEqualTo(17790) |> ignore

  member this.MyProgram = File.ReadAllText "D07.txt"
    