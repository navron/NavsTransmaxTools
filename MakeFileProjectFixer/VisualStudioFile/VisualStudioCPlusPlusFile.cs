using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MakeFileProjectFixer.MakeFile;
using Microsoft.Build.Evaluation;

namespace MakeFileProjectFixer.VisualStudioFile
{
    internal class VisualStudioCPlusPlusFile : IVisualStudioFile
    {
        public string ProjectName { get; set; }
        public string FileName { get; set; }
        public string AssemblyName { get; set; }
        public List<string> CodeFiles { get; set; } = new List<string>();
        public List<string> RawReferencesIncludes { get; set; } = new List<string>();
        public HashSet<string> ReferencesSet { get; set; } = new HashSet<string>();
        public List<string> ExpectedMakeProjectReference { get; set; } = new List<string>();

        //  Can't open a project twice, so keep a reference to it
        private Project msProject;


        public VisualStudioCPlusPlusFile(string file)
        {
            FileName = file;

            // File made be null if loading from json, in which case the Assembly and ProjectName are already set
            if (file == null) return;
            AssemblyName = GetAssemblyName(file);
            ProjectName = Path.GetFileNameWithoutExtension(file);
        }
        private string GetAssemblyName(string vsFileName)
        {
            if (msProject == null) msProject = new Project(vsFileName);
            var property = msProject.GetProperty("AssemblyName");
            return property.EvaluatedValue;
        }
        public void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects, List<IVisualStudioFile> vsFiles)
        {
            ExpectedMakeProjectReference = GetExpectedMakeProjectRefenences(makeProjects);
        }

        public void ScanFileForReferences()
        {
            // Build A Scan list of all Valid files from this project
            CodeFiles = BuildFileScanList(FileName);

            // Scan Each file for RawReferencesIncludes Statements
            foreach (var includeFile in CodeFiles)
            {
                var statements = ScanCodeFileForIncludeStatements(includeFile);
                RawReferencesIncludes.AddRange(statements);
            }

            var cleanCodeFiles = CodeFiles.Select(Path.GetFileNameWithoutExtension).ToList();
            ReferencesSet = ProcessIncludeStatements(RawReferencesIncludes, cleanCodeFiles);
        }

        private List<string> BuildFileScanList(string vsFileName)
        {
            var folder = Path.GetDirectoryName(vsFileName);
            if (folder == null)
            {
                throw new Exception($"Can't get folder from filename {vsFileName}");
            }
            // File List to Scan
            var fileList = new List<string>();

            // open project file
            // Get all .h and cpp files

            // open check file 
            // search all lines for all #include "filename.h" 
            // add file name to includeList

            var projCollection = new ProjectCollection();
            if (!File.Exists(vsFileName))
                throw new Exception($"Project File {vsFileName} not found");
            var project = projCollection.LoadProject(vsFileName);

            // Get the files that need scanning (missing .h files that need scanning)
            foreach (ProjectItem item in project.Items)
            {
                // File list will include Test files
                if (item.ItemType != "ClCompile" && item.ItemType != "ClInclude") continue;

                var checkFile = Path.Combine(folder, item.EvaluatedInclude);
                // Visual Studio is normally ok with missing files
                if (!File.Exists(checkFile)) continue;

                fileList.Add(checkFile);
            }
            return fileList;
        }

        private List<string> ScanCodeFileForIncludeStatements(string file)
        {
            var list = new List<string>();

            if (!File.Exists(file))
            {
                // Error Project file has an not existing file, VS doesn't care about these files.
                return list;
            }

            var lines = File.ReadLines(file);
            foreach (var line in lines)
            {
                if (!line.ToLower().Contains("#include")) continue;
                var f = line.Remove(0, "#include".Length);
                f = f.Replace('"', ' ');
                f = f.Trim();
                f = f.Trim('<');
                f = f.Trim('>');
                list.Add(f);
            }
            return list;
        }

        /// <summary>
        /// Process Include Statements and reduce to a clean set that should match Make Project publish
        /// </summary>
        /// <param name="rawReferencesIncludes"></param>
        /// <param name="codeFileNames">List of File Names without Path or extension</param>
        /// <returns></returns>
        private HashSet<string> ProcessIncludeStatements(List<string> rawReferencesIncludes, List<string> codeFileNames)
        {
            // Take the File name or the folder name 
           // var list = RawReferencesIncludes.Select(referencesInclude => referencesInclude.Split('/').First()).ToList();
           var list = new List<string>();
            foreach (var include in rawReferencesIncludes)
            {
                var t = include.Split('/');
                if (t.Length > 1)
                {
                    
                }
                var s = t.First();

                    list.Add(s);
            }

            var hashSet = new HashSet<string>();
            foreach (var reference in list)
            {
                // don’t add references that are own project
                if (codeFileNames.Contains(reference)) continue;

                // don’t add duplicate references
                if (hashSet.Contains(reference)) continue;

                // only include .h files or not extension
                if (reference.ToLower().Contains(".h") || !reference.ToLower().Contains('.'))
                {
                    var cleanValue = reference.Replace(".h", "");
                    hashSet.Add(cleanValue);
                }           
            }
            return hashSet;
        }

        public List<string> GetExpectedMakeProjectRefenences(List<MakeProject> makeProjects)
        {
            var list = new List<string>();
            foreach (var reference in ReferencesSet)
            {
                if (reference == null) continue;

                // Check for Publish file
                var mp = makeProjects.FirstOrDefault(m => m.PublishCppHeaderFiles.Contains(reference));
                if (mp == null)
                {
                    // check for actual make Projects eg. aslex
                    var item = reference.Replace(".h", "").Replace(".cpp", "");
                    // Must Equal, include case, otherwise fix the project files
                    mp = makeProjects.FirstOrDefault(m => m.ProjectName == item);

                    if (mp == null) continue; // not found
                }

                if (list.Contains(mp.ProjectName)) continue; // already added

                if (mp.ProjectName == ProjectName) continue; // Don't Add Myself

                list.Add(mp.ProjectName);
            }
            if (!list.Any())
            {
                list.Add("cpplibraries");
            }
           
            return list;
        }
    }
}
