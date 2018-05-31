using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using ProjectFixer.MakeFile;
using Serilog;

namespace ProjectFixer.VisualStudioFile
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
        public List<string> MyExpectedMakeProjectReference { get; set; } = new List<string>();

        public VisualStudioCSharpFile UnitTestProject { get; set; }

        //  Can't open a project twice, so keep a reference to it
        private Project msProject;

        public VisualStudioCSharpFile(string file)
        {
            FileName = file;

            // File made be null if loading from json, in which case the Assembly and ProjectName are already set
            if (file == null) return;
            AssemblyName = GetAssemblyName(file);
            ProjectName = Path.GetFileNameWithoutExtension(file);

            var unitTestFileName = Path.Combine(Path.GetDirectoryName(file), "UnitTests");
            unitTestFileName = Path.Combine(unitTestFileName, $"{ProjectName}.UnitTests{Path.GetExtension(file)}");
            if (File.Exists(unitTestFileName))
                UnitTestProject = new VisualStudioCSharpFile(unitTestFileName);
        }

        private string GetAssemblyName(string vsFileName)
        {
            if (msProject == null)
            {
                try
                {
                    msProject = new Project(vsFileName);
                }
                catch (Microsoft.Build.Exceptions.InvalidProjectFileException e)
                {
                    Log.Error($"File {vsFileName} is most likely an .NetCore project, add to ingore list");
                    throw new Exception("Aborting");
                }

            }
            var property = msProject.GetProperty("AssemblyName");
            return property.EvaluatedValue;
        }
        public void ScanFileForReferences()
        {
            ScanFileForReferences(FileName);
            UnitTestProject?.ScanFileForReferences();
        }

        private void ScanFileForReferences(string vsFileName)
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
                else if (referenceName.Contains(@"ait.specs"))
                    TsdReferences.Add(referenceName);
                else if (referenceName.Contains(@"ait.utils"))
                    TsdReferences.Add(referenceName);
                else if (referenceName.Contains(@"ait.screens"))
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
            //   var vsCPlusFiles = vsFiles.OfType<VisualStudioCPlusPlusFile>().Select(vsFile => vsFile).ToList();
            ExpectedMakeProjectReference = GetExpectedMakeProjectRefenences(vsCSharpFiles, makeProjects).ToList();

            UnitTestProject?.BuildExpectedMakeProjectReferences(makeProjects, vsFiles);
        }

        // Requires a Full list of all CSharpe Files for scanning for TSD assemblies
        internal List<string> GetExpectedMakeProjectRefenences(List<VisualStudioCSharpFile> vsFiles, List<MakeProject> makeProjects)
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

            // Now add the Others (assuming non GAC references) and only if there matching make project
            var addOthers = OtherReferences.Where(otherReference => makeProjects.Any(mp => mp.ProjectName == otherReference)).ToList();

            foreach (var s in hashSet)
            {
                MyExpectedMakeProjectReference.Add(s);
            }

            var unittests = UnitTestProject?.GetExpectedMakeProjectRefenences(vsFiles, makeProjects);
            if (unittests != null)
            {
                foreach (var unittest in unittests)
                {
                    if (unittest != ProjectName) hashSet.Add(unittest);
                }
                addOthers = addOthers.Except(unittests.ToList()).ToList();
            }

            // Add either all 3rd party copy or just NUnit (all CSharp projects have unit tests)
            if (addOthers.Any())
            {
                addOthers.ForEach(o => hashSet.Add(o));
            }
            else
            {
                hashSet.Add("lib3rdparty"); //Dont like this     
            }


            return hashSet.ToList();
        }
    }
}
