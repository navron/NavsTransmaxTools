using CommandLine;

namespace VisualStudioProjectFixer
{
    public class Options
    {
        [Option('d', "dir", HelpText = "Source Root Folder")]
        public string SourceCheckRootFolder { get; set; }
    }
}