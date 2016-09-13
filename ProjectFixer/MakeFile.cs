using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectFixer
{
    public class MakeFileProjectContents
    {
        public MakeFileProjectContents()
        {
            RawLines = new List<string>();
            PreLines = new List<string>();
            PostLines = new List<string>();
            DependencyProjects = new List<string>();
        }
        /// <summary>
        /// Project ProjectName
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// List of Current Projects marked are Dependencies
        /// </summary>
        public List<string> DependencyProjects { get; set; }
        /// <summary>
        /// Lines above the ProjectName Line, these are always comments
        /// </summary>
        public List<string> PreLines { get; set; }
        /// <summary>
        /// Bash script lines for this make file project
        /// </summary>
        public List<string> PostLines { get; set; }

        public IList<string> RawLines { get; set; }

        public string FormatMakContents(int lineLength)
        {
            var s = new StringBuilder("NOT DONE");
            return s.ToString();
        }

    }

    public class MakeFile
    {
        readonly Regex projectChecker = new Regex(@"^[a-zA-Z0-9_\.\-]*:");
        // Space in here is a known mistake, Rebuilding the make files will fix this
        Regex projectChecker2 = new Regex(@"^[a-zA-Z0-9_\.\- ]*:");

        public MakeFile()
        {
            Projects = new Dictionary<string, MakeFileProjectContents>();
        }


        /// <summary>
        /// Array of the make file Raw Lines, order as per file contents
        /// </summary>
        /// <remarks>Used to check compliance format</remarks>
        internal List<string> RawLines { get; set; }

        /// <summary>
        /// File name full path
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The Project Header
        /// </summary>
        public MakeFileProjectContents Header { get; set; }

        /// <summary>
        /// Array of the make file Project Lines (Project lines squashed to single line and non project lines removed)
        /// </summary>
        public Dictionary<string, MakeFileProjectContents> Projects { get; set; }

        public void ProcessFile(string fileName)
        {
            RawLines = File.ReadLines(fileName).ToList();
            FileName = fileName;
            ProcessMakeFileRawLines(RawLines);
        }

        public void ProcessMakeFileRawLines(List<string> rawLines)
        {
            //Process the header
            var headerLines = GetRawHeader(rawLines);
            Header = ProcessRawHeader(headerLines);

            //Process the body
            int startpos = headerLines.Count;
            while (startpos < rawLines.Count)
            {
                var c = ProcessBody(rawLines, startpos);
                startpos += c.RawLines.Count;
                Projects.Add(c.ProjectName,c);
            }
        }

        public MakeFileProjectContents ProcessBody(List<string> rawlines, int startPos)
        {
            var lines = rawlines.GetRange(startPos, rawlines.Count - startPos);
            var projectLines = GetRawProjectLines(lines);
            var c = ProcessRawHeader(projectLines);
            return c;
        }



        public List<string> GetRawHeader(IList<string> rawLines)
        {
            // Find the header stop which is the end of file or first project
            var afterFirst = false;
            var lines = new List<string>();
            foreach (var line in rawLines)
            {
                var match = projectChecker.IsMatch(line);
                if (!match)
                {
                    lines.Add(line);
                    continue;
                }
                if(afterFirst) break;
                afterFirst = true;
                lines.Add(line);
            }
            return lines;
        }
        public List<string> GetRawProjectLines(IList<string> rawLines)
        {
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

        // Combine lines with slash \ line continue ending character into a single line
        public List<string> CombineLines(IList<string> rawLines)
        {
            var combineLines = new List<string>();
            var lineContinues = false;

            var newline = string.Empty;
            foreach (var line in rawLines)
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
                    combineLines.Add(thisLine);
                    lineContinues = false;
                }
            }
            return combineLines;
        }


        public MakeFileProjectContents ProcessRawHeader(IList<string> rawLines)
        {
            var c = new MakeFileProjectContents();
            // Fine the header stop which is the end of file or first project
            var afterName = false;
            foreach (var line in rawLines)
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
                        throw new Exception($"Internal Error Processing {FileName} at line {line}, line had more that one : in it, no good reason for this");
                    }
                    c.ProjectName = lineSplit[0].Trim(); // There may be a space, known problem that is ok with make files
                    var dependences = lineSplit[1].Split(' ');
                    // Remove empty strings
                    c.DependencyProjects = dependences.Where(dependence => !string.IsNullOrEmpty(dependence)).ToList();

                    continue;
                }
                c.PostLines.Add(line);
            }
            c.RawLines = rawLines;
            return c;
        }

        /// <summary>
        /// Format the header section
        /// </summary>
        /// <param name="lineLength">Max length of the line before applying an slash </param>
        /// <param name="preComment"></param>
        public string FormatHeader(int lineLength, string preComment)
        {
            var s = new StringBuilder("NOT DONE");

            return s.ToString();
        }

        /// <summary>
        /// Returns a formated mak file
        /// </summary>
        /// <returns></returns>
        public string FormatFile()
        {
            var items = from project in Projects orderby project.Key ascending select project;

            var s = new StringBuilder("NOT DONE");


            return s.ToString();


        }
    }
}
