module Src02

let SetItem (idx:int) (value:'a) (s:'a seq) =
  Seq.mapi (fun i v -> if i=idx then value else v) s

let Read (index:int) (program:int seq) =
  try
    Some(Seq.item (Seq.item index program) program)
  with
    | ex -> None

let Write (index:int) (value:int option) (program:int seq) =
  match value with
  | Some(v) -> SetItem (Seq.item index program) v program
  | None -> program

let inline (!>) op x y =
  match (x,y) with
  | (Some(a),Some(b)) -> Some(op a b)
  | _ -> None

let RunAt (index:int) (program:int seq) =
  let operation = if (Seq.item index program)=1 then ((!>) (+)) else ((!>) (*))
  Write (index+3) (operation (Read (index+1) program) (Read (index+2) program)) program

let Run (program:int seq) =
  let rec Runi (program:int seq) (index:int) =
    if (Seq.item index program)=99 then
      program
    else
      Runi (RunAt index program) (index+4)
  Runi program 0

let RunWithInputs (program:int seq) (noun:int) (verb:int) =
  program |> SetItem 1 noun |> SetItem 2 verb |> Run

let FindInputsFor (outputToFind:int) (program:int seq) =
  seq {
    for noun in 0 .. 99 do
      for verb in 0 .. 99 ->
        let output = RunWithInputs program noun verb |> Seq.head
        if output=outputToFind then Some(noun*100+verb) else None
  } |> Seq.choose id |> Seq.head

  