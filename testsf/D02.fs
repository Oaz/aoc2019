namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Intcode
open Src02

[<TestFixture>]
type Tests02() =

    [<Test>]
    member this.Part1() =
        Check.That(RunWithInputs this.MyProgram 12 2).IsEqualTo(3931283) |> ignore

    [<Test>]
    member this.Part2() =
        Check.That(this.MyProgram |> FindInputsFor 19690720).IsEqualTo(6979) |> ignore

    member this.MyProgram = File.ReadAllText "D02.txt" |> LoadIntProgram
    