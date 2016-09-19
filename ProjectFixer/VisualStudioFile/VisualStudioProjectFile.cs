using System.Collections.Generic;

namespace ProjectFixer.VisualStudioFile
{
    class VisualStudioProjectFile
    {
        public VisualStudioProjectFile()
        {
            DependencyProjects = new List<string>();
        }

        /// <summary>
        /// Name of the Project, should make the Make file Project
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// File name full path
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// List of TSD Projects that are Dependencies
        /// </summary>
        public List<string> DependencyProjects { get; set; }
    }


    class CSharpProjectFile : VisualStudioProjectFile
    {


    }

    class VisualCProjectFile : VisualStudioProjectFile
    {

    }

    class MyClass
    {
        
    }
}
