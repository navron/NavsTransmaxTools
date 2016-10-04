using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeProjectFixer.VisualStudioFile
{

    static class VisualStudioFileHelper
    {
        public static bool IncludeFile(string file)
        {
            // Don't want Unit Test in the list. Assume that these are ok
            if (file.Contains(@"\UnitTest")) return false;  // UnitTests and UnitTestSupport Folders

            // Don't want Test in the list. Assume that these are ok
            if (file.Contains(@"\test\")) return false;

            // Remove 3rdparty Project Files. On build machines the 3rdparty lib are check out to $src\lib\3rdparty and thus pick up
            if (file.Contains(@"\3rdparty\")) return false;

            var excludedlist = new List<string>
            {
                "Tsd.Libraries.Common.Eventing", // In code but not in build system, Need to ask about this 
                "ManagementConsole" // Jono special, Its missing a number of files. Needs work, Not in build.
            };
            // Exclude any known problems
            if (excludedlist.Any(file.Contains)) return false;

            return true;
        }
    }

}
