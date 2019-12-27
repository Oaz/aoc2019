module Intcode

type Computer<'intcode
  when 'intcode : (static member op_Explicit :  'intcode -> int)
  and  'intcode : (static member (+) : 'intcode * 'intcode -> 'intcode)
  and  'intcode : (static member (*) : 'intcode * 'intcode -> 'intcode)
  > =
  {
    counter: int;
    running: bool;
    program: Map<int,'intcode>;
    input: List<'intcode>;
    output: List<'intcode>;
    zero: 'intcode;
    one: 'intcode
  }

let inline LoadProgram (parse:string->'intcode) (s:string) =
  let code = s.Split [|','|] |> Seq.mapi (fun i -> fun s -> (i, parse s))
  {
    counter = 0;
    running = true;
    program = Map.ofSeq code;
    input = List.empty;
    output = List.empty;
    zero = parse "0";
    one = parse "1"
  }

let LoadIntProgram = LoadProgram int32
let LoadLongProgram = LoadProgram int64
let LoadBigProgram = LoadProgram bigint.Parse

let inline MemoryDump (computer:Computer<'intcode>) =
  let size = Map.count computer.program
  [0..size-1] |> Seq.map (fun i -> computer.program.[i])

let inline Input (l:List<'intcode>) (c:Computer<'intcode>) = { c with input = c.input @ l }

let inline Output (c:Computer<'intcode>) = Seq.ofList c.output
let inline ClearOutput (c:Computer<'intcode>) = { c with output = List.empty }
let inline AppendOutput (v:'intcode) (c:Computer<'intcode>) = { c with output=c.output @ [v] }

let inline Goto (pc:int) (c:Computer<'intcode>) = { c with counter=pc }
let inline ShiftCounter (delta:int) (c:Computer<'intcode>) = Goto (c.counter+delta) c
let inline MemoryWrite (i:int) (v:'intcode) (c:Computer<'intcode>) = { c with program=c.program.Add(i,v) }

let inline RunOne (computer:Computer<'intcode>) =
  let opcode = int computer.program.[computer.counter]
  let read (i:int) = match (opcode / (pown 10 (i+1))) % 10 with
                     | 0 -> int computer.program.[computer.counter+i]
                     | _ -> computer.counter+i
  //let mem (i:int) = Map.tryFind (read i) computer.program |> Option.defaultValue computer.zero
  let mem (i:int) = computer.program.[read i]
  let popinput (c:Computer<'intcode>) = { c with input=List.tail c.input }
  let jumpif (b:bool) (c:Computer<'intcode>) = Goto (if (mem 1 |> int)<>0 = b then (mem 2 |> int) else c.counter+3) c
  let instruction = opcode % 100
  let oneElseZero (b:bool) = if b then computer.one else computer.zero
  match instruction with
  | 1 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) + (mem 2))
  | 2 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) * (mem 2))
  | 3 -> computer |> ShiftCounter 2 |> MemoryWrite (read 1) (List.head computer.input) |> popinput
  | 4 -> computer |> ShiftCounter 2 |> AppendOutput (mem 1)
  | 5 -> computer |> jumpif true
  | 6 -> computer |> jumpif false
  | 7 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) < (mem 2) |> oneElseZero)
  | 8 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) = (mem 2) |> oneElseZero)
  | _ -> {computer with running=false}
  
let inline Run (computer:Computer<'intcode>) =
  Seq.unfold (fun c -> Some(c,RunOne c)) computer
  |> Seq.takeWhile (fun c -> c.running)

let inline RunToHalt (computer:Computer<'intcode>) = computer |> Run |> Seq.last


  