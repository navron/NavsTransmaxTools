using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MakeFileProjectFixer.MakeFile;

namespace MakeFileProjectFixer.VisualStudioFile
{
    interface IVisualStudioFile
    {
        // Project Name, Should match in Make File, Case Sensitive
        string ProjectName { get;  }

        string FileName { get;  }

        string AssemblyName { get; }

        // CS or C++
//        private enum ProjectTypeValue { NotSet = 0, CSharp, CPlusPlus }
//        public ProjectTypeValue ProjectType { get; set; }

        // List of TSD Reference DLL and 3rd Party References (excluding .Net Framework)
        List<string> RequiredReferences { get; }
        List<string> ExpectedMakeProjectReference { get; }

        void ScanFileForReferences();

        void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects, List<IVisualStudioFile> vsFiles); // , List<IVisualStudioFile> visualStudioFiles);


        // List of #include Files C++
        // public List<string> RequiredReferencesCpp { get; set; }

        //public enum ProjectFound
        //{
        //    NotLooked = 0,
        //    NotFound,
        //    Found,
        //    FoundCaseWrong
        //}

        // List of Expected Make Project References and if they where found
        // public Dictionary<string, ProjectFound> ExpectedMakeProjectReferences { get; set; }

        //protected VisualStudioFile(string file)
        //{
        //    // file is null when loading via json file
        //    if (file != null)
        //        ProcessFile(file);
        //}

        //private void ProcessFile(string file)
        //{
        //    RequiredReferencesCSharp = new List<string>();
        //    RequiredReferencesCpp = new List<string>();
        //    ExpectedMakeProjectReferences = new Dictionary<string, ProjectFound>();
        //    FileName = file;
        //    ProjectName = Path.GetFileNameWithoutExtension(file);

        //    var extension = Path.GetExtension(file);
        //    if (extension == null) return;
        //    if (extension.ToLower().Contains(@"csproj")) ProjectType = ProjectTypeValue.CSharp;
        //    if (extension.ToLower().Contains(@"vcxproj")) ProjectType = ProjectTypeValue.CPlusPlus;
        //}

        //public void ScanFileForReferences()
        //{
        //    if (ProjectType == ProjectTypeValue.CSharp)
        //    {
        //        var vscs = new VsCsharp();
        //        vscs.OpenProject(FileName);
        //        RequiredReferencesCSharp = vscs.GetTsdReferences();
        //        AssemblyName = vscs.GetAssemblyName();
        //    }
        //    if (ProjectType == ProjectTypeValue.CPlusPlus)
        //    {
        //        var vscpp = new VsCplusplus();
        //        if(FileName.Contains("assvc."))
        //        {

        //        }
        //        RequiredReferencesCpp = vscpp.ScanCppProjectForIncludeStatements(FileName);
        //    }
        //}

        // Scan the ExpectedMakeProjectReferences and match up the Case to the Make Project list
        // Fist Fix the Make Project name from the Visual Studio Project Names (this method was done ass about)
        //public void MatchUpMakeProject(List<MakeProject> makeProjects)
        //{
        //    var updateReferences = new Dictionary<string, ProjectFound>();
        //    foreach (var reference in ExpectedMakeProjectReferences)
        //    {
        //        var found = makeProjects.Any(m => string.Equals(m.ProjectName, reference.Key, StringComparison.Ordinal));
        //        if (found)
        //        {
        //            updateReferences[reference.Key] = ProjectFound.Found;
        //            continue;
        //        }

        //        var foundCaseWrong =makeProjects.Any(m => string.Equals(m.ProjectName, reference.Key, StringComparison.OrdinalIgnoreCase));
        //        if (foundCaseWrong)
        //        {
        //            updateReferences[reference.Key] = ProjectFound.FoundCaseWrong;
        //            continue;
        //        }
        //        updateReferences[reference.Key] = ProjectFound.NotFound;
        //    }
        //    ExpectedMakeProjectReferences = updateReferences;
        //}
    }
}
