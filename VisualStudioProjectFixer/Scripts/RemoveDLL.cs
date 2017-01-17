using System;
using CommandLine;
using Microsoft.Build.Evaluation;
using Serilog;

namespace VisualStudioProjectFixer.Scripts
{
    // System.Core was removed with .Net 4
    // nunit.core.extensions was removed with Nunit3

    [Verb("RemoveDLL", HelpText = "Remove an dll from all projects")]
    public class RemoveDLL : Options
    {
        [Option('d', "dir", HelpText = "Root Folder")]
        public string RootFolder { get; set; }

        [Option("dllname", HelpText = "dll file name (without extension")]
        public string DllName { get; set; }

        public void Run()
        {
            var files = Helper.GetProjectFiles(RootFolder, new[] { @"*.csproj" });
            foreach (var file in files)
            {
                var project = new Project(file);
                var references = project.GetItems("Reference");
                foreach (var reference in references)
                {
                    if (reference.Xml.Include.Contains(DllName))
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