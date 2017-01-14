using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("FixDependencies", HelpText = "Allocates Correct Dependency options of add/remove")]
    internal class FixDependencies: Store
    {
        [Option('a', "add", HelpText = "add missing dependencies")]
        public bool AddMissingDependencies { get; set; }

        [Option('r', "remove", HelpText = "Remove un-required dependencies")]
        public bool RemoveNotRequiredDependencies { get; set; }

        [Option('p', HelpText = "Process VisualStudio C++ Projects")]
        public bool VisualStudioCpp { get; set; }

        [Option('s', HelpText = "Process VisualStudio C# Projects")]
        public bool VisualStudioCs { get; set; }

        //method not unit tested
        public void Run()
        {
            Log.Debug($"Running {GetType().Name}");

            BuildStore();
            foreach (MakeProject makeProject in MakeProjects)
            {
                var vsProject = VisualStudioFiles.FirstOrDefault(f => f.ProjectName == makeProject.ProjectName);
                if (vsProject == null)
                {
                    if (Verbose) Console.WriteLine($"Didn't find MakeProject:{makeProject.ProjectName} VisualStudio File");
                    continue; // Safe, there will be Make project that don't match up
                }
                if (vsProject.ProjectName == "assvc")
                {

                }

                //If add missing Dependencies
                if (AddMissingDependencies)
                {
                    foreach (var reference in vsProject.ExpectedMakeProjectReferences)
                    {
                        if (!makeProject.DependencyProjects.Contains(reference.Key))
                        {
                            makeProject.DependencyProjects.Add(reference.Key);
                        }
                    }
                }
                //If remove missing Dependencies
                if (RemoveNotRequiredDependencies)
                {
                    var removelist = new List<string>();
                    foreach (var dependency in makeProject.DependencyProjects)
                    {
                        var vsfile = VisualStudioFiles.FirstOrDefault(v => v.ProjectName == dependency);
                        if (vsfile == null) continue; // This code only process make Projects that are Visual Studio projects

                        if (VisualStudioCs && vsfile.ProjectType == VisualStudioFile.VisualStudioFile.ProjectTypeValue.CSharp)
                        {
                            if (!vsProject.ExpectedMakeProjectReferences.ContainsKey(dependency))
                                removelist.Add(dependency);
                        }
                        if (VisualStudioCpp && vsfile.ProjectType == VisualStudioFile.VisualStudioFile.ProjectTypeValue.Cpp)
                        {
                            if (!vsProject.ExpectedMakeProjectReferences.ContainsKey(dependency))
                                removelist.Add(dependency);
                        }
                    }
                    foreach (var dependency in removelist)
                    {
                        makeProject.DependencyProjects.Remove(dependency);
                    }
                }
            }

            //if (!FixErrors) return;
            foreach (var makeFile in MakeFiles)
            {
                makeFile.WriteFile(this);
            }
        }
    }
}
