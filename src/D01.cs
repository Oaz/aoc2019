namespace src01
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reactive.Linq;

  public static class Code
  {
    public static int FuelForMass(int mass) => mass/3 - 2;
    public static int TotalFuelForMass(int mass) 
     => Observable.Generate(mass, m => m > 0, m => FuelForMass(m), m => m).ToEnumerable().Skip(1).Sum();

    public static int SumOf(this IEnumerable<string> input, Func<int,int> getValue)
      => input.Select(l => getValue(int.Parse(l))).Sum();

  }
}
