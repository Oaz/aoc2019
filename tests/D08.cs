namespace tests08
{
    using NUnit.Framework;
    using NFluent;
    using System.IO;
    using System.Linq;
    using src08;

    public class Tests
    {
        
        [Test]
        public void Part1()
        {
            var layers = Layer.Read(File.ReadAllText("D08.txt"),25*6);
            var l = layers.OrderBy(l => l.NumberOf('0')).First();
            Check.That(
                l.NumberOf('1')*l.NumberOf('2')
            ).IsEqualTo(2176);
        }
        
        [Test]
        public void Stacking()
        {
            var layers = Layer.Read("0222112222120000",2*2);
            Check.That(layers.Stack().Data).ContainsExactly("0110");
        }    

        [Test]
        public void Part2()
        {
            var z = new int[]{5,6,7,8,9,10,11,12,13,14,15}.Chunks(3);
            var text = Layer.Read(File.ReadAllText("D08.txt"),25*6).Stack();
            text.SaveAsImage("/tmp/password.png",25,5);
            Check.That(text.Data).ContainsExactly("011001000110010111001000110010100011010010010100011000001010110001110001010100000010010100100100010010010001001010010010001000110000100100101110000100");
        }

    }
}