using CommandLine;

namespace ProjectFixer.Scripts
{
    [Verb("Store", HelpText = "Debugging Verb, used to create json files of processed visual studio project and make files")]
    public class StoreTest : Options
    {

        [Option("MakeFiles", HelpText = "Scans Make Files")]
        public bool DoMakeFiles { get; set; }

        [Option("CSharp", HelpText = "Scans CSharp Projects")]
        public bool DoCsFiles { get; set; }

        [Option("Cpp", HelpText = "Scans C++ Projects")]
        public bool DoCppFiles { get; set; }

        public void Run()
        {
            var store = new Data.Store(this);

            if (DoMakeFiles) store.BuildMakeFiles();
            if (DoCsFiles) store.BuildVisualStudioCSharp();
            if (DoCppFiles) store.BuildVisualStudioCpp();
        }
    }
}
