using System.Collections.Generic;
using System.Text;
using CommandLine;
using Microsoft.Build.Evaluation;
using Serilog;
using VisualStudioProjectFixer.Store;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("DetectLocalReference", HelpText = "Detects references that have been reference relative to current project")]
    public class DetectLocalReference : Options
    {
        public void Run()
        {
            List<string> sourceFileList = Helper.GetProjectFiles(RootFolder, new[] { "*.csproj" });

            Log.Debug($"Checking {sourceFileList.Count} files");

            if (RunAsParallel)
            {
                
            }
            else
            {
                sourceFileList.ForEach(HasLocalReference);
            }
        }

        private void HasLocalReference(string fileName)
        {
            if (fileName.Contains(".UnitTests")) return;

            if (!Helper.CheckCSharpeFile(fileName)) return;
            Log.Debug($"Checking: {fileName}");

            var project = new Project(fileName);
            var references = project.GetItems("ProjectReference");
            foreach (var reference in references)
            {
                Log.Warning("CS Project {project} has Project References {ref}, --Manually fix",fileName, reference.EvaluatedInclude);
            }
        }
    }
}