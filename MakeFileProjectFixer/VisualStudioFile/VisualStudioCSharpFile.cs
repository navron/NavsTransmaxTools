using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MakeFileProjectFixer.MakeFile;
using Microsoft.Build.Evaluation;
using Serilog;

namespace MakeFileProjectFixer.VisualStudioFile
{
    internal class VisualStudioCSharpFile : IVisualStudioFile
    {
        public string ProjectName { get; set; }
        public string FileName { get; set; }
        public string AssemblyName { get; set; }

        public HashSet<string> TsdReferences { get; set; } = new HashSet<string>();
        public HashSet<string> OtherReferences { get; set; } = new HashSet<string>();
        public HashSet<string> IgnoredReferences { get; set; } = new HashSet<string>();
        public HashSet<string> ComReferences { get; set; } = new HashSet<string>();

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
            ScanFileForReferences(FileName);
        }

        public void ScanFileForReferences(string vsFileName)
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
                    // But DynMessSign.Workstation needs to be whole ??
                    var split = referenceName.Split('.');
                    OtherReferences.Add(split[0]);
                }
            }
            // Haven't gone to 64bit yet
            var comReferences = msProject.GetItems("COMReference");
            foreach (ProjectItem projectItem in comReferences)
            {
                ComReferences.Add(projectItem.EvaluatedInclude);
            }
        }

        public void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects, List<IVisualStudioFile> vsFiles)
        {
            var vsCSharpFiles = vsFiles.OfType<VisualStudioCSharpFile>().Select(vsFile => vsFile).ToList();
            var vsCPlusFiles = vsFiles.OfType<VisualStudioCPlusPlusFile>().Select(vsFile => vsFile).ToList();
            ExpectedMakeProjectReference = GetExpectedMakeProjectRefenences(vsCSharpFiles, makeProjects);
        }

        // Requires a Full list of all CSharpe Files for scanning for TSD assemblies
        private List<string> GetExpectedMakeProjectRefenences(List<VisualStudioCSharpFile> vsFiles, List<MakeProject> makeProjects)
        {
            var hashSet = new HashSet<string>();
            foreach (var tsdRefenence in TsdReferences)
            {
                //   if (tsdRefenence == null) continue;

                // examples
                //Tsd.AccessControl.Workstation.Security
                //Tsd.Libraries.Workstation.SettingsUti
                //Tsd.Libraries.Workstation.Interop.Windows

                var project = vsFiles.FirstOrDefault(v => v.AssemblyName == tsdRefenence);
                if (project != null)
                {
                    if (hashSet.Contains(project.ProjectName))
                    {
                        Log.Warning($"Visual Studio File {project.ProjectName} as a duplicate TSD Reference ");
                    }
                    if (project.ProjectName == ProjectName) continue; // Don't Add Myself

                    hashSet.Add(project.ProjectName);
                    continue; // go to next 
                }

                // is in make file
                var checkFor = tsdRefenence.Split('.').Last();
                var test = makeProjects.FirstOrDefault(mp => mp.ProjectName == checkFor);
                if (test != null)
                {
                    hashSet.Add(test.ProjectName);
                }
            }
            // COM References to Make Project, Project Names are incorrect case, not fixing, just working around
            foreach (var comReference in ComReferences)
            {
                if (makeProjects.Any(makeProject => makeProject.ProjectName.Equals(comReference, StringComparison.OrdinalIgnoreCase)))
                {
                    hashSet.Add(comReference); //  Case may be wrong
                }
            }
            if (OtherReferences.Any())
            {
                hashSet.Add("lib3rdparty");
            }
            // Now add the Others (assuming non GAC references)
            //foreach (var otherReference in OtherReferences)
            //{
            //    hashSet.Add(otherReference);
            //}
            return hashSet.ToList();
        }
    }
}
