using System;
using CommandLine;
using MakeFileProjectFixer.Data;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("FormatMakeFile", HelpText = "Format Make Files")]
    internal class FormatMakeFile : Options
    {
        public FormatMakeFile()
        {
            SearchPatterns = new[] { "*.mak" };
        }

        public void Run()
        {
            Log.Debug($"Running {GetType().Name}", ConsoleColor.Cyan);
            var store = new Store(this);
            store.BuildStoreMakeFilesOnly();

            store.WriteMakeFiles();
        }
    }
}
