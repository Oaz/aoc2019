module Src06

open System.Linq

type OrbitMap(definition: string seq) =

  let orbitMap =
    definition
    |> Seq.map (fun s -> s.Split(')'))
    |> Seq.map (fun x -> (x.[1], x.[0]))
    |> Map.ofSeq

  member this.ChainStartingFrom(obj:string) =
    Seq.unfold (fun o -> if orbitMap.ContainsKey o then Some(o,orbitMap.[o]) else None) obj
    |> Seq.map (fun o -> orbitMap.[o])

  member this.CountAllOrbits
    with get() = orbitMap
      |> Map.toSeq
      |> Seq.map (fun x -> this.ChainStartingFrom(fst x) |> Seq.length)
      |> Seq.sum

  member this.ClosestSharedObject (a:string) (b:string) =
    let chainA = this.ChainStartingFrom a
    let chainB = this.ChainStartingFrom b
    chainA.Intersect(chainB) |> Seq.head

  member this.CountHopsFromTo (a:string) (b:string) =
    let cso = this.ClosestSharedObject a b
    let CountFrom (o:string) =
      this.ChainStartingFrom o
      |> Seq.takeWhile (fun x -> x<>cso)
      |> Seq.length
    (CountFrom a)+(CountFrom b)
