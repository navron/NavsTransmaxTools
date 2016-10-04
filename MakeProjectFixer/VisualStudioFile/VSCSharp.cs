using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;

namespace MakeProjectFixer.VisualStudioFile
{
    internal class VsCsharp : IDisposable
    {
        private ProjectCollection projectCollection;
        private Project project;
        public VsCsharp()
        {
        }

        public void OpenProject(string file)
        {
            projectCollection = new ProjectCollection();
            project = projectCollection.LoadProject(file);
        }
        public List<string> GetTsdReferences()
        {
            var tsdReferences = new List<string>();

            //var projCollection = new ProjectCollection();
            //var p = projCollection.LoadProject(projectFileName);
            var references = project.GetItems("Reference");
            foreach (var item in references)
            {
                var include = item.EvaluatedInclude;
                var temp = include.Split(',');
                if (temp[0].Contains(@"Tsd."))
                {
                    tsdReferences.Add(temp[0]);
                }
            }
            return tsdReferences;
        }

        public string GetAssemblyName()
        {
            var p = project.GetProperty("AssemblyName");
            return p.EvaluatedValue;
        }

        public Dictionary<string, VisualStudioFile.ProjectFound> GetExpectedMakeProjectRefenences(List<string> tsdReferences)
        {
            var expectedMakeProjectReferences = new Dictionary<string, VisualStudioFile.ProjectFound>();
            foreach (var tsdRefenence in tsdReferences)
            {
                if (tsdRefenence == null) continue;

                var t = tsdRefenence.Split('.');
                var projectName = t.Last();
                if (projectName.ToLower() == "workstation")
                {
                    projectName = tsdRefenence.Replace(@"Tsd.", "");
                }

                if (expectedMakeProjectReferences.ContainsKey(projectName))
                {
                    Console.WriteLine($"Visual Studio File XXX as a duplicate TSD Reference {projectName}");
                }
                expectedMakeProjectReferences.Add(projectName, VisualStudioFile.ProjectFound.NotLooked);
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
