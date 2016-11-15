using System;
using System.Linq;
using CommandLine;
using MakeProjectFixer.Data;

namespace MakeProjectFixer.Checkers
{
    [Verb("matchMakeFiletoVisualStudioProjectCase", HelpText = "Matches Make Files Names to  VisualStudio Project Name Case")]
    internal class matchMakeFiletoVisualStudioProjectCase : Store
    {
        public void Run()
        {
            Program.Console.WriteLine($"Running {GetType().Name}", ConsoleColor.Cyan);

            BuildStore();

            foreach (var visualStudioFile in VisualStudioFiles)
            {
                foreach (var reference in visualStudioFile.ExpectedMakeProjectReferences)
                {
                    if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.FoundCaseWrong)
                    {
                        var makeproject = MakeProjects.FirstOrDefault(m => string.Equals(m.ProjectName, reference.Key, StringComparison.OrdinalIgnoreCase));
                        if (makeproject != null)
                        {
                            Program.Console.WriteLine($"Project {visualStudioFile.ProjectName} has wrong case make reference to {makeproject.ProjectName} should be {reference.Key}", ConsoleColor.Green);
                            makeproject.ProjectName = reference.Key;
                        }
                    }
                    if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.NotFound)
                    {
                        Program.Console.WriteLine($"Project {visualStudioFile.ProjectName} missing make project reference {reference.Key}", ConsoleColor.Red);
                    }
                }
                visualStudioFile.MatchUpMakeProject(MakeProjects);
            }

            foreach (var makeFile in MakeFiles)
            {
                makeFile.WriteFile(this);
            }
        }
    }
}
