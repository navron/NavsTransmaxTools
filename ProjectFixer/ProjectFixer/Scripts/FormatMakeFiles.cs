using System;
using CommandLine;
using ProjectFixer.Data;
using Serilog;

namespace ProjectFixer.Scripts
{
    [Verb("FormatMakeFiles", HelpText = "Format Make Files")]
    internal class FormatMakeFiles : Options
    {
        //public FormatMakeFiles()
        //{
        //    SearchPatterns = new[] { "*.mak" };
        //}

        public void Run()
        {
            Log.Debug($"Running {GetType().Name}", ConsoleColor.Cyan);
            var store = new Store(this);
            store.BuildMakeFiles();

            store.WriteMakeFiles();
        }
    }
}
