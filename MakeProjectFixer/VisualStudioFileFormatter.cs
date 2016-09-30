using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using MakeProjectFixer.VisualStudioFile;

namespace MakeProjectFixer
{
    [Verb("VSProjectCaseReferenceFixer", HelpText = "Fix Project Reference Case in Visual Studio Project files")]
    internal class VisualStudioProjectReferenceCaseFixer : Options
    {
        public List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; private set; }

        public VisualStudioProjectReferenceCaseFixer()
        {
            VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
        }

        public void Run()
        {
            // Step 1 Read all Visual Studio Files 
            SearchPatterns = new[] { "*.csproj"};
            // Find and limit return set to what is required
            var files = Helper.FindFiles(this).Where(VisualStudioFileHelper.IncludeFile);

            // Step 2 Scan All files
            Parallel.ForEach(files, (file) =>
            {
                var vsFile = new VisualStudioFile.VisualStudioFile(file);
                vsFile.ScanFile();
                VisualStudioFiles.Add(vsFile);
            });

            foreach (VisualStudioFile.VisualStudioFile vsfile in VisualStudioFiles)
            {
               //sfile.
            }
            // 
        }
    }

    [Verb("MakeProjectCaseMakeVisualStudioProject", HelpText = "Set the  Project Reference Case in Visual Studio Project files")]
    internal class MakeProjectCaseMakeVisualStudioProject : Options
    {
        public List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; private set; }

        public MakeProjectCaseMakeVisualStudioProject()
        {
            VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
        }

        public void Run()
        {
            //// Step 1 Read all Visual Studio Files 
            //SearchPatterns = new[] { "*.csproj" };
            //// Find and limit return set to what is required
            //var files = Helper.FindFiles(this).Where(VisualStudioFileHelper.IncludeFile);

            //// Step 2 Scan All files
            //Parallel.ForEach(files, (file) =>
            //{
            //    var vsFile = new VisualStudioFile.VisualStudioFile(file);
            //    vsFile.ScanFile();
            //    VisualStudioFiles.Add(vsFile);
            //});

            // 
        }
    }
}
