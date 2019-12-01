namespace tests
{
    using NUnit.Framework;
    using NFluent;
    using System.IO;
    using src;

    public class Tests
    {
        [TestCase(12, 2)]
        [TestCase(14, 2)]
        [TestCase(1969, 654)]
        [TestCase(100756, 33583)]
        public void GetFuelForMass(int givenMass, int expectedFuel)
        {
            Check.That(D01.FuelForMass(givenMass)).IsEqualTo(expectedFuel);
        }
        
        [Test]
        public void GetSumOfFuelForMasses()
        {
            Check.That(
                new string[] {"12", "14", "1969"}.SumOf(D01.FuelForMass)
            ).IsEqualTo(658);
        }
        
        [Test]
        public void Part1()
        {
            Check.That(
                File.ReadAllLines("D01.txt").SumOf(D01.FuelForMass)
            ).IsEqualTo(3401852);
        }
    }
}