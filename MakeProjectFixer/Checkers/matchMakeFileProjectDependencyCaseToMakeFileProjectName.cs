using System;
using System.Collections.Generic;
using System.Linq;
using ColorConsole;
using CommandLine;
using MakeProjectFixer.Data;
using MakeProjectFixer.MakeFile;

namespace MakeProjectFixer.Checkers
{
    [Verb("matchMakeFileProjectDependencyCaseToMakeFileProjectName", HelpText = "Matches the Make Project Dependency List Case to Make File ProjectName Case")]
    internal class matchMakeFileProjectDependencyCaseToMakeFileProjectName : Store
    {
        readonly ConsoleWriter console = new ConsoleWriter();

        public void Run()
        {
            Program.Console.WriteLine($"Running {GetType().Name}", ConsoleColor.Cyan);

            BuildStoreMakeFilesOnly();

            var allprojects = new List<MakeProject>();
            allprojects.AddRange(MakeProjects);
            allprojects.AddRange(MakeHeaderProjects);

            CheckProjects(allprojects);

            foreach (var makeFile in MakeFiles)
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
                        console.Write($"Make Project: {makeProject.ProjectName}", ConsoleColor.Red);
                        console.WriteLine($" Dependency: {dependencyProject} does not exist", ConsoleColor.Yellow);
                        continue; // Error bug , should throw?
                    }
                    changeSet[dependencyProject] = project.ProjectName;
                }
                if (changeSet.Count > 0)
                {
                    console.WriteLine($"Fixing MakeProject: {makeProject.ProjectName}", ConsoleColor.Green);
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
