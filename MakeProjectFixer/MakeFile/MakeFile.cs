﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MakeProjectFixer.MakeFile
{
    public class MakeFile
    {
        // Space in here is a known mistake, Rebuilding the make files will fix this
        readonly Regex projectChecker = new Regex(@"^[a-zA-Z0-9_\.\- ]*:");

        public MakeFile()
        {
            Projects = new List<MakeProject>();
        }

        /// <summary>
        /// Array of the make file Raw Lines, order as per file contents
        /// </summary>
        /// <remarks>Used to check compliance format</remarks>
        internal string[] RawLines { get; set; }

        /// <summary>
        /// SingleFile name full path
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The Project Header
        /// </summary>
        public MakeProject Header { get; set; }

        /// <summary>
        /// List of Project Configurations
        /// </summary>
        public List<MakeProject> Projects { get; set; }

        public void ReadFile(string fileName)
        {
            RawLines = File.ReadAllLines(fileName);
            FileName = fileName;
            ProcessMakeFileRawLines(RawLines.ToList());
        }

        public void WriteFile(int lineLength, bool sortProjects)
        {
            var output = FormatFile(lineLength, sortProjects);
            File.WriteAllLines(FileName, output);
        }

        internal void ProcessMakeFileRawLines(List<string> rawLines)
        {
            //Process the header
            var headerLines = GetRawHeaderLines(rawLines);
            Header = ProcessRawProject(headerLines);

            //Process the body
            int startpos = headerLines.Count;
            while (startpos < rawLines.Count)
            {
                var c = ProcessBody(rawLines, startpos);
                startpos += c.RawLines.Count;
                Projects.Add(c);
            }
        }

        internal MakeProject ProcessBody(List<string> rawlines, int startPos)
        {
            var lines = rawlines.GetRange(startPos, rawlines.Count - startPos);
            var projectLines = GetRawProjectLines(lines);
            var c = ProcessRawProject(projectLines);
            return c;
        }

        internal List<string> GetRawHeaderLines(IList<string> rawLines)
        {
            // var combineLines = MakeFileHelper.CombineLines(rawLines);
            // Find the header stop which is the end of file or first project
            var afterFirst = false;
            var lines = new List<string>();
            foreach (var line in rawLines)
            {
                var match = projectChecker.IsMatch(line);
                if (!match)
                {
                    if (afterFirst && !string.IsNullOrWhiteSpace(line) && line.StartsWith("#"))
                    {
                        break;
                    }

                    lines.Add(line);
                    continue;
                }
                if (afterFirst) break;
                afterFirst = true;
                lines.Add(line);
            }
            return lines;
        }

        internal List<string> GetRawProjectLines(IList<string> rawLines)
        {
            // var combineLines = MakeFileHelper.CombineLines(rawLines);
            // Find the Project stop which is the end of file or a line that is not empty and doesn't start with a tab 
            var afterMatch = false;
            var lines = new List<string>();
            foreach (var line in rawLines)
            {
                if (!afterMatch)
                {
                    lines.Add(line);
                    afterMatch = projectChecker.IsMatch(line);
                    continue;
                }
                if (string.IsNullOrEmpty(line) || line.StartsWith("\t"))
                {
                    lines.Add(line);
                    continue;
                }
                break;
            }
            return lines;
        }

        // Process both the Head and Project Sections
        internal MakeProject ProcessRawProject(IList<string> rawLines)
        {
            var combineLines = MakeFileHelper.CombineLines(rawLines);
            var c = new MakeProject();
            // Fine the header stop which is the end of file or first project
            var afterName = false;
            foreach (var line in combineLines)
            {
                var match = projectChecker.IsMatch(line);
                if (!match && !afterName)
                {
                    c.PreLines.Add(line);
                    continue;
                }
                afterName = true;
                if (match)
                {
                    var lineSplit = line.Split(':');
                    if (lineSplit.Count() != 2)
                    {
                        throw new Exception(
                            $"Internal Error Processing {FileName} at line {line}, line had more that one : in it, no good reason for this");
                    }
                    c.ProjectName = lineSplit[0].Trim();
                    // There may be a space, known problem that is ok with make files
                    var dependences = lineSplit[1].Split(' ', '\t');
                    c.DependencyProjects = MakeFileHelper.CleanLines(dependences);

                    continue;
                }
                c.PostLines.Add(line);
            }
            c.RawLines = rawLines;
            return c;
        }


        /// <summary>
        /// Returns a formatted make file
        /// </summary>
        public List<string> FormatFile(int lineLength, bool sortProjects)
        {
            var output = new List<string>();
            output.AddRange(Header.FormatMakeProject(lineLength, sortProjects));

            if (sortProjects)
            {
                Projects.Sort((a, b) => string.Compare(a.ProjectName, b.ProjectName, StringComparison.OrdinalIgnoreCase));
            }

            foreach (MakeProject project in Projects)
            {
                output.Add(string.Empty);
                output.AddRange(project.FormatMakeProject(lineLength, sortProjects));
            }

            return output;
        }

        public bool ScanForErrorsExtraDependencyInTheMakeFileHeader(bool fixProblems)
        {
            // Check For Directory Make file
            if (IsDirectoryMakeFile())
            {
                return ScanForErrorsExtraDependencyInTheMakeFileHeaderForDirectoryMakeFile(fixProblems);
            }

            var removeItems = new List<string>();
            var problems = false;
            foreach (var headerDependencyName in Header.DependencyProjects)
            {
                if (Projects.Any(p => p.ProjectName == headerDependencyName)) continue;

                // Extra Dependency in the make file header -- Report
                Console.WriteLine($"Extra \"{headerDependencyName}\" Dependency in make file: {FileName}");
                problems = true;
                if (!fixProblems) continue;
                Console.WriteLine($"Deleting Dependency {headerDependencyName} from make file {FileName} header section");
                removeItems.Add(headerDependencyName);
            }
            if (!fixProblems) return problems;
            foreach (var removeItem in removeItems)
            {
                Header.DependencyProjects.Remove(removeItem);
            }
            return problems;
        }

        private bool IsDirectoryMakeFile()
        {
            var name = Path.GetFileNameWithoutExtension(FileName);
            var folder = Path.GetDirectoryName(FileName);
            if (name == null || folder == null)
                throw new Exception($"FileName is invalid: {FileName} --aborting");
            return folder.Contains(name);
        }

        private bool ScanForErrorsExtraDependencyInTheMakeFileHeaderForDirectoryMakeFile(bool fixProblems)
        {
            var folder = Path.GetDirectoryName(FileName);
            if (folder == null) throw new Exception($"FileName is invalid: {FileName} --aborting");
            var folderName = Path.GetFileNameWithoutExtension(FileName);

            var files = Directory.EnumerateFiles(folder, "*.mak", SearchOption.TopDirectoryOnly).ToList();
            var fileProjects= new List<string>();
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var temp = $"{folderName}_{name}";
                fileProjects.Add(temp);
            }
            var problems = false;
            var removeItems = new List<string>();
            foreach (var headerDependencyName in Header.DependencyProjects)
            {
                // Any file is ok
                if (fileProjects.Any(f =>f == headerDependencyName)) continue;
                // and any project in the make file is ok
                if (Projects.Any(p => p.ProjectName == headerDependencyName)) continue;

                // Extra Dependency in the make file header -- Report
                Console.WriteLine($"Extra \"{headerDependencyName}\" Dependency in make file: {FileName}");
                problems = true;
                if (!fixProblems) continue;
                Console.WriteLine($"Deleting Dependency {headerDependencyName} from make file {FileName} header section");
                removeItems.Add(headerDependencyName);
            }
            if (!fixProblems) return problems;
            foreach (var removeItem in removeItems)
            {
                Header.DependencyProjects.Remove(removeItem);
            }
            return problems;
        }

        public bool ScanForErrorsMissingDependencyFromTheMakeFileHeader(bool fixProblems)
        {
            var problems = false;
            foreach (var project in Projects)
            {
                if (Header.DependencyProjects.Any(p => p == project.ProjectName)) continue;

                // Missing Dependency from the make file header
                Console.WriteLine($"Missing Dependency in header for \"{project.ProjectName}\"");
                problems = true;
                if (!fixProblems) continue;
                Console.WriteLine($"Add Dependency {project.ProjectName} to make file {FileName} header section");
                Header.DependencyProjects.Add(project.ProjectName);
            }
            return problems;
        }

        public bool ScanForErrorsProjectHeaderSyntax(bool fixProblems)
        {
            var problems = false;
            Regex projectCheckerOk = new Regex(@"^[a-zA-Z0-9_\.\- ]*:");
            Regex projectCheckerSpaceMissing = new Regex(@"^[a-zA-Z0-9_\.\- ]*:");
            foreach (var line in RawLines)
            {
                if (projectCheckerSpaceMissing.IsMatch(line)
                    && !projectCheckerOk.IsMatch(line))
                {
                    Console.WriteLine($"Found Syntax problem in file \"{FileName}\", Run MakeFormat to fix with default options");
                    problems = true;
                }
            }
            return problems;
        }

        /// <summary>
        /// Search all the Project Raw lines for copy .h files to COM\Include
        /// This is how to determine c++ references
        /// </summary>
        public void ProcessPublishItems()
        {
            //TODO
        }
    }
}
