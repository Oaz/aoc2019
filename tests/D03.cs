namespace tests03
{
    using NUnit.Framework;
    using NFluent;
    using System.IO;
    using src03;

    public class Tests
    {
        [Test]
        public void CoordsForWire()
        {
            Check.That(Code.CoordsForWire("R5,U2,L3,D1")).ContainsExactly(
                new Coords[] {
                    Coords.At(1,0),
                    Coords.At(2,0),
                    Coords.At(3,0),
                    Coords.At(4,0),
                    Coords.At(5,0),
                    Coords.At(5,1),
                    Coords.At(5,2),
                    Coords.At(4,2),
                    Coords.At(3,2),
                    Coords.At(2,2),
                    Coords.At(2,1)
                }
            );
        }

        [Test]
        public void Intersections()
        {
            Check.That(Code.Intersections("R8,U5,L5,D3","U7,R6,D4,L4")).IsEquivalentTo(
                new Coords[] { Coords.At(3,3), Coords.At(6,5) }
            );
        }

        [TestCase("R8,U5,L5,D3","U7,R6,D4,L4",6)]
        [TestCase("R75,D30,R83,U83,L12,D49,R71,U7,L72","U62,R66,U55,R34,D71,R55,D58,R83",159)]
        [TestCase("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51","U98,R91,D20,R16,D67,R40,U7,R15,U6,R7",135)]
        public void ShortestCrossDistance(string wire1, string wire2, int expectedShortestDistance)
        {
            Check.That(Code.ShortestDistance(wire1,wire2)).IsEqualTo(expectedShortestDistance);
        }
        
        [Test]
        public void Part1()
        {
            var wires = File.ReadAllLines("D03.txt");
            Check.That(Code.ShortestDistance(wires[0],wires[1])).IsEqualTo(209);
        }

        [TestCase("R8,U5,L5,D3",3,3,20)]
        [TestCase("U7,R6,D4,L4",3,3,20)]
        [TestCase("R8,U5,L5,D3",6,5,15)]
        [TestCase("U7,R6,D4,L4",6,5,15)]
        public void StepsTo(string wire, int x, int y, int expectedSteps)
        {
            var coords = Code.CoordsForWire(wire);
            Check.That(coords.CountStepsTo(Coords.At(x,y))).IsEqualTo(expectedSteps);
        }

        [TestCase("R8,U5,L5,D3","U7,R6,D4,L4",30)]
        [TestCase("R75,D30,R83,U83,L12,D49,R71,U7,L72","U62,R66,U55,R34,D71,R55,D58,R83",610)]
        [TestCase("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51","U98,R91,D20,R16,D67,R40,U7,R15,U6,R7",410)]
        public void ShortestSteps(string wire1, string wire2, int expectedShortestSteps)
        {
            Check.That(Code.ShortestSteps(wire1,wire2)).IsEqualTo(expectedShortestSteps);
        }
        
        [Test]
        public void Part2()
        {
            var wires = File.ReadAllLines("D03.txt");
            Check.That(Code.ShortestSteps(wires[0],wires[1])).IsEqualTo(43258);
        }
    }
}