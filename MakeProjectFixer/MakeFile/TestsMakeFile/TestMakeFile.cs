using System.Linq;
using NUnit.Framework;

namespace MakeProjectFixer.MakeFile.TestsMakeFile
{
    [TestFixture]
    public class TestMakeFile
    {
        [TestCase(new[] { "test_one: one two", "ThirdLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "ThirdLine" })]
        [TestCase(new[] { "test_one: one two", "", "ThirdLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "", "ThirdLine" })]
        [TestCase(new[] { "test_one: one two", "", "\tThirdLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "", "\tThirdLine" })]
        [TestCase(new[] { "test_one: one two", "", "\tThirdLine", "FourLine" }, "test_one", new[] { "one", "two" }, new string[] { }, new[] { "", "\tThirdLine" })]
        [TestCase(new[] { "#Comment", "test_one: one two", "", "\tThirdLine" }, "test_one", new[] { "one", "two" }, new[] { "#Comment" }, new[] { "", "\tThirdLine" })]
        public void TestProcessRawProject(string[] given, string projectName, string[] projects, string[] preLines, string[] postLines)
        {
            var make = new MakeFile();
            var result = make.ProcessRawProject(given);

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

        [TestCase(new[] { "test_one: one two \\", "ThirdLine" }, new[] { "test_one: one two \\", "ThirdLine" })]
        //Correct don't strip off empty lines
        [TestCase(new[] { "#comment", "test_one: one two \\", "ThirdLine", "", "" }, new[] { "#comment", "test_one: one two \\", "ThirdLine", "", "" })]
        [TestCase(new[] { "#comment", "test_one: one two \\", "ThirdLine", "", "one: two" }, new[] { "#comment", "test_one: one two \\", "ThirdLine", "" })]
        [TestCase(new[] { "#comment", "test_one: one two \\", "ThirdLine", "", "#comment one", "one: two" }, new[] { "#comment", "test_one: one two \\", "ThirdLine", "" })]
        public void GetRawHeaderLines(string[] given, string[] expected)
        {
            var make = new MakeFile();
            var actual = make.GetRawHeaderLines(given);
            CollectionAssert.AreEquivalent(expected, actual);
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

        [TestCase("ProjectFixer.Tests.TestMake1Simple.mak", "test_one", 2)]
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

        [TestCase("ProjectFixer.Tests.TestProcessComplexRaw.mak",
                new[] { "#Comment header","head_one: one_lib two \\","\tthree","","head_one_path=head/one","head_one_prefix=Tsd.head.one","" },
                new[] {"#Comment two","two: three","\tdodo","" })]
        public void TestProcessMakeFileRawLinesComplex(string givenResource, string[] header, string[] project)
        {
            var given = TestHelper.GetLinesFromResource(givenResource);
            var make = new MakeFile();
            make.ProcessMakeFileRawLines(given.ToList());
            CollectionAssert.AreEquivalent(header, make.Header.RawLines);
            CollectionAssert.AreEquivalent(project, make.Projects[0].RawLines);
        }

        [TestCase(new[] { "one: two three" }, 20, false, new[] { "one: two three" })]
        [TestCase(new[] { "one: two three" }, 20, true, new[] { "one: three two" })]
        [TestCase(new[] { "one: two three" }, 8, true, new[] { "one: three \\", "\t\ttwo" })]
        [TestCase(new[] { "one: two three \\", "\t\tfour" }, 10, false, new[] { "one: two \\", "\t\tthree \\", "\t\tfour" })]
        [TestCase(new[] { "one: two three \\", "\t\tfour" }, 10, true, new[] { "one: four \\", "\t\tthree \\", "\t\ttwo" })]

        [TestCase(new[] { "#Comment", "one: two three", "Post Action" }, 8, true, new[] { "#Comment", "one: three \\", "\t\ttwo", "Post Action" })]
        [TestCase(new[] { "#Comment", "one: two three", "", "Post Action", "", }, 8, true, new[] { "#Comment", "one: three \\", "\t\ttwo", "", "Post Action" })]
        public void FormatMakeProject(string[] givenProject, int lineLength, bool sort, string[] expectedProject)
        {
            var make = new MakeFile();
            var c = make.ProcessRawProject(givenProject);
            var actual = c.FormatMakeProject(lineLength, sort);
            CollectionAssert.AreEquivalent(expectedProject, actual);
        }

        [TestCase("ProjectFixer.Tests.TestFormatRaw.mak", "ProjectFixer.Tests.TestFormatUnSorted.mak", 80, false)]
        [TestCase("ProjectFixer.Tests.TestFormatRaw.mak", "ProjectFixer.Tests.TestFormatUnSortedLine20.mak", 20, false)]
        [TestCase("ProjectFixer.Tests.TestFormatRaw.mak", "ProjectFixer.Tests.TestFormatSorted.mak", 80, true)]
        [TestCase("ProjectFixer.Tests.TestFormatRaw.mak", "ProjectFixer.Tests.TestFormatSortedLine20.mak", 20, true)]
        [TestCase("ProjectFixer.Tests.TestFormatComplexRaw.mak", "ProjectFixer.Tests.TestFormatComplexRawSortedLine20.mak", 20, true)]
        [TestCase("ProjectFixer.Tests.TestRealAS.mak", "ProjectFixer.Tests.TestRealASSortedLine80.mak", 80, true)]
        public void TestFormatFile(string givenResource, string expectedResource, int lineLength, bool sortProjects)
        {
            var given = TestHelper.GetLinesFromResource(givenResource);
            var expected = TestHelper.GetLinesFromResource(expectedResource);
            var make = new MakeFile();
            make.ProcessMakeFileRawLines(given.ToList());
            var actual = make.FormatFile(lineLength, sortProjects);
    //        SingleFile.WriteAllLines(@"c:\temp\out.mak",actual);
            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
