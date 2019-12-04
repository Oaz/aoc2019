namespace src04
{
  using System;
  using System.Linq;
  using System.Collections.Generic;

  public static class Code
  {
    public static bool HasTwoAdjacentIdenticalChars(string s)
     => Enumerable.Range(0,s.Length-1).Select(i => s[i]==s[i+1]).FirstOrDefault(x => x);
    public static bool CharsAlwaysIncrease(string s)
     => Enumerable.Range(0,s.Length-1).Select(i => s[i+1]-s[i]).All(x => x>=0);
    public static bool IsGoodPassword(string s)
     => HasTwoAdjacentIdenticalChars(s) && CharsAlwaysIncrease(s);
    public static int CountPassword(int begin, int end, Func<string,bool> criteria)
     => Enumerable
          .Range(begin,end-begin+1)
          .Select(i => i.ToString())
          .Where(s => criteria(s))
          .Count();

    public static IEnumerable<int> SizesOfIdenticalGroups(string s)
      => s.Take(s.Length-1)
          .Zip(s.Skip(1))
          .Select(g => g.First == g.Second)
          .Aggregate(
            Enumerable.Repeat(0,1),
            (sizes,identicalChars) => identicalChars
              ? sizes.Skip(1).Prepend(sizes.First()+1)
              : sizes.Prepend(0)
          ).Where(size => size > 0)
          .Select(size => size+1)
          .Reverse();

    public static IList<T> ChangeHead<T>(this IList<T> l, Func<T,T> f)
    {
      l[0] = f(l[0]);
      return l;
    }

    public static bool HasAcceptableGroupSizes(string s) => SizesOfIdenticalGroups(s).Contains(2);

    public static bool IsReallyGoodPassword(string s)
     => HasAcceptableGroupSizes(s) && CharsAlwaysIncrease(s);
  }
}
