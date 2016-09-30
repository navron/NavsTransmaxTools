using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using MakeProjectFixer.MakeFile;
using MakeProjectFixer.VisualStudioFile;

namespace MakeProjectFixer
{
    internal class Store : Options
    {
        public List<MakeFile.MakeFile> MakeFiles { get; private set; }
        public List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; private set; }
        public List<MakeProject> MakeProjects { get; private set; }
        public List<MakeProject> MakeHeaderProjects { get; private set; }

        public Store()
        {
            MakeFiles = new List<MakeFile.MakeFile>();
            MakeProjects = new List<MakeProject>();
            MakeHeaderProjects = new List<MakeProject>();
            VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
        }

        public void BuildStore()
        {
            BuildStoreMakeFilesOnly();
            ProcessVisualStudioFilesStage1BuildFileList();
            ProcessVisualStudioFilesStage2BuildExpectedMakeProjectRefenences();
            ProcessVisualStudioFilesStage3MatchUpMakeProject();
        }

        public void BuildStoreMakeFilesOnly()
        {
            ProcessMakeFiles();
            ProcessBuildMakeProjectList();
        }

        private void ProcessMakeFiles()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                MakeFiles = Helper.JsonSerialization.ReadFromJsonFile<List<MakeFile.MakeFile>>(JsonFile);
                return;
            }

            // This takes about 1 sec, 
            // Step 1 Read all make files
            SearchPatterns = new[] { "*.mak" };

            var files = Helper.FindFiles(this);
            Parallel.ForEach(files, (file) =>
            {
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);
                make.ProcessPublishItems(); //TODO
                MakeFiles.Add(make);
            });

            Helper.PreProcessedFileSave(JsonFile, MakeFiles);
        }

        private void ProcessBuildMakeProjectList()
        {
            foreach (MakeFile.MakeFile makeFile in MakeFiles)
            {
                MakeProjects.AddRange(makeFile.Projects);
                MakeHeaderProjects.Add(makeFile.Header);
            }
        }

        private void ProcessVisualStudioFilesStage1BuildFileList()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
                return;
            }

            // Step 2 Read all Visual Studio Files 
            SearchPatterns = new[] { "*.csproj", "*.vcxproj" };
            // Find and limit return set to what is required
            var files = Helper.FindFiles(this).Where(VisualStudioFileHelper.IncludeFile);

            Parallel.ForEach(files, (file) =>
            {
                var vsFile = new VisualStudioFile.VisualStudioFile(file);
                vsFile.ScanFile();
                VisualStudioFiles.Add(vsFile);
            });

            Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        }

        private void ProcessVisualStudioFilesStage2BuildExpectedMakeProjectRefenences()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
                return;
            }

            foreach (var visualStudioFile in VisualStudioFiles)
            {
                visualStudioFile.BuildExpectedMakeProjectRefenences();
            }
            //Parallel.ForEach(VisualStudioFiles, (visualStudioFile) =>
            //{
            //    visualStudioFile.BuildExpectedMakeProjectRefenences();
            //});

            Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        }

        private void ProcessVisualStudioFilesStage3MatchUpMakeProject()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
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

            Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        }
    }

}

