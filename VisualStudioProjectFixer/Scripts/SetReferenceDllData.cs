using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Evaluation;
using Serilog;
using VisualStudioProjectFixer.Store;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("SetReferenceDLLData", HelpText = "Set the Reference DLL strong name")]
    public class SetReferenceDllData
    {
        [Option('d', "dir", HelpText = "Source Root Folder")]
        public string RootFolder { get; set; }

        [Option('f', "file", HelpText = "CS Project File")]
        public string FileName { get; set; }

        [Option('a', "all", HelpText = "Check all references")]
        public bool CheckAllReferences { get; set; }

        [Option('p', "parallel", HelpText = "Run in parallel mode")]
        public bool RunAsParallel { get; set; }

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
                files = Helper.GetProjectFiles(RootFolder, new[] { @"*.csproj" });
            }

            if (RunAsParallel)
                Parallel.ForEach(files, file => ProcessFile(file, CheckAllReferences));
            else
                files.ForEach(file => ProcessFile(file, CheckAllReferences));
        }

        private void ProcessFile(string fileName, bool checkAllReferences)
        {
            if (!Helper.CheckCSharpeFile(fileName)) return;
            Log.Information($"Processing: {fileName}");

            var project = new Project(fileName);
            var references = project.GetItems("Reference");
            foreach (var reference in references)
            {
                var include = reference.EvaluatedInclude;
                var values = include.Split(',');
                if (values.Length == 1 || checkAllReferences)
                {
                    var dllName = values[0];
                    if (!UpdateThisDll(dllName))
                    {
                        Log.Debug($"SetReferenceDLLData Skipping {dllName}");
                        continue;
                    }
                    Log.Debug($"SetReferenceDLLData Fixing {dllName}");
                    var r = DllInformation.Instance.GetDllInfo(dllName);
                    if (!string.IsNullOrEmpty(r))
                    {
                        reference.UnevaluatedInclude = r;
                    }
                }
                else if (values.Length > 1)
                {
                    Log.Debug($"SetReferenceDLLData Not Touching {include}");
                }
            }

            if (!project.IsDirty) return;
            Log.Information($"Project Updated: {fileName}");
            project.Save();
        }

        // Should this DLL Be checked ?
        bool UpdateThisDll(string dllName)
        {
            // System Microsoft Files that are in the GAC
             if (dllName.Contains("System.")) return false;
            var dontChangeSystem = new[] { "System","WindowsBase", "WindowsFormsIntegration",
                                            "PresentationCore", "PresentationFramework", "PresentationUI"
            };
            if (dontChangeSystem.Contains(dllName)) return false;

            // These are in the GAC, ignore
            if (dllName.Contains("MapInfo.")) return false;
            if (dllName.Contains("DVTel.")) return false;

            // .Net Framework That are in the GAC
            var dontChangeDotNetFramework = new[] { "UIAutomationProvider", "UIAutomationTypes", "UIAutomationClient",
                                                    "Microsoft.CSharp", "Microsoft.VisualC","Microsoft.VisualBasic" ,"Microsoft.Build",
                                                    "CustomMarshalers", "ReachFramework",
                                                    "Microsoft.Build.Utilities.v4.0",   "Microsoft.SqlServer.Management.Sdk.Sfc",
                                                    "Microsoft.SqlServer.ConnectionInfo"};
            if (dontChangeDotNetFramework.Contains(dllName)) return false;

            if (dllName.Contains("Microsoft.")) return false;

            return true;
        }
    }
}