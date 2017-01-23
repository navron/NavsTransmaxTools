using CommandLine;

namespace VisualStudioProjectFixer
{
    public class Options
    {
        [Option('d', "dir", HelpText = "Root Folder")]
        public string RootFolder { get; set; }

        [Option('p', "parallel", HelpText = "Run in parallel mode")]
        public bool RunAsParallel { get; set; }

        [Option('c', "continue", HelpText = "Continue processing if possible")]
        public bool ContinueProcessing { get; set; }
    }
}