using System;
using CommandLine;
using ProjectFixer.Data;
using Serilog;

namespace ProjectFixer.Scripts
{
    [Verb("MakeFileChangeCase", HelpText = "Matches Make File Projects Names to Visual Studio Project Name Case")]
    internal class MakeFileChangeCase : Options
    {
        [Option( @"ProjectName", Required = true, HelpText = "Specifies the correct case to change to ")]
        public string ProjectName { get; set; }

        public void Run()
        {
            Log.Information($"Running {GetType().Name}");

            var store = new Store(this);
            store.BuildMakeFiles();

            foreach (var makeProject in store.GetAllMakeProjects)
            {
                if (makeProject.ProjectName.Equals(ProjectName, StringComparison.OrdinalIgnoreCase))
                    makeProject.ProjectName = ProjectName;

                var remove = string.Empty;
                foreach (var dependencyProject in makeProject.DependencyProjects)
                {
                    if (dependencyProject.Equals(ProjectName, StringComparison.OrdinalIgnoreCase))
                    {
                        remove = dependencyProject;
                    }
                }
                if (!string.IsNullOrEmpty(remove))
                {
                    makeProject.DependencyProjects.Remove(remove);
                    makeProject.DependencyProjects.Add(ProjectName);
                }
            }
     
            store.WriteMakeFiles();
        }
    }
}

