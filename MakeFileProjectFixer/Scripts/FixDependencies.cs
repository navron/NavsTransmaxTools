using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using MakeFileProjectFixer.Utility;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("FixDependencies", HelpText = "Allocates Correct Dependency options of add/remove")]
    internal class FixDependencies : Options
    {
        [Option("cpp", HelpText = "Process VisualStudio C++ Projects")]
        public bool VisualStudioCpp { get; set; }

        [Option("cs", HelpText = "Process VisualStudio C# Projects")]
        public bool VisualStudioCSharpe { get; set; }

        //method not unit tested
        public void Run()
        {
            var store = new Store(this);
            store.BuildStore();

            using (new LoggingTimer(GetType().Name))
            {
                foreach (MakeProject makeProject in store.MakeProjects)
                {
                    ProcessMakeProject(makeProject, store);
                }

                store.WriteMakeFiles();
            }
        }

        private void ProcessMakeProject(MakeProject makeProject, Store store)
        {
            // Check if there is an matching VisualStudio project 
            var vsProject = store.VisualStudioFiles.FirstOrDefault(f => f.ProjectName == makeProject.ProjectName);
            if (vsProject == null)
            {
                Log.Debug($"Didn't find MakeProject:{makeProject.ProjectName} matching VisualStudio File");
                return; // Safe, there will be Make project that don't match up
            }

            // May be total wrong, also what about non visual targets
            makeProject.DependencyProjects = vsProject.ExpectedMakeProjectReference;

            //var difference = 
            //// If add missing Dependencies
            //if (AddMissingDependencies)
            //{
            //    //   Log.Information("Adding Dependencies");
            //    foreach (var reference in vsProject.ExpectedMakeProjectReference)
            //    {
            //        if (!makeProject.DependencyProjects.Contains(reference))
            //        {
            //            makeProject.DependencyProjects.Add(reference);
            //        }
            //    }
            //}

            //// If remove missing Dependencies
            //if (RemoveNotRequiredDependencies)
            //{
            //    // Log.Information("Removing Dependencies");
            //    var removelist = new List<string>();
            //    foreach (var dependency in makeProject.DependencyProjects)
            //    {
            //        var isHeader = store.MakeHeaderProjects.Any(mhp => dependency.Contains(mhp.ProjectName));
            //        if (isHeader) removelist.Add(dependency);

            //        var vsfile = store.VisualStudioFiles.Any(v => v.ProjectName == dependency);
            //        var realProject = store.MakeProjects.Any(mp => mp.ProjectName == dependency);
            //        if (realProject && !vsfile) continue; // This code only process make Projects that are Visual Studio projects
            //        if (!realProject) removelist.Add(dependency); // Can't have this (missing project)


            //        if (!vsProject.ExpectedMakeProjectReference.Contains(dependency))
            //            removelist.Add(dependency);
            //    }

            //    foreach (var dependency in removelist)
            //    {
            //        makeProject.DependencyProjects.Remove(dependency);
            //    }
            //}
        }
    }
}
