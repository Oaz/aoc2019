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
            Check.That(Code.CountPassword(231832,767346,Code.IsGoodPassword)).IsEqualTo(1330);
        }

        [TestCase("123456",new int[] {})]
        [TestCase("113456",new int[] {2})]
        [TestCase("112233",new int[] {2,2,2})]
        [TestCase("123444",new int[] {3})]
        [TestCase("111122",new int[] {4,2})]
        public void SizesOfIdenticalGroups(string number, int[] expectedResult)
        {
            Check.That(Code.SizesOfIdenticalGroups(number)).IsEqualTo(expectedResult);
        }

        [TestCase("123456",false)]
        [TestCase("113456",true)]
        [TestCase("112233",true)]
        [TestCase("123444",false)]
        [TestCase("111122",true)]
        public void HasAcceptableGroupSizes(string number, bool expectedResult)
        {
            Check.That(Code.HasAcceptableGroupSizes(number)).IsEqualTo(expectedResult);
        }

        [Test]
        public void Part2()
        {
            Check.That(Code.CountPassword(231832,767346,Code.IsReallyGoodPassword)).IsEqualTo(876);
        }
    }
}