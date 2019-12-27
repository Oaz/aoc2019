module Src02

open Intcode

let RunWithInputs (computer:Computer<int>) (noun:int) (verb:int) : int =
  { computer with program = computer.program.Add(1,noun).Add(2,verb) } |> Run |> Seq.last |> MemoryDump |> Seq.head

let FindInputsFor (outputToFind:int) (computer:Computer<int>) =
  seq {
    for noun in 0 .. 99 do
      for verb in 0 .. 99 ->
        let output = RunWithInputs computer noun verb
        if output=outputToFind then Some(noun*100+verb) else None
  } |> Seq.choose id |> Seq.head

  