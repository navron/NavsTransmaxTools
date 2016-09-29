using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using MakeProjectFixer.MakeFile;
using MakeProjectFixer.VisualStudioFile;

namespace MakeProjectFixer
{
    [Verb("MakeDependencyAllocator", HelpText = "Allocates Correct Dependency")]
    internal class MakeDependencyAllocator : MakeOptions
    {
        [Option(@"temp", HelpText = "Specifies a directory where pre-process stage may save files")]
        public string PreProcessedFolder { get; set; }

        [Option(@"clean", Default = false, HelpText = "clean and rewrite pre-process files")]
        public bool CleanPreProcessedFiles { get; set; }

        public List<MakeFile.MakeFile> MakeFiles { get; private set; }
        public List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; private set; }

        public List<MakeFileProject> MakeProjects { get; private set; }

        public MakeDependencyAllocator()
        {
            MakeFiles = new List<MakeFile.MakeFile>();
            VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
            MakeProjects = new List<MakeFileProject>();
        }

        //method not unit tested
        public void Run()
        {
            // Step 1 Read all make files
            ProcessMakeFilesStage1(this,"1");
            ProcessBuildMakeProjectList(this, "1");

            // Step 2 Read all Visual Studio Files 
            ProcessVisualStudioFilesStage2(this, "2");

            ProcessVisualStudioFilesStage3(this, "3");

            ProcessVisualStudioFilesStage4(this,"4");
        }


        /// <returns>True if Object should be preprocessed</returns>
        private bool PreProcessedObject(string name, out string file, MakeOptions options, string stage)
        {
            file = Path.Combine(PreProcessedFolder, name + ".json");
            if (!string.IsNullOrEmpty(options.Stage) && options.Stage.Contains(stage)) return false;

            if (string.IsNullOrEmpty(PreProcessedFolder) || CleanPreProcessedFiles) return false;
            if (!File.Exists(file)) return false;

            return true;
        } 

        private void PreProcessedFileSave(string name, object saveObject)
        {
            // if PreProcessedFolder is empty then don't save
            if (string.IsNullOrEmpty(PreProcessedFolder)) return;
            var file = Path.Combine(PreProcessedFolder, name + ".json");

            JsonSerialization.WriteToJsonFile(file, saveObject);
        }

        private void ProcessMakeFilesStage1(MakeOptions options, string stage)
        {
            string jsonFile;
            var fileName = "MakeFiles" + stage;
            if (PreProcessedObject(fileName, out jsonFile, options, stage))
            {
                MakeFiles = JsonSerialization.ReadFromJsonFile<List<MakeFile.MakeFile>>(jsonFile);
                return;
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

            PreProcessedFileSave(fileName, MakeFiles);
        }

        private void ProcessBuildMakeProjectList(MakeOptions options, string stage)
        {
            string jsonFile;
            var fileName = "MakeProjectList" + stage;
            if (PreProcessedObject(fileName, out jsonFile, options, stage))
            {
                MakeProjects = JsonSerialization.ReadFromJsonFile<List<MakeFileProject>>(jsonFile);
                return;
            }

            foreach (MakeFile.MakeFile makeFile in MakeFiles)
            {
                MakeProjects.AddRange(makeFile.Projects);
            }

            PreProcessedFileSave(fileName, MakeProjects);
        }


        private void ProcessVisualStudioFilesStage2(MakeOptions options, string stage)
        {
            string jsonFile;
            var fileName = "VisualStudioFilesStage" + stage;
            if (PreProcessedObject(fileName, out jsonFile, options, stage))
            {
                VisualStudioFiles = JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(jsonFile);
                return;
            }

            // Step 2 Read all Visual Studio Files 
            options.SearchPatterns = new[] { "*.csproj", "*.vcxproj" };
            // Find and limit return set to what is required
            var files = Helper.FindFiles(options).Where(VisualStudioFileHelper.IncludeFile);

            Parallel.ForEach(files, (file) =>
            {
                var vsFile = new VisualStudioFile.VisualStudioFile(file);
                vsFile.ScanFile();
                VisualStudioFiles.Add(vsFile);
            });


            PreProcessedFileSave(fileName, VisualStudioFiles);
        }

        private void ProcessVisualStudioFilesStage3(MakeOptions options, string stage)
        {
            string jsonFile;
            var fileName = "VisualStudioFilesStage" + stage;
            if (PreProcessedObject(fileName, out jsonFile, options, stage))
            {
                VisualStudioFiles = JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(jsonFile);
                return;
            }

            foreach (var visualStudioFile in VisualStudioFiles)
            {
                visualStudioFile.MakeExpectedMakeProjectRefenences();
            }
            //Parallel.ForEach(VisualStudioFiles, (visualStudioFile) =>
            //{
            //    visualStudioFile.MakeExpectedMakeProjectRefenences();
            //});

            PreProcessedFileSave(fileName, VisualStudioFiles);
        }

        private void ProcessVisualStudioFilesStage4(MakeOptions options, string stage)
        {
            string jsonFile;
            var fileName = "VisualStudioFilesStage" + stage;
            if (PreProcessedObject(fileName, out jsonFile, options, stage))
            {
                VisualStudioFiles = JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(jsonFile);
                return;
            }

            foreach (var visualStudioFile in VisualStudioFiles)
            {
                visualStudioFile.MatchUpMakeProject(MakeProjects);
            }
            //Parallel.ForEach(VisualStudioFiles, (visualStudioFile) =>
            //{
            //  visualStudioFile.MatchUpMakeProject(MakeProjects);
            //});

            PreProcessedFileSave(fileName, VisualStudioFiles);

            if (options.Verbose)
            {
                foreach (var visualStudioFile in VisualStudioFiles)
                {
                    foreach (var reference in visualStudioFile.ExpectedMakeProjectReferences)
                    {
                        if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.FoundCaseWrong)
                        {
                            Console.WriteLine($"Project {visualStudioFile.MakeProjectName} has wrong case reference to {reference.Key}");
                        }
                        if (reference.Value == VisualStudioFile.VisualStudioFile.ProjectFound.NotFound)
                        {
                            Console.WriteLine($"Project {visualStudioFile.MakeProjectName} missing make project reference {reference.Key}");
                        }
                    }
                    visualStudioFile.MatchUpMakeProject(MakeProjects);
                }
            }
        }

    }
}
