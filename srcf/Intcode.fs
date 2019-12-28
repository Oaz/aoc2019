module Intcode

let loopUntil start f cond =
  let proceed (c,x) =
    let nextCond = cond x
    if c then None else Some(x,(nextCond,if nextCond then x else f x))
  Seq.unfold proceed (false,start)

type Computer<'intcode
  when 'intcode : (static member op_Explicit :  'intcode -> int)
  and  'intcode : (static member (+) : 'intcode * 'intcode -> 'intcode)
  and  'intcode : (static member (*) : 'intcode * 'intcode -> 'intcode)
  > =
  {
    counter: int;
    relativeBase: int;
    running: bool;
    halted: bool;
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
    relativeBase = 0;
    running = false;
    halted = false;
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

let inline GetInput (c:Computer<'intcode>) = c.input
let inline Input (l:List<'intcode>) (c:Computer<'intcode>) = { c with input = c.input @ l }
let inline HasSomeInputLeft (c:Computer<'intcode>) = (GetInput >> List.isEmpty >> not) c

let inline Output (c:Computer<'intcode>) = c.output
let inline ClearOutput (c:Computer<'intcode>) = { c with output = List.empty }
let inline AppendOutput (v:'intcode) (c:Computer<'intcode>) = { c with output=c.output @ [v] }

let inline Goto (pc:int) (c:Computer<'intcode>) = { c with counter=pc }
let inline ShiftCounter (delta:int) (c:Computer<'intcode>) = Goto (c.counter+delta) c
let inline MemoryWrite (i:int) (v:'intcode) (c:Computer<'intcode>) = { c with program=c.program.Add(i,v) }
let inline IsHalted (c:Computer<'intcode>) = c.halted

let inline Step (computer:Computer<'intcode>) =
  let opcode = int computer.program.[computer.counter]
  let read (i:int) = match (opcode / (pown 10 (i+1))) % 10 with
                     | 0 -> int computer.program.[computer.counter+i]
                     | 2 -> int computer.program.[computer.counter+i]+computer.relativeBase
                     | _ -> computer.counter+i
  let mem (i:int) = Map.tryFind (read i) computer.program |> Option.defaultValue computer.zero
  let jumpif (b:bool) (c:Computer<'intcode>) = Goto (if (mem 1 |> int)<>0 = b then (mem 2 |> int) else c.counter+3) c
  let instruction = opcode % 100
  let oneElseZero (b:bool) = if b then computer.one else computer.zero
  let popinput (c:Computer<'intcode>) = { c with input=List.tail c.input }
  let stoprunning = {computer with running=false}
  match instruction with
  | 1 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) + (mem 2))
  | 2 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) * (mem 2))
  | 3 -> if List.isEmpty computer.input
         then stoprunning
         else computer |> ShiftCounter 2 |> MemoryWrite (read 1) (List.head computer.input) |> popinput
  | 4 -> computer |> ShiftCounter 2 |> AppendOutput (mem 1)
  | 5 -> computer |> jumpif true
  | 6 -> computer |> jumpif false
  | 7 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) < (mem 2) |> oneElseZero)
  | 8 -> computer |> ShiftCounter 4 |> MemoryWrite (read 3) ((mem 1) = (mem 2) |> oneElseZero)
  | 9 -> {computer with relativeBase=computer.relativeBase+(mem 1|>int)} |> ShiftCounter 2
  | 99 -> {stoprunning with halted=true} |> ShiftCounter 1
  | _ -> stoprunning
  
let inline Steps (computer:Computer<'intcode>) =
  loopUntil computer Step (fun c -> not (c.running))

let inline Run (computer:Computer<'intcode>) :Computer<'intcode> =
  {computer with running=true} |> Steps |> Seq.last
 
let inline RunIO (before:list<'intcode>*Computer<'intcode>) : list<'intcode>*Computer<'intcode> =
  let result = Input (fst before) (snd before) |> Run
  (Output result,ClearOutput result)
