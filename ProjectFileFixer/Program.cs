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

            new Program().Execute(args[0], args[1]);
        }

        private void Execute(string stage, string sourceCheckRootFolder)
        {
            string[] sourceSearchPatterns = { "*.csproj", "*.config", "*.Config" };
            //            const string sourceCheckRootFolder = @"C:\Dev\tools\";

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
                new ReferenceRules {Name = "Iesi.Collections", RemoveAllMetaData = true},
                new ReferenceRules {Name = "Caliburn.Micro", RemoveAllMetaData = true},
                new ReferenceRules {Name = "PresentationUI", RemoveAllMetaData = true},
                new ReferenceRules {Name = "NHibernate", RemoveAllMetaData = true},
                new ReferenceRules {Name = "NHibernate.ByteCode.Castle", RemoveAllMetaData = true},
                new ReferenceRules {Name = "NVelocity", RemoveAllMetaData = true},
                new ReferenceRules {Name = "CabLib", RemoveAllMetaData = true},
                new ReferenceRules {Name = "aspdu.net", RemoveAllMetaData = true},
                new ReferenceRules {Name = "NConsole", RemoveAllMetaData = false},


                new ReferenceRules {Name = "System", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Configuration", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Core", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Data", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Data.DataSetExtensions", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Data.SqlServerCe", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Drawing", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.EnterpriseServices", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Management.Automation", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.ServiceModel", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Web", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Web.Mobile", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Web.Services", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Xml", RemoveAllMetaData = true},
                new ReferenceRules {Name = "System.Xml.Linq", RemoveAllMetaData = true},

            };

            // Stage Rules
            if (stage.Contains("2")) stage = stage + "1";
            if (stage.Contains("3")) stage = stage + "1";
            if (stage.Contains("4")) stage = stage + "1";
            if (stage.Contains("5")) stage = stage + "1";
            if (stage.Contains("6")) stage = stage + "1";
            if (stage.Contains("7")) stage = stage + "1";
            if (stage.Contains("8")) stage = stage + "1";

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

            if (stage.Contains("5"))
            {
                Stage5TsdProjectsAreVersion1(sourceFileList);
            }
            if (stage.Contains("6"))
            {
                Stage6ProjectSetDotVersion(sourceFileList, "4.6.2");
            }
            if (stage.Contains("7"))
            {
                Stage7RemoveSystemCore(sourceFileList);
            }
            if (stage.Contains("8"))
            {
                State8RemovePerfer32Bit(sourceFileList);
            }
        }

        /// <summary>
        /// Set all executables to 64bit
        /// </summary>
        /// <param name="sourceFileList"></param>
        private void State8RemovePerfer32Bit(List<string> sourceFileList)
        {
            foreach (var file in sourceFileList)
            {
                if (file.Contains(".csproj"))
                {
                    var project = new Project(file);

                    var outputtype = project.GetProperty("OutputType");

                    if (outputtype.EvaluatedValue.Contains("Exe"))
                    {
                        project.SetProperty("Prefer32Bit", "false");
                    }

                    if (project.IsDirty) Console.WriteLine($"Changed: {file}");

                    project.Save();
                }
            }
        }

        List<string> Stage1FindProjectFiles(string rootFolder, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                .SelectMany(
                    searchPattern => Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
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
            if (!fileName.Contains("csproj")) return;
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
                    if (
                        String.Compare(test, 0, reference.EvaluatedInclude, 0, test.Length,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        reference.RemoveMetadata(@"Name");
                    }
                }

                if (temp[0].Contains(@"Tsd."))
                {
                    // May do something if an TSD project
                }
            }
            if (project.IsDirty) Console.WriteLine($"Changed: {fileName}");
            project.Save();
        }

        void Stage3ProjectRemoveVersionNumbers(List<string> sourceFileList, List<ReferenceRules> rules)
        {
            foreach (var fileName in sourceFileList)
            {
                Stage3aProjectRemoveVersionNumbers(fileName, rules);
            }
        }

        void Stage3aProjectRemoveVersionNumbers(string fileName, List<ReferenceRules> rules)
        {
            // Set the Correct version of different DLL
            if (!fileName.Contains("csproj")) return;
            var project = new Project(fileName);

            var references = project.GetItems("Reference");
            foreach (ProjectItem reference in references)
            {
                foreach (var rule in rules)
                {
                    // Need to handle name that include other names etc
                    // Castle.ActiveRecord and Castle
                    // NHibernate NHibernate.ByteCode.Castle

                    // Does 
                    if (rule.RemoveAllMetaData && reference.UnevaluatedInclude.Contains(rule.Name + ","))
                    {
                        reference.UnevaluatedInclude = rule.Name;
                    }
                }
            }
            if (project.IsDirty) Console.WriteLine($"Changed: {fileName}");
            project.Save();
        }

        void Stage4ProjectMarkAsDirty(List<string> sourceFileList)
        {
            foreach (var fileName in sourceFileList)
            {
                if (!fileName.Contains("csproj")) continue;
                var project = new Project(fileName);
                project.MarkDirty();
                project.Save();
            }
        }

        void Stage5TsdProjectsAreVersion1(List<string> sourceFileList)
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
                                reference.UnevaluatedInclude = String.Join(",", values);
                            }
                        }

                    }
                }
                if (project.IsDirty) Console.WriteLine($"Changed: {fileName}");
                project.Save();
            }
        }

        void Stage6ProjectSetDotVersion(List<string> sourceFileList, string version)
        {
            foreach (var fileName in sourceFileList)
            {
                Stage6aProjectSetDotVersion(fileName, version);
            }
        }


        void Stage6aProjectSetDotVersion(string fileName, string version)
        {
            //TODO
            // Set the Correct version of different DLL


            //var project = new Project(fileName);

            //var references = project.GetItems("Reference");
            //foreach (ProjectItem reference in references)
            //{
            //    foreach (var rule in rules)
            //    {
            //        // Need to handle name that include other names etc
            //        // Castle.ActiveRecord and Castle
            //        // NHibernate NHibernate.ByteCode.Castle

            //        // Does 
            //        if (rule.RemoveAllMetaData && reference.UnevaluatedInclude.Contains(rule.Name + ","))
            //        {
            //            reference.UnevaluatedInclude = rule.Name;
            //        }
            //    }
            //}
            //if (project.IsDirty) Console.WriteLine($"Changed: {fileName}");
            //project.Save();
        }

        private void Stage7RemoveSystemCore(List<string> sourceFileList)
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
                else if (file.Contains(".config") || file.Contains(".Config"))
                {
                    var lineList = File.ReadAllLines(file).ToList();
                    lineList = lineList.Where(x => !x.Contains("System.Core")).ToList();
                    File.WriteAllLines(file, lineList.ToArray());
                    Console.WriteLine($"Changed: {file}");
                }
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Application exiting with error:{exception?.Message ?? "UnknownReason"}");
            Console.WriteLine(exception?.StackTrace);
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