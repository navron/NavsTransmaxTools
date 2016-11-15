using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using MakeProjectFixer.Data;
using MakeProjectFixer.MakeFile;

namespace MakeProjectFixer.Checkers
{
    [Verb("mkReduceDepends", HelpText = "Reduces Allocates Dependency To Depth 1,2,3")]
    internal class MkReduceDepends : Store
    {
        readonly List<MakeProject> allProjects = new List<MakeProject>();

        public void Run()
        {
            Program.Console.WriteLine($"Running {GetType().Name}", ConsoleColor.Cyan);

            BuildStoreMakeFilesOnly();

            allProjects.AddRange(MakeProjects);
            allProjects.AddRange(MakeHeaderProjects);

            // Only Check Projects
            foreach (MakeProject makeProject in MakeProjects)
            {
                // if project A uses X,Y & Z
                // and Project X uses project Y then remove project Y from Project A

                var removeList = new List<string>();
                foreach (var dependencyProject in makeProject.DependencyProjects)
                {
                    var depProject = allProjects.FirstOrDefault(p => p.ProjectName == dependencyProject);
                    if (depProject == null)
                    {
                        Program.Console.WriteLine($"Missing dependencyProject {dependencyProject} in Make Project {makeProject.ProjectName}", ConsoleColor.Red);
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
            foreach (var makeFile in MakeFiles)
            {
                makeFile.WriteFile(this);
            }
        }
    }
}
