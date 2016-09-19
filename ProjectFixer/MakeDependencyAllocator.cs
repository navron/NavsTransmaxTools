using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ProjectFixer
{
    [Verb("MakeDependencyAllocator", HelpText = "Allocates Correct Dependency")]
    internal class MakeDependencyAllocator : MakeOptions
    {
        [Option(@"temp", HelpText = "Specifies a directory where pre-process stage may save files")]
        public string TempFolder { get; set; }

        [Option(@"clean", Default = false, HelpText = "clean and rewrite pre-process files")]
        public bool CleanPreProcessedFiles { get; set; }

        public List<MakeFile.MakeFile> MakeFiles { get; private set; }
        public List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; private set; }

        public MakeDependencyAllocator()
        {
            MakeFiles = new List<MakeFile.MakeFile>();
            VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
        }

        //method not unit tested
        public void Run()
        {
            ProcessMakeFiles(this);
            // Step 1 Read all make files
            //var files = Helper.FindFiles(options);
            //var makeFiles = new List<MakeFile.MakeFile>();
            //Parallel.ForEach(files, (file) =>
            //{
            //    var make = new MakeFile.MakeFile();
            //    make.ReadFile(file);
            //    make.ProcessPublishItems();
            //    makeFiles.Add(make);
            //});

            //// Step 2 Read all Visual Studio Files 
            //var visualStudioFiles = new VisualStudioFile.VisualStudioFile();
            //options.SearchPatterns = new[] { "*.csproj", "*.vcxproj" };
            //visualStudioFiles.BuildProjectFileList(options);
            //// Scan for C++ Files
            //// Scan for CS Files

            //// Step 3 
        }

        public void ProcessMakeFiles(MakeOptions options)
        {
            if (!string.IsNullOrEmpty(TempFolder) && !CleanPreProcessedFiles)
            {
                // read from temp location and return the preprocess file
                //TODO
            }

            // This takes about 1 sec, 
            // Step 1 Read all make files
            var files = Helper.FindFiles(options);
            Parallel.ForEach(files, (file) =>
            {
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);
                make.ProcessPublishItems(); //TODO
                MakeFiles.Add(make);
            });

            if (!string.IsNullOrEmpty(TempFolder))
            {
                //save MakeFiles to disk
                // still useful for testing 
                JsonSerialization.WriteToJsonFile<List<MakeFile.MakeFile>>(@"C:\temp\MakeFiles.json", MakeFiles);
            }
        }

        public void ProcessVisualCShrapeFiles(MakeOptions options)
        {
            // Step 2 Read all Visual Studio Files 
            options.SearchPatterns = new[] { "*.csproj", "*.vcxproj" };
            var files = Helper.FindFiles(options);
            Parallel.ForEach(files, (file) =>
            {
                var vsFile = new VisualStudioFile.VisualStudioFile();
            //    vsFile.ReadFile(file);
             //   make.ProcessPublishItems(); //TODO
                MakeFiles.Add(make);
            });

            //visualStudioFiles.BuildProjectFileList(options);

        }

    }
}
