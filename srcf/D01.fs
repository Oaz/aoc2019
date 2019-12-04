module Src01

let FuelForMass mass =
  mass/3 - 2

let TotalFuelForMass mass =
  Seq.unfold (fun m -> Some(m,FuelForMass m)) mass
    |> Seq.takeWhile (fun x -> x > 0)
    |> Seq.skip 1
    |> Seq.sum

let SumOf  (f : int -> int) (l : string seq) =
  l |> Seq.sumBy(int >> f)


