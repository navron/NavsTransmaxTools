using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MakeProjectFixer.MakeFile;
using MakeProjectFixer.Util;
using MakeProjectFixer.VisualStudioFile;

namespace MakeProjectFixer.Data
{
    /// <summary>
    /// Holds an in memory set of Make file an Visual studio files so
    /// that they made be queried quickly by 
    /// </summary>
    internal class Store : Options
    {
        public List<MakeFile.MakeFile> MakeFiles { get; private set; }
        public List<VisualStudioFile.VisualStudioFile> VisualStudioFiles { get; private set; }
        public List<MakeProject> MakeProjects { get; private set; }
        public List<MakeProject> MakeHeaderProjects { get; private set; }
        public List<MakeProject> AllMakeProjects { get; private set; }

        public Store()
        {
            MakeFiles = new List<MakeFile.MakeFile>();
            MakeProjects = new List<MakeProject>();
            MakeHeaderProjects = new List<MakeProject>();
            AllMakeProjects = new List<MakeProject>();
            VisualStudioFiles = new List<VisualStudioFile.VisualStudioFile>();
        }

        // Build Both the Make Files and Visual Studio files Store
        public void BuildStore()
        {
            Program.Console.WriteLine($"Running {GetType().Name}", ConsoleColor.Cyan);

            BuildStoreMakeFilesOnly();

            ProcessVsFilesStage1BuildFileList();
            ProcessVsFilesStage2ScanFilesForReferences();
            ProcessVsFilesStage3BuildExpectedMakeProjectReferences();
            ProcessVsFilesStage4MatchUpMakeProject();
        }

        // Only build the Make Files
        public void BuildStoreMakeFilesOnly()
        {
            ProcessMakeFiles();
            ProcessBuildMakeProjectList();
            CheckMakeProjects(MakeProjects);
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
                make.ScanRawLinesForPublishItems();
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
            AllMakeProjects.AddRange(MakeProjects);
            AllMakeProjects.AddRange(MakeHeaderProjects);

            foreach (var makeProject in AllMakeProjects)
            {
                foreach (var project in makeProject.DependencyProjects)
                {
                    var t = AllMakeProjects.FirstOrDefault(a => a.ProjectName == project);
                    if (t == null) continue; //bug that should ok to ignore
                    if (makeProject.DependencyProjects2.ContainsKey(project))
                    {
                        Program.Console.WriteLine($"Warning Make Project {makeProject.ProjectName} has duplicate reference {project} Hand Edit to fix", ConsoleColor.Red);
                        continue;
                    }
                    makeProject.DependencyProjects2.Add(project, t.IsHeader);
                }
            }
        }

        private void ProcessVsFilesStage1BuildFileList()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
                CheckVisualStudioFiles(VisualStudioFiles);
                return;
            }

            // Step 2 Read all Visual Studio Files 
            SearchPatterns = new[] { "*.csproj", "*.vcxproj" };
            // Find and limit return set to what is required
            var files = Helper.FindFiles(this).Where(VisualStudioFileHelper.IncludeFile);

            if (SerialMode)
            {
                foreach (var file in files)
                {
                    var vsFile = new VisualStudioFile.VisualStudioFile(file);
                    VisualStudioFiles.Add(vsFile);
                }
            }
            else
            {
                Parallel.ForEach(files, (file) => VisualStudioFiles.Add(new VisualStudioFile.VisualStudioFile(file)));
            }
            Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        }

        private void ProcessVsFilesStage2ScanFilesForReferences()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
                CheckVisualStudioFiles(VisualStudioFiles);
                return;
            }
            
            if (SerialMode)
            {
                foreach (var vsFile in VisualStudioFiles)
                {
                    vsFile.ScanFileForReferences();
                }
            }
            else
            {
                Parallel.ForEach(VisualStudioFiles, (vsFile) => vsFile.ScanFileForReferences());
            }

            Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        }

        private void ProcessVsFilesStage3BuildExpectedMakeProjectReferences()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
                CheckVisualStudioFiles(VisualStudioFiles);
                return;
            }

            if (SerialMode)
            {
                foreach (var vsFile in VisualStudioFiles)
                {
                    if (vsFile.ProjectName.Contains("assvc"))
                    {

                    }
                    BuildExpectedMakeProjectReferences(vsFile, MakeProjects, VisualStudioFiles);
                }
            }
            else
            {
                Parallel.ForEach(VisualStudioFiles, (vsFile) => BuildExpectedMakeProjectReferences(vsFile, MakeProjects, VisualStudioFiles));
            }

            Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        }

        // Method done before MatchUpMakeProject
        public void BuildExpectedMakeProjectReferences(VisualStudioFile.VisualStudioFile vsFile, List<MakeProject> makeProjects, List<VisualStudioFile.VisualStudioFile> vsFiles)
        {
            if (vsFile.ProjectName == "VideoManagement.Workstation")
            {

            }
            if (vsFile.ProjectType == VisualStudioFile.VisualStudioFile.ProjectTypeValue.CSharp)
            {
                var vscs = new VsCsharp();
                vsFile.ExpectedMakeProjectReferences = vscs.GetExpectedMakeProjectRefenences(vsFile, vsFiles);
            }
            if (vsFile.ProjectType == VisualStudioFile.VisualStudioFile.ProjectTypeValue.Cpp)
            {
                var vscpp = new VsCplusplus();
                vsFile.ExpectedMakeProjectReferences = vscpp.GetExpectedMakeProjectRefenences(vsFile, makeProjects);
            }
            vsFile.ExpectedMakeProjectReferences.Add("prebuild_3rdparty",VisualStudioFile.VisualStudioFile.ProjectFound.Found); // Stupid rule, need to fix 
        }

        private void ProcessVsFilesStage4MatchUpMakeProject()
        {
            if (Helper.PreProcessedObject(System.Reflection.MethodBase.GetCurrentMethod().Name, this))
            {
                VisualStudioFiles = Helper.JsonSerialization.ReadFromJsonFile<List<VisualStudioFile.VisualStudioFile>>(JsonFile);
                CheckVisualStudioFiles(VisualStudioFiles);
                return;
            }

            if (SerialMode)
            {
                foreach (var visualStudioFile in VisualStudioFiles)
                {
                    if (visualStudioFile.ProjectName.Contains("acsvc"))
                    {

                    }
                    visualStudioFile.MatchUpMakeProject(MakeProjects);
                }
            }
            else
            {
                Parallel.ForEach(VisualStudioFiles, (vsFile) => vsFile.MatchUpMakeProject(MakeProjects));
            }

            Helper.PreProcessedFileSave(JsonFile, VisualStudioFiles);
        }

        public void CheckMakeProjects(List<MakeProject> makeProjects)
        {
            foreach (MakeProject makeProject in makeProjects)
            {
                if (makeProject.ProjectName == null)
                {
                    // Most likely an make file error. 
                    // Print out in Red and abort

                    Program.Console.WriteLine("Error with Make File Project, Project Name Null", ConsoleColor.Red);
                    foreach (var line in makeProject.FormatMakeProject(true, 200, false))
                    {
                        Program.Console.WriteLine(line, ConsoleColor.DarkYellow);
                    }
                    throw new Exception("Aborting");
                }
            }
        }
        public void CheckVisualStudioFiles(List<VisualStudioFile.VisualStudioFile> vsFiles)
        {
            foreach (var vsFile in vsFiles)
            {
                if (vsFile == null)
                {
                    throw new Exception("VSFile null, try deleting json files Aborting");
                }
            }
        }
    }
}

