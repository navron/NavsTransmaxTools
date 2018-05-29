using CommandLine;

namespace MakeFileProjectFixer
{
    // Documentation at https://github.com/commandlineparser/commandline/wiki
    public class Options
    {
        private string folder;
        [Option('d', @"dir", Required = false, HelpText = "Base folder to search for files ", Default = "CurrentDir")]
        public string Folder
        {
            get => folder;
            set => folder = value == "CurrentDir" ? System.IO.Directory.GetCurrentDirectory() : value;
        }

        [Option('f', @"file", Required = false, HelpText = "Specifies a single file, Program does not scan for files")]
        public string SingleFile { get; set; }

        //[Option("SearchPatterns", HelpText = "file types to search for")]
        // Set in Code
        public string[] SearchPatterns { get; set; }

        [Option('v', @"verbose", HelpText = "Verbosely output information")]
        public bool Verbose { get; set; }

        [Option("debug", HelpText = "Debug output information")]
        public bool Debug { get; set; }

        [Option(@"length", HelpText = "Length of the Line that the project dependences wrap, --Zero to disable--", Default = 100)]
        public int LineLength { get; set; }

        [Option("sortproject", HelpText = "Sort the project within the make file", Default = false)]
        public bool SortProject { get; set; }

        // TODO THIS SHOULD BE BASED ON TASK LEVEL
        [Option("sortdepends", HelpText = "Sort the project depends within the project", Default = false)]
        public bool SortDependencies { get; set; }


        [Option("format", HelpText = "Apply new formatting", Default = false)]
        public bool FormatProject { get; set; }

        [Option('r', @"reuse", HelpText = "clean previous build make and visual studio process files stored in the current folder")]
        public bool ReuseTemporaryFiles { get; set; }

        [Option('p', "parallel", HelpText = "Run in parallel mode if supported")]
        public bool RunAsParallel { get; set; }
    }
}
