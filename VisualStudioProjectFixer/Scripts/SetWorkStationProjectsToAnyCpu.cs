using System;
using System.Collections.Generic;
using CommandLine;
using Microsoft.Build.Evaluation;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("SetWorkStationProjectsToAnyCPU", HelpText = "")]
    public class SetWorkStationProjectsToAnyCpu : Options
    {
        public void Run()
        {
            List<string> sourceFileList = Helper.GetProjectFiles(RootFolder, Config.GetSourceSearchPatterns);
            foreach (var filepath in sourceFileList)
            {
                if (filepath.Contains(@"\ws\") && filepath.Contains(".csproj"))
                {
                    var project = new Project(filepath);

                    var platformTargets = project.GetItems("PlatformTarget");

                    foreach (var platformTarget in platformTargets)
                    {
                        platformTarget.SetMetadataValue("PlatformTarget", "AnyCPU");
                    }

                    if (project.IsDirty) Console.WriteLine($"Changed: {filepath}");

                    project.Save();
                }
            }
        }
    }
}