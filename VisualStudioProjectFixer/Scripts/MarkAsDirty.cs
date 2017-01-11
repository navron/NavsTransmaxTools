using System.Collections.Generic;
using System.Text;
using CommandLine;
using Microsoft.Build.Evaluation;
using Serilog;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("MarkAsDirty", HelpText = "Mark All Project files as Dirty so that all a saved")]
    public class MarkAsDirty
    {
        [Option('d', "dir", HelpText = "Source Root Folder")]
        public string RootFolder { get; set; }

        public void Run()
        {
            List<string> sourceFileList = Helper.GetProjectFiles(RootFolder, new[] { "*.csproj" });

            sourceFileList.ForEach(fileName =>
                {
                    var project = new Project(fileName);
                    project.MarkDirty();
                    project.Save(fileName, Encoding.UTF8);
                });
        }
    }
}