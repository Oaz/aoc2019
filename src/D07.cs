namespace src07
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public abstract class AbstractIntcodeComputer
  {
    public AbstractIntcodeComputer(int[] program) { Program = program; }
    public int[] Program;
    public int Counter = 0;

    public abstract int ReadFromInput();
    public abstract void WriteToOutput(int val);
    public static Dictionary<int, Operation> Operations;
    static AbstractIntcodeComputer()
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

  public class IntcodeComputer : AbstractIntcodeComputer
  {
    public IntcodeComputer(int[] program) : base(program) { }

    public override int ReadFromInput()
    {
      return Input.Dequeue();
    }
    public override void WriteToOutput(int val)
    {
      Output.Enqueue(val);
    }
    public Queue<int> Input = new Queue<int>();
    public Queue<int> Output = new Queue<int>();
  }

  public interface Operation
  {
    void Execute(AbstractIntcodeComputer computer, int[] args);
    int Length { get; }
  }

  public class OperationBinary : Operation
  {
    public OperationBinary(Func<int,int,int> o) { op = o; }
    Func<int, int, int> op;
    public void Execute(AbstractIntcodeComputer computer, int[] args)
      => computer.Program[args[2]] = op(computer.Program[args[0]], computer.Program[args[1]]);
    public int Length => 4;
  }

  public class OperationInput : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, int[] args) => computer.Program[args[0]] = computer.ReadFromInput();
    public int Length => 2;
  }

  public class OperationOutput : Operation
  {
    public void Execute(AbstractIntcodeComputer computer, int[] args) => computer.WriteToOutput(computer.Program[args[0]]);
    public int Length => 2;
  }

  public class OperationJumpIf : Operation
  {
    public OperationJumpIf(bool c) { condition = c; }
    bool condition;
    public void Execute(AbstractIntcodeComputer computer, int[] args)
    {
      if ((computer.Program[args[0]] == 0) != condition) computer.Counter = computer.Program[args[1]];
    }
    public int Length => 3;
  }

  public class PhaseSettings
  {
    public PhaseSettings(int value) => this.value = value;
    int value;
    public int ForAmplifier(int index) => (value/powers[index])%10;
    public static int[] powers = new int[] {10000, 1000, 100, 10, 1};
    public static implicit operator PhaseSettings(int v) => new PhaseSettings(v);
    public static implicit operator int(PhaseSettings ps) => ps.value;
    public static IEnumerable<PhaseSettings> AllCombinations(Func<char,bool> condition)
      => from i in Enumerable.Range(0,100000)
          let s = i.ToString("00000")
          where s.All(c => condition(c))
          where s.GroupBy(c => c).Count() == 5
          select new PhaseSettings(i);
  }

  public class Amplifier : AbstractIntcodeComputer
  {
    public Amplifier(int[] program, int phaseSetting)
      : base((int[])program.Clone()) => Input.Enqueue(phaseSetting);
    public Queue<int> Input = new Queue<int>();
    public Amplifier Next;
    public bool WaitingForNextAmplifier = false;
    public override int ReadFromInput() => Input.Dequeue();
    public override void WriteToOutput(int val)
    {
      Next.Input.Enqueue(val);
      WaitingForNextAmplifier = true;
    }
    public void RunUntilHaltOrOutputToNextAmplifier()
    {
      while (!HasHalted && !WaitingForNextAmplifier) RunOne();
    }
  }

  public abstract class AmplifierSeries
  {
    public AmplifierSeries(int[] program) => this.program = program;
    int[] program;

    public int ComputeOutputSignal(PhaseSettings phaseSettings)
    {
      var amplifiers = Enumerable
        .Range(0,5)
        .Select(i => new Amplifier(program,phaseSettings.ForAmplifier(i)))
        .ToArray();
      var first = amplifiers.First();
      var last = amplifiers.Last();
      amplifiers.Zip(amplifiers.Skip(1).Append(first)).ToList().ForEach(p => p.First.Next = p.Second);
      first.Input.Enqueue(0);

      var current = first;
      while(!last.HasHalted)
      {
        current.RunUntilHaltOrOutputToNextAmplifier();
        current.WaitingForNextAmplifier = false;
        current = current.Next;
      }
      return first.Input.Dequeue();
    }

    public PhaseSettings FindSettingsForMaxOutputSignal()
    {
      var tries = from setting in PhaseSettings.AllCombinations(IsValidSetting)
                  orderby ComputeOutputSignal(setting) descending
                  select setting;
      return tries.First();
    }

    public int FindMaxOutputSignal() => ComputeOutputSignal(FindSettingsForMaxOutputSignal());

    public abstract bool IsValidSetting(char c);
  }

  public class SimpleAmplifierSeries : AmplifierSeries
  {
    public SimpleAmplifierSeries(int[] program) : base(program) {}
    public override bool IsValidSetting(char c) => c<='4';
  }

  public class LoopingAmplifierSeries : AmplifierSeries
  {
    public LoopingAmplifierSeries(int[] program) : base(program) {}
    public override bool IsValidSetting(char c) => c>='5';
  }
}
