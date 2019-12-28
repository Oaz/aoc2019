module Src25

open Intcode
open System.Linq;
open System.Text.RegularExpressions;

type Status =
  {
    program: Computer<int64>;
    text: string;
    title: string;
    description: string;
    directions: seq<char>;
    items: seq<string>
    inventory: seq<string>
  }

let placePattern =
  Regex ( "== (.+) ==\\n(.*)\\n\\n"
        + "Doors here lead:\\n(?:- (.+)\\n)+"
        + "(?:\\nItems here:\\n(?:- (.+)\\n)+)?"
        + "\\nCommand" )

let analysisPattern = 
  Regex ( "== (.+) ==\\nAnalyzing...\\n\\n"
        + "Doors here lead:\\n(?:- (.+?)\\n)+"
        + "\\n(.+?)(?:\\n\\n\\n|$)", RegexOptions.Singleline )

let inventoryPattern =
  Regex ("Items in your inventory:\\n(?:- (.+)\\n)+\\nCommand")

let defaultPattern = Regex("(.+)\\n\\nCommand")

let (|Regex|_|) (pattern:Regex) input =
    let m = pattern.Match(input)
    if m.Success
    then Some(List.tail [ for g in m.Groups -> [for c in g.Captures -> c.Value]])
    else None

let ExecuteAndGetStatus x =
  let (output,newProgram) = RunASCII x
  let status = {
    program = newProgram;
    text = output;
    title = "";
    description = "";
    directions = Seq.empty;
    items = Seq.empty;
    inventory = Seq.empty
  }
  let charForDirection = Seq.head >> System.Char.ToUpper
  let update st title description directions =
    {
      st with title=title; description=description;
              directions=(directions |> Seq.map charForDirection)
    }    
  match output with
  | Regex analysisPattern [[title];directions;[description]] ->
      update status title description directions
  | Regex placePattern [[title];[description];directions;items] ->
    { update status title description directions with items=items }
  | Regex inventoryPattern [items] -> { status with inventory=items }
  | Regex defaultPattern [[description]] -> { status with description=description }
  | _ -> { status with title="PARSING ERROR" }


let Init program =
  ExecuteAndGetStatus ("",program)

let InputAction action (status:Status) =
  match action with
  | 'N' -> "north"
  | 'S' -> "south"
  | 'E' -> "east"
  | 'W' -> "west"
  | 'T' -> "take "+ (Seq.head status.items)
  | 'I' -> "inv"
  | _ -> ""

let SingleAction status action =
  ExecuteAndGetStatus ((InputAction action status)+"\n",status.program)

let Explore program actions =
  Seq.fold SingleAction (Init program) actions

let ReversePath (s:string) :string =
  s.Replace('N', 's').Replace('S', 'N').Replace('s', 'S')
   .Replace('E', 'w').Replace('W', 'E').Replace('w', 'W').Reverse().ToArray()
  |> System.String

let TakeAll ss =
  System.String.Join("", ss |> Seq.map (fun p -> p + "T" + (ReversePath p)));
