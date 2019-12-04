namespace tests04
{
    using NUnit.Framework;
    using NFluent;
    using src04;

    public class Tests
    {
        [TestCase("123456",false)]
        [TestCase("113456",true)]
        [TestCase("122456",true)]
        [TestCase("123356",true)]
        [TestCase("123446",true)]
        [TestCase("123455",true)]
        public void HasTwoAdjacentDigits(string number, bool expectedResult)
        {
            Check.That(Code.HasTwoAdjacentIdenticalChars(number)).IsEqualTo(expectedResult);
        }

        [TestCase("123456",true)]
        [TestCase("113456",true)]
        [TestCase("122456",true)]
        [TestCase("123356",true)]
        [TestCase("123446",true)]
        [TestCase("123455",true)]
        [TestCase("213456",false)]
        [TestCase("132456",false)]
        [TestCase("124356",false)]
        [TestCase("123546",false)]
        [TestCase("123465",false)]
        public void AlwaysIncrease(string number, bool expectedResult)
        {
            Check.That(Code.CharsAlwaysIncrease(number)).IsEqualTo(expectedResult);
        }

        [Test]
        public void Part1()
        {
            Check.That(Code.CountGoodPassword(231832,767346)).IsEqualTo(1330);
        }

    }
}