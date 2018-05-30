using System.Collections.Generic;
using System.Linq;
using CommandLine;
using ProjectFixer.Data;
using ProjectFixer.MakeFile;
using Serilog;

namespace ProjectFixer.Scripts
{
    [Verb("ReduceDependency", HelpText = "Reduces Allocates Dependency To Depth 1,2,3")]
    internal class ReduceDependency : Options
    {
        public void Run()
        {
            Log.Debug($"Running {GetType().Name}");

            var store = new Store(this);
            store.BuildStoreMakeFilesOnly();

            var allProjects = store.GetAllMakeProjects;

            // Only Check Projects
            foreach (MakeProject makeProject in store.MakeProjects)
            {
                // if project A uses X,Y & Z
                // and Project X uses project Y then remove project Y from Project A

                var removeList = new List<string>();
                foreach (var dependencyProject in makeProject.DependencyProjects)
                {
                    var depProject = allProjects.FirstOrDefault(p => p.ProjectName == dependencyProject);
                    if (depProject == null)
                    {
                        Log.Warning($"Missing dependencyProject {dependencyProject} in Make Project {makeProject.ProjectName}");
                        continue;
                    }
                    foreach (var depDepProject in depProject.DependencyProjects)
                    {
                        if (makeProject.DependencyProjects.Contains(depDepProject))
                            removeList.Add(depDepProject);
                    }
                }
                foreach (var item in removeList)
                {
                    makeProject.DependencyProjects.Remove(item);
                }
            }
            store.WriteMakeFiles();
        }
    }
}
