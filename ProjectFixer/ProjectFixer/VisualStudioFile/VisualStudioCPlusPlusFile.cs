using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using ProjectFixer.MakeFile;
using ProjectFixer.Utility;
using Serilog;

namespace ProjectFixer.VisualStudioFile
{
    internal class VisualStudioCPlusPlusFile : IVisualStudioFile
    {
        public string ProjectName { get; set; }
        public string FileName { get; set; }
        public string AssemblyName { get; set; }
        public List<string> CodeFiles { get; set; } = new List<string>();
        private List<string> CleanCodeFiles { get; set; } = new List<string>(); // //Testing
        public List<string> RawReferencesIncludes { get; set; } = new List<string>();
        public HashSet<string> ReferencesSet { get; set; } = new HashSet<string>();
        public List<string> ExpectedMakeProjectReference { get; set; } = new List<string>();

        public VisualStudioCPlusPlusFile UnitTestProject { get; set; }

        //  Can't open a project twice, so keep a reference to it
        private Project msProject;

        public VisualStudioCPlusPlusFile(string file)
        {
            FileName = file;

            // File made be null if loading from json, in which case the Assembly and ProjectName are already set
            if (file == null) return;
            AssemblyName = GetAssemblyName(file);
            ProjectName = Path.GetFileNameWithoutExtension(file);

            var unitTestFileName = Path.Combine(Path.GetDirectoryName(file), "UnitTests");
            unitTestFileName = Path.Combine(unitTestFileName, $"{ProjectName}.UnitTests{Path.GetExtension(file)}");
            if (File.Exists(unitTestFileName))
                UnitTestProject = new VisualStudioCPlusPlusFile(unitTestFileName);
        }
        private string GetAssemblyName(string vsFileName)
        {
            // I think I copied this from the CSharp code, don't think it works with CPP
            if (msProject == null)
            {
                try
                {
                    // This only works with the version of Visual Studio that built the program.
                    // Error is Exception The imported project "c:\Microsoft.Cpp.Default.props" was not found.
                    msProject = new Project(vsFileName);
                }
                catch (Exception e)
                {
                    Log.Error($"File {vsFileName} cause a problem");
                    Log.Error($"Exception {e.Message}{Environment.NewLine}{e.StackTrace}");
                    throw new Exception("Aborting");
                }
            }
            var property = msProject.GetProperty("AssemblyName");
            return property.EvaluatedValue;
        }
        public void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects, List<IVisualStudioFile> vsFiles)
        {
            // var vsCSharpFiles = vsFiles.OfType<VisualStudioCSharpFile>().Select(vsFile => vsFile).ToList();
            //    var vsCPlusFiles = vsFiles.OfType<VisualStudioCPlusPlusFile>().Select(vsFile => vsFile).ToList();
            //   var includeUnitTestReferences = IncludeUnitTestReferences(makeProjects);
            //   ExpectedMakeProjectReference = GetExpectedMakeProjectRefenences(makeProjects, vsCPlusFiles, includeUnitTestReferences);
        }

        private bool IncludeUnitTestReferences(List<MakeProject> makeProjects)
        {
            var makeProject = makeProjects.FirstOrDefault(mp => mp.ProjectName == ProjectName);
            return makeProject != null && makeProject.IncludeUnitTestReferences;
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
            // limit to distinct values
            RawReferencesIncludes = RawReferencesIncludes.Distinct().ToList();

            CleanCodeFiles = CodeFiles.Select(Path.GetFileName).ToList();
            ReferencesSet = ProcessIncludeStatements(RawReferencesIncludes, CleanCodeFiles);

            UnitTestProject?.ScanFileForReferences();
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

            //var projCollection = new ProjectCollection();
            //if (!File.Exists(vsFileName))
            //    throw new Exception($"Project File {vsFileName} not found");
            //var project = projCollection.LoadProject(vsFileName);

            //// Get the files that need scanning (missing .h files that need scanning)
            //foreach (ProjectItem item in project.Items)
            //{
            //    // File list will include Test files
            //    if (item.ItemType != "ClCompile" && item.ItemType != "ClInclude") continue;

            //    var checkFile = Path.Combine(folder, item.EvaluatedInclude);
            //    // Visual Studio is normally ok with missing files
            //    if (!File.Exists(checkFile)) continue;

            //    fileList.Add(checkFile);
            //}
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
                if (line.ToLower().Contains("#include"))
                {
                    var f = GetHashInclude(line);
                    if (f.Contains(".h", StringComparison.OrdinalIgnoreCase))
                        list.Add(f);
                }

                if (line.ToLower().Contains("#using") && line.ToLower().Contains("tsd."))
                    list.Add(GetHashUsing(line));
            }
            return list;
        }

        // Bad coding here, don't look to closely 
        internal static string GetHashInclude(string line)
        {
            var f = line.Trim();
            f = f.Remove(0, "#include".Length);
            f = f.Trim();
            f = f.Split(' ').First();
            f = f.Replace('"', ' ');
            f = f.Trim();
            f = f.Trim('<');
            f = f.Trim('>');
            return f;
        }
        internal static string GetHashUsing(string line)
        {
            var t = line.Trim();
            t = t.Remove(0, "#using".Length);
            t = t.Replace('"', ' ');
            t = t.Trim();
            t = t.Trim('<');
            t = t.Trim('>');
            t = t.Replace(".dll", "");
            return t;
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
                // don’t add references that are own by the project
                if (codeFileNames.Contains(reference)) continue;

                // don’t add duplicate references
                if (hashSet.Contains(reference)) continue; //Hash set!!

                // Add any .Net TSD Libraries
                if (reference.Contains(@"Tsd.", StringComparison.OrdinalIgnoreCase))
                {
                    hashSet.Add(reference);
                }

                // only include .h files or not extension
                //if (reference.Contains(".h", StringComparison.OrdinalIgnoreCase) || !reference.Contains('.'))
                //{
                //    var cleanValue = reference.Replace(".h", "");
                //    hashSet.Add(cleanValue);
                //}
                hashSet.Add(reference);
            }
            return hashSet;
        }

        public List<string> GetExpectedMakeProjectRefenences(List<MakeProject> makeProjects, List<VisualStudioCSharpFile> vsFiles, bool includeUnitTestReferences)
        {
            // Add References for tsd if matching .Net Project files (raise warning if not matching)
            DebugRefenences = new List<string>();
            var hashSet = new HashSet<string>();
            foreach (var reference in ReferencesSet)
            {
                if (reference == null) continue;

                if (reference.Contains("Tsd.", StringComparison.OrdinalIgnoreCase))
                {
                    var project = vsFiles.FirstOrDefault(v => v.AssemblyName == reference);
                    if (project != null)
                    {
                        if (project.ProjectName == ProjectName) continue; // Don't Add Myself, wont happen in this cpp case

                        hashSet.Add(project.ProjectName);
                        DebugRefenences.Add($"{project.ProjectName} | .Net Lib");
                        continue; // go to next 
                    }
                    else
                    {
                        Log.Warning($"Possible error with reference {reference} in CPP {ProjectName}, missing from CSharp projects");
                    }
                }

                // Use the header file to find the publishing make file
                if (reference.Contains(".h", StringComparison.OrdinalIgnoreCase))
                {
                    // Check for Publish file
                    //var mp = makeProjects.FirstOrDefault(m => m.PublishCppHeaderFiles.Where(p=>p.Contains(reference, StringComparison.OrdinalIgnoreCase));
                    MakeProject mp = null;
                    foreach (var makeProject in makeProjects)
                    {
                        if (makeProject.PublishCppHeaderFiles.Any(file => file == reference)) // Must match, fix case if needed
                        {
                            mp = makeProject;
                        }
                        if (mp != null) break;
                    }

                    if (mp == null)
                    {
                        // check for actual make Projects eg. aslex
                        var item = reference.Replace(".h", "").Replace(".cpp", "");
                        // Must Equal, include case, otherwise fix the project files
                        mp = makeProjects.FirstOrDefault(m => m.ProjectName == item);

                        if (mp == null) continue; // not found
                    }
                    DebugRefenences.Add($"{mp.ProjectName} | .h publish set for {reference}");
                    if (hashSet.Contains(mp.ProjectName)) continue; // already added

                    if (mp.ProjectName == ProjectName) continue; // Don't Add Myself

                    hashSet.Add(mp.ProjectName);
                }
                // folder or system C++ values
                if (!reference.Contains("."))
                {
                    // Check for Publish file
                    var mp = makeProjects.FirstOrDefault(m => m.ProjectName == reference);
                    if (mp == null) continue;
                    if (mp.ProjectName != ProjectName) // Don't Add Myself
                    {
                        hashSet.Add(mp.ProjectName);
                        DebugRefenences.Add($"{mp.ProjectName} | . folder set for {reference}");
                    }
                }
            }

            var addOthers = new List<string>();

            if (includeUnitTestReferences)
            {
                var unittestsReferences = UnitTestProject?.GetExpectedMakeProjectRefenences(makeProjects, vsFiles, false);
                if (unittestsReferences != null)
                {
                    // Don't add this project
                    foreach (var unittest in unittestsReferences)
                    {
                        if (unittest != ProjectName) hashSet.Add(unittest);
                    }
                }
            }

            // Now if the hashset is empty wait on the cpp libraries call, (its a catch all thing)
            if (!hashSet.Any())
            {
                hashSet.Add("cpplibraries");
            }

            return hashSet.ToList();
        }

        public List<string> DebugRefenences { get; set; }
    }
}
