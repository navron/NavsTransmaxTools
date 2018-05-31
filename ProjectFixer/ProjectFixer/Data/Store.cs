using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using NUnit.Framework;
using ProjectFixer.MakeFile;
using ProjectFixer.Utility;
using ProjectFixer.VisualStudioFile;

namespace ProjectFixer.Data
{
    /// <summary>
    /// Holds an in memory set of Make file an Visual studio files so
    /// that they made be queried quickly by 
    /// </summary>
    /// <remarks>This Verb is useful for testing and design</remarks>
    [Verb("Store", HelpText = "Debugging Verb, used to create json files of processed visual studio project and make files")]
    internal class Store : Options
    {
        private readonly Options options;


        public List<IVisualStudioFile> VisualStudioFiles { get; private set; } = new List<IVisualStudioFile>();


        private readonly StoreMakeFile storeMakeFile = new StoreMakeFile();
        public List<MakeProject> MakeProjects => storeMakeFile.MakeProjects;
        public List<MakeProject> MakeHeaderProjects => storeMakeFile.MakeHeaderProjects;
        public List<MakeFile.MakeFile> MakeFiles => storeMakeFile.MakeFiles;

        private readonly StoreCsFiles storeCsFiles = new StoreCsFiles();

        private readonly StoreCppFiles storeCppFiles = new StoreCppFiles();
        public List<VisualStudioCPlusPlusFile> CppFiles => storeCppFiles.Files;

        [Option("MakeFiles", HelpText = "Scans Make Files")]
        public bool DoMakeFiles { get; set; }

        [Option("CSharp", HelpText = "Scans CSharp Projects")]
        public bool DoCsFiles { get; set; }

        [Option("Cpp", HelpText = "Scans C++ Projects")]
        public bool DoCppFiles { get; set; }

        // Call directly when testing via verb
        public Store() // required for testing
        {
            options = this;
            if (DoMakeFiles) BuildMakeFiles();
            if (DoCsFiles) BuildVisualStudioCSharp();
            if (DoCppFiles) BuildVisualStudioCpp();
        }

        public Store(Options options)
        {
            this.options = options;
        }

        // Build Both the Make Files and Visual Studio files Store
        public void Run()
        {
            BuildMakeFiles();
            BuildVisualStudioCSharp();
            BuildVisualStudioCpp();

            // Add the Visual Files into one list and build expected Make Project References
            VisualStudioFiles.AddRange(storeCsFiles.Files);
            VisualStudioFiles.AddRange(storeCppFiles.Files);
            // Build Expected MakeProject References
            Parallel.ForEach(VisualStudioFiles, ParallelOption, (vsFile) =>
                vsFile.BuildExpectedMakeProjectReferences(MakeProjects, VisualStudioFiles));
            // For Debugging only
            Helper.PreProcessedFileSave("VisualStudioFiles.json", VisualStudioFiles);
        }

        public void BuildMakeFiles()
        {
            storeMakeFile.Build(this);
        }
        public void BuildVisualStudioCSharp()
        {
            storeCsFiles.Build(this);
        }
        public void BuildVisualStudioCpp()
        {
            storeCppFiles.Build(this);
        }




        //private void ProcessVsFilesStage4MatchUpMakeProject()
        //{
        //    Log.Debug($"Running ProcessVsFilesStage4MatchUpMakeProject");
        //    if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
        //    {
        //        VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
        //        CheckVisualStudioFiles(VisualStudioFiles);
        //        return;
        //    }

        //    if (RunAsParallel)
        //        Parallel.ForEach(VisualStudioFiles, (vsFile) => vsFile.MatchUpMakeProject(MakeProjects));
        //    else
        //        VisualStudioFiles.ForEach(vsFile => vsFile.MatchUpMakeProject(MakeProjects));

        //    Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        //}




        public void WriteMakeFiles() => storeMakeFile.WriteMakeFiles(options);

        public MakeFile.MakeFile FindMakeFileFromMakeProject(MakeProject makeProject) =>
                    storeMakeFile.FindMakeFileFromMakeProject(makeProject);



        /// <summary>
        /// Return All the Make Project and Header Projects
        /// </summary>
        public List<MakeProject> GetAllMakeProjects
        {
            get
            {
                var list = new List<MakeProject>();
                list.AddRange(MakeHeaderProjects);
                list.AddRange(MakeProjects);
                return list;
            }
        }
    }
}

