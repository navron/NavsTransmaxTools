using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MakeProjectFixer.VisualStudioFile.TestVisualStudioFile
{
    [TestFixture]
    public class TestCSFiles
    {

        [Test]
        public void ScanCSFile()
        {
            const string file = @"C:\Dev\ac\ws\AccessControl.Workstation\AccessControl.Workstation.csproj";
            var vscs = new VsCsharp();
            vscs.OpenProject(file);
            var ass = vscs.GetAssemblyName();

            //       vscpp.ScanCppProjectForIncludeStatements(file);
        }

    }
}
