using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DependencyChecker.Checkers
{
    internal class MakeFileChecker
    {
        private readonly Stack<string> dependencyStack = new Stack<string>();
        // List of projects in a mak file with all their configured dependencies
        private readonly Dictionary<string, string[]> makeFileDic = new Dictionary<string, string[]>();
        // List of physical *.mak files
        private readonly List<MakeFile> makeFiles = new List<MakeFile>();
        private readonly Dictionary<string, bool> makeProjectDependenciesCheckingList = new Dictionary<string, bool>();

        private readonly IOptions options;

        public MakeFileChecker(IOptions options)
        {
            this.options = options;
        }

        /// <summary>
        ///     Check Make files for Known problems
        /// </summary>
        public void CheckMakeFilesForDependencies()
        {
            ScanMakeFiles();
            ProcessMakeFile();

            dependencyStack.Clear();
            makeProjectDependenciesCheckingList.Clear();

            if (makeFileDic.Any(makeProject => !CheckMakeProjectDependencies(makeProject)))
            {
                return; // Error Stop
            }
            Trace.TraceInformation("Make files have been checked for project dependencies -- Passed");
        }

        private void ScanMakeFiles()
        {
            // Hard-coded build scripts directory
            var srcfolder = Path.Combine(options.SourceDirectory, @"buildscripts");
            var files = Directory.EnumerateFiles(srcfolder, "*.mak", SearchOption.AllDirectories);

            makeFiles.Clear();
            foreach (var file in files)
            {
                makeFiles.Add(new MakeFile { FileName = file });
            }
            Trace.TraceInformation("Make files scan done. {0} project files", makeFiles.Count);
        }

        private void ProcessMakeFile()
        {
            makeFileDic.Clear();


            foreach (var makeFile in makeFiles)
            {

                makeFile.ProcessRawLines();

                if (makeFile.Projects == null)
                {
                    Trace.TraceError("Make File contains no projects: {0}", makeFile.FileName);
                    throw new Program.StopException(); // error
                }

                // Add Make file projects to an internal dictionary
                foreach (var project in makeFile.Projects)
                {

                    // Check for Duplicate projects across all Make files.
                    if (makeFileDic.ContainsKey(project.Key))
                    {
                        var temp = makeFiles.Where(m => m.Projects.ContainsKey(project.Key))
                                    .Select(m => m.FileName)
                                    .ToList();
                        var files = string.Join(", ", temp);

                        Trace.TraceError("Make: Duplicate Project [{0}] in Make files: {1}", project.Key, files);
                        throw new Program.StopException(); // error
                    }
                    makeFileDic.Add(project.Key, project.Value);
                }
            }
        }

        private bool CheckMakeProjectDependencies(KeyValuePair<string, string[]> makeProject)
        {
            var makefile = makeFiles.FirstOrDefault(m => m.Projects.ContainsKey(makeProject.Key));
            if (makefile == null)
            {
                Trace.TraceError("Internal Error unknown make file for make project {0}", makeProject.Key);
                return false; // This should never happen
            }

            if (makeProject.Value.Contains(makeProject.Key))
            {
                Trace.TraceError("Make Project [{0}] contains a reference to itself in file {1}", makeProject.Key,
                    makefile.FileName);
                return false;
            }

            // Short Cut, that is needed. 
            if (makeProjectDependenciesCheckingList.ContainsKey(makeProject.Key) &&
                makeProjectDependenciesCheckingList[makeProject.Key].Equals(true))
            {
                return true; // great already been checked
            }


            makeProjectDependenciesCheckingList.Add(makeProject.Key, false);
            dependencyStack.Push(makeProject.Key);
            foreach (var projectRefenence in makeProject.Value)
            {
                if (!makeFileDic.ContainsKey(projectRefenence))
                {
                    Trace.TraceError(
                        "There is no make project: [{0}] that is reference by make project [{1}] in file {2}",
                        projectRefenence, makeProject.Key, makefile.FileName);
                    return false; // Error Missing projectRefenence
                }

                if (dependencyStack.Contains(projectRefenence))
                {
                    var depency = string.Join(", ", dependencyStack.ToArray());
                    Trace.TraceError("Make file circular dependency detected. From: [{0}] ({1}) in file {2}",
                        makeProject.Key, depency, makefile.FileName);
                    return false;
                }

                // Go Deep and check all projectRefenence for this project
                var goDeep = makeFileDic.FirstOrDefault(d => d.Key == projectRefenence);
                if (!CheckMakeProjectDependencies(goDeep))
                {
                    return false; // push up the return value
                }
            }

            // Clean up the Stack and mark what has been check as ok
            makeProjectDependenciesCheckingList[makeProject.Key] = true;
            dependencyStack.Pop();
            return true;
        }


        private class MakeFile
        {
            public string FileName { get; set; }

            // TODO  // Used by the Format and compliance check  feature not done
            // public string Text { get; set; }
            // makeFile.Text = File.ReadAllText(makeFile.FileName); 

            /// <summary>
            /// Array of the make file Raw Lines, order as per file contents
            /// </summary>
            public string[] RawLines { get; private set; }

            /// <summary>
            /// Array of the make file Project Lines (Project lines squashed to single line and non project lines removed)
            /// </summary>
            public List<string> ProjectsLines { get; private set; }

            /// <summary>
            /// The Root Project (the one that lists all projects in the file, Always at the top and first by design)
            /// </summary>
            public string Root { get; set; }

            /// <summary>
            /// Map of Projects and there Dependencies
            /// </summary>
            public Dictionary<string, string[]> Projects { get; private set; }

            public MakeFile()
            {
                Projects = new Dictionary<string, string[]>();
                ProjectsLines = new List<string>();
                RawLines = new string[] { };
            }

            /// <summary>
            /// Process the Raw Lines into Project and Dependencies
            /// </summary>
            public void ProcessRawLines()
            {
                if (string.IsNullOrEmpty(FileName))
                {
                    Trace.TraceError("Internal Error File Name Null");
                    throw new Program.StopException();
                }

                // Read from the file
                RawLines = File.ReadAllLines(FileName);
                if (RawLines.Length == 0)
                {
                    Trace.TraceError("Internal Error Make File {0} has no lines, ", FileName);
                    throw new Program.StopException();
                }

                // Ok this is some crappy code that just works 

                // Step 1, Combine lines with \ line continue ending character into a single line
                var lines1 = new List<string>();
                var lineContinues = false;

                var newline = string.Empty;
                foreach (var line in RawLines)
                {
                    var thisLine = line;
                    if (lineContinues)
                    {
                        thisLine = newline + thisLine;
                    }

                    if (thisLine.EndsWith(@"\"))
                    {
                        var i = thisLine.IndexOf(@"\", StringComparison.Ordinal);
                        var t = thisLine.Remove(i);
                        newline = t;
                        lineContinues = true;
                    }
                    else
                    {
                        lines1.Add(thisLine);
                        lineContinues = false;
                    }
                }

                // Step 2, Strip out Make Projects lines 
                ProjectsLines = new List<string>();
                foreach (var line in lines1)
                {
                    // Space in here is a known mistake, Rebuilding the make files will fix this
                    if (!Regex.IsMatch(line, @"^[a-zA-Z0-9_\.\- ]*:")) continue;

                    // Remove Tabs expressions
                    var temp = line.Replace("\t", " ");
                    ProjectsLines.Add(temp);
                }

                Projects = new Dictionary<string, string[]>();
                // Step 3, Split the Line in the Project and Dependences
                foreach (var line in ProjectsLines)
                {
                    var lineSplit = line.Split(':');
                    if (lineSplit.Count() != 2)
                    {
                        Trace.TraceError("Internal Error Processing {0} at line {1}, line had more that one : in it, no good reason for this", FileName, line);
                        throw new Program.StopException(); // error
                    }

                    var project = lineSplit[0].Trim(); // There may be a space, known problem that is ok with make files
                    var dependences = lineSplit[1].Split(' ');
                    // Remove empty strings
                    dependences = dependences.Where(dependence => !string.IsNullOrEmpty(dependence)).ToArray();

                    // The First line is the include list in the file, mark it as special 
                    if (string.IsNullOrEmpty(Root))
                    {
                        Root = project;
                    }
                    Projects.Add(project, dependences);
                }
            }
        }
    }
}