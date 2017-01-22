using System;
using System.Collections.Generic;
using System.Linq;

namespace MakeFileProjectFixer.MakeFile
{
    static class MakeFileHelper
    {
        // Combine lines with slash \ line continue ending character into a single line
        internal static List<string> CombineLines(IList<string> rawLines)
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

        internal static List<string> ExpandLines(string line, int lineLength)
        {
            var lines = new List<string>();
            var test = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string current = string.Empty;
            foreach (var s in test)
            {
                if (string.IsNullOrEmpty(current))
                {
                    current = s.Contains(":") ? s : "\t\t" + s;
                }
                else
                {
                    current = current + " " + s;
                }

                if (current.Length >= lineLength - 3)
                {
                    if (s != test.Last())
                    {
                        current = current + " \\";
                    }
                    lines.Add(current);
                    //New lines start with two tabs
                    current = string.Empty;
                }
            }
            if (!string.IsNullOrWhiteSpace(current))
            {
                lines.Add(current);
            }
            return lines;
        }

        /// <summary>
        /// Cleans a list of items
        /// </summary>
        /// <param name="rawItems"></param>
        /// <returns></returns>
        internal static List<string> CleanItems(string[] rawItems)
        {
            var list = rawItems.Select(r => r.Trim()).ToList();
            list.RemoveAll(string.IsNullOrWhiteSpace);
            return list;
        }
    }
}
