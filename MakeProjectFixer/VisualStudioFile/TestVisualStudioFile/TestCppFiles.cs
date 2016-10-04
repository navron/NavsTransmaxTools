using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MakeProjectFixer.VisualStudioFile.TestVisualStudioFile
{
    [TestFixture]
    public class TestCppFiles
    {

        [Test]
        public void ScanCppFile()
        {
            //const string file = @"C:\Dev\ac\as\acsvc\acsvc.vcxproj";
            const string file = @"C:\Dev\fr\as\frvwmmssvc\frvwmmssvc.vcxproj";
            var vscpp = new VsCplusplus();
            vscpp.ScanCppProjectForIncludeStatements(file);
        }

    }
}
