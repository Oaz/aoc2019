namespace Tests

open System.IO
open NUnit.Framework
open NFluent
open Intcode
open Src25

module goto =
  [<Literal>]
  let Ornament = "NE"
  [<Literal>]
  let DarkMatter = "NENN"
  [<Literal>]
  let Astrolabe = "NWN"
  [<Literal>]
  let Hologram = "NWNE"
  [<Literal>]
  let KleinBottle = "NWNEE"
  [<Literal>]
  let CandyCane = "NWW"
  [<Literal>]
  let Tambourine = "NWWWW"
  [<Literal>]
  let WhirledPeas = "SE"
  [<Literal>]
  let Security = "NWNESWN"

[<TestFixture>]
type Tests25() =

  [<TestCase("", "Hull Breach", [|"NS"|])>]
  [<TestCase("N", "Holodeck", [|"ESW";"giant electromagnet"|])>]
  [<TestCase("NE", "Warp Drive Maintenance", [|"NW";"ornament"|])>]
  [<TestCase("NEN", "Kitchen", [|"NES";"escape pod"|])>]
  [<TestCase("NENN", "Sick Bay", [|"S";"dark matter"|])>]
  [<TestCase("NENE", "Hallway", [|"W"|])>]
  [<TestCase("NW", "Science Lab", [|"NEW"|])>]
  [<TestCase("NWN", "Crew Quarters", [|"ESW";"astrolabe"|])>]
  [<TestCase("NWNE", "Engineering", [|"ESW";"hologram"|])>]
  [<TestCase("NWNEE", "Corridor", [|"W";"klein bottle"|])>]
  [<TestCase("NWNES", "Arcade", [|"NW";"molten lava"|])>]
  [<TestCase("NWNESW", "Security Checkpoint", [|"NE"|])>]
  [<TestCase("NWNESWN", "Pressure-Sensitive Floor", [|"S"|])>]
  [<TestCase("NWNW", "Stables", [|"E"|])>]
  [<TestCase("NWW", "Gift Wrapping Center", [|"EW";"candy cane"|])>]
  [<TestCase("NWWW", "Passages", [|"ESW";"photons"|])>]
  [<TestCase("NWWWS", "Hot Chocolate Fountain", [|"N"|])>]
  [<TestCase("NWWWW", "Storage", [|"E";"tambourine"|])>]
  [<TestCase("S", "Observatory", [|"NE";"infinite loop"|])>]
  [<TestCase("SE", "Navigation", [|"W";"whirled peas"|])>]
  member this.Places(actions, title, others) =
    let status = Explore this.MyProgram actions
    Check.That(status.title).IsEqualTo(title) |> ignore
    Check.That(status.directions).ContainsExactly(others |> Array.head |> List.ofSeq) |> ignore
    Check.That(status.items).ContainsExactly(others |> Array.tail) |> ignore

  [<TestCase(goto.Ornament, "ornament")>]
  [<TestCase(goto.DarkMatter, "dark matter")>]
  [<TestCase(goto.Astrolabe, "astrolabe")>]
  [<TestCase(goto.Hologram, "hologram")>]
  [<TestCase(goto.KleinBottle, "klein bottle")>]
  [<TestCase(goto.CandyCane, "candy cane")>]
  [<TestCase(goto.Tambourine, "tambourine")>]
  [<TestCase(goto.WhirledPeas, "whirled peas")>]
  member this.TakeItems(actions, itemInInventory) =
    let status = Explore this.MyProgram (actions+"TI")
    Check.That(status.inventory).ContainsExactly([itemInInventory]) |> ignore

  [<Test>]
  member this.Part1() =
    let fulltake = TakeAll [goto.Hologram;goto.Astrolabe;goto.KleinBottle;goto.Tambourine]
    let status = Explore this.MyProgram (fulltake+goto.Security)
    Check.That(status.title).IsEqualTo("Pressure-Sensitive Floor") |> ignore
    let expected =
      "A loud, robotic voice says \"Analysis complete! You may proceed.\" and you enter the cockpit.\n"
      + "Santa notices your small droid, looks puzzled for a moment, realizes what has happened, and radios your ship directly.\n"
      + "\"Oh, hello! You should be able to get in by typing 134349952 on the keypad at the main airlock.\""
    Check.That(status.description).IsEqualTo(expected) |> ignore

  member this.MyProgram = File.ReadAllText "D25.txt" |> LoadLongProgram
    