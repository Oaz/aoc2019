namespace src02
{
  using System;

  public static class Code
  {
    public static int[] Run(this int[] program)
    {
      var index = 0;
      while (program[index]!=99)
      {
        program.RunAt(index);
        index += 4;
      }
      return program;
    }

    public static int[] RunAt(this int[] program, int index)
    {
      var operation = (program[index]==1) ? (Func<int,int,int>) Add : Mul;
      program.Write(index+3, operation(program.Read(index+1), program.Read(index+2)));
      return program;
    }

    public static int Read(this int[] program, int index) => program[program[index]];
    public static void Write(this int[] program, int index, int value) => program[program[index]] = value;

    public static int Add(int a, int b) => a+b;
    public static int Mul(int a, int b) => a*b;


  }
}
