namespace src16
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public static class Code
  {

    public static string EightFirstDigitsAfter100Phases(this string x)
    {
      return x.ReadInput().Phases(new int[] { 0, 1, 0, -1 }).Take(100).Last().Take(8).Out();
    }
    public static IEnumerable<IEnumerable<int>> Phases(this IEnumerable<int> input, IEnumerable<int> pattern)
    {
      var patterns = pattern.Patterns(input.Count());
      return Generate(input, x => ApplyPatterns(x)).Skip(1);
      IEnumerable<int> ApplyPatterns(IEnumerable<int> x) =>
        patterns.Select(p => Math.Abs(p.Zip(x).Select(x => x.First * x.Second).Sum() % 10)).ToArray();
    }
    public static IEnumerable<int>[] Patterns(this IEnumerable<int> pattern, int size) =>
      Enumerable.Range(1, size).Select(pos => pattern.Full(pos, size).ToArray()).ToArray();

    public static IEnumerable<int> Full(this IEnumerable<int> pattern, int position, int size) =>
      pattern.RepeatByPosition(position).RepeatInfinitely().Skip(1).Take(size);
    public static IEnumerable<T> RepeatInfinitely<T>(this IEnumerable<T> l) =>
      Generate(l, Identity).SelectMany(Identity);
    public static IEnumerable<int> RepeatByPosition(this IEnumerable<int> pattern, int position) =>
      pattern.SelectMany(x => Enumerable.Repeat(x, position));

    public static T Identity<T>(T x) => x;
    public static IEnumerable<T> Generate<T>(T x, Func<T, T> f)
    {
      for (; ; ) { yield return x; x = f(x); }
    }
    public static string Out(this IEnumerable<int> input) => string.Join("", input.Select(x => x.ToString()));
    public static IEnumerable<int> ReadInput(this string input) => input.Select(c => (int)c - (int)'0');
  }
}
