namespace src13
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public class Arcade : AbstractIntcodeComputer
  {
    public Arcade(BigInteger[] program) : base(program) {
      writes = new Action<int>[] {Write1,Write2,Write3};
    }

    public Coords CurrentPosition;
    public int Score;
    public static Coords scorePosition = Coords.At(-1,0);

    public Dictionary<Coords,int> Screen = new Dictionary<Coords, int>();
    public Coords Ball;
    public Coords Paddle;

    public int NumberOfBlockTiles()
    {
      writeIndex = 0;
      Run();
      return Screen.Values.Count(v => v==2);
    }

    public int ScoreAfterBreakingAllBlocks()
    {
      Score = 0;
      writeIndex = 0;
      Program[0] = 2;
      Run();
      return Score;
    }

    public override BigInteger ReadFromInput()
    {
      return Math.Sign(Ball.X-Paddle.X);
    }
    int writeIndex;
    readonly Action<int>[] writes;
    public void Write1(int v) => CurrentPosition = Coords.At(v,0);
    public void Write2(int v) => CurrentPosition = Coords.At(CurrentPosition.X,v);
    public void Write3(int v) {
      if(CurrentPosition.Equals(scorePosition))
      {
        Score = v;
        return;
      }
      Screen[CurrentPosition]=v;
      if(v==3)
        Paddle = CurrentPosition;
      else if(v==4)
        Ball = CurrentPosition;
    }

    public override void WriteToOutput(BigInteger val)
    {
      writes[writeIndex]((int)val);
      writeIndex = (writeIndex+1)%writes.Length;
    }
  }

  public readonly struct Coords
  {
    public static Coords At(int x, int y) => new Coords(x,y);
    public Coords(int x, int y) { X=x; Y=y; }
    public readonly int X;
    public readonly int Y;

    public override string ToString() => $"({X},{Y})";

    public static Coords operator +(Coords a, Coords b) => At(a.X+b.X, a.Y+b.Y);
    public static Coords operator -(Coords a, Coords b) => At(a.X-b.X, a.Y-b.Y);
  }

  public static class CoordsExtensions
  {
    public static Coords Bulk(this IEnumerable<Coords> l, Func<IEnumerable<int>,int> f) =>
      Coords.At(f(l.Select(c => c.X)),f(l.Select(c => c.Y)));
    public static Coords Min(this IEnumerable<Coords> l) => l.Bulk(x => x.Min());
    public static Coords Max(this IEnumerable<Coords> l) => l.Bulk(x => x.Max());
  }

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
