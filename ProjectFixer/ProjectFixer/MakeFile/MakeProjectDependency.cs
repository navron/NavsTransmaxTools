using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProjectFixer.MakeFile
{
    class MakeProjectDependency
    {
        public int Level;
        public bool Checking = false;
        public bool Checked = false;

        [JsonIgnore]
        public readonly List<MakeProjectDependency> DependencyMakeProject = new List<MakeProjectDependency>();
        [JsonIgnore]
        public readonly MakeProject MakeProject;

        public readonly string ProjectName;

        public MakeProjectDependency(MakeProject makeProject)
        {
            MakeProject = makeProject;
            Checking = false;
            Checked = false;
            Level = 0;
            ProjectName = makeProject.ProjectName;
        }
    }
}