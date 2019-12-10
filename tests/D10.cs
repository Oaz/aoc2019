namespace tests10
{
    using NUnit.Framework;
    using NFluent;
    using System.IO;
    using System.Linq;
    using src10;

    public class Tests
    {
        [TestCase(3,2,6,0,3,-2)]
        [TestCase(6,0,3,2,-3,2)]
        [TestCase(6,0,1,0,-1,0)]
        public void SightVectors(int fx,int fy,int tx, int ty, int dx,int dy)
        {
            Check.That(Space.SightVector(Coords.At(fx,fy),Coords.At(tx,ty))).IsEqualTo(Coords.At(dx,dy));
        }     

        [Test]
        public void Best()
        {
            var lines=new string[] {
                "......#.#.",
                "#..#.#....",
                "..#######.",
                ".#.#.###..",
                ".#..#.....",
                "..#....#.#",
                "#..#....#.",
                ".##.#..###",
                "##...#..#.",
                ".#....####"};
            Check.That(new Space(lines).Best().Item1).IsEqualTo(Coords.At(5,8));
        }

        [Test]
        public void BestOnLargeExample()
        {
            Check.That(new Space(largeExample).Best().Item1).IsEqualTo(Coords.At(11,13));
        }

        [Test]
        public void Find200OnLargeExample()
        {
            var kills = new Space(largeExample).KillsFrom(Coords.At(11,13)).Take(200).ToList();
            Check.That(kills.Take(3)).ContainsExactly(new Coords[] {Coords.At(11,12),Coords.At(12,1),Coords.At(12,2)});
            Check.That(kills.Last()).IsEqualTo(Coords.At(8,2));
        }
        
        [Test]
        public void Part1()
        {
            var space = new Space(File.ReadAllLines("D10.txt"));
            var best = space.Best();
            Check.That(best.Item1).IsEqualTo(Coords.At(11,19));
            Check.That(best.Item2).IsEqualTo(253);
        }

        [Test]
        public void Part2()
        {
            var space = new Space(File.ReadAllLines("D10.txt"));
            Check.That(space.KillsFrom(Coords.At(11,19)).Take(200).Last()).IsEqualTo(Coords.At(8,15));
        }

        string[] largeExample =new string[] {
            ".#..##.###...#######",
            "##.############..##.",
            ".#.######.########.#",
            ".###.#######.####.#.",
            "#####.##.#.##.###.##",
            "..#####..#.#########",
            "####################",
            "#.####....###.#.#.##",
            "##.#################",
            "#####.##.###..####..",
            "..######..##.#######",
            "####.##.####...##..#",
            ".#####..#.######.###",
            "##...#.##########...",
            "#.##########.#######",
            ".####.#.###.###.#.##",
            "....##.##.###..#####",
            ".#.#.###########.###",
            "#.#.#.#####.####.###",
            "###.##.####.##.#..##"
            };
    }
}