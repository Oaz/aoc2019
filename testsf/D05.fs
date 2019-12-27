namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Intcode

[<TestFixture>]
type Tests05() =

  [<Test>]
  member this.Part1() =
    Check.That(this.MyProgram |> Input [1] |> Run |> Output |> Seq.last)
          .IsEqualTo(13547311) |> ignore

  [<Test>]
  member this.Part2() =
    Check.That(this.MyProgram |> Input [5] |> Run |> Output |> Seq.last)
          .IsEqualTo(236453) |> ignore

  member this.MyProgram = File.ReadAllText "D05.txt" |> LoadIntProgram
    