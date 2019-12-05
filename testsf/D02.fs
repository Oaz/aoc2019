namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Src02

[<TestFixture>]
type Tests02() =
        
    [<Test>]
    member this.AddOperation() =
        Check.That([1;9;10;3;2;3;11;0;99;30;40;50] |> RunAt 0)
            .ContainsExactly([1;9;10;70;2;3;11;0;99;30;40;50])
            |> ignore

    [<Test>]
    member this.MulOperation() =
        Check.That([1;9;10;70;2;3;11;0;99;30;40;50] |> RunAt 4)
            .ContainsExactly([3500;9;10;70;2;3;11;0;99;30;40;50])
            |> ignore

    [<Test>]
    member this.RunProgramUntilHalt() =
        Check.That([1;9;10;3;2;3;11;0;99;30;40;50] |> Run)
            .ContainsExactly([3500;9;10;70;2;3;11;0;99;30;40;50])
            |> ignore


    [<TestCaseSource("SomeOtherPrograms")>]
    member this.RunSomeOtherPrograms(input, expectedOutput) =
        Check.That(Run input).ContainsExactly(expectedOutput) |> ignore

    static member SomeOtherPrograms = 
        [|
          [|[|1;0;0;0;99|]         ; [|2;0;0;0;99|]|];
          [|[|2;3;0;3;99|]         ; [|2;3;0;6;99|]|];
          [|[|2;4;4;5;99;0|]       ; [|2;4;4;5;99;9801|]|];
          [|[|1;1;1;4;99;5;6;0;99|]; [|30;1;1;4;2;5;6;0;99|]|];
        |]

    [<Test>]
    member this.Part1() =
        Check.That(RunWithInputs this.MyInput 12 2 |> Seq.head).IsEqualTo(3931283) |> ignore

    [<Test>]
    member this.RunWithInputsDoesNotChangeTheInitialProgram() =
        let program = [1;0;0;0;99;7;8;9]
        Check.That(RunWithInputs program 5 6).ContainsExactly([15;5;6;0;99;7;8;9]) |> ignore
        Check.That(RunWithInputs program 6 7).ContainsExactly([17;6;7;0;99;7;8;9]) |> ignore
        Check.That(program).IsEqualTo([1;0;0;0;99;7;8;9]) |> ignore

    [<TestCase(15,506)>]
    [<TestCase(17,607)>]
    member this.FindInputs(outputToFind, expectedInput) =
        Check.That([1;0;0;0;99;7;8;9] |> FindInputsFor outputToFind).IsEqualTo(expectedInput) |> ignore

    [<Test>]
    member this.Part2() =
        Check.That(this.MyInput |> FindInputsFor 19690720).IsEqualTo(6979) |> ignore

    member this.MyInput = (File.ReadAllText "D02.txt").Split [|','|] |> Seq.map int
    