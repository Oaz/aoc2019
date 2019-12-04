namespace src04
{
  using System.Linq;

  public static class Code
  {
    public static bool HasTwoAdjacentIdenticalChars(string s)
     => Enumerable.Range(0,s.Length-1).Select(i => s[i]==s[i+1]).FirstOrDefault(x => x);
    public static bool CharsAlwaysIncrease(string s)
     => Enumerable.Range(0,s.Length-1).Select(i => s[i+1]-s[i]).All(x => x>=0);
    public static bool IsGoodPassword(string s)
     => HasTwoAdjacentIdenticalChars(s) && CharsAlwaysIncrease(s);
    public static int CountGoodPassword(int begin, int end)
     => Enumerable
          .Range(begin,end-begin+1)
          .Select(i => i.ToString())
          .Where(s => IsGoodPassword(s))
          .Count();
  }
}
