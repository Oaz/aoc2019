namespace tests02
{
    using NUnit.Framework;
    using NFluent;
    using System.IO;
    using System.Linq;
    using src02;

    public class Tests
    {
        [Test]
        public void AddOperation()
        {
            Check.That(
                new int[] {1,9,10,3,2,3,11,0,99,30,40,50}.RunAt(0)
            ).ContainsExactly(new int[] {1,9,10,70,2,3,11,0,99,30,40,50});
        }

        [Test]
        public void MulOperation()
        {
            Check.That(
                new int[] {1,9,10,70,2,3,11,0,99,30,40,50}.RunAt(4)
            ).ContainsExactly(new int[] {3500,9,10,70,2,3,11,0,99,30,40,50});
        }

        [Test]
        public void RunProgramUntilHalt()
        {
            Check.That(
                new int[] {1,9,10,3,2,3,11,0,99,30,40,50}.Run()
            ).ContainsExactly(new int[] {3500,9,10,70,2,3,11,0,99,30,40,50});
        }

        [TestCase(new int[] {1,0,0,0,99},new int[] {2,0,0,0,99})]
        [TestCase(new int[] {2,3,0,3,99},new int[] {2,3,0,6,99})]
        [TestCase(new int[] {2,4,4,5,99,0},new int[] {2,4,4,5,99,9801})]
        [TestCase(new int[] {1,1,1,4,99,5,6,0,99},new int[] {30,1,1,4,2,5,6,0,99})]
        public void RunSomeOtherPrograms(int[] input, int[] expectedOutput)
        {
            Check.That(input.Run()).ContainsExactly(expectedOutput);
        }
        
        [Test]
        public void Part1()
        {
            Check.That(MyProgram.RunWithInputs(12,2)[0]).IsEqualTo(3931283);
        }

        [Test]
        public void RunWithInputsDoesNotChangeTheInitialProgram()
        {
            var program = new int[] {1,0,0,0,99,7,8,9};
            Check.That(program.RunWithInputs(5,6)).ContainsExactly(new int[] {15,5,6,0,99,7,8,9});
            Check.That(program.RunWithInputs(6,7)).ContainsExactly(new int[] {17,6,7,0,99,7,8,9});
            Check.That(program).ContainsExactly(new int[] {1,0,0,0,99,7,8,9});
        }

        [TestCase(15,506)]
        [TestCase(17,607)]
        public void FindInputs(int outputToFind, int expectedInput)
        {
            Check.That(new int[] {1,0,0,0,99,7,8,9}.FindInputsFor(outputToFind)).IsEqualTo(expectedInput);
        }
        
        [Test]
        public void Part2()
        {
            Check.That(MyProgram.FindInputsFor(19690720)).IsEqualTo(6979);
        }

        public int[] MyProgram {
            get => File.ReadAllText("D02.txt").Split(',').Select(n => int.Parse(n)).ToArray();
        }
    }
}