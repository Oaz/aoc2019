namespace src25
{
  using System;
  using System.Numerics;
  using System.Collections.Generic;
  using System.Linq;
  using System.IO;
  using System.Text;
  using System.Text.RegularExpressions;

  public class Explorer : AbstractIntcodeComputer
  {
    public static Status Try(IEnumerable<BigInteger> program, string actions) =>
      new Explorer(program).Execute(actions);

    public Explorer(IEnumerable<BigInteger> program)
      : base(program)
    {
      AsciiIn = new Queue<BigInteger>();
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

    public Queue<BigInteger> AsciiIn;
    public Queue<string> Actions;
    public StringBuilder Output;
    public Status CurrentStatus;

    public override BigInteger ReadFromInput()
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
    public override void WriteToOutput(BigInteger val)
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

  public class ASCIIComputer : AbstractIntcodeComputer
  {
    public ASCIIComputer(IEnumerable<BigInteger> program, IEnumerable<char> input, Action<char> output)
      : base(program)
    {
      Input = input.GetEnumerator();
      Output = output;
    }

    IEnumerator<char> Input;
    Action<char> Output;

    public override BigInteger ReadFromInput() => Input.MoveNext() ? Input.Current : Input.Current;
    public override void WriteToOutput(BigInteger val) => Output((char)val);
  }
  public class LinuxASCIIComputer : AbstractIntcodeComputer
  {
    public LinuxASCIIComputer(IEnumerable<BigInteger> program, Stream asciiIn, Stream asciiOut)
      : base(program)
    {
      AsciiIn = asciiIn;
      AsciiOut = asciiOut;
    }

    Stream AsciiIn;
    Stream AsciiOut;

    public override BigInteger ReadFromInput() => AsciiIn.ReadByte();
    public override void WriteToOutput(BigInteger val) => AsciiOut.WriteByte((byte)val);
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
      while (!HasHalted)
        RunOne();
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
      var modes = new int[] { (opcode / 100) % 10, (opcode / 1000) % 10, (opcode / 10000) % 10 };
      var args = Enumerable
                  .Range(Counter + 1, operation.Length - 1)
                  .Zip(modes).Select(ChooseMode).ToArray();
      operation.Execute(this, args);
      if (Program[Counter] == opcode)
        Counter += operation.Length;
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
    public IntcodeComputer(int[] program) : base(program.AsBig()) { }
    public IntcodeComputer(BigInteger[] program) : base(program) { }

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
