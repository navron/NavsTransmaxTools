using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace VisualStudioProjectFixer
{
    public class Options
    {
        [Option('d', "dir", HelpText = "Source Root Folder")]
        public string SourceCheckRootFolder { get; set; }
    }
}