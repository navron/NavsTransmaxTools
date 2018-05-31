using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using ProjectFixer.Data;
using ProjectFixer.MakeFile;
using ProjectFixer.Utility;
using Serilog;

namespace ProjectFixer.Scripts
{
    [Verb("CircularDependeyCheck", HelpText = "Checks for Circular Dependency in Make files")]
    internal class CircularDependeyCheck : Options
    {
        private readonly Stack<string> dependencyStack = new Stack<string>();
        // Report, What to see
        // for each project the StackLevel=X, Project Name, Dependency= Dependency List=LevelX,
        // order by StackLevel then Project Name. Sort Dependency list

        public void Run()
        {
            Log.Information($"Running {GetType().Name}");

            var store = new Store(this);
            store.BuildMakeFiles();

            // Convert all Make Projects (including headers) to an MakeProjectDependency class
            var allProjectDependency = store.GetAllMakeProjects.Select(makeProject => new MakeProjectDependency(makeProject)).ToList();
            //var allProjectDependency = store.MakeProjects.Select(makeProject => new MakeProjectDependency(makeProject)).ToList();

            // Look up all dependencyProject and check some basic rules
            var error = false;
            foreach (var project in allProjectDependency)
            {
                foreach (var dependencyProject in project.MakeProject.DependencyProjects)
                {
                    var depProject = allProjectDependency.FirstOrDefault(p => p.MakeProject.ProjectName == dependencyProject);
                    if (depProject == null)
                    {
                        Log.Error("Make Project: {ProjectName}, Dependency: {DependencyProject} does not exist", project.MakeProject.ProjectName, dependencyProject);
                        error = true;
                    }
                    if (depProject != null)
                    {
                        project.DependencyMakeProject.Add(depProject);
                    }

                    if (dependencyProject == project.MakeProject.ProjectName)
                    {
                        Log.Error($"Make Project: {project.MakeProject.ProjectName} contains a reference to itself in file ...", ConsoleColor.Red);
                        error = true;
                    }
                }
            }

            if (error)
            {
                Log.Error($"Error Detected Aborting");
                return;
            }
            // Check all the Projects
            var checkValue = CheckMakeProjectDependencies(allProjectDependency, store);
            // If not Dependencies errors then do a report
            if (checkValue)
            {
                ReportMakeFileCircularDependencyCheck(allProjectDependency);
            }
        }

        // Level is how many sets must be done before this can start, its a bad measurement because if Project A => B and C, that mean A is level 2 but if D=>B then both A and D can be done? Not making sense
        private int level = 0;
        /// <summary>
        /// Check A Make Project Dependency that it does not change Circular calls
        /// </summary>
        /// <remarks>This method is using recursion</remarks>
        /// <param name="reducedSetOfProjects">The list of projects to check, each recursion the list is smaller</param>
        /// <param name="store"></param>
        /// <returns>True that the given list has been checked and is OK</returns>
        internal bool CheckMakeProjectDependencies(List<MakeProjectDependency> reducedSetOfProjects, Store store)
        {
            foreach (var makeProject in reducedSetOfProjects)
            {
                level++;
                if (!CheckMakeProject(makeProject, store))
                {
                    // Stop and return, Circular dependency has been found
                    return false;
                }
                level--;
            }
            return true;
        }

        // True if OK, False if Error
        private bool CheckMakeProject(MakeProjectDependency makeProjectDependency, Store store)
        {
            // Has the MakeProject already been checked
            if (makeProjectDependency.Checked)
            {
                return true; // great already been checked
            }
            // If checking then, can't be check again unless there an error
            if (makeProjectDependency.Checking == true && makeProjectDependency.Checked == false)
            {
                // error print stack
                Log.Error("Error Circular Reference Detected for Project: {@ProjectName}", makeProjectDependency.MakeProject.ProjectName);
                Log.Error("Stack: {@dependencyStack}", dependencyStack);
                foreach (var item in dependencyStack)
                {
                    var mp = store.GetAllMakeProjects.First(x => x.ProjectName == item);
                    Log.Information("Area {@Area} Project: {@ProjectName}, Dependency: {@DependencyProjects}", mp.ProjectArea, mp.ProjectName,mp.DependencyProjects);
                }
                Log.Information($"How to Read the above, the Stack is the order in which project was check.");
                Log.Information($"The Project [{makeProjectDependency.MakeProject.ProjectName}] marked a Circular Reference detected, will appear in the stack ");
                return false;
            }

            makeProjectDependency.Checking = true;

            dependencyStack.Push(makeProjectDependency.MakeProject.ProjectName);
            var checkValue = CheckMakeProjectDependencies(makeProjectDependency.DependencyMakeProject, store);
            dependencyStack.Pop();

            var r = makeProjectDependency.DependencyMakeProject.Select(p => p.Level).Concat(new[] { 0 }).Max();
            makeProjectDependency.Level = ++r;

            makeProjectDependency.Checked = true;
            return checkValue;
        }

        private void ReportMakeFileCircularDependencyCheck(List<MakeProjectDependency> allProjects)
        {
            var a = allProjects.OrderBy(o => o.Level);
            Helper.PreProcessedFileSave("ReportMakeFileCircularDependency.json", a);
            var levelCount = new Dictionary<int, int>();
            var sb = new StringBuilder();
            var g = allProjects.GroupBy(p => p.Level).OrderByDescending(i => i.Count());
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
    }
}
