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

let MakeAmplifierSeries (program:string) (len:int) =
  LoadIntProgram program |> Seq.replicate len

let InitializeWithSettings (input:seq<int>) (amp:AmplifierSeries) : AmplifierSeries =
  input |> Seq.zip amp |> Seq.map (fun (c,s) -> c |> Input [s])

let ThroughAmplifiers (before:int*AmplifierSeries) : int*AmplifierSeries =
  let amp (i,a) =
    let (o,b) = RunSingleIO (i,Seq.head a)
    (o,Seq.append (Seq.tail a) [b])
  Seq.fold (fun (i,a) -> fun _ -> amp (i,a)) before (snd before)

let SimpleSignal (amp:AmplifierSeries) : int =
  ThroughAmplifiers (0,amp) |> fst
 
let FeedbackLoopSignal (amp:AmplifierSeries) =
  loopUntil (0,amp) ThroughAmplifiers (snd >> Seq.last >> IsHalted)
  |> Seq.last |> fst

let FindMaxOutput f (s:seq<int>) (amp:AmplifierSeries) =
  let computeSignalFor (settings:seq<int>) =
    InitializeWithSettings settings amp |> f
  let allresults = [ for settings in (List.ofSeq s |> AllPermutations |> List.map Seq.ofList) do
                     yield (settings,computeSignalFor settings)]
  allresults |> List.maxBy snd
