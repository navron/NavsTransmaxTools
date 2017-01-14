using System.Threading.Tasks;
using CommandLine;
using MakeFileProjectFixer.Utility;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{

    [Verb("ScanForErrors", HelpText = "Scan Make Files for Errors")]
    internal class ScanForErrors : Options
    {
        public ScanForErrors()
        {
            SearchPatterns = new[] { "*.mak" };
        }

        [Option('a', @"ScanAll", HelpText = "Scan All Tests", Default = false)]
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
            using (new LoggingTimer(GetType().Name))
            {
                var files = Helper.FindFiles(this);

                if (RunAsParallel)
                    Parallel.ForEach(files, CheckMakeFile);
                else
                    files.ForEach(CheckMakeFile);
            }
        }

        void CheckMakeFile(string file)
        {
            Log.Debug($"Scanning {file}");
            var make = new MakeFile.MakeFile();
            make.ReadFile(file);

            if (ScanAll)
                ScanForErrorsExtraDependencyInTheMakeFileHeader =
                    ScanForErrorsMissingDependencyFromTheMakeFileHeader =
                        ScanForErrorsProjectHeaderSyntax = true;

            var errorFound = false;
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
    }
}
