namespace src05
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public class IntcodeComputer
  {
    public IntcodeComputer(int[] program) { Program = program; }
    public int[] Program;
    public int Counter = 0;
    public Queue<int> Input = new Queue<int>();
    public Queue<int> Output = new Queue<int>();
    public static Dictionary<int, Operation> Operations;
    static IntcodeComputer()
    {
      Operations = new Dictionary<int, Operation>
      {
        { 1, new OperationBinary((a,b) => a+b) },
        { 2, new OperationBinary((a,b) => a*b) },
        { 3, new OperationInput() },
        { 4, new OperationOutput() },
        { 5, new OperationJumpIf(true) },
        { 6, new OperationJumpIf(false) },
        { 7, new OperationBinary((a,b) => a<b ? 1 : 0) },
        { 8, new OperationBinary((a,b) => a==b ? 1 : 0) }
      };
    }

    public IntcodeComputer Run()
    {
      Counter = 0;
      while (Program[Counter] != 99) RunOne();
      return this;
    }

    public IntcodeComputer MoveTo(int index)
    {
      Counter = index;
      return this;
    }

    public IntcodeComputer RunOne()
    {
      var opcode = Program[Counter];
      var operation = Operations[opcode % 100];
      var modes = Observable.Generate(opcode / 100, _ => true, m => m / 10, m => m % 10).Take(3).ToEnumerable();
      var args = Enumerable
                  .Range(Counter + 1, operation.Length - 1)
                  .Zip(modes)
                  .Select(p => p.Second == 0 ? Program[p.First] : p.First)
                  .ToArray();
      operation.Execute(this, args);
      if(Program[Counter] == opcode)
        Counter += operation.Length;
      return this;
    }
  }

  public interface Operation
  {
    void Execute(IntcodeComputer computer, int[] args);
    int Length { get; }
  }

  public class OperationBinary : Operation
  {
    public OperationBinary(Func<int,int,int> o) { op = o; }
    Func<int, int, int> op;
    public void Execute(IntcodeComputer computer, int[] args)
      => computer.Program[args[2]] = op(computer.Program[args[0]], computer.Program[args[1]]);
    public int Length => 4;
  }

  public class OperationInput : Operation
  {
    public void Execute(IntcodeComputer computer, int[] args) => computer.Program[args[0]] = computer.Input.Dequeue();
    public int Length => 2;
  }

  public class OperationOutput : Operation
  {
    public void Execute(IntcodeComputer computer, int[] args) => computer.Output.Enqueue(computer.Program[args[0]]);
    public int Length => 2;
  }

  public class OperationJumpIf : Operation
  {
    public OperationJumpIf(bool c) { condition = c; }
    bool condition;
    public void Execute(IntcodeComputer computer, int[] args)
    {
      if ((computer.Program[args[0]] == 0) != condition) computer.Counter = computer.Program[args[1]];
    }
    public int Length => 3;
  }
}
