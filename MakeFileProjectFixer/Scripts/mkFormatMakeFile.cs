using System;
using CommandLine;
using MakeFileProjectFixer.Data;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("mkFormat", HelpText = "Format Make Files")]
    internal class mkFormatMakeFile : Store
    {
        public mkFormatMakeFile()
        {
            SearchPatterns = new[] { "*.mak" };
        }

        public void Run()
        {
            Log.Debug($"Running {GetType().Name}", ConsoleColor.Cyan);

            BuildStoreMakeFilesOnly();

            foreach (MakeFile.MakeFile makeFile in MakeFiles)
            {
                makeFile.WriteFile(this);
            }

            //var files = Helper.FindFiles(this);
            //Parallel.ForEach(files, (file) =>
            //{
            //    if (Verbose) Console.WriteLine($"Formatting {file}");
            //    var make = new MakeFile.MakeFile();
            //    make.ReadFile(file);
            //    make.WriteFile(this);
            //});
        }
    }
}
