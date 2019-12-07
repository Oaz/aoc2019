namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Src06

[<TestFixture>]
type Tests06() =

    let example = [
      "COM)B";"B)C";"C)D";"D)E";"E)F";"B)G";
      "G)H";"D)I";"E)J";"J)K";"K)L"
    ]

    [<TestCase("D", [|"C";"B";"COM"|])>]
    [<TestCase("L", [|"K";"J";"E";"D";"C";"B";"COM"|])>]
    member this.ChainStartingFrom(startingObject,expectedChain) =
      Check.That(OrbitMap(example).ChainStartingFrom startingObject)
           .ContainsExactly(expectedChain) |> ignore
        
    [<Test>]
    member this.CountAllOrbits() =
      Check.That(OrbitMap(example).CountAllOrbits).IsEqualTo(42) |> ignore    

    [<Test>]
    member this.Part1() =
      Check.That(OrbitMap(this.MyInput).CountAllOrbits).IsEqualTo(402879) |> ignore

    [<Test>]
    member this.FromMeToSan() =
      let orbitMap = OrbitMap(example @ ["K)YOU";"I)SAN"])
      Check.That(orbitMap.ClosestSharedObject "YOU" "SAN").IsEqualTo("D") |> ignore
      Check.That(orbitMap.CountHopsFromTo "YOU" "SAN").IsEqualTo(4) |> ignore

    [<Test>]
    member this.Part2() =
      Check.That(OrbitMap(this.MyInput).CountHopsFromTo "YOU" "SAN").IsEqualTo(484) |> ignore

    member this.MyInput = File.ReadAllLines "D06.txt"
