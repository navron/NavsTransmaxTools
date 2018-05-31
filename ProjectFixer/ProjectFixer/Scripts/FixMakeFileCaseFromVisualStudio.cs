using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using ProjectFixer.Data;
using ProjectFixer.MakeFile;
using ProjectFixer.Utility;
using Serilog;

namespace ProjectFixer.Scripts
{
    [Verb("FixMakeFileCaseFromVisualStudio", HelpText = "Matches Make File Projects Names to Visual Studio Project Name Case")]
    internal class FixMakeFileCaseFromVisualStudio : Options
    {
        public void Run()
        {
            Log.Information($"Running {GetType().Name}");

            var store = new Store(this).BuildStore();

            // Build Set of Expected Make Project Names from VisualStudioFiles
            var hashSet = new HashSet<string>();
            foreach (var studioFile in store.VisualStudioFiles)
            {
                // Add this project name (not all project names are in the expected list)
                hashSet.Add(studioFile.ProjectName);

                // Add all the Expected Make List
                foreach (var projectName in studioFile.ExpectedMakeProjectReference)
                {
                    hashSet.Add(projectName);
                }
            }

            // store.VisualStudioFiles.ForEach(studioFile => studioFile.ExpectedMakeProjectReference.ForEach(projectName => hashSet.Add(projectName)));
            // Check HashSet for duplicate case entries
            Helper.PreProcessedFileSave("FixMakeFileCaseFromVisualStudioHashSet.json", hashSet);
            bool founderrors = false;
            foreach (var p1 in hashSet) // Ok bad coding for now
            {
                foreach (var p2 in hashSet)
                {
                    if (p1 == p2 || !string.Equals(p1, p2, StringComparison.OrdinalIgnoreCase)) continue;
                    Log.Error($"VisualStudioFiles contain Make Projects reference of different case '{p1}' and '{p2}'  -- Fix manually with  MakeFileChangeCase --ProjectName={p2}");
                    Log.Error($"This means the two visual studio files are referencing the project with different case, fix the visual Studio project");

                    var p1List = new List<string>();
                    store.VisualStudioFiles.ForEach(vs => vs.ExpectedMakeProjectReference.ForEach(mp => { if (mp == p1) p1List.Add(vs.FileName); }));
                    var p2List = new List<string>();
                    store.VisualStudioFiles.ForEach(vs => vs.ExpectedMakeProjectReference.ForEach(mp => { if (mp == p2) p2List.Add(vs.FileName); }));

                    Log.Information("This case {P} is in these files {files}", p1, p1List);
                    Log.Information("This case {P} is in these files {files}", p2, p2List);
                    founderrors = true;
                }
            }

            if (founderrors) Environment.Exit(-1);

            var list = hashSet.ToList();
            list.Sort(StringComparer.OrdinalIgnoreCase);
            Helper.PreProcessedFileSave("FixMakeFileCaseFromVisualStudioHashSetSortedList.json", list);
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

