using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MakeFileProjectFixer.MakeFile;
using Microsoft.Build.Evaluation;
using Serilog;

namespace MakeFileProjectFixer.VisualStudioFile
{
    internal class VisualStudioCSharpFile : IVisualStudioFile
    {
        public string ProjectName { get; }
        public string FileName { get; }
        public string AssemblyName { get; }
        public List<string> RequiredReferences { get; set; }
        public List<string> ExpectedMakeProjectReference { get; set; }

      //  private ProjectCollection msProjectCollection;
        private Project msProject;

        public VisualStudioCSharpFile(string file)
        {
            FileName = file;
            RequiredReferences = new List<string>();
            ExpectedMakeProjectReference = new List<string>();

            //TODO Set ProjectName and AssemblyName
            if (file != null)
            {
                AssemblyName = GetAssemblyName(file);
                ProjectName = Path.GetFileNameWithoutExtension(file);
            }
        }

        public void ScanFileForReferences()
        {
            RequiredReferences = GetTsdReferences(FileName);
        }

        public void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects, List<IVisualStudioFile> vsFiles)
        {
            ExpectedMakeProjectReference = GetExpectedMakeProjectRefenences(vsFiles);
        }

        private List<string> GetTsdReferences(string vsFileName)
        {
            var list = new List<string>();

            if (msProject == null) msProject = new Project(vsFileName);
            var references = msProject.GetItems("Reference");
            foreach (var reference in references)
            {
                var include = reference.EvaluatedInclude;
                var temp = include.Split(',');
                if (temp[0].Contains(@"Tsd."))
                {
                    list.Add(temp[0]);
                }
            }
            return list;
        }

        private string GetAssemblyName(string vsFileName)
        {
            if(msProject == null) msProject = new Project(vsFileName);
            var property = msProject.GetProperty("AssemblyName");
            return property.EvaluatedValue;
        }

        private List<string> GetExpectedMakeProjectRefenences(List<IVisualStudioFile> vsFiles)
        {
            var expectedMakeProjectReferences = new List<string>();
            foreach (var tsdRefenence in RequiredReferences)
            {
                if (tsdRefenence == null) continue;

                // examples
                //Tsd.AccessControl.Workstation.Security
                //Tsd.Libraries.Workstation.SettingsUti
                //Tsd.Libraries.Workstation.Interop.Windows

                var project = vsFiles.FirstOrDefault(v => v.AssemblyName == tsdRefenence);
                if (project == null) continue;

                if (expectedMakeProjectReferences.Contains(project.ProjectName))
                {
                    Log.Warning($"Visual Studio File {project.ProjectName} as a duplicate TSD Reference ");
                }
                if (project.ProjectName == ProjectName) continue; // Don't Add Myself

                expectedMakeProjectReferences.Add(project.ProjectName);
            }
            return expectedMakeProjectReferences;
        }


        // Dictionary<string,string> Old String, New String
        public void UpdateCaseReference(string fileName, Dictionary<string, string> tsdReferenceUpdate)
        {
            //var projCollection = new ProjectCollection();
            //var p = projCollection.LoadProject(fileName);

            // Ok this sucks, the tsdReference is a sub string of the Reference.
            // try a search and replace and check with git, can't be that many

            // This adds a new line at the end, not good
            //var lines = File.ReadAllLines(fileName);
            //var newlines = new List<string>();
            //foreach (var line in lines)
            //{
            //    var newLine = line;
            //    foreach (KeyValuePair<string, string> pair in tsdReferenceUpdate)
            //    {
            //        newLine = line.Replace(pair.Key, pair.Value);
            //    }
            //    newlines.Add(newLine);
            //}
            //File.WriteAllLines(fileName,newlines);

            var text = File.ReadAllText(fileName);
            foreach (KeyValuePair<string, string> pair in tsdReferenceUpdate)
            {
                text = text.Replace(pair.Key, pair.Value);
            }
            File.WriteAllText(fileName, text, Encoding.UTF8);
        }
    }
}
