using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColorConsole;
using CommandLine;
using MakeProjectFixer.VisualStudioFile;

namespace MakeProjectFixer.Checkers
{
    [Verb("vsFixCaseReference", HelpText = "Fix Project Reference Case in Visual Studio Project files")]
    internal class VsFixCaseReference : Store
    {
   //     private List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; set; }

        public VsFixCaseReference()
        {
           // VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
        }

        public void Run()
        {
            Program.Console.WriteLine($"Running {this.GetType().Name}", ConsoleColor.Cyan);
            // Step 1 Read all Visual Studio Files 
          //  SearchPatterns = new[] { "*.csproj" };
            ////    // Find and limit return set to what is required
            //var files = Helper.FindFiles(this).Where(VisualStudioFileHelper.IncludeFile);

            //// Step 2 Scan All files and get TSD References 
            //Parallel.ForEach(files, (file) =>
            //{
            //    var vsFile = new VisualStudioFile.VisualStudioFile(file);
            //    vsFile.ScanFileForReferences();
            //    VisualStudioFiles.Add(vsFile);
            //});
            this.BuildStore();
          //  VisualStudioFiles = store.VisualStudioFiles;

            // Step 3 for each VS File check that the case of each Reference is correct
            foreach (VisualStudioFile.VisualStudioFile vsfile in VisualStudioFiles)
            {
                var updateDic = new Dictionary<string, string>(); //Old String, New String
                foreach (var tsdReference in vsfile.TsdReferences)
                {
                    var found = VisualStudioFiles.FirstOrDefault(vs => string.Equals(vs.AssemblyName, tsdReference, StringComparison.Ordinal));
                    if (found != null) continue;
                    var caseWrong = VisualStudioFiles.FirstOrDefault(vs => string.Equals(vs.AssemblyName, tsdReference, StringComparison.OrdinalIgnoreCase));
                    if (caseWrong != null)
                    {
                        updateDic.Add(tsdReference, caseWrong.AssemblyName);
                        continue;
                    }
                    // StaticData & WebProxies and StoredProcedures pass this point
                    //      Program.Console.WriteLine($"VisualStudioFile: {vsfile.ProjectName} Reference {tsdReference} does not exist", ConsoleColor.Yellow);
                }
                if (updateDic.Count > 0)
                {
                    Program.Console.WriteLine($"Updating {vsfile.ProjectName} for {updateDic.Count} references", ConsoleColor.Green);
                    var cs = new VsCsharp();
                    cs.UpdateCaseReference(vsfile.FileName, updateDic);
                }
            }
        }

    }
}

