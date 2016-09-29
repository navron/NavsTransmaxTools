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
        public string MakeProjectName { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }
        public string ProjectType { get; set; }

        public List<string> TsdRefenences { get; set; }

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
            TsdRefenences = new List<string>();
            ExpectedMakeProjectReferences = new Dictionary<string, ProjectFound>();
            FileName = file;
            MakeProjectName = Path.GetFileNameWithoutExtension(file);

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
                    TsdRefenences.Add(temp[0]);
                }
            }
        }

        public void MakeExpectedMakeProjectRefenences()
        {
            foreach (var tsdRefenence in TsdRefenences)
            {
                if (tsdRefenence == null) continue;

                var t = tsdRefenence.Split('.');
                var projectName = t.Last();
                if (projectName.ToLower() == "workstation")
                {
                    projectName = tsdRefenence.Replace(@"Tsd.","");
                }

                if (ExpectedMakeProjectReferences.ContainsKey(projectName))
                {
                    Console.WriteLine($"Visual Studio File {FileName} as a duplicate TSD Reference {projectName}");
                }
                ExpectedMakeProjectReferences.Add(projectName, ProjectFound.NotLooked);
            }
        }

        public void MatchUpMakeProject(List<MakeFileProject> makeProjects)
        {
            var updateReferences = new Dictionary<string, ProjectFound>();
            foreach (var references in ExpectedMakeProjectReferences)
            {
                var found = makeProjects.Any(m => string.Equals(m.ProjectName, references.Key, StringComparison.Ordinal));
                if (found)
                {
                    updateReferences[references.Key] = ProjectFound.Found;
                    continue;
                }

                var foundCaseWrong = makeProjects.Any(m => string.Equals(m.ProjectName,references.Key, StringComparison.OrdinalIgnoreCase));
                if (foundCaseWrong)
                {
                    updateReferences[references.Key] = ProjectFound.FoundCaseWrong;
                    continue;
                }
                updateReferences[references.Key] = ProjectFound.NotFound;
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