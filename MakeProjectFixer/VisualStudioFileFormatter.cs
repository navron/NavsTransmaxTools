using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using MakeProjectFixer.VisualStudioFile;

namespace MakeProjectFixer
{

    [Verb("MakeProjectCaseMatchVisualStudioProject", HelpText = "Set the Project Reference Case in Visual Studio Project files")]
    internal class MakeProjectCaseMatchVisualStudioProject : Options
    {
        public List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; private set; }

        public MakeProjectCaseMatchVisualStudioProject()
        {
            VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
        }

        public void Run()
        {
            Program.Console.WriteLine($"Running {this.GetType().Name}", ConsoleColor.Cyan);

            //// Step 1 Read all Visual Studio Files 
            //SearchPatterns = new[] { "*.csproj" };
            //// Find and limit return set to what is required
            //var files = Helper.FindFiles(this).Where(VisualStudioFileHelper.IncludeFile);

            //// Step 2 Scan All files
            //Parallel.ForEach(files, (file) =>
            //{
            //    var vsFile = new VisualStudioFile.VisualStudioFile(file);
            //    vsFile.ScanFileForReferences();
            //    VisualStudioFiles.Add(vsFile);
            //});

            // 
        }
    }
}
