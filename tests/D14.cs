namespace tests14
{
  using NUnit.Framework;
  using NFluent;
  using System;
  using System.IO;
  using src14;
  using System.Collections.Generic;

  public class Tests
  {
    [Test]
    public void Definition1()
    {
      var r = new Reaction("10 ORE => 10 A");
      Check.That(r.Output).IsEqualTo("A");
      Check.That(r.Quantity).IsEqualTo(10);
      Check.That(r.Inputs).ContainsExactly(new Dictionary<string, long>{{"ORE",10}});
    }
    
    [Test]
    public void Definition2()
    {
      var r = new Reaction("1 ORE => 1 B");
      Check.That(r.Output).IsEqualTo("B");
      Check.That(r.Quantity).IsEqualTo(1);
      Check.That(r.Inputs).ContainsExactly(new Dictionary<string, long>{{"ORE",1}});;
    }
    
    [Test]
    public void Definition3()
    {
      var r = new Reaction("7 A, 1 B => 3 C");
      Check.That(r.Output).IsEqualTo("C");
      Check.That(r.Quantity).IsEqualTo(3);
      Check.That(r.Inputs).ContainsExactly(new Dictionary<string, long>{{"A",7},{"B",1}});
    }

    [Test]
    public void Example1()
    {
        var f = new Factory(new string[] {
            "10 ORE => 10 A",
            "1 ORE => 1 B",
            "7 A, 1 B => 1 C",
            "7 A, 1 C => 1 D",
            "7 A, 1 D => 1 E",
            "7 A, 1 E => 1 FUEL"
        });
      Check.That(f.Reactions.Count).IsEqualTo(6);
      Check.That(f.OreForSingleFuel()).IsEqualTo(31);
    }

    [Test]
    public void Example2()
    {
        var f = new Factory(new string[] {
            "9 ORE => 2 A",
            "8 ORE => 3 B",
            "7 ORE => 5 C",
            "3 A, 4 B => 1 AB",
            "5 B, 7 C => 1 BC",
            "4 C, 1 A => 1 CA",
            "2 AB, 3 BC, 4 CA => 1 FUEL"
        });
      Check.That(f.Reactions.Count).IsEqualTo(7);
      Check.That(f.OreForSingleFuel()).IsEqualTo(165);
    }

    [Test]
    public void Example3()
    {
        var f = new Factory(new string[] {
            "157 ORE => 5 NZVS",
            "165 ORE => 6 DCFZ",
            "44 XJWVT, 5 KHKGT, 1 QDVJ, 29 NZVS, 9 GPVTF, 48 HKGWZ => 1 FUEL",
            "12 HKGWZ, 1 GPVTF, 8 PSHF => 9 QDVJ",
            "179 ORE => 7 PSHF",
            "177 ORE => 5 HKGWZ",
            "7 DCFZ, 7 PSHF => 2 XJWVT",
            "165 ORE => 2 GPVTF",
            "3 DCFZ, 7 NZVS, 5 HKGWZ, 10 PSHF => 8 KHKGT"
        });
      Check.That(f.OreForSingleFuel()).IsEqualTo(13312);
      Check.That(f.FuelWithTrillionOre()).IsEqualTo(82892753);
    }

    [Test]
    public void Example4()
    {
        var f = new Factory(new string[] {
            "2 VPVL, 7 FWMGM, 2 CXFTF, 11 MNCFX => 1 STKFG",
            "17 NVRVD, 3 JNWZP => 8 VPVL",
            "53 STKFG, 6 MNCFX, 46 VJHF, 81 HVMC, 68 CXFTF, 25 GNMV => 1 FUEL",
            "22 VJHF, 37 MNCFX => 5 FWMGM",
            "139 ORE => 4 NVRVD",
            "144 ORE => 7 JNWZP",
            "5 MNCFX, 7 RFSQX, 2 FWMGM, 2 VPVL, 19 CXFTF => 3 HVMC",
            "5 VJHF, 7 MNCFX, 9 VPVL, 37 CXFTF => 6 GNMV",
            "145 ORE => 6 MNCFX",
            "1 NVRVD => 8 CXFTF",
            "1 VJHF, 6 MNCFX => 4 RFSQX",
            "176 ORE => 6 VJHF"
        });
      Check.That(f.OreForSingleFuel()).IsEqualTo(180697);
      Check.That(f.FuelWithTrillionOre()).IsEqualTo(5586022);
    }

    [Test]
    public void Example5()
    {
        var f = new Factory(new string[] {
            "171 ORE => 8 CNZTR",
            "7 ZLQW, 3 BMBT, 9 XCVML, 26 XMNCP, 1 WPTQ, 2 MZWV, 1 RJRHP => 4 PLWSL",
            "114 ORE => 4 BHXH",
            "14 VRPVC => 6 BMBT",
            "6 BHXH, 18 KTJDG, 12 WPTQ, 7 PLWSL, 31 FHTLT, 37 ZDVW => 1 FUEL",
            "6 WPTQ, 2 BMBT, 8 ZLQW, 18 KTJDG, 1 XMNCP, 6 MZWV, 1 RJRHP => 6 FHTLT",
            "15 XDBXC, 2 LTCX, 1 VRPVC => 6 ZLQW",
            "13 WPTQ, 10 LTCX, 3 RJRHP, 14 XMNCP, 2 MZWV, 1 ZLQW => 1 ZDVW",
            "5 BMBT => 4 WPTQ",
            "189 ORE => 9 KTJDG",
            "1 MZWV, 17 XDBXC, 3 XCVML => 2 XMNCP",
            "12 VRPVC, 27 CNZTR => 2 XDBXC",
            "15 KTJDG, 12 BHXH => 5 XCVML",
            "3 BHXH, 2 VRPVC => 7 MZWV",
            "121 ORE => 7 VRPVC",
            "7 XCVML => 6 RJRHP",
            "5 BHXH, 4 VRPVC => 5 LTCX"
        });
      Check.That(f.OreForSingleFuel()).IsEqualTo(2210736);
      Check.That(f.FuelWithTrillionOre()).IsEqualTo(460664);
    }

    [Test]
    public void Part1And2()
    {
      var f = new Factory(File.ReadAllLines("D14.txt"));
      Check.That(f.OreForSingleFuel()).IsEqualTo(720484);
      Check.That(f.FuelWithTrillionOre()).IsEqualTo(1993284);
    }
  }
}