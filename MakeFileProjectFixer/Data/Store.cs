﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using MakeFileProjectFixer.MakeFile;
using MakeFileProjectFixer.Utility;
using MakeFileProjectFixer.VisualStudioFile;
using Serilog;

namespace MakeFileProjectFixer.Data
{
    /// <summary>
    /// Holds an in memory set of Make file an Visual studio files so
    /// that they made be queried quickly by 
    /// </summary>
    [Verb("Store", HelpText = "Debugging Verb, used to create json files of processed visual studio project and make files")]
    internal class Store : Options
    {
        private Options options;
        public List<MakeFile.MakeFile> MakeFiles { get; private set; } =  new List<MakeFile.MakeFile>();
        public List<IVisualStudioFile> VisualStudioFiles { get; private set; } = new List<IVisualStudioFile>();
        public List<VisualStudioCSharpFile> CSharpFiles { get; private set; }= new List<VisualStudioCSharpFile>();
        public List<VisualStudioCPlusPlusFile> CPlusPlusFiles { get; private set; }= new List<VisualStudioCPlusPlusFile>();
        public List<MakeProject> MakeProjects { get; private set; }= new List<MakeProject>();
        public List<MakeProject> MakeHeaderProjects { get; private set; }= new List<MakeProject>();

        // Call directory when testing via verb
        public Store() // required for testing
        {
            options = this;
        } 

        public Store(Options options)
        {
            this.options = options;
        }


        // Build Both the Make Files and Visual Studio files Store
        public void BuildStore()
        {
            BuildStoreMakeFiles();
            BuildStoreVisalStudioFiles();
        }

        // Only build the Make Files
        public void BuildStoreMakeFilesOnly()
        {
            BuildStoreMakeFiles();
        }

        private void BuildStoreMakeFiles()
        {
            using (new LoggingTimer("Building Store MakeFiles"))
            {
                MakeFiles = Stage1ReadMakeFiles();
                
                // Build a MakeProject Set
                MakeFiles.ForEach(mk=> MakeProjects.AddRange(mk.Projects));
                // Build a MakeProject Header Set
                MakeFiles.ForEach(mk => MakeHeaderProjects.Add(mk.Header));

                Stage2CheckMakeProjects(MakeProjects);
                Stage2CheckMakeProjects(MakeHeaderProjects);
                Helper.PreProcessedFileSave("MakeProjects.json", MakeProjects);
                Helper.PreProcessedFileSave("MakeHeaderProjects.json", MakeHeaderProjects);
            }
        }

        private void BuildStoreVisalStudioFiles()
        {
            using (new LoggingTimer("Building Store Visual Studio"))
            {
                Stage3BuildCSharpFileList();
                Stage3BuildCPlusPlusFileList();

                // Join the two sets of files
                VisualStudioFiles.AddRange(CSharpFiles);
                VisualStudioFiles.AddRange(CPlusPlusFiles);
                CheckVisualStudioFiles(VisualStudioFiles);

                // Build Expected MakeProject References
                if (RunAsParallel)
                    Parallel.ForEach(VisualStudioFiles, (vsFile) => vsFile.BuildExpectedMakeProjectReferences(MakeProjects, VisualStudioFiles));
                else
                    VisualStudioFiles.ForEach(vsFile => vsFile.BuildExpectedMakeProjectReferences(MakeProjects, VisualStudioFiles));

                // For Debugging only
                Helper.PreProcessedFileSave("VisualStudioFiles.json", VisualStudioFiles);
            }
        }

        private List<MakeFile.MakeFile> Stage1ReadMakeFiles()
        {
            if (Helper.PreProcessedObject(MethodBase.GetCurrentMethod().Name, this))
            {
                return Helper.JsonSerialization.ReadFromJsonFile<List<MakeFile.MakeFile>>(JsonFile);
            }

             var list = new List<MakeFile.MakeFile>();
            // This takes about 1 sec, Step 1 Read all make files
            options.SearchPatterns = new[] { "*.mak" };
            var files = Helper.FindFiles(options);

            Parallel.ForEach(files, (file) =>
            {
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);
                make.ScanRawLinesForPublishItems();
                list.Add(make);
            });

            Helper.PreProcessedFileSave(JsonFile, list);
            return list;
        }

        // Build a Set of Make Project from the Make Files
        //private void Stage2BuildMakeProjects(List<MakeFile.MakeFile> makeFiles)
        //{
        //    foreach (MakeFile.MakeFile makeFile in makeFiles)
        //    {
        //        MakeProjects.AddRange(makeFile.Projects);
        //        MakeHeaderProjects.Add(makeFile.Header);
        //    }
        //    AllMakeProjects.AddRange(MakeProjects);
        //    AllMakeProjects.AddRange(MakeHeaderProjects);

        //    //foreach (var makeProject in AllMakeProjects)
        //    //{
        //    //    foreach (var project in makeProject.DependencyProjects)
        //    //    {
        //    //        var t = AllMakeProjects.FirstOrDefault(a => a.ProjectName == project);
        //    //        if (t == null) continue; //bug that should ok to ignore
        //    //        if (makeProject.DependencyProjects.Contains(project))
        //    //        {
        //    //            Log.Debug($"Warning Make Project {makeProject.ProjectName} has duplicate reference {project} Hand Edit to fix", ConsoleColor.Red);
        //    //            continue;
        //    //        }
        //    //        makeProject.DependencyProjects.Add(project, t.IsHeader);
        //    //    }
        //    //}
        //}

        public void Stage2CheckMakeProjects(List<MakeProject> makeProjects)
        {
            foreach (MakeProject makeProject in makeProjects)
            {
                if (makeProject.ProjectName != null) continue;
                // Most likely an make file error. 
                // Print out in Red and abort
                Log.Error("Error with Make File Project, Project Name Null");
                foreach (var line in makeProject.FormatMakeProject(true, 200, false))
                {
                    Log.Warning(line);
                }
                throw new Exception("Aborting");
            }
        }

        private void Stage3BuildCSharpFileList()
        {
            Log.Debug($"Running {MethodBase.GetCurrentMethod().Name}");
            if (Helper.PreProcessedObject(MethodBase.GetCurrentMethod().Name, this))
            {
                CSharpFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioCSharpFile>>(JsonFile);
                CheckVisualStudioFiles(new List<IVisualStudioFile>(CSharpFiles));
                return;
            }

            // Step 2 Read all Visual Studio Files 
            options.SearchPatterns = new[] { "*.csproj" };
            // Find and limit return set to what is required
            var files = Helper.FindFiles(options).Where(VisualStudioFileHelper.IncludeFile).ToList();

            if (RunAsParallel)
                Parallel.ForEach(files, (file) => CSharpFiles.Add(new VisualStudioCSharpFile(file)));
            else
                files.ForEach(file => CSharpFiles.Add(new VisualStudioCSharpFile(file)));

            // Scan Visual Studio Files For internal References
            if (RunAsParallel)
                Parallel.ForEach(CSharpFiles, (vsFile) => vsFile.ScanFileForReferences());
            else
                CSharpFiles.ForEach(file => file.ScanFileForReferences());

            Helper.PreProcessedFileSave(JsonFile, CSharpFiles);
        }

        private void Stage3BuildCPlusPlusFileList()
        {
            Log.Debug($"Running {MethodBase.GetCurrentMethod().Name}");
            if (Helper.PreProcessedObject(MethodBase.GetCurrentMethod().Name, this))
            {
                CPlusPlusFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioCPlusPlusFile>>(JsonFile);
                CheckVisualStudioFiles(new List<IVisualStudioFile>(CPlusPlusFiles));
                return;
            }

            // Step 2 Read all Visual Studio Files 
            options.SearchPatterns = new[] { "*.vcxproj" };
            // Find and limit return set to what is required
            var files = Helper.FindFiles(options).Where(VisualStudioFileHelper.IncludeFile).ToList();

            // Create File list
            if (RunAsParallel)
                Parallel.ForEach(files, (file) => CPlusPlusFiles.Add(new VisualStudioCPlusPlusFile(file)));
            else
                files.ForEach(file => CPlusPlusFiles.Add(new VisualStudioCPlusPlusFile(file)));

            // Scan Visual Studio Files For internal References
            if (RunAsParallel)
                Parallel.ForEach(CPlusPlusFiles, (vsFile) => vsFile.ScanFileForReferences());
            else
                CPlusPlusFiles.ForEach(file => file.ScanFileForReferences());

            Helper.PreProcessedFileSave(JsonFile, CPlusPlusFiles);
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



        public void CheckVisualStudioFiles(List<IVisualStudioFile> vsFiles)
        {
            if (vsFiles.Any(vsFile => vsFile == null))
            {
                throw new Exception("VSFile null, try deleting json files Aborting");
            }
        }
    }
}
