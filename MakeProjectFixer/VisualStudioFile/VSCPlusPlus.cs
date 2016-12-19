using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MakeProjectFixer.MakeFile;
using Microsoft.Build.Evaluation;

namespace MakeProjectFixer.VisualStudioFile
{

    internal class VsCplusplus
    {

        // Return include list
        public List<string> ScanCppProjectForIncludeStatements(string projectFileName)
        {
            var fileList = new List<string>();

            // open project file
            // Get all .h and cpp files

            // open check file 
            // search all lines for all #include "filename.h" 
            // add file name to includeList

            var projCollection = new ProjectCollection();
            if (!File.Exists(projectFileName))
                throw new Exception($"Project File {projectFileName} not found");
            var p = projCollection.LoadProject(projectFileName);

            // This add the cpp that need scanning (missing .h files that need scanning)
            foreach (ProjectItem item in p.Items)
            {
                if (item.ItemType == "ClCompile" || item.ItemType == "ClInclude")
                {
                    // File list will include Test files
                    fileList.Add(item.EvaluatedInclude);
                }
            }

            var folder = Path.GetDirectoryName(projectFileName);
            if (folder == null)
            {
                throw new Exception($"Error getting folder name from {projectFileName}");
            }
            var includeFiles = new List<string>();
            foreach (var includeFile in fileList)
            {
                var file = Path.Combine(folder, includeFile);
                var list = ScanFileForIncludeFiles(file);
                includeFiles.AddRange(list);
            }

            var includeReferences = new List<string>();
            foreach (var includeFile in includeFiles)
            {
                // don’t add references that are own project
                if (fileList.Contains(includeFile)) continue;
                // don’t add duplicate references
                if (includeReferences.Contains(includeFile)) continue;

                // only include .h files 
                if (!includeFile.ToLower().Contains(".h")) continue;

                includeReferences.Add(includeFile);
            }
            return includeReferences;
        }

        private List<string> ScanFileForIncludeFiles(string file)
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
                if (line.ToLower().Contains("#include"))
                {
                    var f = line.Remove(0, "#include".Length);
                    f = f.Replace('"', ' ');
                    f = f.Trim();
                    f = f.Trim('<');
                    f = f.Trim('>');
                    list.Add(f);
                }
            }
            return list;
        }

        public Dictionary<string, VisualStudioFile.ProjectFound> 
            GetExpectedMakeProjectRefenences(VisualStudioFile vsFile, List<MakeProject> makeProjects)
        {
            var expectedMakeProjectReferences = new Dictionary<string, VisualStudioFile.ProjectFound>();
            foreach (var refenence in vsFile.RequiredReferencesCpp)
            {
                if (refenence == null) continue;
                if (refenence.Contains("assvc.h"))
                {

                }
                // Check for Publish file
                var mp = makeProjects.FirstOrDefault(m => m.PublishCppHeaderFiles.Contains(refenence));
                if (mp == null)
                {
                    // check for actual make Projects eg. aslex
                    var item = refenence.Replace(".h", "").Replace(".cpp", "");
                    // Must Equal, include case, otherwise fix the project files
                    mp = makeProjects.FirstOrDefault(m => m.ProjectName == item);

                    if (mp == null) continue; // not found
                }

                if (expectedMakeProjectReferences.ContainsKey(mp.ProjectName)) continue; // already added

                if (mp.ProjectName == vsFile.ProjectName) continue; // Don't Add Myself

                expectedMakeProjectReferences.Add(mp.ProjectName, VisualStudioFile.ProjectFound.Found);
            }
            return expectedMakeProjectReferences;
        }

    }
}
