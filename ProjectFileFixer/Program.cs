using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectFileFixer
{
    public enum Stages
    {
        FindAll = 1,
        UpgradeToVS15,
        RemoveMeta,
        RemoveVersion,
        MarkAsDirty,
        SetTsdVersion,
        Stage7,
        RemoveSystemCore,
        Perfer64BIt,
        RemoveDotNet35WebConfig,
        ChangeCSTargetTov14,
        SetWorkStationProjectsToAnyCPU
    }

    class Program
    {
        private const string help = "Usage: <source directory> <stage> <stage>...\n" +
                                    "   Each stage is additive\n" +
                                    "       e.g C:/git/streams 1 2 3\n" +
                                    "Stages:\n" +
                                    "   1: Find all csproj files in a give directory\n" +
                                    "   2: Upgrade all csprog file to toolsversion 14.0 and framework 4.6.2\n" +
                                    "   3: Remove meta data from refernces\n" +
                                    "   4: Remove version numbers from refernces\n" +
                                    "   5: Mark project as dirty\n" +
                                    "   6: Set Tsd refernces to version 1.0.0.0\n" +
                                    "   7: Does nothing at the moment\n" +
                                    "   8: Removes System.Core from project files the given directory\n" +
                                    "   9: Set executables to perfer 64 bit\n" +
                                    "   10: Remove .net 3.5 version numbers from web config files\n";


        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (args.Length < 1 || args[0] == "-h")
            {
                Console.WriteLine(help);
                return;
            }

            string[] sourceSearchPatterns = { "*.csproj", "*.config", "*.Config" };
            //            const string sourceCheckRootFolder = @"C:\Dev\tools\";

            var sourceFileList = new List<string>();
            var sourceCheckRootFolder = args[0];
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
            var stages = new List<Stages>();
            for (int i = 1; i < args.Count(); i++)
            {
                stages.Add((Stages)Enum.Parse(typeof(Stages), args[i]));
            }

            sourceFileList = Stage1FindProjectFiles(sourceCheckRootFolder, sourceSearchPatterns);
            Console.WriteLine($"Scanned returned {sourceFileList.Count} files");

            foreach (var stage in stages)
            {
                switch (stage)
                {
                    case (Stages.FindAll):
                        {
                            break;
                        }
                    case Stages.UpgradeToVS15:
                        {
                            UpgradeProjectsToVS2015(sourceCheckRootFolder);
                            break;
                        }
                    case Stages.RemoveMeta:
                        {
                            ProjectRemoveMetaData(sourceFileList);
                            break;
                        }
                    case Stages.RemoveVersion:
                        {
                            ProjectRemoveVersionNumbers(sourceFileList, referenceRules);
                            break;
                        }
                    case Stages.MarkAsDirty:
                        {
                            ProjectMarkAsDirty(sourceFileList);
                            break;
                        }
                    case Stages.SetTsdVersion:
                        {
                            TsdProjectsAreVersion1(sourceFileList);
                            break;
                        }
                    case Stages.Stage7:
                        {
                            ProjectSetDotVersion(sourceFileList, "4.6.2");
                            break;
                        }
                    case Stages.RemoveSystemCore:
                        {
                            RemoveSystemCore(sourceFileList);
                            break;
                        }
                    case Stages.Perfer64BIt:
                        {
                            RemovePerfer32Bit(sourceFileList);
                            break;
                        }
                    case Stages.RemoveDotNet35WebConfig:
                        {
                            RemoveDotNet35WebConfig(sourceFileList);
                            break;
                        }
                    case Stages.ChangeCSTargetTov14:
                        {
                            ChangeCSTargetTov14(sourceFileList);
                            break;
                        }
                    case Stages.SetWorkStationProjectsToAnyCPU:
                        {
                            SetWorkStationProjectsToAnyCPU(sourceFileList);
                            break;
                        }
                    default:
                        {
                            throw new Exception("Stage not found");
                        }

                }
            }
        }

        private static void SetWorkStationProjectsToAnyCPU(List<string> sourceFileList)
        {
            foreach (var filepath in sourceFileList)
            {
                if (filepath.Contains(@"\ws\") && filepath.Contains(".csproj"))
                {
                    var project = new Project(filepath);

                    var platformTargets = project.GetItems("PlatformTarget");

                    foreach (var platformTarget in platformTargets)
                    {
                        platformTarget.SetMetadataValue("PlatformTarget", "AnyCPU");
                    }

                    if (project.IsDirty) Console.WriteLine($"Changed: {filepath}");

                    project.Save();
                }
            }
        }

        private static void ChangeCSTargetTov14(List<string> sourceFileList)
        {
            foreach (var filepath in sourceFileList)
            {
                if (filepath.Contains(".csproj"))
                {
                    string text = File.ReadAllText(filepath);
                    text = text.Replace("Project=\"$(MSBuildExtensionsPath)\\Microsoft\\VisualStudio\\v9.0\\WebApplications\\Microsoft.WebApplication.targets\""
                                        , "Project=\"$(MSBuildExtensionsPath)\\Microsoft\\VisualStudio\\v14.0\\WebApplications\\Microsoft.WebApplication.targets\"");
                    File.WriteAllText(filepath, text, Encoding.UTF8);
                }
            }
        }

        private static void RemoveDotNet35WebConfig(List<string> sourceFileList)
        {
            foreach (var filepath in sourceFileList)
            {
                if (filepath.Contains(".config") || filepath.Contains(".Config"))
                {
                    string text = File.ReadAllText(filepath);
                    text = text.Replace("3.5", "4.0");
                    File.WriteAllText(filepath, text);
                }
            }
        }

        private static void UpgradeProjectsToVS2015(string sourcepath)
        {
            RunPowershell.ExecutePowerShellCommand(sourcepath);
        }
        /// <summary>
        /// Set all executables to 64bit
        /// </summary>
        /// <param name="sourceFileList"></param>
        private static void RemovePerfer32Bit(List<string> sourceFileList)
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

        static void ProjectRemoveMetaData(List<string> sourceFileList)
        {
            // Process each file in parallel
            //Parallel.ForEach(sourceFileList, Stage2aProjectRemoveMetaData);

            foreach (var fileName in sourceFileList)
            {
                Stage2aProjectRemoveMetaData(fileName);
            }
        }

        static void Stage2aProjectRemoveMetaData(string fileName)
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

        static void ProjectRemoveVersionNumbers(List<string> sourceFileList, List<ReferenceRules> rules)
        {
            foreach (var fileName in sourceFileList)
            {
                Stage3aProjectRemoveVersionNumbers(fileName, rules);
            }
        }

        static void Stage3aProjectRemoveVersionNumbers(string fileName, List<ReferenceRules> rules)
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
                                reference.UnevaluatedInclude = String.Join(",", values);
                            }
                        }

                    }
                }
                if (project.IsDirty) Console.WriteLine($"Changed: {fileName}");
                project.Save();
            }
        }

        static void ProjectSetDotVersion(List<string> sourceFileList, string version)
        {
            foreach (var fileName in sourceFileList)
            {
                Stage6aProjectSetDotVersion(fileName, version);
            }
        }


        static void Stage6aProjectSetDotVersion(string fileName, string version)
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

        static List<string> Stage1FindProjectFiles(string rootFolder, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                .SelectMany(
                    searchPattern => Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
            return files.ToList();
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
