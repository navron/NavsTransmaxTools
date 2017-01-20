using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using MakeFileProjectFixer.Utility;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("FixMakeFileCaseFromVisualStudio", HelpText = "Matches Make File Projects Names to Visual Studio Project Name Case")]
    internal class FixMakeFileCaseFromVisualStudio : Options
    {
        public void Run()
        {
            Log.Information($"Running {GetType().Name}");

            var store = new Store(this);
            store.BuildStore();

            // Build Set of Expected Make Project Names from VisualStudioFiles
            var hashSet = new HashSet<string>();
            foreach (var storeVisualStudioFile in store.VisualStudioFiles)
            {
                foreach (var projectName in storeVisualStudioFile.ExpectedMakeProjectReference)
                {
                    hashSet.Add(projectName);
                }
            }

            Helper.PreProcessedFileSave("Test.json", hashSet);
            // store.VisualStudioFiles.ForEach(studioFile => studioFile.ExpectedMakeProjectReference.ForEach(projectName => hashSet.Add(projectName)));
            // Check HashSet for duplicate case entries
            foreach (var p1 in hashSet) // Ok bad coding for now
            {
                foreach (var p2 in hashSet)
                {
                    if (p1 == p2 || !string.Equals(p1, p2, StringComparison.OrdinalIgnoreCase)) continue;
                    Log.Error($"VisualStudioFiles contain Make Projects reference of different case '{p1}' and '{p2}'");
                    Environment.Exit(-1);
                }
            }

            var list = hashSet.ToList();
            list.Sort();
            store.MakeHeaderProjects.ForEach(mp => FixProjectNameAndDependcyes(mp, list));
            store.MakeProjects.ForEach(mp => FixProjectNameAndDependcyes(mp, list));

            store.WriteMakeFiles();
        }

        //Scan the ExpectedMakeProjectReferences and match up the Case to the Make Project list
        //First Fix the Make Project name from the Visual Studio Project Names(this method was done ass about)
        private void FixProjectNameAndDependcyes(MakeProject makeProject, List<string> expectedProjectNames)
        {
            if (!expectedProjectNames.Contains(makeProject.ProjectName))
            {
                // Case may be 
                var test = expectedProjectNames.FirstOrDefault(p => string.Equals(makeProject.ProjectName, p, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(test))
                {
                    makeProject.ProjectName = test;
                }
            }
            var update = new Dictionary<string, string>();
            foreach (var dependencyProject in makeProject.DependencyProjects)
            {
                if (expectedProjectNames.Contains(dependencyProject)) continue;
                var newName = expectedProjectNames.FirstOrDefault(expectedProjectName => string.Equals(expectedProjectName, dependencyProject, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(newName)) continue;
                update.Add(dependencyProject, newName);
            }
            foreach (var v in update)
            {
                makeProject.DependencyProjects.Remove(v.Key);
                makeProject.DependencyProjects.Add(v.Value);
            }
        }
    }
}

