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

        public HashSet<string> TsdReferences { get; set; } = new HashSet<string>();
        public HashSet<string> OtherReferences { get; set; } = new HashSet<string>();
        public HashSet<string> IgnoredReferences { get; set; } = new HashSet<string>();

        public List<string> ExpectedMakeProjectReference { get; set; } = new List<string>();

        //  Can't open a project twice, so keep a reference to it
        private Project msProject;

        public VisualStudioCSharpFile(string file)
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

        public void ScanFileForReferences()
        {
            if (msProject == null) msProject = new Project(FileName);

            var references = msProject.GetItems("Reference");
            foreach (var reference in references)
            {
                var include = reference.EvaluatedInclude;
                var referenceSplit = include.Split(',');
                var referenceName = referenceSplit[0];
                if (referenceName.Contains(@"Tsd."))
                    TsdReferences.Add(referenceName);
                else if (referenceName.StartsWith("System.") || referenceName == "System")
                    IgnoredReferences.Add(referenceName);
                else
                {
                    // Only take the first part of the name, so that "Castle.Core" and "Castle.ActiveRecord" are one record of Castle
                    var split = referenceName.Split('.');
                    OtherReferences.Add(split[0]);
                }
            }
        }

        public void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects, List<IVisualStudioFile> vsFiles)
        {
            var vsCSharpFiles = vsFiles.OfType<VisualStudioCSharpFile>().Select(vsFile => vsFile as VisualStudioCSharpFile).ToList();
            ExpectedMakeProjectReference = GetExpectedMakeProjectRefenences(vsCSharpFiles);
        }

        // Requires a Full list of all CSharpe Files for scanning for TSD assemblies
        private List<string> GetExpectedMakeProjectRefenences(List<VisualStudioCSharpFile> vsFiles)
        {
            var hashSet = new HashSet<string>();
            foreach (var tsdRefenence in TsdReferences)
            {
                if (tsdRefenence == null) continue;

                // examples
                //Tsd.AccessControl.Workstation.Security
                //Tsd.Libraries.Workstation.SettingsUti
                //Tsd.Libraries.Workstation.Interop.Windows

                var project = vsFiles.FirstOrDefault(v => v.AssemblyName == tsdRefenence);
                if (project == null) continue;

                if (hashSet.Contains(project.ProjectName))
                {
                    Log.Warning($"Visual Studio File {project.ProjectName} as a duplicate TSD Reference ");
                }
                if (project.ProjectName == ProjectName) continue; // Don't Add Myself

                hashSet.Add(project.ProjectName);
            }
            // Now add the Others (assuming non GAC references)
            foreach (var otherReference in OtherReferences)
            {
                hashSet.Add(otherReference);
            }
            return hashSet.ToList();
        }
    }
}
