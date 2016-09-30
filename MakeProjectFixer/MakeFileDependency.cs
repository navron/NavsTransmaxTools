using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ColorConsole;
using CommandLine;
using MakeProjectFixer.MakeFile;
using Newtonsoft.Json;

namespace MakeProjectFixer
{
    [Verb("MakeFileDependencyChecker", HelpText = "Allocates Correct Dependency")]
    internal class MakeFileDependencyChecker : Store
    {
        [Option(@"add", HelpText = "add missing Dependency")]
        public bool AddMissingDependencies { get; set; }

        [Option(@"remove", HelpText = "Remove un-required Dependency")]
        public bool RemoveNotRequiredDependencies { get; set; }

        //method not unit tested
        public void Run()
        {
            this.BuildStore();
            foreach (MakeProject makeProject in MakeProjects)
            {
                var vsProject = VisualStudioFiles.FirstOrDefault(f => f.ProjectName == makeProject.ProjectName);
                if (vsProject == null)
                {
                    if (Verbose) Console.WriteLine($"Didn't find MakeProject:{makeProject.ProjectName} VisualStudio File");
                    continue; // Safe, there will be Make project that don't match up
                }
                //If add missing Dependencies
                if (AddMissingDependencies)
                {
                    foreach (var reference in vsProject.ExpectedMakeProjectReferences)
                    {
                        if (!makeProject.DependencyProjects.Contains(reference.Key))
                            makeProject.DependencyProjects.Add(reference.Key);
                    }
                }
                //If add missing Dependencies
                if (RemoveNotRequiredDependencies)
                {
                    var removelist = new List<string>();
                    foreach (var dependency in makeProject.DependencyProjects)
                    {
                        if (!vsProject.ExpectedMakeProjectReferences.ContainsKey(dependency))
                            removelist.Add(dependency);
                    }
                    foreach (var dependency in removelist)
                    {
                        makeProject.DependencyProjects.Remove(dependency);
                    }
                }
            }

            if (!FixErrors) return;
            foreach (var makeFile in MakeFiles)
            {
                makeFile.WriteFile(this.LineLength, this.SortProject);
            }
        }
    }

    class MakeProjectDependency 
    {
        public int Level;
        public bool Checking = false;
        public bool Checked = false;

        [JsonIgnore]
        public List<MakeProjectDependency> DependencyMakeProject = new List<MakeProjectDependency>();
        [JsonIgnore]
        public MakeProject MakeProject;

        public string ProjectName;

        public MakeProjectDependency(MakeProject makeProject)
        {
            MakeProject = makeProject;
            Checking = false;
            Checked = false;
            Level = 0;
            ProjectName = makeProject.ProjectName;
        }
    }

    [Verb("MakeFileCircularDependencyCheck", HelpText = "Checks for Circular Dependency in Make files")]
    internal class MakeFileCircularDependencyCheck : Store
    {
        private readonly Stack<string> dependencyStack = new Stack<string>();
        // Report, What to see
        // for each project the StackLevel=X, Project Name, Dependency= Dependency List=LevelX,
        // order by StackLevel then Project Name. Sort Dependency list

        readonly ConsoleWriter console = new ConsoleWriter();

        readonly List<MakeProjectDependency> allProjects = new List<MakeProjectDependency>();
        //method not unit tested
        public void Run()
        {
            this.BuildStoreMakeFilesOnly();

            foreach (var makeProject in MakeProjects)
            {
                allProjects.Add(new MakeProjectDependency(makeProject));
            }
            foreach (var makeProject in MakeHeaderProjects)
            {
                allProjects.Add(new MakeProjectDependency(makeProject));
            }

            // Look up all dependencyProject and fill out list
            bool error = false;
            foreach (var project in allProjects)
            {
                foreach (var dependencyProject in project.MakeProject.DependencyProjects)
                {
                    var depProject = allProjects.FirstOrDefault(p => p.MakeProject.ProjectName == dependencyProject);
                    if (depProject == null)
                    {
                        console.WriteLine($"Make Project: {project.MakeProject.ProjectName} Dependency: {dependencyProject} does not exist", ConsoleColor.Red);
                        error = true;
                    }
                    if (depProject != null)
                    {
                        project.DependencyMakeProject.Add(depProject);
                    }

                    if (dependencyProject == project.MakeProject.ProjectName)
                    {
                        console.WriteLine($"Make Project: {project.MakeProject.ProjectName} contains a reference to itself in file ...", ConsoleColor.Red);
                        error = true;
                    }
                }
            }

            if (error)
            {
                console.Write($"Error Detected Aborting", ConsoleColor.Red);
                return;
            }

            var checkValue = CheckMakeProjectDependencies(allProjects);
            if (checkValue)
            {
                ReportMakeFileCircularDependencyCheck();
            }
        }

        private void ReportMakeFileCircularDependencyCheck()
        {
            var sb = new StringBuilder();
            var a = allProjects.OrderBy(o => o.Level);
            Helper.PreProcessedFileSave("ReportMakeFileCircularDependency.json", a);
            var levelCount = new Dictionary<int,int>();
            sb = new StringBuilder();
            var g = allProjects.GroupBy(p => p.Level).OrderByDescending(i=>i.Count());
            foreach (IGrouping<int, MakeProjectDependency> item in g)
            {
                levelCount[item.Key] = item.Count();
                var s = $"Level={item.Key} Item Count={ item.Count()}";
                sb.AppendLine(s);
                Console.WriteLine(s);
            }
            File.WriteAllText("ReportMakeFileCircularDependencyCount.txt", sb.ToString());

            sb = new StringBuilder();
            foreach (var item in a)
            {
                // for each project the StackLevel=X, Project Name, Dependency= Dependency List=LevelX,
                var d = string.Join(",", item.MakeProject.DependencyProjects);
                var s = $"Level={item.Level} Count:{levelCount[item.Level]} ProjectName={item.ProjectName}, Dependency={d}";
                sb.AppendLine(s);
                Console.WriteLine(s);
            }
            File.WriteAllText("ReportMakeFileCircularDependency.txt", sb.ToString());

        }

        private int level = 0;
        // using recursion
        internal bool CheckMakeProjectDependencies(List<MakeProjectDependency> makeProjectDependencies)
        {
            foreach (var makeProject in makeProjectDependencies)
            {
                level++;
                if (!CheckMakeProject(makeProject))
                {
                    // Stop and return
                    return false;
                }
                level--;
            }
            return true;
        }

        // True if Ok, False if Error
        private bool CheckMakeProject(MakeProjectDependency makeProject)
        {
            // Has the MakeProject already been checked
            if (makeProject.Checked)
            {
                return true; // great already been checked
            }

            if (makeProject.Checking == true && makeProject.Checked == false)
            {
                // error print stack
                console.WriteLine($"Error Circular Reference Detected for Project {makeProject.MakeProject.ProjectName}", ConsoleColor.Red);
                var stack = string.Join(", ", dependencyStack);
                console.WriteLine($"Stack for Project {stack}: ", ConsoleColor.Red);
                foreach (var item in dependencyStack)
                {
                    var t = allProjects.FirstOrDefault(p => p.MakeProject.ProjectName == item);
                    PrintDependencyList(t);
                }
                return false;
            }

            makeProject.Checking = true;
          
            dependencyStack.Push(makeProject.MakeProject.ProjectName);
            var checkValue = CheckMakeProjectDependencies(makeProject.DependencyMakeProject);
            dependencyStack.Pop();

            int r = makeProject.DependencyMakeProject.Select(p => p.Level).Concat(new[] {0}).Max();
            makeProject.Level = ++r;

            makeProject.Checked = true;
            return checkValue;
        }

        private void PrintDependencyList(MakeProjectDependency makeProject)
        {
            console.Write($"Project: {makeProject.MakeProject.ProjectName} ", ConsoleColor.Yellow);
            var dependencis = string.Join(", ", makeProject.MakeProject.DependencyProjects);
            console.WriteLine($"Dependency: {dependencis}", ConsoleColor.DarkYellow);
        }
    }
}
