namespace tests06
{
  using NUnit.Framework;
  using NFluent;
  using System.IO;
  using System.Linq;
  using src06;

  public class Tests
  {
    [TestCase("D",new string[] {"COM","B","C"})]
    [TestCase("L",new string[] {"COM","B","C","D","E","J","K"})]
    [TestCase("COM",new string[] {})]
    public void FindChains(string startingObject, string[] expectedChain)
      => Check.That(new OrbitMap(example).ChainStartingFrom(startingObject))
              .ContainsExactly(expectedChain);
    
    [Test]
    public void CountAllOrbits() => Check.That(new OrbitMap(example).Count).IsEqualTo(42);
    
    [Test]
    public void Part1() => Check.That(MyOrbits.Count).IsEqualTo(402879);
    
    [Test]
    public void FromMeToSan()
    {
      var orbitMap = new OrbitMap(example.Append("K)YOU").Append("I)SAN"));
      Check.That(orbitMap.ClosestSharedObject("YOU","SAN")).IsEqualTo("D");
      Check.That(orbitMap.CountHopsFromTo("YOU","SAN")).IsEqualTo(4);
    }

    [Test]
    public void Part2() => Check.That(MyOrbits.CountHopsFromTo("YOU","SAN")).IsEqualTo(484);

    public OrbitMap MyOrbits
    {
      get => new OrbitMap(File.ReadAllLines("D06.txt").ToArray());
    }

    static string[] example = new string[] {
      "COM)B","B)C","C)D","D)E","E)F","B)G",
      "G)H","D)I","E)J","J)K","K)L"
    };
  }
}