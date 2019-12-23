namespace tests22
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using src22;
  using System.Collections.Generic;
  using System.Linq;
  using System.Numerics;

  public class Tests
  {
    IEnumerable<int> example = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    [Test]
    public void CheckDealIntoNewStack()
    {
      var deck = new Deck(10, "deal into new stack");
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 });
      Check.That(deck.PositionOf(7)).IsEqualTo(2);
      Check.That(deck.AtPosition(7)).IsEqualTo(2);
    }

    [TestCase("3", new int[] { 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 }, 8, 4, 4, 0)]
    [TestCase("-4", new int[] { 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 }, 5, 1, 7, 3)]
    public void CheckCutNCards(string n, int[] expectedFullDeal,
        int expectedPositionOf1, int expectedPositionOf7,
        int expectedAtPosition1, int expectedAtPosition7
    )
    {
      var deck = new Deck(10, "cut " + n);
      Check.That(deck.FullDeal).ContainsExactly(expectedFullDeal);
      Check.That(deck.PositionOf(1)).IsEqualTo(expectedPositionOf1);
      Check.That(deck.PositionOf(7)).IsEqualTo(expectedPositionOf7);
      Check.That(deck.AtPosition(1)).IsEqualTo(expectedAtPosition1);
      Check.That(deck.AtPosition(7)).IsEqualTo(expectedAtPosition7);
    }

    [Test]
    public void CheckDealWithIncrement()
    {
      var deck = new Deck(10, "deal with increment 3");
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 0, 7, 4, 1, 8, 5, 2, 9, 6, 3 });
      Check.That(deck.PositionOf(7)).IsEqualTo(1);
      Check.That(deck.AtPosition(7)).IsEqualTo(9);
    }

    [Test]
    public void Example2()
    {
      var deck = new Deck(
          10,
          "deal with increment 7",
          "deal into new stack",
          "deal into new stack"
      );
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 0, 3, 6, 9, 2, 5, 8, 1, 4, 7 });
      Check.That(deck.PositionOf(7)).IsEqualTo(9);
      Check.That(deck.AtPosition(7)).IsEqualTo(1);
      Check.That(deck.Coefs).IsEqualTo((3L, 0L));
    }

    [Test]
    public void Example3()
    {
      var deck = new Deck(
          10,
          "cut 6",
          "deal with increment 7",
          "deal into new stack"
      );
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 3, 0, 7, 4, 1, 8, 5, 2, 9, 6 });
      Check.That(deck.PositionOf(7)).IsEqualTo(2);
      Check.That(deck.AtPosition(7)).IsEqualTo(2);
      Check.That(deck.Coefs).IsEqualTo((7L, 3L));
    }

    [Test]
    public void Example3Bis()
    {
      var deck = new Deck(
          10,
          "cut 3",
          "deal with increment 3"
      );
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 3, 0, 7, 4, 1, 8, 5, 2, 9, 6 });
      Check.That(deck.PositionOf(7)).IsEqualTo(2);
      Check.That(deck.AtPosition(7)).IsEqualTo(2);
      Check.That(deck.Coefs).IsEqualTo((7L, 3L));
    }

    [Test]
    public void Example3Ter()
    {
      var deck = new Deck(
          10,
          "deal with increment 3",
          "cut -1"
      );
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 3, 0, 7, 4, 1, 8, 5, 2, 9, 6 });
      Check.That(deck.PositionOf(7)).IsEqualTo(2);
      Check.That(deck.AtPosition(7)).IsEqualTo(2);
      Check.That(deck.Coefs).IsEqualTo((7L, 3L));
    }

    [Test]
    public void Example4()
    {
      var deck = new Deck(
          10,
          "deal with increment 7",
          "deal with increment 9",
          "cut -2"
      );
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 6, 3, 0, 7, 4, 1, 8, 5, 2, 9 });
      Check.That(deck.PositionOf(7)).IsEqualTo(3);
      Check.That(deck.AtPosition(7)).IsEqualTo(5);
      Check.That(deck.Coefs).IsEqualTo((7L, 6L));
    }

    [Test]
    public void Example4Bis()
    {
      var deck = new Deck(
          10,
          "deal with increment 3",
          "cut -2"
      );
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 6, 3, 0, 7, 4, 1, 8, 5, 2, 9 });
      Check.That(deck.PositionOf(7)).IsEqualTo(3);
      Check.That(deck.AtPosition(7)).IsEqualTo(5);
      Check.That(deck.Coefs).IsEqualTo((7L, 6L));
    }

    [Test]
    public void Example5()
    {
      var deck = new Deck(
        10,
        "deal into new stack",
        "cut -2",
        "deal with increment 7",
        "cut 8",
        "cut -4",
        "deal with increment 7",
        "cut 3",
        "deal with increment 9",
        "deal with increment 3",
        "cut -1"
      );
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 9, 2, 5, 8, 1, 4, 7, 0, 3, 6 });
      Check.That(deck.PositionOf(7)).IsEqualTo(6);
      Check.That(deck.AtPosition(7)).IsEqualTo(0);
      Check.That(deck.Coefs).IsEqualTo((3L, 9L));
    }

    [Test]
    public void Example5Bis()
    {
      var deck = new Deck(10, "deal into new stack", "deal with increment 3");
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 9, 2, 5, 8, 1, 4, 7, 0, 3, 6 });
      Check.That(deck.PositionOf(7)).IsEqualTo(6);
      Check.That(deck.AtPosition(7)).IsEqualTo(0);
      Check.That(deck.Coefs).IsEqualTo((3L, 9L));
    }

    [Test]
    public void Example5Ter()
    {
      var deck = new Deck(10, "deal with increment 7", "cut 3");
      Check.That(deck.FullDeal).ContainsExactly(new int[] { 9, 2, 5, 8, 1, 4, 7, 0, 3, 6 });
      Check.That(deck.PositionOf(7)).IsEqualTo(6);
      Check.That(deck.AtPosition(7)).IsEqualTo(0);
      Check.That(deck.Coefs).IsEqualTo((3L, 9L));
    }

    [Test]
    public void Table()
    {
      for (int i = 0; i < 10; i++)
        System.Console.WriteLine("inc=" + i + " / ea=" + Arithmetic.ExtendedEuclidean(10, i));
    }

    [Test]
    public void Part1()
    {
      var deck = new Deck(10007, File.ReadAllLines("D22.txt"));
      Check.That(deck.PositionOf(2019)).IsEqualTo(1498);
      Check.That(deck.AtPosition(1498)).IsEqualTo(2019);
      Check.That(deck.Coefs).IsEqualTo((9646L, 2419L));
      var (a, b) = deck.Coefs;
      Check.That((a * 1498L + b) % deck.Size).IsEqualTo(2019);
    }

    [Test]
    public void CheckDealIntoNewStackBigValues()
    {
      var deck = new Deck(119315717514047, "deal into new stack");
      Check.That(deck.PositionOf(119315717514046)).IsEqualTo(0);
      Check.That(deck.PositionOf(0)).IsEqualTo(119315717514046);
      Check.That(deck.AtPosition(0)).IsEqualTo(119315717514046);
      Check.That(deck.AtPosition(119315717514046)).IsEqualTo(0);
    }

    [TestCase("3", 119315717514045, 4, 4, 0)]
    [TestCase("-4", 5, 11, 119315717514044, 119315717514040)]
    public void CheckCutNCardsBigValues(string n,
        long expectedPositionOf1, long expectedPositionOf7,
        long expectedAtPosition1, long expectedAtPosition119315717514044
    )
    {
      var deck = new Deck(119315717514047, "cut " + n);
      Check.That(deck.PositionOf(1)).IsEqualTo(expectedPositionOf1);
      Check.That(deck.PositionOf(7)).IsEqualTo(expectedPositionOf7);
      Check.That(deck.AtPosition(1)).IsEqualTo(expectedAtPosition1);
      Check.That(deck.AtPosition(119315717514044)).IsEqualTo(expectedAtPosition119315717514044);
    }

    [Test]
    public void CheckDealWithIncrementBigValues()
    {
      var deck = new Deck(119315717514047, "deal with increment 3");
      Check.That(deck.PositionOf(0)).IsEqualTo(0);
      Check.That(deck.PositionOf(39771905838016)).IsEqualTo(1);
      Check.That(deck.PositionOf(79543811676032)).IsEqualTo(2);
      Check.That(deck.PositionOf(1)).IsEqualTo(3);
      Check.That(deck.PositionOf(39771905838017)).IsEqualTo(4);
      Check.That(deck.PositionOf(79543811676033)).IsEqualTo(5);
      Check.That(deck.PositionOf(2)).IsEqualTo(6);
      Check.That(deck.PositionOf(39771905838018)).IsEqualTo(7);
      Check.That(deck.PositionOf(79543811676034)).IsEqualTo(8);
      Check.That(deck.PositionOf(7)).IsEqualTo(21);
      Check.That(deck.PositionOf(39771905838014)).IsEqualTo(119315717514042);
      Check.That(deck.PositionOf(79543811676030)).IsEqualTo(119315717514043);
      Check.That(deck.PositionOf(119315717514046)).IsEqualTo(119315717514044);
      Check.That(deck.PositionOf(39771905838015)).IsEqualTo(119315717514045);
      Check.That(deck.PositionOf(79543811676031)).IsEqualTo(119315717514046);
      //=====
      Check.That(deck.AtPosition(0)).IsEqualTo(0);
      Check.That(deck.AtPosition(1)).IsEqualTo(39771905838016);
      Check.That(deck.AtPosition(2)).IsEqualTo(79543811676032);
      Check.That(deck.AtPosition(3)).IsEqualTo(1);
      Check.That(deck.AtPosition(4)).IsEqualTo(39771905838017);
      Check.That(deck.AtPosition(5)).IsEqualTo(79543811676033);
      Check.That(deck.AtPosition(6)).IsEqualTo(2);
      Check.That(deck.AtPosition(7)).IsEqualTo(39771905838018);
      Check.That(deck.AtPosition(8)).IsEqualTo(79543811676034);
      Check.That(deck.AtPosition(119315717514042)).IsEqualTo(39771905838014);
      Check.That(deck.AtPosition(119315717514043)).IsEqualTo(79543811676030);
      Check.That(deck.AtPosition(119315717514044)).IsEqualTo(119315717514046);
      Check.That(deck.AtPosition(119315717514045)).IsEqualTo(39771905838015);
      Check.That(deck.AtPosition(119315717514046)).IsEqualTo(79543811676031);
    }

    [Test]
    public void Part2()
    {
      var modulo = 119315717514047;
      var rank = 101741582076661;
      var u0 = 2020;
      var deck = new Deck(modulo, File.ReadAllLines("D22.txt"));
      var (a, b) = deck.Coefs;
      // u(0) = u0
      // u(n+1) = (a.u(n)+b)%modulo
      // find u(rank)
      var p = BigInteger.ModPow(a, rank, modulo);
      var c = (long)(((a - 1) * p * u0 + b * (p - 1)) % modulo);
      Check.That((long)c).IsEqualTo(60394280843335);
      var (_,u,_) = Arithmetic.ExtendedEuclidean(a-1,modulo);
      var urank = modulo+Arithmetic.ModuloMultiply(u,c,modulo);
      Check.That(urank).IsEqualTo(74662303452927);
    }
  }
}