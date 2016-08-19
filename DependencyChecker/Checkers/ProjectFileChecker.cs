using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;

namespace DependencyChecker.Checkers
{
    /// <summary>
    ///     For checking CS Project files
    /// </summary>
    internal class ProjectFileChecker
    {
        private readonly Stack<string> dependencyStack = new Stack<string>();

        private readonly IOptions options;
        private List<ProjectFile> projectFiles = new List<ProjectFile>();

        public ProjectFileChecker(IOptions options)
        {
            this.options = options;
        }

        /// <summary>
        ///     Check Project files for Known problems
        /// </summary>
        public void CheckProjectFilesForCircularDependencies()
        {
            ScanProjectFiles();
            ProcessProjectFiles();

            dependencyStack.Clear();
            if (projectFiles.Any(projectFile => !CheckProjectFileForRefenenceProblems(projectFile)))
            {
                return;
            }
            Trace.TraceInformation("Project files has been checked for project dependencies problems -- Passed");

            if (projectFiles.Any(projectFile => !CheckForProjectReference(projectFile)))
            {
                return;
            }
            Trace.TraceInformation("Project files has been checked for invalid project references -- Passed");

        }

        private void ScanProjectFiles()
        {
            var excludedlist = new List<string>();
            excludedlist.Add("Tsd.Libraries.Common.Eventing"); // In code but not in build system, Need to Talk to MattH about this
            excludedlist.Add("ManagementConsole"); // Jono special, Its missing a number of files. Needs work, Not in build.

            // Only doing CS Project files for now, could easily include C++ projects
            var files = Directory.EnumerateFiles(options.SourceDirectory, "*.csproj", SearchOption.AllDirectories).ToList();

            projectFiles.Clear();
            foreach (var file in files)
            {
                // Don't want Unit Test in the list. Assume that these are ok
                if (file.Contains(@"\UnitTest")) continue;  // UnitTests and UnitTestSupport Folders

                // Don't want Test in the list. Assume that these are ok
                if (file.Contains(@"\test\")) continue;

                // Remove 3rdparty Project Files. On build machines the 3rdparty lib are check out to $src\lib\3rdparty and thus pick up
                if (file.Contains(@"\3rdparty\")) continue;

                // Exclude any known problems
                if (excludedlist.Any(s => file.Contains(s))) continue;

                projectFiles.Add(new ProjectFile {FileName = file});
            }

            Trace.TraceInformation("Project files scan done. {0} project files (Full Set)", projectFiles.Count);
        }

        private void ProcessProjectFiles()
        {
            // This is slow need so need to parallel the task (testing shows about 27 secs in Parallel to 58 secs)
            Parallel.ForEach(projectFiles, ProcessProjectFile);

            // Remove Non Tsd Files
            projectFiles = projectFiles.Where(projectFile => projectFile.AssemblyName.Contains(@"Tsd.")).ToList();
            Trace.TraceInformation("Processing Project files done. {0} project files (TSD limited set)",
                projectFiles.Count);
        }

        private void ProcessProjectFile(ProjectFile projectFile)
        {
            var projCollection = new ProjectCollection();
            var p = projCollection.LoadProject(projectFile.FileName);
            projectFile.AssemblyName = p.GetPropertyValue("TargetName");

            projectFile.TsdRefenences = new List<string>();
            var references = p.GetItems("Reference");
            foreach (var item in references)
            {
                var include = item.EvaluatedInclude;
                var temp = include.Split(',');

                if (temp[0].Contains(@"Tsd."))
                {
                    projectFile.TsdRefenences.Add(temp[0]);
                }
            }

            // Get Project References, These are all user errors in TMX build system
            var pr = p.GetItems("ProjectReference");
            foreach (var item in pr)
            {
                projectFile.ProjectReference.Add(item.EvaluatedInclude);
            }
        }

        // For each project files walk down the reference list and if 
        // you reach your self then you have a circular dependency
        private bool CheckProjectFileForRefenenceProblems(ProjectFile projectFile)
        {
            if (projectFile.Walked) return true; //Job Already Done

            if (projectFile.TsdRefenencesComplete.Contains(projectFile.AssemblyName))
            {
                Trace.TraceError("Project Assembly contains a reference to itself: {0}", projectFile.AssemblyName);
                return false;
            }
            dependencyStack.Push(projectFile.AssemblyName);

            projectFile.Checking = true;
            foreach (var tsdRefenence in projectFile.TsdRefenences)
            {
                var file = projectFiles.FirstOrDefault(p => p.AssemblyName == tsdRefenence);
                if (file == null) continue; // Great nothing to look up (most likely an non TSD reference)

                if (!file.Walked)
                {
                    if (file.Checking)
                    {
                        var depencyList = string.Join(", ", dependencyStack.ToArray());
                        Trace.TraceError("There is a circular dependency from : {0} ({1})", file.AssemblyName, depencyList);
                        return false;
                    }
                    CheckProjectFileForRefenenceProblems(file);
                }

                projectFile.TsdRefenencesComplete.AddRange(file.TsdRefenences);
            }
            projectFile.Walked = true;
            dependencyStack.Pop();
            return true;
        }

        private bool CheckForProjectReference(ProjectFile projectFile)
        {
            if (projectFile.ProjectReference.Count == 0) return true; // Ok

            foreach (var projectReference in projectFile.ProjectReference)
            {
                Trace.TraceError("TSD Project {0} should not include a Project Reference to {1}. Change to an include reference", projectFile.FileName, projectReference);
            }
            return true;
        }

        /// <summary>
        ///     Rewrites the Make Files in an optimise format
        /// </summary>
        public void FormatMakeFile()
        {
            // TODO Feature not done
            // This function would rewrite all make files by order the put A-Z and optimising the reference 
            // This is need to speed up the build by allow the make infrastructure to parallel more 
        }

        private class ProjectFile
        {
            public ProjectFile()
            {
                Walked = false;
                TsdRefenences = new List<string>();
                TsdRefenencesComplete = new List<string>();
                ProjectReference = new List<string>();
            }

            public string FileName { get; set; }
            public string AssemblyName { get; set; }

            // Tsd References used by this project
            public List<string> TsdRefenences { get; set; }

            // List of Tsd References including tsd References that those references use
            public List<string> TsdRefenencesComplete { get; private set; }

            // List of Project Reference
            public List<string> ProjectReference { get; private set; }

            public bool Walked { get; set; }
            public bool Checking { get; set; }
        }
    }
}