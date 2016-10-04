using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MakeProjectFixer.MakeFile;
using Newtonsoft.Json;

namespace MakeProjectFixer.VisualStudioFile
{
    class VisualStudioFile
    {
        // Project Name, Should match in Make File, Case Sensitive
        public string ProjectName { get; set; }

        public string FileName { get; set; }

        public string AssemblyName { get; set; }

        // CS or C++
        public enum ProjectTypeValue { NotSet = 0, Cs, Cpp }

        public ProjectTypeValue ProjectType { get; }

        // List of TSD Reference DLL C#
        public List<string> TsdReferences { get; private set; }

        // List of #include Files C++
        public List<string> IncludeReferences { get; private set; }

        public enum ProjectFound
        {
            NotLooked = 0,
            NotFound,
            Found,
            FoundCaseWrong
        }

        // List of Expected Make Project References and if they where found
        public Dictionary<string, ProjectFound> ExpectedMakeProjectReferences { get; set; }

        public VisualStudioFile(string file)
        {
            TsdReferences = new List<string>();
            IncludeReferences = new List<string>();
            ExpectedMakeProjectReferences = new Dictionary<string, ProjectFound>();
            FileName = file;
            ProjectName = Path.GetFileNameWithoutExtension(file);

            var extension = Path.GetExtension(file);
            if (extension != null)
            {
                if (extension.ToLower().Contains(@"csproj")) ProjectType = ProjectTypeValue.Cs;
                if (extension.ToLower().Contains(@"vcxproj")) ProjectType = ProjectTypeValue.Cpp;
            }
        }

        public void ScanFileForReferences()
        {
            if (ProjectType == ProjectTypeValue.Cs)
            {
                var vscs = new VsCsharp();
                vscs.OpenProject(FileName);
                TsdReferences = vscs.GetTsdReferences();
                AssemblyName = vscs.GetAssemblyName();
            }
            if (ProjectType == ProjectTypeValue.Cpp)
            {
                var vscpp = new VsCplusplus();
                IncludeReferences = vscpp.ScanCppProjectForIncludeStatements(FileName);
            }
        }

        // Method done before MatchUpMakeProject
        public void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects)
        {
            if (ProjectType == ProjectTypeValue.Cs)
            {
                var vscs = new VsCsharp();
                ExpectedMakeProjectReferences = vscs.GetExpectedMakeProjectRefenences(TsdReferences);
            }
            if (ProjectType == ProjectTypeValue.Cpp)
            {
                var vscpp = new VsCplusplus();
                ExpectedMakeProjectReferences = vscpp.GetExpectedMakeProjectRefenences(IncludeReferences, makeProjects);
            }
        }

        // Scan the ExpectedMakeProjectReferences and match up the Case to the Make Project list
        // Fist Fix the Make Project name from the Visual Studio Project Names (this method was done ass about)
        public void MatchUpMakeProject(List<MakeProject> makeProjects)
        {
            var updateReferences = new Dictionary<string, ProjectFound>();
            foreach (var reference in ExpectedMakeProjectReferences)
            {
                var found = makeProjects.Any(m => string.Equals(m.ProjectName, reference.Key, StringComparison.Ordinal));
                if (found)
                {
                    updateReferences[reference.Key] = ProjectFound.Found;
                    continue;
                }

                var foundCaseWrong =
                    makeProjects.Any(
                        m => string.Equals(m.ProjectName, reference.Key, StringComparison.OrdinalIgnoreCase));
                if (foundCaseWrong)
                {
                    updateReferences[reference.Key] = ProjectFound.FoundCaseWrong;
                    continue;
                }
                updateReferences[reference.Key] = ProjectFound.NotFound;
            }
            ExpectedMakeProjectReferences = updateReferences;
        }
    }
}
