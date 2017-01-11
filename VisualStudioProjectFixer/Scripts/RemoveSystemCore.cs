using System;
using CommandLine;
using Microsoft.Build.Evaluation;
using Serilog;

namespace VisualStudioProjectFixer.Scripts
{
    // TODO NOT TESTED, NOT ENABLE
    [Verb("RemoveMetaData", HelpText = "Remove Reference Meta Data like HintPath and TargetFramework")]
    public class RemoveSystemCore
    {
        [Option('d', "dir", HelpText = "Root Folder")]
        public string RootFolder { get; set; }

        public void Run()
        {
            var files = Helper.GetProjectFiles(RootFolder, new[] { @"*.csproj" });
            foreach (var file in files)
            {
                var project = new Project(file);
                var references = project.GetItems("Reference");
                foreach (var reference in references)
                {
                    if (reference.Xml.Include.Contains("System.Core"))
                    {
                        project.RemoveItem(reference);
                        Console.WriteLine($"{reference.Xml.Include} has been removed from {project.FullPath}");
                        break;
                    }
                }
                if (!project.IsDirty) continue;
                Log.Information($"Changed: {file}");
                project.Save();
            }
        }
    }
}