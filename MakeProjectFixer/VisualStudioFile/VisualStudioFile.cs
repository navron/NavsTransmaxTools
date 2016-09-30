using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MakeProjectFixer.MakeFile;
using Microsoft.Build.Evaluation;
using Newtonsoft.Json;

namespace MakeProjectFixer.VisualStudioFile
{
    class VisualStudioFile
    {
        // Project Name, Should match in Make File, Case Sensitive
        public string ProjectName { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }
        // CS or C++
        public string ProjectType { get; set; }

        // List of TSD Reference DLL
        public List<string> TsdReferences { get; set; }


        public enum ProjectFound
        {
            NotLooked = 0,
            NotFound,
            Found,
            FoundCaseWrong
        }

        // List of Expected Make Project References and if they where found
        public Dictionary<string, ProjectFound> ExpectedMakeProjectReferences { get; set; }

        public VisualStudioFile(string file)
        {
            TsdReferences = new List<string>();
            ExpectedMakeProjectReferences = new Dictionary<string, ProjectFound>();
            FileName = file;
            ProjectName = Path.GetFileNameWithoutExtension(file);

            var extension = Path.GetExtension(file);
            if (extension != null) ProjectType = extension.ToLower();
        }

        public void ScanFile()
        {
            var projCollection = new ProjectCollection();
            var p = projCollection.LoadProject(FileName);

            var references = p.GetItems("Reference");
            foreach (var item in references)
            {
                var include = item.EvaluatedInclude;
                var temp = include.Split(',');
                if (temp[0].Contains(@"Tsd."))
                {
                    TsdReferences.Add(temp[0]);
                }
            }
        }

        public void BuildExpectedMakeProjectRefenences()
        {
            foreach (var tsdRefenence in TsdReferences)
            {
                if (tsdRefenence == null) continue;

                var t = tsdRefenence.Split('.');
                var projectName = t.Last();
                if (projectName.ToLower() == "workstation")
                {
                    projectName = tsdRefenence.Replace(@"Tsd.", "");
                }

                if (ExpectedMakeProjectReferences.ContainsKey(projectName))
                {
                    Console.WriteLine($"Visual Studio File {FileName} as a duplicate TSD Reference {projectName}");
                }
                ExpectedMakeProjectReferences.Add(projectName, ProjectFound.NotLooked);
            }
        }

        public void MatchUpMakeProject(List<MakeProject> makeProjects)
        {
            var updateReferences = new Dictionary<string, ProjectFound>();
            foreach (var reference in ExpectedMakeProjectReferences)
            {
                var found = makeProjects.Any(m => string.Equals(m.ProjectName, reference.Key, StringComparison.Ordinal));
                if (found)
                {
                    updateReferences[reference.Key] = ProjectFound.Found;
                    continue;
                }

                var foundCaseWrong = makeProjects.Any(m => string.Equals(m.ProjectName, reference.Key, StringComparison.OrdinalIgnoreCase));
                if (foundCaseWrong)
                {
                    updateReferences[reference.Key] = ProjectFound.FoundCaseWrong;
                    continue;
                }
                updateReferences[reference.Key] = ProjectFound.NotFound;
            }
            ExpectedMakeProjectReferences = updateReferences;
        }
    }

    static class VisualStudioFileHelper
    {
        public static bool IncludeFile(string file)
        {
            // Don't want Unit Test in the list. Assume that these are ok
            if (file.Contains(@"\UnitTest")) return false;  // UnitTests and UnitTestSupport Folders

            // Don't want Test in the list. Assume that these are ok
            if (file.Contains(@"\test\")) return false;

            // Remove 3rdparty Project Files. On build machines the 3rdparty lib are check out to $src\lib\3rdparty and thus pick up
            if (file.Contains(@"\3rdparty\")) return false;

            var excludedlist = new List<string>
            {
                "Tsd.Libraries.Common.Eventing", // In code but not in build system, Need to ask about this 
                "ManagementConsole" // Jono special, Its missing a number of files. Needs work, Not in build.
            };
            // Exclude any known problems
            if (excludedlist.Any(file.Contains)) return false;

            return true;
        }
    }
}