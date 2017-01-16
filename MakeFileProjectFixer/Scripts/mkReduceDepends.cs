using System.Collections.Generic;
using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("mkReduceDepends", HelpText = "Reduces Allocates Dependency To Depth 1,2,3")]
    internal class MkReduceDepends : Options
    {
        readonly List<MakeProject> allProjects = new List<MakeProject>();

        public void Run()
        {
            Log.Debug($"Running {GetType().Name}");

            var store = new Store(this.Folder);
            store.BuildStoreMakeFilesOnly();

            allProjects.AddRange(store.MakeProjects);
            allProjects.AddRange(store.MakeHeaderProjects);

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
            foreach (var makeFile in store.MakeFiles)
            {
                makeFile.WriteFile(this);
            }
        }
    }
}
