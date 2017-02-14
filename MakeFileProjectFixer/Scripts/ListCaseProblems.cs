using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using CommandLine;
using MakeFileProjectFixer.Data;
using MakeFileProjectFixer.MakeFile;
using MakeFileProjectFixer.Utility;
using MakeFileProjectFixer.VisualStudioFile;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("ListCaseProblems", HelpText = "List Missing Make Projects based on Make Projects Dependencies")]
    internal class ListCaseProblems : Options
    {
        [Option("fix", HelpText = "Apply changes")]
        public bool Fix { get; set; }

        //method not unit tested
        public void Run()
        {
            var store = new Store(this);
            store.BuildStore();
            using (new LoggingTimer(GetType().Name))
            {
                if (CheckMakeProjectNonGenircePublishValues(store, Fix)) return;
                if (DoCheckMakeProjects(store, Fix)) return;
                if (CheckMakeProjectHaveUnquiePublishValues(store, Fix)) return;
                if (DoVisualStuidoProjects(store, Fix)) return;
                if (DoVisualStuidoProjects2(store, Fix)) return;
            }
        }

        // Check that the publishing value is the same case as the file name in the visual studio project
        private bool DoCheckMakeProjects(Store store, bool fix)
        {
            Log.Information("Running DoCheckMakeProjects");
            var haveUpdated = false;
            foreach (var makeProject in store.MakeProjects)
            {
                if (!makeProject.PublishCppHeaderFiles.Any()) continue;

                // find Visual file
                var vs = store.VisualStudioFiles.FirstOrDefault(v => v.ProjectName == makeProject.ProjectName);
                if (vs == null)
                {
                    Log.Warning("Missing Visual Studio file for Make project {Project}", makeProject.ProjectName);
                    continue;
                }

                var vscpp = vs as VisualStudioCPlusPlusFile;
                if (vscpp == null)
                {
                    Log.Warning("CSProject Visual Studio file cast to cpp failed for Make project {Project}, Ignore", makeProject.ProjectName);
                    continue;
                }

                foreach (var cppHeaderFile in makeProject.PublishCppHeaderFiles)
                {
                    var cleanCodeFiles = vscpp.CodeFiles.Select(Path.GetFileName).ToList();
                    foreach (var file in cleanCodeFiles)
                    {
                        if (!file.Equals(cppHeaderFile, StringComparison.OrdinalIgnoreCase) ||
                            file.Equals(cppHeaderFile)) continue;
                        Log.Information("Make Project {Project} Publish file {file} should be {casefile}", makeProject.ProjectName, cppHeaderFile, file);

                        if (!fix) continue;
                        var newList = new List<string>();
                        foreach (var postLine in makeProject.PostLines)
                        {
                            var line = postLine;
                            if (postLine.Contains(cppHeaderFile))
                            {
                                line = postLine.Replace(cppHeaderFile, file);
                            }
                            newList.Add(line);
                        }
                        makeProject.PostLines = newList;
                        haveUpdated = true;
                    }
                }
            }
            store.WriteMakeFiles();
            return haveUpdated;
        }

        // Check 
        private bool CheckMakeProjectHaveUnquiePublishValues(Store store, bool fix)
        {
            Log.Information("Running CheckMakeProjectHaveUnquiePublishValues");
            var fail = false;
            var dicSet = new Dictionary<string, MakeProject>();
            foreach (var makeProject in store.MakeProjects)
            {
                foreach (var cppHeaderFile in makeProject.PublishCppHeaderFiles)
                {
                    if (dicSet.ContainsKey(cppHeaderFile))
                    {
                        Log.Error("MakeProjects:{0},{1} both publish {file}", makeProject.ProjectName, dicSet[cppHeaderFile].ProjectName, cppHeaderFile);
                        fail = true;
                    }
                    dicSet[cppHeaderFile] = makeProject;
                }
            }
            return fail;
        }

        private bool CheckMakeProjectNonGenircePublishValues(Store store, bool fix)
        {
            Log.Information("Running CheckMakeProjectNonGenircePublishValues");
            var fail = false;
            foreach (var makeProject in store.MakeProjects)
            {
                foreach (var cppHeaderFile in makeProject.PublishCppHeaderFiles)
                {
                    if (cppHeaderFile == "$@.h")
                    {
                        Log.Error("MakeProject:{0} publishes \"$@.h\"  ", makeProject.ProjectName);
                        fail = true;
                    }
                }
            }
            return fail;
        }

        private bool DoVisualStuidoProjects(Store store, bool fix)
        {
            Log.Information("Running DoVisualStuidoProjects");
            var dicSet = new Dictionary<string, MakeProject>();
            foreach (var makeProject in store.MakeProjects)
            {
                foreach (var cppHeaderFile in makeProject.PublishCppHeaderFiles)
                {
                    dicSet[cppHeaderFile] = makeProject;
                }
            }
            var list = dicSet.Select(item => item.Key).ToList();

            foreach (var cPlusPlusFile in store.CPlusPlusFiles)
            {
                foreach (var reference in cPlusPlusFile.ReferencesSet)
                {
                    foreach (var item in list)
                    {
                        if (!reference.Equals(item, StringComparison.OrdinalIgnoreCase) || reference.Equals(item))
                            continue;
                        Log.Information("Ref {Ref} in VS {file} is incorrect case, should be {case}", reference, cPlusPlusFile.ProjectName, item);
                        if (fix)
                        {
                            ChangeCaseIn(Path.GetDirectoryName(cPlusPlusFile.FileName), reference, item);
                        }
                    }
                }
            }
            return false;
        }

        private bool DoVisualStuidoProjects2(Store store, bool fix)
        {
            Log.Information("Running DoVisualStuidoProjects2");
            var dicSet = new Dictionary<string, MakeProject>();
            foreach (var makeProject in store.MakeProjects)
            {
                dicSet[makeProject.ProjectName] = makeProject;
            }
            var list = dicSet.Select(item => item.Key).ToList();

            foreach (var cPlusPlusFile in store.CPlusPlusFiles)
            {
                foreach (var reference in cPlusPlusFile.ReferencesSet)
                {
                    var test = reference.Split('.').First();
                    foreach (var item in list)
                    {
                        if (!test.Equals(item, StringComparison.OrdinalIgnoreCase) || test.Equals(item))
                            continue;
                        Log.Information("Possible Ref {Ref} in VS {file} is incorrect case, should be {case}", test, cPlusPlusFile.ProjectName, item);
                        if (fix)
                        {
                            ChangeCaseIn(Path.GetDirectoryName(cPlusPlusFile.FileName), test, item);
                        }
                    }
                }
            }
            return false;
        }

        private void ChangeCaseIn(string directoryName, string reference, string shouldbe)
        {
            var searchPatterns = new[] {"*.h", "*.cpp"};
            var files = searchPatterns.AsParallel()
             .SelectMany(searchPattern => Directory.EnumerateFiles(directoryName, searchPattern, SearchOption.AllDirectories))
             .ToList();

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                text = text.Replace(reference, shouldbe);
                File.WriteAllText(file,text);
            }
        }
    }
}
