using System.Linq;
using NUnit.Framework;

namespace ProjectFixer.Tests
{
    [TestFixture]
    public class TestMakeFile
    {
        [TestCase(new[] { "test_one: one two", "ThirdLine" }, new[] { "test_one: one two", "ThirdLine"})]
        [TestCase(new[] { "test_one: one two \\", "\tThirdLine" }, new[] { "test_one: one two \tThirdLine" })]
        public void TestCombineLines(string[] given, string[] expected)
        {
            //     var s = new string[] { "#comment", "test_one: one two \\", "\tthree", "\t\tSecondLine" };
            var make = new MakeFile();
            var result = make.CombineLines(given);
            Assert.AreEqual(expected.Length, result.Count);
            foreach (var line in expected)
            {
                Assert.Contains(line, result);
            }
        }

        [TestCase(new[] { "test_one: one two", "ThirdLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "ThirdLine" })]
        [TestCase(new[] { "test_one: one two", "", "ThirdLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "", "ThirdLine" })]
        [TestCase(new[] { "test_one: one two", "", "\tThirdLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "", "\tThirdLine" })]
        [TestCase(new[] { "test_one: one two", "", "\tThirdLine", "FourLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "", "\tThirdLine" })]
        [TestCase(new[] { "#Comment", "test_one: one two", "", "\tThirdLine" }, "test_one", new[] { "one", "two" }, new[] { "#Comment" }, new[] { "", "\tThirdLine" })]
        public void TestProcessRawHeader(string[] given, string projectName, string[] projects, string[] preLines, string[] postLines)
        {
            var make = new MakeFile();
            var result = make.ProcessRawHeader(given);

            Assert.AreEqual(projectName, result.ProjectName);
            Assert.AreEqual(projects.Length, result.DependencyProjects.Count);
            foreach (var line in projects)
            {
                Assert.Contains(line, result.DependencyProjects);
            }
            Assert.AreEqual(preLines.Length, result.PreLines.Count);
            foreach (var line in preLines)
            {
                Assert.Contains(line, result.PreLines);
            }
            Assert.AreEqual(postLines.Length, result.PostLines.Count);
            foreach (var line in postLines)
            {
                Assert.Contains(line, result.PostLines);
            }
        }

        [TestCase(new[] { "test_one: one two", "", "ThirdLine" }, 2)]
        [TestCase(new[] { "test_one: one two", "", "\tThirdLine" }, 3)]
        [TestCase(new[] { "test_one: one two", "", "\tThirdLine", "FourLine" }, 3)]
        [TestCase(new[] { "#Comment", "test_one: one two", "", "\tThirdLine", "FourLine" }, 4)]
        public void TestGetNextProjectLines(string[] given, int expected)
        {
            var make = new MakeFile();
            var lines = make.GetRawProjectLines(given);
            Assert.AreEqual(expected, lines.Count);
        }

        [TestCase("ProjectFixer.Tests.TestSmallValid1Simple.mak", "test_one", 2)]
        [TestCase("ProjectFixer.Tests.TestSmallValid2Complex.mak", "core_one", 2)]
        public void TestProcessMakeFileRawLines(string givenResource, string headerName, int projects)
        {
            var make = new MakeFile();
            var makeFileRawLines = TestHelper.GetLinesFromResource(givenResource);
            make.ProcessMakeFileRawLines(makeFileRawLines.ToList());

            Assert.AreEqual(headerName, make.Header.ProjectName);
            Assert.AreEqual(projects, make.Projects.Count);
            // This test not be correct
            Assert.AreEqual(make.Projects.Count, make.Header.DependencyProjects.Count);
        }
    }
}
