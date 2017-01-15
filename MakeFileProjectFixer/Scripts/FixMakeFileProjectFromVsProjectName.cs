using System;
using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("FixMakeFileProjectFromVSProjectName", HelpText = "Matches Make Files Names to VisualStudio Project Name Case")]
    internal class FixMakeFileProjectFromVsProjectName : Store
    {
        public void Run()
        {
            Log.Information($"Running {GetType().Name}");

            BuildStore();


            // Scan the ExpectedMakeProjectReferences and match up the Case to the Make Project list
            // First Fix the Make Project name from the Visual Studio Project Names (this method was done ass about)
            //public void MatchUpMakeProject(List<MakeProject> makeProjects)
            //{
            //    var updateReferences = new Dictionary<string, ProjectFound>();
            //    foreach (var reference in ExpectedMakeProjectReferences)
            //    {
            //        var found = makeProjects.Any(m => string.Equals(m.ProjectName, reference.Key, StringComparison.Ordinal));
            //        if (found)
            //        {
            //            updateReferences[reference.Key] = ProjectFound.Found;
            //            continue;
            //        }

            //        var foundCaseWrong =makeProjects.Any(m => string.Equals(m.ProjectName, reference.Key, StringComparison.OrdinalIgnoreCase));
            //        if (foundCaseWrong)
            //        {
            //            updateReferences[reference.Key] = ProjectFound.FoundCaseWrong;
            //            continue;
            //        }
            //        updateReferences[reference.Key] = ProjectFound.NotFound;
            //    }
            //    ExpectedMakeProjectReferences = updateReferences;
            //}


            //foreach (var visualStudioFile in VisualStudioFiles)
            //{
            //    foreach (var reference in visualStudioFile.ExpectedMakeProjectReferences)
            //    {
            //        if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.FoundCaseWrong)
            //        {
            //            var makeproject = MakeProjects.FirstOrDefault(m => string.Equals(m.ProjectName, reference.Key, StringComparison.OrdinalIgnoreCase));
            //            if (makeproject != null)
            //            {
            //                Log.Debug($"Project {visualStudioFile.ProjectName} has wrong case make reference to {makeproject.ProjectName} should be {reference.Key}", ConsoleColor.Green);
            //                makeproject.ProjectName = reference.Key;
            //            }
            //        }
            //        if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.NotFound)
            //        {
            //            Log.Debug($"Project {visualStudioFile.ProjectName} missing make project reference {reference.Key}", ConsoleColor.Red);
            //        }
            //    }
            //    visualStudioFile.MatchUpMakeProject(MakeProjects);
            //}

            //foreach (var makeFile in MakeFiles)
            //{
            //    makeFile.WriteFile(this);
            //}
        }
    }
}
