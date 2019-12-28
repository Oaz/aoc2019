namespace Tests

open System
open System.IO
open System.Linq
open NUnit.Framework
open NFluent
open Intcode

[<TestFixture>]
type Tests09() =

  [<Test>]
  member this.Part1() =
    Check.That(this.MyProgram |> Input [1L] |> Run |> Output |> Seq.head)
          .IsEqualTo(2745604242L) |> ignore

  [<Test>]
  member this.Part2() =
    Check.That(this.MyProgram |> Input [2L] |> Run |> Output |> Seq.head)
          .IsEqualTo(51135L) |> ignore

  member this.MyProgram = File.ReadAllText "D09.txt" |> LoadLongProgram

    