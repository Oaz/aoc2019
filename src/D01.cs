namespace src
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public static class D01
  {
    public static int FuelForMass(int mass) => mass/3 - 2;

    public static int SumOf(this IEnumerable<string> input, Func<int,int> getValue)
      => input.Select(l => getValue(int.Parse(l))).Sum();

  }
}
