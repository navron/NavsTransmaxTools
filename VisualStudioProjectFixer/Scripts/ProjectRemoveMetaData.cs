using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.Build.Evaluation;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("ProjectRemoveMetaData", HelpText = "")]
    public class ProjectRemoveMetaData
    {
        [Option('f', "file", HelpText = "CS Project File")]
        public string FileName { get; set; }

        public void Run()
        {
            if (!FileName.Contains("csproj")) return;
            var project = new Project(FileName);

            var references = project.GetItems("Reference");
            foreach (ProjectItem reference in references)
            {
                var include = reference.EvaluatedInclude;
                var temp = include.Split(',');

                if (reference.HasMetadata(@"RequiredTargetFramework"))
                    reference.RemoveMetadata(@"RequiredTargetFramework");

                // Remove All SpecificVersion and HintPath
                if (reference.HasMetadata(@"SpecificVersion"))
                    reference.RemoveMetadata(@"SpecificVersion");

                if (reference.HasMetadata(@"HintPath"))
                    reference.RemoveMetadata(@"HintPath");

                // if Name is the same as the included then remove it
                if (reference.HasMetadata(@"Name"))
                {
                    var test = reference.GetMetadataValue(@"Name");
                    if (string.Compare(test, 0, reference.EvaluatedInclude, 0, test.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        reference.RemoveMetadata(@"Name");
                    }
                }

                if (temp[0].Contains(@"Tsd."))
                {
                    // May do something if an TSD project
                }
            }
            if (project.IsDirty) Console.WriteLine($"Changed: {FileName}");
            project.Save();
        }

        static void ProjectRemoveVersionNumbers(List<string> sourceFileList, List<Config.ReferenceRules> rules)
        {
            foreach (var fileName in sourceFileList)
            {
                // Stage3aProjectRemoveVersionNumbers(fileName, rules);
            }
        }


        static List<string> Stage1FindProjectFiles(string rootFolder, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                .SelectMany(searchPattern =>Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
            return files.ToList();
        }

        private static void RemoveSystemCore(List<string> sourceFileList)
        {
            foreach (var file in sourceFileList)
            {
                if (file.Contains(".csproj"))
                {
                    var project = new Project(file);

                    var references = project.GetItems("Reference");

                    foreach (var reference in references)
                    {
                        if (reference.Xml.Include.Contains("System.Core"))
                        {
                            project.RemoveItem(reference);
                            Console.WriteLine($"{reference.Xml.Include} has been removed from {project.FullPath}");
                            break;
                        }
                    }
                    if (project.IsDirty) Console.WriteLine($"Changed: {file}");

                    project.Save();
                }
                //else if (file.Contains(".config") || file.Contains(".Config"))
                //{
                //    var lineList = File.ReadAllLines(file).ToList();
                //    lineList = lineList.Where(x => !x.Contains("System.Core")).ToList();
                //    File.WriteAllLines(file, lineList.ToArray());
                //    Console.WriteLine($"Changed: {file}");
                //}
            }
        }
    }
}