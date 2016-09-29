﻿using CommandLine;

namespace MakeProjectFixer
{
    // Documentation at https://github.com/gsscoder/commandline/wiki/Latest-Version


    class Options
    {
        private string folder;

        [Option('d', @"dir", Required = false, HelpText = "Base folder to search for files ", Default = "CurrentDir")]
        public string Folder
        {
            get { return folder; }
            set { folder = value == "CurrentDir" ? System.IO.Directory.GetCurrentDirectory() : value; }
        }
        [Option('f', @"file", Required = false, HelpText = "Specifies a single file, Program does not scan for files")]
        public string SingleFile { get; set; }

        //[Option("SearchPatterns", HelpText = "file types to search for")]
        public string[] SearchPatterns { get; set; }

        [Option(@"verbose", HelpText = "Verbosely output information")]
        public bool Verbose { get; set; }
    }

    class MakeOptions : Options
    {
        [Option(@"length", HelpText = "Length of the Line that the project dependences wrap, --Zero to disable--", Default = 100)]
        public int LineLength { get; set; }

        [Option("sort", HelpText = "Sort the project within the make file", Default = false)]
        public bool SortProject { get; set; }

        public MakeOptions()
        {
            SearchPatterns = new[] { "*.mak" };    
        }
    }
}
