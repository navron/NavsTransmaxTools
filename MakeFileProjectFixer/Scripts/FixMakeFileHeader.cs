using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using MakeFileProjectFixer.Data;
using Serilog;

namespace MakeFileProjectFixer.Scripts
{
    [Verb("FixMakeFileHeader", HelpText = "Make sure that the head only contains its own project files")]
    internal class FixMakeFileHeader : Options
    {
        public void Run()
        {
            Log.Information($"Running {GetType().Name}");

            var store = new Store(this);
            store.BuildStoreMakeFilesOnly();

            foreach (var makeFile in store.MakeFiles)
            {
                if (!makeFile.Header.IsHeader) throw new Exception($"Make File {makeFile.FileName} has header {makeFile.Header.ProjectName}set incorrectly");

                if (makeFile.IsDirectoryMakeFile())
                    makeFile.Header.DependencyProjects = GetMakeFileDirectoryDependencyList(makeFile, store);
                else
                    CheckMakeFileFile(makeFile);
            }

            store.WriteMakeFiles();
        }

        private void CheckMakeFileFile(MakeFile.MakeFile makeFile)
        {
            Log.Debug($"Scanning MakeFile file:{makeFile.FileName}");

            // The header should only contain its own make file 
            // expect folder makefile, which should contain all header projects in that folder
            var projectList = makeFile.Projects.Where(mp => !mp.ShouldExcluded).Select(mp => mp.ProjectName).ToList();


            var test = new List<string>();
            foreach (var makeFileProject in makeFile.Projects)
            {
                if (makeFileProject.ShouldExcluded)
                {
                    test.Add(makeFileProject.ProjectName);
                }
            }
            var additions = makeFile.Header.DependencyProjects.Except(projectList).ToList();
            if (additions.Any())
            {
                Log.Information("Make File {FileName} has {@Additions} additional projects", makeFile.FileName, additions);
                additions.ForEach(extra => makeFile.Header.DependencyProjects.Remove(extra));
            }

            var missings = projectList.Except(makeFile.Header.DependencyProjects).ToList();
            if (missings.Any())
            {
                Log.Information("Make File {FileName} has {@Missings} missing projects", makeFile.FileName, missings);
                missings.ForEach(missing => makeFile.Header.DependencyProjects.Add(missing));
            }
        }

        private List<string> GetMakeFileDirectoryDependencyList(MakeFile.MakeFile makeFile, Store store)
        {
            Log.Debug($"Scanning Directory MakeFile:{makeFile.FileName}");

            var folder = Path.GetDirectoryName(makeFile.FileName);
            if (folder == null) throw new Exception($"FileName is invalid: {makeFile.FileName} --aborting");
            var folderName = Path.GetFileNameWithoutExtension(makeFile.FileName);

            var files = Directory.EnumerateFiles(folder, "*.mak", SearchOption.TopDirectoryOnly).ToList();
            // Get a listing of all project files with the extension and not including folder project file
            // select into a string with the folder name as the prefix 
            var fileMakeHeaders = (from file in files select Path.GetFileNameWithoutExtension(file) into name where name != folderName select $"{folderName}_{name}").ToList();
           // var fileProjectStore = (from project in store.MakeHeaderProjects where !project.ShouldExcluded && fileMakeHeaders.Contains(project.ProjectName) select project.ProjectName).ToList();

            var makeHeadersExcludeList = (from project in store.MakeHeaderProjects where project.ShouldExcluded && fileMakeHeaders.Contains(project.ProjectName) select project.ProjectName).ToList();

            // The Project List that is in this make file, Directory Make files may still have projects
            var makeProjectsList = makeFile.Projects.Select(p => p.ProjectName).ToList();


            var combineMakeDirectoryAndProjects = new List<string>();
            combineMakeDirectoryAndProjects.AddRange(fileMakeHeaders.Except(makeHeadersExcludeList));
            combineMakeDirectoryAndProjects.AddRange(makeProjectsList);

            if (makeFile.Header.DependencyProjects.Except(combineMakeDirectoryAndProjects).Any())
            {
                Log.Information("Make Folder File {FileName} has {@Addiations} additional projects", makeFile.FileName, makeFile.Header.DependencyProjects.Except(combineMakeDirectoryAndProjects));
            }

            var missings = combineMakeDirectoryAndProjects.Except(makeFile.Header.DependencyProjects).ToList();
            missings = missings.Except(makeFile.Header.DependencyProjects).ToList();
            if (missings.Except(makeFile.Header.DependencyProjects).Any())
            {
                Log.Information("Make File {FileName} has {@Missings} missing projects", makeFile.FileName, missings.Except(makeFile.Header.DependencyProjects));
            }
            return combineMakeDirectoryAndProjects;
        }
    }
}

