using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
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
    internal class Store
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

        // Called by other Scripts
        public Store(Options options)
        {
            this.options = options;
        }

        // Build Both the Make Files and Visual Studio files Store
        public Store BuildStore()
        {
            BuildMakeFiles();
            BuildVisualStudioCSharp();
            BuildVisualStudioCpp();

            // Add the Visual Files into one list and build expected Make Project References
            VisualStudioFiles.AddRange(storeCsFiles.Files);
            VisualStudioFiles.AddRange(storeCppFiles.Files);
            // Build Expected MakeProject References
            Parallel.ForEach(VisualStudioFiles, options.ParallelOption, (vsFile) =>
                vsFile.BuildExpectedMakeProjectReferences(MakeProjects, VisualStudioFiles));
            // For Debugging only
            Helper.PreProcessedFileSave("VisualStudioFiles.json", VisualStudioFiles);
            return this;
        }

        public void BuildMakeFiles()
        {
            storeMakeFile.Build(options);
        }
        public void BuildVisualStudioCSharp()
        {
            storeCsFiles.Build(options);
        }
        public void BuildVisualStudioCpp()
        {
            storeCppFiles.Build(options);
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

