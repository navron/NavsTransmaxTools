using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectFixer.MakeFile
{
    /// <summary>
    /// Store for a single Make File, Contains a Header and Project(s)
    /// </summary>
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

        /// <summary>
        /// Process the Make File and Build Make Projects, then process them
        /// </summary>
        /// <param name="fileName"></param>
        public void ProcessFile(string fileName)
        {
            // Build all the Make files 
            FileName = fileName;
            RawLines = File.ReadAllLines(FileName);
            ProcessMakeFileRawLines(RawLines.ToList());

            // Process MakeProject
            Projects.ForEach(mp => mp.ProcessFile());

            var folder = Path.GetDirectoryName(fileName).Split(Path.DirectorySeparatorChar).Last();
            Projects.ForEach(mp => mp.ProjectArea = folder);
        }

        public void WriteFile(Options options)
        {
            var output = FormatFile(options);
            File.WriteAllLines(FileName, output);
        }

        internal void ProcessMakeFileRawLines(List<string> rawLines)
        {
            //Process the header
            var headerLines = GetRawHeaderLines(rawLines);
            Header = ProcessRawProject(headerLines);
            Header.IsHeader = true;

            //Process the body
            int startpos = headerLines.Count;
            while (startpos < rawLines.Count)
            {
                var c = ProcessBody(rawLines, startpos);
                c.IsHeader = false;
                startpos += c.RawLines.Count;
                Projects.Add(c);
            }
        }

        private MakeProject ProcessBody(List<string> rawlines, int startPos)
        {
            var lines = rawlines.GetRange(startPos, rawlines.Count - startPos);
            var projectLines = GetRawProjectLines(lines);
            var c = ProcessRawProject(projectLines);
            return c;
        }

        internal List<string> GetRawHeaderLines(IList<string> rawLines)
        {
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
                    c.DependencyProjects = MakeFileHelper.CleanItems(dependences);

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
        public List<string> FormatFile(Options options)
        {
            var output = new List<string>();
            output.AddRange(Header.FormatMakeProject(options.FormatProject, options.LineLength, options.SortDependencies));

            if (options.SortProject)
            {
                Projects.Sort((a, b) => string.Compare(a.ProjectName, b.ProjectName, StringComparison.OrdinalIgnoreCase));
            }

            foreach (MakeProject project in Projects)
            {
                output.Add(string.Empty);
                output.AddRange(project.FormatMakeProject(options.FormatProject, options.LineLength, options.SortDependencies));
            }

            return output;
        }

        public bool IsDirectoryMakeFile()
        {
            var name = Path.GetFileNameWithoutExtension(FileName);
            var folder = Path.GetDirectoryName(FileName);
            if (name == null || folder == null)
                throw new Exception($"FileName is invalid: {FileName} --aborting");
            return folder.EndsWith($"\\{name}");
        }
    }
}
