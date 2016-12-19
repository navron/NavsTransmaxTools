using System;
using CommandLine;
using MakeProjectFixer.Util;

namespace MakeProjectFixer.Checkers
{

    [Verb("mkScanErrors", HelpText = "Scan Make Files for Errors")]
    internal class mkScanForErrors : Options
    {
        public mkScanForErrors()
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
            Program.Console.WriteLine($"Running {GetType().Name}", ConsoleColor.Cyan);

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
                    make.WriteFile(this);
                }
            }
            //);
        }
    }
}
