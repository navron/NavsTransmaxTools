using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MakeProjectFixer.MakeFile
{
    public class MakeProject
    {
        public MakeProject()
        {
            RawLines = new List<string>();
            PreLines = new List<string>();
            PostLines = new List<string>();
            DependencyProjects = new List<string>();
            PublishCppHeaderFiles = new List<string>();
        }

        /// <summary>
        /// Project ProjectName
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// List of Current Projects marked are Dependencies
        /// </summary>
        public List<string> DependencyProjects { get; set; }

        /// <summary>
        /// Lines above the ProjectName Line, these are always comments
        /// </summary>
        public List<string> PreLines { get; set; }

        /// <summary>
        /// Bash script lines for this make file project
        /// </summary>
        public List<string> PostLines { get; set; }

        [JsonIgnore]
        public IList<string> RawLines { get; set; }

        /// <summary>
        /// Used to store a lookup reference if this project publishes c++ header files
        /// </summary>
        public List<string> PublishCppHeaderFiles { get; set; }

        public List<string> FormatMakeProject(int lineLength, bool sortDependenies)
        {
            if (sortDependenies)
            {
                DependencyProjects.Sort();
            }

            var projectLine = $"{ProjectName}: {string.Join(" ", DependencyProjects)}";
            var projectLines = new List<string>();
            if (!string.IsNullOrEmpty(ProjectName))
            {
                projectLines = MakeFileHelper.ExpandLines(projectLine, lineLength);
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
    }
}