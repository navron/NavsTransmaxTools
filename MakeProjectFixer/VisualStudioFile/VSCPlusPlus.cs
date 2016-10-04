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
            if(!File.Exists(projectFileName))
                throw new Exception($"Project File {projectFileName} not found");
            var p = projCollection.LoadProject(projectFileName);

            foreach (ProjectItem item in p.Items)
            {
                if (item.ItemType == "ClCompile")
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
                    list.Add(f);
                }
            }
            return list;
        }

        public Dictionary<string, VisualStudioFile.ProjectFound> GetExpectedMakeProjectRefenences
                        (List<string> includeReferences, List<MakeProject> makeProjects)
        {
            var expectedMakeProjectReferences = new Dictionary<string, VisualStudioFile.ProjectFound>();
            foreach (var refenence in includeReferences)
            {
                if (refenence == null) continue;

                var mp = makeProjects.FirstOrDefault(m => m.PublishCppHeaderFiles.Contains(refenence));
                if(mp == null) continue; // not found

                if (expectedMakeProjectReferences.ContainsKey(mp.ProjectName)) continue; // already added

                expectedMakeProjectReferences.Add(mp.ProjectName,VisualStudioFile.ProjectFound.Found);
            }
            return expectedMakeProjectReferences;
        }

    }
}
