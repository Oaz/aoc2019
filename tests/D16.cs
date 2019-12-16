namespace tests16
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using src16;
  using System.Collections.Generic;
  using System.Linq;

  public class Tests
  {
    [TestCase(new int[] { 3, 4, 5 }, 2, new int[] { 3, 3, 4, 4, 5, 5 })]
    [TestCase(new int[] { 0,1,0,-1 }, 3, new int[] { 0,0,0,1,1,1,0,0,0,-1,-1,-1 })]
    public void GetRepeatedPattern(IEnumerable<int> pattern, int position, IEnumerable<int> expected)
    {
      Check.That(pattern.RepeatByPosition(position)).ContainsExactly(expected);
    }

    [TestCase(new int[] { 3, 4, 5 }, new int[] { 3,4,5,3,4,5,3,4,5,3 })]
    public void GetInfiniteRepetition(IEnumerable<int> pattern, IEnumerable<int> expected)
    {
      Check.That(pattern.RepeatInfinitely().Take(10)).ContainsExactly(expected);
    }

    [TestCase(new int[] { 0,1,0,-1 }, 1, new int[] { 1,0,-1,0,1,0,-1,0 })]
    [TestCase(new int[] { 0,1,0,-1 }, 2, new int[] { 0,1,1,0,0,-1,-1,0 })]
    [TestCase(new int[] { 0,1,0,-1 }, 3, new int[] { 0,0,1,1,1,0,0,0 })]
    [TestCase(new int[] { 0,1,0,-1 }, 4, new int[] { 0,0,0,1,1,1,1,0 })]
    [TestCase(new int[] { 0,1,0,-1 }, 5, new int[] { 0,0,0,0,1,1,1,1 })]
    public void GetFullPattern(IEnumerable<int> pattern, int position, IEnumerable<int> expected)
    {
      Check.That(pattern.Full(position,8)).ContainsExactly(expected);
    }

    [TestCase(new int[] { 1,2,3,4,5,6,7,8 }, new int[] { 4,8,2,2,6,1,5,8 })]
    [TestCase(new int[] { 4,8,2,2,6,1,5,8 }, new int[] { 3,4,0,4,0,4,3,8 })]
    [TestCase(new int[] { 3,4,0,4,0,4,3,8 }, new int[] { 0,3,4,1,5,5,1,8 })]
    [TestCase(new int[] { 0,3,4,1,5,5,1,8 }, new int[] { 0,1,0,2,9,4,9,8 })]
    public void GetPhase(IEnumerable<int> input, IEnumerable<int> expected)
    {
      Check.That(input.Phases(new int[] { 0,1,0,-1 }).First()).ContainsExactly(expected);
    }

    [TestCase("80871224585914546619083218645595", "24176176")]
    [TestCase("19617804207202209144916044189917", "73745418")]
    [TestCase("69317163492948606335995924319873", "52432133")]
    public void After100Phases(string input, string expected)
    {
      Check.That(input.EightFirstDigitsAfter100Phases()).IsEqualTo(expected);
    }

    [TestCase("12345678", new int[] { 1,2,3,4,5,6,7,8 })]
    public void ReadInputs(string input, IEnumerable<int> expected)
    {
      Check.That(input.ReadInput()).ContainsExactly(expected);
    }

    [Test]
    public void Part1()
    {
      Check.That(
          File.ReadAllText("D16.txt").EightFirstDigitsAfter100Phases()
      ).IsEqualTo("10332447");
    }
  }
}