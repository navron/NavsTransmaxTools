using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MakeFileProjectFixer.MakeFile
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
            PreDefinedIncludeDependency = new List<string>();
            PreDefinedExcludeDependency = new List<string>();

        }

        /// <summary>
        /// Project ProjectName
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Project Area, service/[common, db, as, ws]
        /// </summary>
        public string ProjectArea { get; set; }

        /// <summary>
        /// List of Current Projects marked as Dependencies
        /// </summary>
        public List<string> DependencyProjects { get; set; }

        /// <summary>
        /// list of Dependencies from the PreLines with Tag [include]
        /// </summary>
        public List<string> PreDefinedIncludeDependency { get; set; }
        public List<string> PreDefinedExcludeDependency { get; set; }

        /// <summary>
        /// Lines above the ProjectName Line, these are always comments
        /// </summary>
        public List<string> PreLines { get; set; }

        /// <summary>
        /// List of Project Lines (This is really a single command line with \ a the line ending)
        /// </summary>
        public List<string> ProjectLines { get; set; }

        /// <summary>
        /// Bash script lines for this make file project
        /// </summary>
        public List<string> PostLines { get; set; }

        /// <summary>
        /// This Project is the Header section of the make file, Special rules apply
        /// </summary>
        public bool IsHeader { get; set; }

        // [JsonIgnore]
        //public MakeFile MakeFile { get; set; }

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

            var projectLine = $"{ProjectName}: {string.Join(" ", AllDependenies)}";
            var projectLines = new List<string>();
            if (!string.IsNullOrEmpty(ProjectName))
            {
                if (IsHeader)
                {
                    projectLines = MakeFileHelper.ExpandLines(projectLine, lineLength);
                }
                else
                {
                    //projectLines.Add(projectLine);
                    projectLines = MakeFileHelper.ExpandLines(projectLine, lineLength + 40);
                }
            }

            PreLines.RemoveAll(string.IsNullOrWhiteSpace);

            // Going to Allow Empty Lines, Expect at the end
            while (PostLines.Count > 0 && string.IsNullOrWhiteSpace(PostLines.Last()))
            {
                PostLines.RemoveAt(PostLines.Count - 1);
            }

            var project = new List<string>();
            project.AddRange(CleanLines(PreLines));
            project.AddRange(CleanLines(projectLines));
            project.AddRange(CleanLines(PostLines));
            return project;
        }

        private List<string> CleanLines(List<string> lines)
        {
            return lines.Select(line => line.TrimEnd()).ToList();
        }

        public void ProcessFile()
        {
            PublishCppHeaderFiles = GetPublishedCppHeaderFiles(PostLines);
            PreDefinedIncludeDependency = GetPreDefinedDependencies("[include]", (PreLines));
            PreDefinedExcludeDependency = GetPreDefinedDependencies("[exclude]", PreLines);
        }

        internal List<string> GetPublishedCppHeaderFiles(List<string> postLines)
        {
            //Sample
            //scripts/cpy $(as_lib_path) /$@/Header.gsoap $(GSOAP_IMPORT)
            //scripts/cpy $(as_lib_path) /$@/TypesImpl.h $(AS_INC)

            // but not
            // scripts/generate $(as_fr_path)/$@ tem.VicWeatherAlgService.h

            //I know, crappy code, just hope it works enough. 
            var list = new HashSet<string>();
            foreach (var line in postLines)
            {
                if(!line.Contains("scripts/cpy")) continue;
                if(line.Contains(".html")) continue;

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
            return list.ToList();
        }

        private List<string> GetPreDefinedDependencies(string tag, List<string> preLines)
        {
            var list = new List<string>();
            foreach (var line in preLines)
            {
                var index = line.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
                if (index < 0) continue;
                var values = line.Remove(index, tag.Length);
                values = values.Remove(0, 1); // remove the comment at the start of the line
                var split = values.Split(' ');
                list.AddRange(MakeFileHelper.CleanItems(split));
            }
            return list;
        }

        /// <summary>
        /// Should this project be excluded from either the Make File header or director Folder
        /// </summary>
        public bool ShouldExcluded => PreLines.Any(preLine => preLine.Contains("[excludeheader]", StringComparison.OrdinalIgnoreCase));

        // Join Both Dependencies lists and return the union set
        private List<string> AllDependenies => PreDefinedIncludeDependency.Union(DependencyProjects).Except(PreDefinedExcludeDependency).ToList();


        // bad quick coding, to fix up
        public bool IncludeUnitTestReferences
        {
             get
             {
                 foreach (var line in PostLines)
                 {                 
                     if (line.Contains("/blddll") || line.Contains("/bldlib"))
                     {
                         line.Trim();
                         if (line.EndsWith("1"))
                         {
                             return false;
                         }
                         return true;
                     }
                 }

                 return true;
             }
        }
    }
}