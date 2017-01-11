using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;

namespace VisualStudioProjectFixer.Scripts
{
    static class Helper
    {
        // Expain to suit needs


        internal static List<string> GetProjectFiles(string rootFolder, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                .SelectMany(
                    searchPattern => Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
            return files.ToList();
        }


        static void ProjectMarkAsDirty(List<string> sourceFileList)
        {
            foreach (var fileName in sourceFileList)
            {
                if (!fileName.Contains("csproj")) continue;
                var project = new Project(fileName);
                project.MarkDirty();
                project.Save();
            }
        }

        static void TsdProjectsAreVersion1(List<string> sourceFileList)
        {
            const string tsdVersion = " Version=1.0.0.0"; // Note Space is required
            foreach (var fileName in sourceFileList)
            {
                if (!fileName.Contains("csproj")) continue;
                var project = new Project(fileName);
                var references = project.GetItems("Reference");
                foreach (var reference in references)
                {
                    var include = reference.EvaluatedInclude;
                    var values = include.Split(',');
                    if (values.Length > 1)
                    {
                        if (values[0].Contains(@"Tsd."))
                        {
                            if (!values[1].Contains(tsdVersion))
                            {
                                values[1] = tsdVersion;
                                reference.UnevaluatedInclude = string.Join(",", values);
                            }
                        }
                    }
                }
                if (project.IsDirty) Console.WriteLine($"Changed: {fileName}");
                project.Save();
            }
        }
    }
}