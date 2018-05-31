using System;
using System.Linq;
using CommandLine;
using ProjectFixer.Data;
using ProjectFixer.Utility;
using Serilog;

namespace ProjectFixer.Scripts
{
    [Verb("RemovedMakeProject", HelpText = "List Missing Make Projects based on Make Projects Dependencies")]
    internal class RemovedMakeProject : Options
    {
        [Option("Project", HelpText = "Make Project Name", Required = true)]
        public string ProjectName { get; set; }

        //method not unit tested
        public void Run()
        {
            var store = new Store(this);
            store.BuildMakeFiles();

            using (new LoggingTimer(GetType().Name))
            {
                foreach (var makeFile in store.MakeFiles)
                {
                    if (makeFile.Header.ProjectName == ProjectName)
                    {
                        Log.Error("Not allow to remove header {Header} in file {file}, Replace it mainly if needed",
                            makeFile.Header.ProjectName, makeFile.FileName);
                        Environment.Exit(-1);
                    }

                    makeFile.Header.DependencyProjects = makeFile.Header.DependencyProjects.Except(new[] { ProjectName }).ToList();
                    makeFile.Projects.ForEach(p => p.DependencyProjects = p.DependencyProjects.Except(new[] { ProjectName }).ToList());
                }

                store.WriteMakeFiles();
            }
        }
    }
}
