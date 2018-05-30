using System.Collections.Generic;
using ProjectFixer.MakeFile;

namespace ProjectFixer.VisualStudioFile
{
    interface IVisualStudioFile
    {
        /// <summary>
        /// Project Name, Should match in Make File, Case Sensitive
        /// </summary>
        string ProjectName { get;  }

        /// <summary>
        /// Full File Name of the Visual Studio Project
        /// </summary>
        string FileName { get;  }

        /// <summary>
        /// Assembly Name of the Project
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// Expected Make Project Reference that this Visual Studio is depended on.
        /// </summary>
        List<string> ExpectedMakeProjectReference { get; }

        /// <summary>
        /// Scan Visual Studio for References
        /// </summary>
        void ScanFileForReferences();

        /// <summary>
        /// Build ExpectedMakeProjectReference
        /// </summary>
        /// <param name="makeProjects">Full list of Make Projects</param>
        /// <param name="visualStudioFiles">Full List of both CS and CPP Visual Studio projects</param>
        void BuildExpectedMakeProjectReferences(List<MakeProject> makeProjects, List<IVisualStudioFile> visualStudioFiles); 
    }
}
