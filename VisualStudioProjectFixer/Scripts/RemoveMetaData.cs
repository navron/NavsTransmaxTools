using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Evaluation;
using Serilog;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("RemoveMetaData", HelpText = "Remove Reference Meta Data like HintPath and TargetFramework")]
    public class RemoveMetaData
    {
        [Option('p', "parallel", HelpText = "Run in parallel mode")]
        public bool RunAsParallel { get; set; }

        [Option('f', "file", HelpText = "CS Project File")]
        public string FileName { get; set; }

        [Option('d', "dir", HelpText = "Root Folder")]
        public string RootFolder { get; set; }

        [Option('h', "HintPath", HelpText = "Remove Hint Page")]
        public bool RemoveHintPath { get; set; }

        [Option('t', "TargetFramework", HelpText = "Remove TargetFramework")]
        public bool RemoveTargetFramework { get; set; }

        [Option('s', "SpecificVersion", HelpText = "Remove SpecificVersion")]
        public bool RemoveSpecificVersion { get; set; }

        public void Run()
        {
            if (string.IsNullOrEmpty(FileName) && string.IsNullOrEmpty(RootFolder))
            {
                Log.Error("Need either a file Name or Folder ");
                Environment.Exit(-1);
            }
            var files = new List<string>();
            if (!string.IsNullOrEmpty(FileName))
            {
                files.Add(FileName);
            }
            if (!string.IsNullOrEmpty(RootFolder))
            {
                files = Helper.GetProjectFiles(RootFolder, new[] {@"*.csproj"});
            }

            if (RunAsParallel)
                Parallel.ForEach(files, ProcessFile);
            else
                files.ForEach(ProcessFile);
        }

        private void ProcessFile(string fileName)
        {
            if (!Helper.CheckCSharpeFile(fileName)) return;

            var project = new Project(fileName);
            var references = project.GetItems("Reference");
            Log.Information($"Processing: {fileName}");
            foreach (ProjectItem reference in references)
            {
                var include = reference.EvaluatedInclude;
                var split = include.Split(',');

                if (RemoveTargetFramework && reference.HasMetadata(@"RequiredTargetFramework"))
                    reference.RemoveMetadata(@"RequiredTargetFramework");

                // Remove All SpecificVersion and HintPath
                if (RemoveSpecificVersion && reference.HasMetadata(@"SpecificVersion") && AllowSpecificVersionRemoval(split[0]))
                    reference.RemoveMetadata(@"SpecificVersion");

                if (RemoveHintPath && reference.HasMetadata(@"HintPath"))
                    reference.RemoveMetadata(@"HintPath");

                // Remove Self, if Name is the same as the included then remove it
                if (reference.HasMetadata(@"Name"))
                {
                    var test = reference.GetMetadataValue(@"Name");
                    if (
                        string.Compare(test, 0, reference.EvaluatedInclude, 0, test.Length,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        reference.RemoveMetadata(@"Name");
                    }
                }

                if (split[0].Contains(@"Tsd."))
                {
                    // May do something if an TSD project
                }
            }
            if (!project.IsDirty) return;
            Log.Information($"Project Updated: {fileName}");
            project.Save();
        }

        private bool AllowSpecificVersionRemoval(string dllName)
        {
            // aspdu.net version is wrong, its one of ours but lacking correct version information 
            var dontAllowFor = new string[] { "aspdu.net" };
            return !dontAllowFor.Contains(dllName);
        }
    }
}
