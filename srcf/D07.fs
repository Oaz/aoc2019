module Src07

open Intcode

let PhaseSettings (input:int) (len:int) : seq<int> =
  Seq.unfold (fun (i,l) -> if l=0 then None else Some(i%10,(i/10,l-1))) (input,len) |> Seq.rev

let rec AllPermutations (l:list<'a>) : list<list<'a>> = [
    for head in l do
    let withoutHead = List.except [head] l
    let others = match withoutHead with
                 | [a] -> [[a]]
                 | _ -> withoutHead |> AllPermutations
    for tail in others do
    yield List.Cons (head,tail)
  ]

type AmplifierSeries = seq<Computer<int>>

let MakeAmplifierSeries (program:string) (len:int) = LoadIntProgram program |> Seq.replicate len

let InitializeWithSettings (input:seq<int>) (amp:AmplifierSeries) : AmplifierSeries =
  input |> Seq.zip amp |> Seq.map (fun (c,s) -> c |> Input [s])

let ComputeOutputSignal (input:int) (amp:AmplifierSeries) : int =
  amp |> Seq.fold (fun io -> (Input io >> Run >> Output)) [input] |> List.head

let FindMaxOutputSignal (amp:AmplifierSeries) (input:int) (s:seq<int>) =
  let allresults = [ for settings in (List.ofSeq s |> AllPermutations |> List.map Seq.ofList) do
                     yield (settings,InitializeWithSettings settings amp |> ComputeOutputSignal input)]
  allresults |> List.maxBy snd
  