using System;
using System.Threading.Tasks;
using CommandLine;

namespace MakeProjectFixer
{
    [Verb("MakeFileFormat", HelpText = "Format Make Files")]
    internal class MakeFileFormat : MakeOptions
    {

        public void Run()
        {
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
    internal class MakeScanErrors : MakeOptions
    {
        [Option(@"fix", HelpText = "Fix Errors", Default = false)]
        public bool FixErrors { get; set; }

        public void Run()
        {
            var files = Helper.FindFiles(this);
            Parallel.ForEach(files, (file) =>
            {
                if (Verbose) Console.WriteLine($"Scanning {file}");
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);
                var found = make.ScanForErrors(FixErrors);
                if (found && FixErrors)
                {
                    make.WriteFile(LineLength, SortProject);
                }
            });
        }
    }

}
