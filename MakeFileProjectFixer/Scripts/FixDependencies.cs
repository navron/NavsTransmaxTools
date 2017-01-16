using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using MakeFileProjectFixer.Utility;
using MakeFileProjectFixer.VisualStudioFile;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("FixDependencies", HelpText = "Allocates Correct Dependency options of add/remove")]
    internal class FixDependencies : Options
    {
        [Option('a', "add", HelpText = "add missing dependencies")]
        public bool AddMissingDependencies { get; set; }

        [Option('r', "remove", HelpText = "Remove un-required dependencies")]
        public bool RemoveNotRequiredDependencies { get; set; }

        [Option("cpp", HelpText = "Process VisualStudio C++ Projects")]
        public bool VisualStudioCpp { get; set; }

        [Option("cs", HelpText = "Process VisualStudio C# Projects")]
        public bool VisualStudioCSharpe { get; set; }

        //method not unit tested
        public void Run()
        {
            var store = new Store(this.Folder);
            store.BuildStore();

            using (new LoggingTimer(GetType().Name))
            {
                foreach (MakeProject makeProject in store.MakeProjects)
                {
                    ProcessMakeProject(makeProject, store.VisualStudioFiles);
                }

                //if (!FixErrors) return;
                foreach (var makeFile in store.MakeFiles)
                {
                    makeFile.WriteFile(this);
                }
            }
        }

        private void ProcessMakeProject(MakeProject makeProject, List<IVisualStudioFile> visualStudioFiles)
        {
            // Check if there is an matching VisualStudio project 
            var vsProject = visualStudioFiles.FirstOrDefault(f => f.ProjectName == makeProject.ProjectName);
            if (vsProject == null)
            {
                Log.Debug($"Didn't find MakeProject:{makeProject.ProjectName} matching VisualStudio File");
                return; // Safe, there will be Make project that don't match up
            }

            // If add missing Dependencies
            if (AddMissingDependencies)
            {
             //   Log.Information("Adding Dependencies");
                foreach (var reference in vsProject.ExpectedMakeProjectReference)
                {
                    if (!makeProject.DependencyProjects.Contains(reference))
                    {
                        makeProject.DependencyProjects.Add(reference);
                    }
                }
            }

            // If remove missing Dependencies
            if (RemoveNotRequiredDependencies)
            {
               // Log.Information("Removing Dependencies");
                var removelist = new List<string>();
                foreach (var dependency in makeProject.DependencyProjects)
                {
                    var vsfile = visualStudioFiles.FirstOrDefault(v => v.ProjectName == dependency);
                    if (vsfile == null)
                        continue; // This code only process make Projects that are Visual Studio projects

                    if (!vsProject.ExpectedMakeProjectReference.Contains(dependency))
                        removelist.Add(dependency);
                }

                foreach (var dependency in removelist)
                {
                    makeProject.DependencyProjects.Remove(dependency);
                }
            }
        }
    }
}
