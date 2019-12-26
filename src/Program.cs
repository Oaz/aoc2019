namespace src
{
  using System;
  using System.IO;
  using System.Linq;

  class Program
  {
    static void Main(string[] args)
    {
      using var input = Console.OpenStandardInput();
      using var output = Console.OpenStandardOutput();
      Console.WriteLine("Hello World!");
      var asciiComputer = new src25.ASCIIComputer(
        src25.IntcodeProgram.Load(File.ReadAllText("D25.txt")),
        src25.LinqX.Generate(0, x=>input.ReadByte()).Skip(1).Where(b => b!=13).Select(b => (char)b),
        c => output.WriteByte((byte)c)
      );
      asciiComputer.Run();
    }
  }
}
