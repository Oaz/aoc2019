namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Src01

[<TestFixture>]
type Tests01() =

    [<TestCase(12, 2)>]
    [<TestCase(14, 2)>]
    [<TestCase(1969, 654)>]
    [<TestCase(100756, 33583)>]
    member this.GetFuelForMass(givenMass,expectedFuel) =
        Check.That(FuelForMass givenMass).IsEqualTo(expectedFuel) |> ignore
        
    [<Test>]
    member this.GetSumOfFuelForMasses() =
        Check.That(["12"; "14"; "1969"] |> SumOf FuelForMass).IsEqualTo(658) |> ignore    

    [<Test>]
    member this.Part1() =
        Check.That(this.MyInput |> SumOf FuelForMass).IsEqualTo(3401852) |> ignore

    [<TestCase(12, 2)>]
    [<TestCase(14, 2)>]
    [<TestCase(1969, 966)>]
    [<TestCase(100756, 50346)>]
    member this.GetTotalFuelForMass(givenMass,expectedFuel) =
        Check.That(TotalFuelForMass givenMass).IsEqualTo(expectedFuel) |> ignore
        
    [<Test>]
    member this.GetSumOfTotalFuelForMasses() =
        Check.That(["12"; "14"; "1969"] |> SumOf TotalFuelForMass).IsEqualTo(970) |> ignore    

    [<Test>]
    member this.Part2() =
        Check.That(this.MyInput |> SumOf TotalFuelForMass).IsEqualTo(5099916) |> ignore

    member this.MyInput = File.ReadAllLines "D01.txt"
