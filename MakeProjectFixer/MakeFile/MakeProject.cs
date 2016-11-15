using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MakeProjectFixer.MakeFile
{
    /// <summary>
    /// A Make project is a single Make Target, May be a header or body target
    /// </summary>
    public class MakeProject
    {
        public MakeProject()
        {
            RawLines = new List<string>();
            PreLines = new List<string>();
            ProjectLines = new List<string>();
            PostLines = new List<string>();
            DependencyProjects = new List<string>();
            PublishCppHeaderFiles = new List<string>();
            DependencyProjects2 = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Project ProjectName
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// List of Current Projects marked are Dependencies
        /// </summary>
        public List<string> DependencyProjects { get; set; }
        // Dependency and IsHeader
        public Dictionary<string, bool> DependencyProjects2 { get; set; } // Can't Remember what this was for?

        /// <summary>
        /// Lines above the ProjectName Line, these are always comments
        /// </summary>
        public List<string> PreLines { get; set; }

        /// <summary>
        /// List of Project Lines
        /// </summary>
        public List<string> ProjectLines { get; set; }

        /// <summary>
        /// Bash script lines for this make file project
        /// </summary>
        public List<string> PostLines { get; set; }

        /// <summary>
        /// This Project is the Header section of the make file
        /// </summary>
        public bool IsHeader { get; set; }

        [JsonIgnore]
        public IList<string> RawLines { get; set; }

        /// <summary>
        /// Used to store a lookup reference if this project publishes c++ header files
        /// </summary>
        public List<string> PublishCppHeaderFiles { get; set; }

        public List<string> FormatMakeProject(bool keepFormatting, int lineLength, bool sortDependenies)
        {
            if (sortDependenies)
            {
                // Header Dependencies First then Project Dependencies 
                DependencyProjects.Sort();
            }

             var projectLine = $"{ProjectName}: {string.Join(" ", DependencyProjects)}";
            var projectLines = new List<string>();
            if (!string.IsNullOrEmpty(ProjectName))
            {
                if (IsHeader)
                {
                    projectLines = MakeFileHelper.ExpandLines(projectLine, lineLength);
                }
                else
                {
                    projectLines.Add(projectLine);
                    //projectLines = MakeFileHelper.ExpandLines(projectLine, lineLength+40);
                }
            }

            PreLines.RemoveAll(string.IsNullOrWhiteSpace);
            // Going to Allow Empty Lines, Expect at the end
            while (PostLines.Count > 0 && string.IsNullOrWhiteSpace(PostLines.Last()))
            {
                PostLines.RemoveAt(PostLines.Count - 1);
            }

            var project = new List<string>();
            project.AddRange(PreLines);
            project.AddRange(projectLines);
            project.AddRange(PostLines);
            return project;
        }

        internal List<string> GetPublishedCppHeaderFiles(List<string> postLines)
        {
            //Sample
            //scripts/cpy $(as_lib_path) /$@/Header.gsoap $(GSOAP_IMPORT)
            //scripts/cpy $(as_lib_path) /$@/TypesImpl.h $(AS_INC)

            //I know, crappy code, just hope it works enough. 
            var list = new List<string>();
            foreach (var line in postLines)
            {
                var s = line.Split(' ');
                foreach (var s1 in s)
                {
                    var s2 = s1.Split('/');
                    foreach (var s3 in s2)
                    {
                        if (s3.ToLower().Contains(".h"))
                        {
                            list.Add(s3.Trim());
                        }
                    }
                }
            }
            return list;
        }
    }
}