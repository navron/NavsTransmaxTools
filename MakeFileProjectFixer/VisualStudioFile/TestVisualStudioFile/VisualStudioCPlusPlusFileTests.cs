using NUnit.Framework;

namespace MakeFileProjectFixer.VisualStudioFile.TestVisualStudioFile
{
    [TestFixture]
    public class VisualStudioCPlusPlusFileTests
    {

        [TestCase("#include myfile.h", "myfile.h")]
        [TestCase("#include myfile.h //junk", "myfile.h")]
        [TestCase("#include <myfile.h>", "myfile.h")]
        [TestCase("#include <myfile.h>//junk", "myfile.h")]
        [TestCase("  #include <myfile.h> //junk", "myfile.h")]
        public void GetHashIncludeTest(string given, string expected)
        {
            var actual = VisualStudioCPlusPlusFile.GetHashInclude(given);
            Assert.AreEqual(expected,actual);
        }

      
        [TestCase("#using <Tsd.MyFile.dll>", "Tsd.MyFile")]
        [TestCase("#using <Tsd.MyFile.dll> //junk", "Tsd.MyFile")]
        public void GetHashUsingTest(string given, string expected)
        {
            var actual = VisualStudioCPlusPlusFile.GetHashUsing(given);
            Assert.AreEqual(expected, actual);
        }
    }
}
