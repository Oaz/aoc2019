namespace src09
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public abstract class AbstractIntcodeComputer
  {
    public AbstractIntcodeComputer(IEnumerable<BigInteger> program) => Program = program.ToList();
    public List<BigInteger> Program;
    public BigInteger ReadAt(BigInteger index) =>
      ResizeToIncludeAnd((int)index, () => Program[(int)index]);
    public BigInteger WriteAt(BigInteger index, BigInteger val) =>
      ResizeToIncludeAnd((int)index, () => Program[(int)index] = val);
    private BigInteger ResizeToIncludeAnd(int index, Func<BigInteger> f)
    {
      if(Program.Count-1 < index)
        Program.AddRange(new BigInteger[index + 1 - Program.Count]);
      return f();
    }

    public int Counter = 0;
    public int RelativeBase = 0;

    public abstract BigInteger ReadFromInput();
    public abstract void WriteToOutput(BigInteger val);
    public static Dictionary<int, Operation> Operations = new Dictionary<int, Operation>
      {
        { 1, new OperationBinary((a,b) => a+b) },
        { 2, new OperationBinary((a,b) => a*b) },
        { 3, new OperationInput() },
        { 4, new OperationOutput() },
        { 5, new OperationJumpIf(true) },
        { 6, new OperationJumpIf(false) },
        { 7, new OperationBinary((a,b) => a<b ? 1 : 0) },
        { 8, new OperationBinary((a,b) => a==b ? 1 : 0) },
        { 9, new OperationChangeRelativeBase() }
      };

    public AbstractIntcodeComputer Run()
    {
      Counter = 0;
      while (!HasHalted) RunOne();
      return this;
    }
    public bool HasHalted => Program[Counter] == 99;
    public AbstractIntcodeComputer MoveTo(int index)
    {
      Counter = index;
      return this;
    }
    public AbstractIntcodeComputer RunOne()
    {
      var opcode = Program[Counter];
      var operation = Operations[(int)(opcode % 100)];
      var modes = Observable.Generate(opcode / 100, _ => true, m => m / 10, m => m % 10).Take(3).ToEnumerable();
      var args = Enumerable
                  .Range(Counter + 1, operation.Length - 1)
                  .Zip(modes).Select(ChooseMode).ToArray();
      operation.Execute(this, args);
      if(Program[Counter] == opcode)
        Counter += operation.Length;
      return this;
    }
    private BigInteger ChooseMode((int,BigInteger) x) =>
      x.Item2 == 1 ? x.Item1 : Program[x.Item1] + (x.Item2 == 0 ? 0 : RelativeBase);
  }

  public interface Operation
  {
    void Execute(AbstractIntcodeComputer computer, BigInteger[] args);
    int Length { get; }
  }

  public class OperationBinary : Operation
  {
    public OperationBinary(Func<BigInteger,BigInteger,BigInteger> o) { op = o; }
    readonly Func<BigInteger, BigInteger, BigInteger> op;
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args)
      => computer.WriteAt(args[2],op(computer.ReadAt(args[0]), computer.ReadAt(args[1])));
    public int Length => 4;
  }

  public class OperationInput : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args) =>
      computer.WriteAt(args[0], computer.ReadFromInput());
    public int Length => 2;
  }

  public class OperationOutput : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args) =>
      computer.WriteToOutput(computer.ReadAt(args[0]));
    public int Length => 2;
  }

  public class OperationChangeRelativeBase : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args) =>
      computer.RelativeBase += (int)computer.ReadAt(args[0]);
    public int Length => 2;
  }

  public class OperationJumpIf : Operation
  {
    public OperationJumpIf(bool c) { condition = c; }
    readonly bool condition;
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args)
    {
      if ((computer.ReadAt(args[0]) == 0) != condition)
        computer.Counter = (int)computer.ReadAt(args[1]);
    }
    public int Length => 3;
  }

  public class IntcodeComputer : AbstractIntcodeComputer
  {
    public IntcodeComputer(int[] program) : base(program.AsBig()) {}
    public IntcodeComputer(BigInteger[] program) : base(program) {}

    public override BigInteger ReadFromInput() => Input.Dequeue();
    public override void WriteToOutput(BigInteger val) => Output.Enqueue(val);
    public Queue<BigInteger> Input = new Queue<BigInteger>();
    public Queue<BigInteger> Output = new Queue<BigInteger>();
  }

  public static class ExtensionsHelper
  {
      public static IEnumerable<BigInteger> AsBig(this IEnumerable<int> l) =>
        l.Select(i => new BigInteger(i));
      public static BigInteger AsBig(this int i) => new BigInteger(i);
  }
}
