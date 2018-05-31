using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProjectFixer.MakeFile;
using ProjectFixer.Utility;
using Serilog;

namespace ProjectFixer.Data
{
    internal class StoreMakeFile
    {
        private const string StoreFile = "StoreMakeFile.json";
        // Raw Set of Make File contents
        public List<MakeFile.MakeFile> MakeFiles { get; set; } = new List<MakeFile.MakeFile>();

        public List<MakeProject> MakeProjects { get; private set; } = new List<MakeProject>();
        public List<MakeProject> MakeHeaderProjects { get; private set; } = new List<MakeProject>();

        public void Build(Options options)
        {
            using (new LoggingTimer("Building Store MakeFiles"))
            {
                BuildStore(options);
            }
        }

        private void BuildStore(Options options)
        {
            MakeFiles = GetPreBuild(options) ?? Stage1ReadMakeFiles(options);

            // Build a MakeProject Set
            MakeFiles.ForEach(mk => MakeProjects.AddRange(mk.Projects));
            // Build a MakeProject Header Set
            MakeFiles.ForEach(mk => MakeHeaderProjects.Add(mk.Header));

            // For Each Make Project do any post build work
            MakeProjects.ForEach(mp => mp.PostBuildWork(MakeProjects));

            Stage2CheckMakeProjects(MakeProjects);
            Stage2CheckMakeProjects(MakeHeaderProjects);
            Helper.PreProcessedFileSave("MakeProjects.json", MakeProjects);
            Helper.PreProcessedFileSave("MakeHeaderProjects.json", MakeHeaderProjects);
        }

        private List<MakeFile.MakeFile> GetPreBuild(Options options)
        {
            Log.Debug($"Running {MethodBase.GetCurrentMethod().Name}");
            if (!Helper.PreProcessedObject(StoreFile, options)) return null;

            var files = JsonSerialization.ReadFromJsonFile<List<MakeFile.MakeFile>>(StoreFile);
            CheckFiles(new List<MakeFile.MakeFile>(files));
            return files;
        }

        private static List<MakeFile.MakeFile> Stage1ReadMakeFiles(Options options)
        {
            var list = new List<MakeFile.MakeFile>();
            // This takes about 1 sec, Read all make files
            options.SearchPatterns = new[] { "*.mak" };
            var files = Helper.FindFiles(options);

            Parallel.ForEach(files, options.ParallelOption, (file) =>
            {
                var makeFile = new MakeFile.MakeFile();
                makeFile.ProcessFile(file);
                list.Add(makeFile);
            });
    
            Helper.PreProcessedFileSave(StoreFile, list);
            return list;
        }

        private void Stage2CheckMakeProjects(List<MakeProject> makeProjects)
        {
            foreach (var makeProject in makeProjects)
            {
                if (makeProject.ProjectName != null) continue;
                // Most likely an make file error. Print out and abort              
                Log.Error("Error with Make File Project, Project Name Null in file {File}", FindMakeFileFromMakeProject(makeProject).FileName);
                foreach (var line in makeProject.FormatMakeProject(true, 200, false))
                {
                    Log.Warning(line);
                }
                throw new Exception("Aborting");
            }
        }

        public MakeFile.MakeFile FindMakeFileFromMakeProject(MakeProject makeProject)
        {
            foreach (var makeFile in MakeFiles)
            {
                // Check Header Project
                if (makeFile.Header.ProjectName == makeProject.ProjectName)
                {
                    return makeFile;
                }

                // Check Projects
                if (makeFile.Projects.Any(mp => mp.ProjectName == makeProject.ProjectName))
                {
                    return makeFile;
                }
            }

            Log.Error("Could not find Make File from Make Project {@ProjectName}", makeProject);
            Environment.Exit(-1);
            return null;
        }

        public void CheckFiles(List<MakeFile.MakeFile> files)
        {
            if (files.Any(file => file == null))
            {
                throw new Exception("VSFile null, try deleting json files Aborting");
            }
        }

        public void WriteMakeFiles(Options options)
        {
            // Should do some thing here to check that it needs writing out.
            foreach (var makeFile in MakeFiles)
            {
                makeFile.WriteFile(options);
            }
            // Delete the Store Make File, so that it is built the next time
            File.Delete(StoreFile);
        }
    }
}
