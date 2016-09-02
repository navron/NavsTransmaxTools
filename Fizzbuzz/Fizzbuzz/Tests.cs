using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Fizzbuzz
{
    [TestFixture]
    class Tests
    {
        [TestCase(2,6, 5)]
        public void TestFizzBuzzRange(int givenstart, int givenEnd, int expectedCount)
        {
            var actual = Program.FizzBuzz(givenstart,givenEnd); 
            Assert.AreEqual(expectedCount, actual.ToList().Count);
        }

        [TestCase(1, "1")]
        [TestCase(3, "Fizz")]
        [TestCase(5, "Buzz")]
        [TestCase(15, "FizzBuzz")]
        public void TestFizzBuzzContents(int given, string expected)
        {
            var actual = Program.FizzBuzzContent(given);
            Assert.AreEqual(expected, actual);
        }

    }
}
