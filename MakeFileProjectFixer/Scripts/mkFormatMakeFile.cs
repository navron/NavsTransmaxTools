using System;
using CommandLine;
using MakeFileProjectFixer.Data;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("mkFormat", HelpText = "Format Make Files")]
    internal class mkFormatMakeFile : Options
    {
        public mkFormatMakeFile()
        {
            SearchPatterns = new[] { "*.mak" };
        }

        public void Run()
        {
            Log.Debug($"Running {GetType().Name}", ConsoleColor.Cyan);
            var store = new Store(this);
            store.BuildStoreMakeFilesOnly();

            foreach (MakeFile.MakeFile makeFile in store.MakeFiles)
            {
                makeFile.WriteFile(this);
            }
        }
    }
}
