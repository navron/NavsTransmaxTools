using NUnit.Framework;

namespace MakeFileProjectFixer.MakeFile.TestsMakeFile
{
    [TestFixture]
    public class TestMakeFileHelper
    {
        [TestCase(new[] { "test_one: one two", "ThirdLine" }, new[] { "test_one: one two", "ThirdLine" })]
        [TestCase(new[] { "test_one: one two \\", "\tThirdLine" }, new[] { "test_one: one two \tThirdLine" })]
        public void TestCombineLines(string[] given, string[] expected)
        {
            var result = MakeFileHelper.CombineLines(given);
            Assert.AreEqual(expected.Length, result.Count);
            foreach (var line in expected)
            {
                Assert.Contains(line, result);
            }
        }

        [TestCase("one: two three", 20, new[] { "one: two three" })]
        [TestCase("one: two three", 8, new[] { "one: two \\", "\t\tthree" })]
        [TestCase("one: two three four", 12, new[] { "one: two three \\", "\t\tfour" })]
        public void TestExpandLines(string given, int length, string[] expected)
        {
            var acutal = MakeFileHelper.ExpandLines(given, length);

            CollectionAssert.AreEquivalent(expected, acutal);
        }

        [TestCase(new[] { "one:", "two" }, new[] { "one:", "two" })]
        [TestCase(new[] { "one:", "\ttwo" }, new[] { "one:", "two" })]
        [TestCase(new[] { "one:", "\ttwo", "" }, new[] { "one:", "two" })]
        [TestCase(new[] { "one:", "\t\t  ", "\ttwo", "" }, new[] { "one:", "two" })]
//Wrong Test        [TestCase(new[] { "one:", "\ttwo\t\tthree", "" }, new[] { "one:", "two", "three" })]
        public void TestCleanLines(string[] given, string[] expected)
        {
            var acutal = MakeFileHelper.CleanLines(given);
            CollectionAssert.AreEquivalent(expected, acutal);
        }
    }
}
