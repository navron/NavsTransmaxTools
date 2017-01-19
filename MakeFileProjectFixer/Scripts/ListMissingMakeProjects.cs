using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using MakeFileProjectFixer.Utility;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("ListMissingMakeProjects", HelpText = "List Missing Make Projects based on Make Projects Dependencies")]
    internal class ListMissingMakeProjects : Options
    {
        //method not unit tested
        public void Run()
        {
            var store = new Store(this);
            store.BuildStoreMakeFilesOnly();

            using (new LoggingTimer(GetType().Name))
            {
                // Build a unique list of all projects and all dependency projects
                var projectList = new HashSet<string>();
                var dependencyList = new HashSet<string>();
                store.GetAllMakeProjects.ForEach(mp =>
                {
                    projectList.Add(mp.ProjectName);
                    mp.DependencyProjects.ForEach(dp => dependencyList.Add(dp));
                });

                Log.Information($"There are projects:{projectList.Count} and unique dependency projects:{dependencyList.Count}");

                foreach (var makeProject in store.GetAllMakeProjects)
                {
                    var missingTargets = makeProject.DependencyProjects.Except(projectList).ToList();
                    if(!missingTargets.Any()) continue;

                    foreach (var makeFile in store.MakeFiles)
                    {
                       if()
                    }
                    var makefile = store.MakeFiles.First(mk => mk.Projects.Contains(makeProject));

                    Log.Warning("Project: {Project} in file {File} has not Make Target for: {@MissingTargets}", makeProject.ProjectName, makefile.FileName, missingTargets);
                }
            }
        }
    }
}
