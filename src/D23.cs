namespace src23
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;

  public class Network
  {
    public Network(int n, IEnumerable<BigInteger> program)
    {
      Bus = new Queue<Packet>();
      NICs = Enumerable.Range(0, n).Select(address => new NIC(program, address, Bus)).ToList();
    }

    public IEnumerable<Packet> Run()
    {
      Packet? packetToNAT = null;
      foreach (var nic in NICs.RepeatInfinitely())
      {
        if(packetToNAT.HasValue && NetworkIsIdle)
        {
          yield return packetToNAT.Value;
          NICs[0].Send(packetToNAT.Value);
          packetToNAT = null;
        }
        nic.RunOne();
        if (!Bus.Any())
          continue;
        var packet = Bus.Dequeue();
        if (packet.Destination == 255)
        {
          packetToNAT = packet;
          continue;
        }
        var targetNIC = NICs[packet.Destination];
        targetNIC.Send(packet);
      }
    }
    public bool NetworkIsIdle => NICs.All(n => n.Inputs.Count==0);
    public readonly List<NIC> NICs;
    public Queue<Packet> Bus;
  }

  public readonly struct Packet
  {
    public Packet(int source, int destination, BigInteger x, BigInteger y)
    {
      Source = source;
      Destination = destination;
      X = x;
      Y = y;
    }
    public readonly int Source;
    public readonly int Destination;
    public readonly BigInteger X;
    public readonly BigInteger Y;

  }
  public class NIC : AbstractIntcodeComputer
  {
    public NIC(IEnumerable<BigInteger> program, int address, Queue<Packet> bus) : base(program)
    {
      Inputs = new Queue<BigInteger>();
      Inputs.Enqueue(address);
      OutputBuffer = new Queue<BigInteger>();
      Bus = bus;
      Address = address;
    }
    public int Address;
    public Queue<BigInteger> Inputs;
    public Queue<BigInteger> OutputBuffer;
    public Queue<Packet> Bus;

    public void Send(Packet packet)
    {
      Inputs.Enqueue(packet.X);
      Inputs.Enqueue(packet.Y);
    }
    public override BigInteger ReadFromInput() => Inputs.Any() ? Inputs.Dequeue() : -1;

    public override void WriteToOutput(BigInteger val)
    {
      OutputBuffer.Enqueue(val);
      if (OutputBuffer.Count == 3)
      {
        var address = (int)OutputBuffer.Dequeue();
        var x = OutputBuffer.Dequeue();
        var y = OutputBuffer.Dequeue();
        var packet = new Packet(Address, address, x, y);
        Bus.Enqueue(packet);
      }
    }
  }

  public class IntcodeProgram
  {
    public static IEnumerable<BigInteger> Load(string s) =>
      s.Split(',').Select(n => BigInteger.Parse(n)).ToArray();
  }

  public abstract class AbstractIntcodeComputer
  {
    public AbstractIntcodeComputer(IEnumerable<BigInteger> program) =>
      Program = program.ToList();
    protected AbstractIntcodeComputer() { }
    public BigInteger ReadAt(BigInteger index) =>
      ResizeToIncludeAnd((int)index, () => Program[(int)index]);
    public BigInteger WriteAt(BigInteger index, BigInteger val) =>
      ResizeToIncludeAnd((int)index, () => Program[(int)index] = val);
    private BigInteger ResizeToIncludeAnd(int index, Func<BigInteger> f)
    {
      if (Program.Count - 1 < index)
        Program.AddRange(new BigInteger[index + 1 - Program.Count]);
      return f();
    }

    public List<BigInteger> Program;
    public int Counter = 0;
    public int RelativeBase = 0;

    protected void CopyTo(AbstractIntcodeComputer dst)
    {
      dst.Program = Program.ToList();
      dst.Counter = Counter;
      dst.RelativeBase = RelativeBase;
    }

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
      var opcode = (int)Program[Counter];
      var operation = Operations[opcode % 100];
      var modes = new int[] {(opcode/100)%10, (opcode/1000)%10, (opcode/10000)%10};
      var args = Enumerable
                  .Range(Counter + 1, operation.Length - 1)
                  .Zip(modes).Select(ChooseMode).ToArray();
      Counter += operation.Length;
      operation.Execute(this, args);
      return this;
    }
    private BigInteger ChooseMode((int, int) x) =>
      x.Item2 == 1 ? x.Item1 : Program[x.Item1] + (x.Item2 == 0 ? 0 : RelativeBase);
  }

  public interface Operation
  {
    void Execute(AbstractIntcodeComputer computer, BigInteger[] args);
    int Length { get; }
  }

  public class OperationBinary : Operation
  {
    public OperationBinary(Func<BigInteger, BigInteger, BigInteger> o) { op = o; }
    readonly Func<BigInteger, BigInteger, BigInteger> op;
    public void Execute(AbstractIntcodeComputer computer, BigInteger[] args)
      => computer.WriteAt(args[2], op(computer.ReadAt(args[0]), computer.ReadAt(args[1])));
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
      public static IEnumerable<int> AsInt(this IEnumerable<BigInteger> l) =>
        l.Select(i => (int)i);
      public static BigInteger AsBig(this int i) => new BigInteger(i);
      public static int AsInt(this BigInteger i) => (int)i;
  }

  public static class LinqX
  {
    public static T Identity<T>(T x) => x;
    public static IEnumerable<T> Generate<T>(T x, Func<T, T> f)
    {
      for (; ; ) { yield return x; x = f(x); }
    }
    public static IEnumerable<T> RepeatInfinitely<T>(this IEnumerable<T> l) =>
      Generate(l, Identity).SelectMany(Identity);
  }
}
