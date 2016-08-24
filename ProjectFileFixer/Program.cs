using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;

namespace ProjectFileFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (args.Length < 1) throw new Exception("No Stage to do");

            new Program().Execute(args[0]);
        }

        private void Execute(string stage)
        {
            string[] sourceSearchPatterns = { "*.csproj" };
            const string sourceCheckRootFolder = @"C:\Dev\";

            var sourceFileList = new List<string>();

            var referenceRules = new List<ReferenceRules>
            {
                new ReferenceRules {Name = "log4net", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.ActiveRecord", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.Components.Validator", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.Core", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.DynamicProxy2", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.Facilities.ActiveRecordIntegration", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.Facilities.AutomaticTransactionManagement", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.MicroKernel", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.Services.Transaction", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Castle.Windsor", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Rhino.Mocks", RemoveAllMetaData = true},
                new ReferenceRules {Name = "nunit.framework", RemoveAllMetaData = true},
            };

            // Stage Rules
            if (stage.Contains("2")) stage = stage + "1";
            if (stage.Contains("3")) stage = stage + "1";
            if (stage.Contains("4")) stage = stage + "1";

            // Run Stages
            if (stage.Contains("1"))
            {
                sourceFileList = Stage1FindProjectFiles(sourceCheckRootFolder, sourceSearchPatterns);
                Console.WriteLine($"Scanned returned {sourceFileList.Count} files");
            }
            if (stage.Contains("2"))
            {
                Stage2ProjectRemoveMetaData(sourceFileList);
            }

            if (stage.Contains("3"))
            {
                Stage3ProjectRemoveVersionNumbers(sourceFileList, referenceRules);
            }

            if (stage.Contains("4"))
            {
                Stage4ProjectMarkAsDirty(sourceFileList);
            }
        }


        List<string> Stage1FindProjectFiles(string rootFolder, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                .SelectMany(searchPattern => Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
            return files.ToList();
        }

        void Stage2ProjectRemoveMetaData(List<string> sourceFileList)
        {
            // Process each file in parallel
            //Parallel.ForEach(sourceFileList, Stage2aProjectRemoveMetaData);

            foreach (var fileName in sourceFileList)
            {
                Stage2aProjectRemoveMetaData(fileName);
            }
        }

        void Stage2aProjectRemoveMetaData(string fileName)
        {
            var project = new Project(fileName);

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
                    if (String.Compare(test, 0, reference.EvaluatedInclude, 0, test.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        reference.RemoveMetadata(@"Name");
                    }
                }

                if (temp[0].Contains(@"Tsd."))
                {
                    // May do something if an TSD project
                }
            }
            if (project.IsDirty)
                Console.WriteLine($"Changed: {fileName}");
            project.Save();
        }

        void Stage3ProjectRemoveVersionNumbers(List<string> sourceFileList, List<ReferenceRules> rules)
        {
            foreach (var fileName in sourceFileList)
            {
                Stage3aProjectRemoveVersionNumbers(fileName, rules);
            }
        }

        void Stage3aProjectRemoveVersionNumbers(string fileName,List<ReferenceRules> rules)
        {
            // Set the Correct version of different DLL

            var project = new Project(fileName);

            var references = project.GetItems("Reference");
            foreach (ProjectItem reference in references)
            {
                foreach (var rule in rules)
                {
                    if (!reference.UnevaluatedInclude.Contains(rule.Name)) continue; // Case Search ?
                    if (rule.RemoveAllMetaData && reference.UnevaluatedInclude.Length != rule.Name.Length)
                    {
                        reference.UnevaluatedInclude = rule.Name;
                    }
                }
            }
            if (project.IsDirty)
                Console.WriteLine($"Changed: {fileName}");
            project.Save();
        }

        void Stage4ProjectMarkAsDirty(List<string> sourceFileList)
        {
            foreach (var fileName in sourceFileList)
            {
                var project = new Project(fileName);
                project.MarkDirty();
                project.Save();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Application exiting with error:{exception?.Message ?? "UnknownReason"}");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(-1);
        }

        // Expain to suit needs
        class ReferenceRules
        {
            public string Name;
            public bool RemoveAllMetaData; // i.e Version, Culture, PublicKeyToken, and processorArchitecture"
        }
    }
}
