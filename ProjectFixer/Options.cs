using System.Collections.Generic;
using CommandLine;

namespace ProjectFixer
{
    // Documentation https://github.com/gsscoder/commandline/wiki/Latest-Version


    class BaseOptions
    {
        private string folder;
        [Option('d', @"dir", Required = false, HelpText = "Base folder to search for files ", Default = "CurrentDir")]
        public string Folder
        {
            get { return folder; }
            set { folder = value == "CurrentDir" ? System.IO.Directory.GetCurrentDirectory() : value; }
        }


        [Option("LineLength", HelpText = "numeric value here")]
        public int LineLength { get; set; }


        [Option('t', "text", Required = true, HelpText = "text value here")]
        public string TextValue { get; set; }

        [Option('n', "numeric", HelpText = "numeric value here")]
        public double NumericValue { get; set; }

        [Option('b', "bool", HelpText = "on|off switch here")]
        public bool BooleanValue { get; set; }

        [Option('t', Separator = ':')]
        public IEnumerable<string> Types { get; set; }

        //[HelpOption]
        //public string GetUsage()
        //{
        //    return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        //}
    }

    [Verb("add", HelpText = "Add file contents to the index.")]
    class AddOptions
    { //normal options here

    }

}
