namespace src25
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;
  using System.IO;
  using System.Text;
  using System.Text.RegularExpressions;

  public class Explorer : AbstractIntcodeComputerForLong
  {
    public static Status Try(string program, string actions) =>
      new Explorer(program).Execute(actions);

    public Explorer(string program)
      : base(program)
    {
      AsciiIn = new Queue<long>();
      Actions = new Queue<string>();
      Output = new StringBuilder();
    }

    Status Execute(string actions)
    {
      actions.ToList().ForEach(c => Actions.Enqueue(c.ToString()));
      Actions.Enqueue("");
      while (!HasHalted && Actions.Any())
        RunOne();
      if (HasHalted)
        CurrentStatus = new Status(Output.ToString());
      return CurrentStatus;
    }

    public Queue<long> AsciiIn;
    public Queue<string> Actions;
    public StringBuilder Output;
    public Status CurrentStatus;

    public override long ReadFromInput()
    {
      if(AsciiIn.Count>0)
        return AsciiIn.Dequeue();
      var text = Output.ToString().Trim();
      Output.Clear();
      CurrentStatus = new Status(text);
      var action = Actions.Dequeue();
      (ChooseInput(CurrentStatus,action)+ "\n").ToList().ForEach(c => AsciiIn.Enqueue(c));
      return AsciiIn.Dequeue();
    }
    public static string ChooseInput(Status place, string action)
    {
      switch(action)
      {
        case "N": return "north";
        case "S": return "south";
        case "E": return "east";
        case "W": return "west";
        case "T": return "take "+place.Items.First();
        case "I": return "inv";
        default: return string.Empty;
      }
    }
    public override void WriteToOutput(long val)
    {
      Output.Append((char)val);
      if(Output.Length > 10000)
      {
        CurrentStatus = new Status(Output.ToString().Substring(9900));
        Actions.Clear();
      }
    }
  }

  public class Status
  {
    public string Text;
    public string Title;
    public string Description;
    public List<string> Directions = new List<string>();
    public List<string> Items = new List<string>();
    public List<string> Inventory = new List<string>();

    public Status(string text)
    {
      Text = text;
      var analysisMatch = analysisPattern.Match(text);
      if (analysisMatch.Success)
      {
        Title = analysisMatch.Groups[1].Value;
        Description = analysisMatch.Groups[3].Value.Trim();
        Directions = analysisMatch.Groups[2].Captures.Select(c => c.Value).ToList();
        return;
      }
      var placeMatch = placePattern.Match(text);
      if (placeMatch.Success)
      {
        Title = placeMatch.Groups[1].Value;
        Description = placeMatch.Groups[2].Value;
        Directions = placeMatch.Groups[3].Captures.Select(c => c.Value).ToList();
        Items = placeMatch.Groups[4].Captures.Select(c => c.Value).ToList();
        return;
      }
      var inventoryMatch = inventoryPattern.Match(text);
      if (inventoryMatch.Success)
      {
        Inventory = inventoryMatch.Groups[1].Captures.Select(c => c.Value).ToList();
        return;
      }
      var defaultMatch = defaultPattern.Match(text);
      if (defaultMatch.Success)
      {
        Description = defaultMatch.Groups[1].Value;
        return;
      }
      Description = text.Replace("\n", "").Trim();
    }
    static Regex analysisPattern = new Regex(
      "== (.+) ==\\nAnalyzing...\\n\\n"
      + "Doors here lead:\\n(?:- (.+?)\\n)+"
      + "\\n(.+?)(\\n\\n\\n|$)", RegexOptions.Singleline);
    static Regex placePattern = new Regex(
      "== (.+) ==\\n(.*)\\n\\n"
      + "Doors here lead:\\n(?:- (.+)\\n)+"
      + "(?:\\nItems here:\\n(?:- (.+)\\n)+)?"
      + "\\nCommand");
    static Regex inventoryPattern = new Regex("Items in your inventory:\\n(?:- (.+)\\n)+\\nCommand");
    static Regex defaultPattern = new Regex("(.+)\\n\\nCommand");
  }

  public class ASCIIComputer : AbstractIntcodeComputerForLong
  {
    public ASCIIComputer(string program, IEnumerable<char> input, Action<char> output)
      : base(program)
    {
      Input = input.GetEnumerator();
      Output = output;
    }

    IEnumerator<char> Input;
    Action<char> Output;

    public override long ReadFromInput() => Input.MoveNext() ? Input.Current : Input.Current;
    public override void WriteToOutput(long val) => Output((char)val);
  }
  public class LinuxASCIIComputer : AbstractIntcodeComputerForLong
  {
    public LinuxASCIIComputer(string program, Stream asciiIn, Stream asciiOut)
      : base(program)
    {
      AsciiIn = asciiIn;
      AsciiOut = asciiOut;
    }

    Stream AsciiIn;
    Stream AsciiOut;

    public override long ReadFromInput() => AsciiIn.ReadByte();
    public override void WriteToOutput(long val) => AsciiOut.WriteByte((byte)val);
  }


  public class IntcodeComputer : AbstractIntcodeComputerForBigInteger
  {
    public IntcodeComputer(int[] program) : this(program.AsBig()) { }
    public IntcodeComputer(IEnumerable<BigInteger> program) : base(program) { }

    public override BigInteger ReadFromInput() => Input.Dequeue();
    public override void WriteToOutput(BigInteger val) => Output.Enqueue(val);
    public Queue<BigInteger> Input = new Queue<BigInteger>();
    public Queue<BigInteger> Output = new Queue<BigInteger>();
  }


  public abstract class AbstractIntcodeComputerForBigInteger : AbstractIntcodeComputer<BigInteger>
  {
    public AbstractIntcodeComputerForBigInteger(string p) : this(IntcodeProgram<BigInteger>.Load(p,BigInteger.Parse)) {}
    public AbstractIntcodeComputerForBigInteger(IEnumerable<BigInteger> program) : base(program, bi => (int)bi, i => i, (a, b) => a + b, (a, b) => a * b) { }
  }

  public abstract class AbstractIntcodeComputerForLong : AbstractIntcodeComputer<long>
  {
    public AbstractIntcodeComputerForLong(string p) : this(IntcodeProgram<long>.Load(p, long.Parse)) { }
    public AbstractIntcodeComputerForLong(IEnumerable<long> program) : base(program, bi => (int)bi, i => i, (a, b) => a + b, (a, b) => a * b) { }
  }


  public class IntcodeProgram<T>
  {
    public static IEnumerable<T> Load(string s, Func<string,T> parse) => s.Split(',').Select(n => parse(n)).ToArray();
  }

  public abstract class AbstractIntcodeComputer<T> where T : IComparable<T>, IEquatable<T>
  {
    public AbstractIntcodeComputer(IEnumerable<T> program, Func<T, int> intify, Func<int, T> untify, Func<T, T, T> add, Func<T, T, T> mul)
    {
      Program = program.ToList();
      I = intify;
      var zero = untify(0);
      var one = untify(1);
      Operations = new Dictionary<int, Operation<T>>
      {
        { 1, new OperationBinary<T>((a,b) => add(a,b)) },
        { 2, new OperationBinary<T>((a,b) => mul(a,b)) },
        { 3, new OperationInput<T>() },
        { 4, new OperationOutput<T>() },
        { 5, new OperationJumpIf<T>(true) },
        { 6, new OperationJumpIf<T>(false) },
        { 7, new OperationBinary<T>((a,b) => a.CompareTo(b)<0 ? one:zero) },
        { 8, new OperationBinary<T>((a,b) => a.Equals(b) ? one:zero) },
        { 9, new OperationChangeRelativeBase<T>() }
      };
    }
    protected AbstractIntcodeComputer() { }
    public T ReadAt(int index) =>
      ResizeToIncludeAnd(index, () => Program[index]);
    public T WriteAt(int index, T val) =>
      ResizeToIncludeAnd(index, () => Program[index] = val);
    private T ResizeToIncludeAnd(int index, Func<T> f)
    {
      if (Program.Count - 1 < index)
        Program.AddRange(new T[index + 1 - Program.Count]);
      return f();
    }

    public Func<T, int> I;
    public List<T> Program;
    public int Counter = 0;
    public int RelativeBase = 0;
    public Dictionary<int, Operation<T>> Operations;

    protected void CopyTo(AbstractIntcodeComputer<T> dst)
    {
      dst.I = I;
      dst.Program = Program.ToList();
      dst.Counter = Counter;
      dst.RelativeBase = RelativeBase;
      dst.Operations = Operations;
    }

    public abstract T ReadFromInput();
    public abstract void WriteToOutput(T val);

    public AbstractIntcodeComputer<T> Run()
    {
      Counter = 0;
      while (!HasHalted)
        RunOne();
      return this;
    }
    public bool HasHalted => I(Program[Counter]) == 99;
    public AbstractIntcodeComputer<T> MoveTo(int index)
    {
      Counter = index;
      return this;
    }
    public AbstractIntcodeComputer<T> RunOne()
    {
      var opcode = I(Program[Counter]);
      var operation = Operations[opcode % 100];
      var modes = new int[] { (opcode / 100) % 10, (opcode / 1000) % 10, (opcode / 10000) % 10 };
      var args = Enumerable
                  .Range(Counter + 1, operation.Length - 1)
                  .Zip(modes).Select(ChooseMode).ToArray();
      operation.Execute(this, args);
      if (I(Program[Counter]) == opcode)
        Counter += operation.Length;
      return this;
    }
    private int ChooseMode((int, int) x) =>
      x.Item2 == 1 ? x.Item1 : I(Program[x.Item1]) + (x.Item2 == 0 ? 0 : RelativeBase);
  }

  public interface Operation<T> where T : IComparable<T>, IEquatable<T>
  {
    void Execute(AbstractIntcodeComputer<T> computer, int[] args);
    int Length { get; }
  }

  public class OperationBinary<T> : Operation<T> where T : IComparable<T>, IEquatable<T>
  {
    public OperationBinary(Func<T, T, T> o) { op = o; }
    readonly Func<T, T, T> op;
    public void Execute(AbstractIntcodeComputer<T> computer, int[] args)
      => computer.WriteAt(args[2], op(computer.ReadAt(args[0]), computer.ReadAt(args[1])));
    public int Length => 4;
  }

  public class OperationInput<T> : Operation<T> where T : IComparable<T>, IEquatable<T>
  {
    public void Execute(AbstractIntcodeComputer<T> computer, int[] args) =>
      computer.WriteAt(args[0], computer.ReadFromInput());
    public int Length => 2;
  }

  public class OperationOutput<T> : Operation<T> where T : IComparable<T>, IEquatable<T>
  {
    public void Execute(AbstractIntcodeComputer<T> computer, int[] args) =>
      computer.WriteToOutput(computer.ReadAt(args[0]));
    public int Length => 2;
  }

  public class OperationChangeRelativeBase<T> : Operation<T> where T : IComparable<T>, IEquatable<T>
  {
    public void Execute(AbstractIntcodeComputer<T> computer, int[] args) =>
      computer.RelativeBase += computer.I(computer.ReadAt(args[0]));
    public int Length => 2;
  }

  public class OperationJumpIf<T> : Operation<T> where T : IComparable<T>, IEquatable<T>
  {
    public OperationJumpIf(bool c) { condition = c; }
    readonly bool condition;
    public void Execute(AbstractIntcodeComputer<T> computer, int[] args)
    {
      if ((computer.I(computer.ReadAt(args[0])) == 0) != condition)
        computer.Counter = computer.I(computer.ReadAt(args[1]));
    }
    public int Length => 3;
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
