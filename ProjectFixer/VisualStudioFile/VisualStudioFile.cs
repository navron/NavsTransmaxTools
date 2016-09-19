using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectFixer.VisualStudioFile
{
    class VisualStudioFile
    {
        public VisualStudioFile()
        {
            Projects = new List<VisualStudioProjectFile>();
        }

        // List of Visual Studio Project Files 
        public List<VisualStudioProjectFile> Projects { get; set; }

        public void BuildProjectFileList(Options options)
        {
            var files = Helper.FindFiles(options);
            BuildProjectFileList(files);
        }

        internal void BuildProjectFileList(List<string> files)
        {
            var excludedlist = new List<string>
            {
                "Tsd.Libraries.Common.Eventing", // In code but not in build system, Need to ask about this 
                "ManagementConsole" // Jono special, Its missing a number of files. Needs work, Not in build.
            };

            foreach (var file in files)
            {
                // Don't want Unit Test in the list. Assume that these are ok
                if (file.Contains(@"\UnitTest")) continue;  // UnitTests and UnitTestSupport Folders

                // Don't want Test in the list. Assume that these are ok
                if (file.Contains(@"\test\")) continue;

                // Remove 3rdparty Project Files. On build machines the 3rdparty lib are check out to $src\lib\3rdparty and thus pick up
                if (file.Contains(@"\3rdparty\")) continue;

                // Exclude any known problems
                if (excludedlist.Any(s => file.Contains(s))) continue;

                if (file.Contains(@"csproj"))
                {
                    Projects.Add(new CSharpProjectFile { FileName = file });
                }
                else if (file.Contains(@"vcxproj"))
                {
                    Projects.Add(new VisualCProjectFile { FileName = file });
                }
                else throw new Exception($"Unknown File {file}, in VisualStudioFile.ProcessFiles");
            }
        }

        public void ScanFiles(List<string> files)
        {
            // Want to do this in parallel

            foreach (var file in files)
            {
                
            }

            //if (file.Contains(@"csproj"))
            //{
            //    Projects.Add(new CSharpProjectFile { FileName = file });
            //}
            //else if (file.Contains(@"vcxproj"))
            //{
            //    Projects.Add(new VisualCProjectFile { FileName = file });
            //}
        }

        public void ScanCSharpProjectFile(CSharpProjectFile project)
        {

        }
        public void ScanVisualCProjectFile(VisualCProjectFile project)
        {

        }
    }
}
