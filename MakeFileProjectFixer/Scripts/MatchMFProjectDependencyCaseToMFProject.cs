using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("MatchMFProjectDependencyCaseToMFProject", HelpText = "Matches the Make Project Dependency List Case to Make File ProjectName Case")]
    internal class MatchMfProjectDependencyCaseToMfProject : Options
    {
        public void Run()
        {
            Log.Debug($"Running {GetType().Name}");

            var store = new Store(this);
            store.BuildStoreMakeFilesOnly();

            var allprojects = new List<MakeProject>();
            allprojects.AddRange(store.MakeProjects);
            allprojects.AddRange(store.MakeHeaderProjects);

            CheckProjects(allprojects);

            foreach (var makeFile in store.MakeFiles)
            {
                makeFile.WriteFile(this);
            }
        }

        private void CheckProjects(List<MakeProject> projects)
        {
            foreach (MakeProject makeProject in projects)
            {
                var changeSet = new Dictionary<string, string>();
                foreach (var dependencyProject in makeProject.DependencyProjects)
                {
                    var ok = projects.FirstOrDefault(m => string.Equals(m.ProjectName, dependencyProject, StringComparison.Ordinal));
                    if (ok != null) continue;
                    var project = projects.FirstOrDefault(m => string.Equals(m.ProjectName, dependencyProject, StringComparison.CurrentCultureIgnoreCase));
                    if (project == null)
                    {
                        Log.Debug($"Make Project: {makeProject.ProjectName}");
                        Log.Debug($" Dependency: {dependencyProject} does not exist");
                        continue; // Error bug , should throw?
                    }
                    changeSet[dependencyProject] = project.ProjectName;
                }
                if (changeSet.Count > 0)
                {
                    Log.Information($"Fixing MakeProject: {makeProject.ProjectName}", ConsoleColor.Green);
                }
                foreach (var item in changeSet)
                {
                    makeProject.DependencyProjects.Remove(item.Key);
                    makeProject.DependencyProjects.Add(item.Value);
                }
            }
        }
    }
}
