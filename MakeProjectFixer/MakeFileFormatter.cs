using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColorConsole;
using CommandLine;
using MakeProjectFixer.MakeFile;

namespace MakeProjectFixer
{
    [Verb("MakeFileFormat", HelpText = "Format Make Files")]
    internal class MakeFileFormatter : Options
    {
        public MakeFileFormatter()
        {
            SearchPatterns = new[] { "*.mak" };
        }

        public void Run()
        {
            Program.Console.WriteLine($"Running {this.GetType().Name}", ConsoleColor.Cyan);

            var files = Helper.FindFiles(this);
            Parallel.ForEach(files, (file) =>
            {
                if (Verbose) Console.WriteLine($"Formatting {file}");
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);
                make.WriteFile(LineLength, SortProject);
            });
        }
    }


    [Verb("MakeScanErrors", HelpText = "Scan Make Files for Errors")]
    internal class MakeFileScanForErrors : Options
    {
        public MakeFileScanForErrors()
        {
            SearchPatterns = new[] { "*.mak" };
        }

        [Option(@"scanall", HelpText = "Scan All Tests", Default = false)]
        public bool ScanAll { get; set; }

        [Option(@"ScanExtraDependencyInTheMakeFileHeader", HelpText = "Scan for ExtraDependencyInTheMakeFileHeader")]
        public bool ScanForErrorsExtraDependencyInTheMakeFileHeader { get; set; }

        [Option(@"ScanMissingDependencyFromTheMakeFileHeader",
             HelpText = "Scan for MissingDependencyFromTheMakeFileHeader")]
        public bool ScanForErrorsMissingDependencyFromTheMakeFileHeader { get; set; }

        [Option(@"ScanProjectHeaderSyntax", HelpText = "Scan for ProjectHeaderSyntax")]
        public bool ScanForErrorsProjectHeaderSyntax { get; set; }

        public void Run()
        {
            Program.Console.WriteLine($"Running {this.GetType().Name}", ConsoleColor.Cyan);

            var files = Helper.FindFiles(this);
            foreach (var file in files)

            //   Parallel.ForEach(files, (file) =>
            {
                if (Verbose) Console.WriteLine($"Scanning {file}");
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);

                if (ScanAll)
                    ScanForErrorsExtraDependencyInTheMakeFileHeader =
                        ScanForErrorsMissingDependencyFromTheMakeFileHeader =
                            ScanForErrorsProjectHeaderSyntax = true;

                var errorFound = false;
                if (ScanForErrorsExtraDependencyInTheMakeFileHeader)
                    errorFound |= make.ScanForErrorsExtraDependencyInTheMakeFileHeader();

                if (ScanForErrorsMissingDependencyFromTheMakeFileHeader)
                    errorFound |= make.ScanForErrorsMissingDependencyFromTheMakeFileHeader();

                if (ScanForErrorsProjectHeaderSyntax)
                    errorFound |= make.ScanForErrorsProjectHeaderSyntax();

                if (errorFound)
                {
                    make.WriteFile(LineLength, SortProject);
                }
            }
            //);
        }
    }


    [Verb("mfMatchVisualStudioProjectCase", HelpText = "Make Files Match VisualStudio Project Name Case")]
    internal class MatchMakeFileAndVisualStudioProjectCase : Store
    {
        public void Run()
        {
            Program.Console.WriteLine($"Running {this.GetType().Name}", ConsoleColor.Cyan);

            this.BuildStore();

            foreach (var visualStudioFile in VisualStudioFiles)
            {
                foreach (var reference in visualStudioFile.ExpectedMakeProjectReferences)
                {
                    if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.FoundCaseWrong)
                    {
                        var makeproject = MakeProjects.FirstOrDefault(m => string.Equals(m.ProjectName, reference.Key, StringComparison.OrdinalIgnoreCase));
                        if (makeproject != null)
                        {
                            Program.Console.WriteLine($"Project {visualStudioFile.ProjectName} has wrong case make reference to {makeproject.ProjectName} should be {reference.Key}",ConsoleColor.Green);
                            makeproject.ProjectName = reference.Key;
                        }
                    }
                    if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.NotFound)
                    {
                        Program.Console.WriteLine($"Project {visualStudioFile.ProjectName} missing make project reference {reference.Key}", ConsoleColor.Red);
                    }
                }
                visualStudioFile.MatchUpMakeProject(MakeProjects);
            }

            foreach (var makeFile in MakeFiles)
            {
                makeFile.WriteFile(this.LineLength, this.SortProject);
            }
        }
    }

    [Verb("MatchMakeProjectDependencyCaseToMakeProjectName", HelpText = "Match the Make Project Dependency Case match the ProjectName Case")]
    internal class MatchMakeProjectDependencyCaseToMakeProjectName : Store
    {
        readonly ConsoleWriter console = new ConsoleWriter();

        public void Run()
        {
            Program.Console.WriteLine($"Running {this.GetType().Name}", ConsoleColor.Cyan);

            this.BuildStoreMakeFilesOnly();

            var allprojects = new List<MakeProject>();
            allprojects.AddRange(MakeProjects);
            allprojects.AddRange(MakeHeaderProjects);

            CheckProjects(allprojects);

            foreach (var makeFile in MakeFiles)
            {
                makeFile.WriteFile(this.LineLength, this.SortProject);
            }
        }

        private void CheckProjects(List<MakeProject> projects)
        {
            foreach (MakeProject makeProject in projects)
            {
                var changeSet = new Dictionary<string, string>();
                foreach (var dependencyProject in makeProject.DependencyProjects)
                {
                    var ok = projects.FirstOrDefault(m => string.Equals(m.ProjectName, dependencyProject, StringComparison.Ordinal));
                    if (ok != null) continue;
                    var project = projects.FirstOrDefault(m => string.Equals(m.ProjectName, dependencyProject, StringComparison.CurrentCultureIgnoreCase));
                    if (project == null)
                    {
                        console.Write($"Make Project: {makeProject.ProjectName}", ConsoleColor.Red);
                        console.WriteLine($" Dependency: {dependencyProject} does not exist", ConsoleColor.Yellow);
                        continue; // Error bug , should throw?
                    }
                    changeSet[dependencyProject] = project.ProjectName;
                }
                if (changeSet.Count > 0)
                {
                    console.WriteLine($"Fixing MakeProject: {makeProject.ProjectName}", ConsoleColor.Green);
                }
                foreach (var item in changeSet)
                {
                    makeProject.DependencyProjects.Remove(item.Key);
                    makeProject.DependencyProjects.Add(item.Value);
                }
            }
        }
    }

}
