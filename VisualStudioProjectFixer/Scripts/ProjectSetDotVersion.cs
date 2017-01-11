using CommandLine;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("SetDotNetVersion", HelpText = "Set VS project .Net Version to .")]
    class ProjectSetDotVersion
    {
        [Option('f', "file", HelpText = "CS Project File")]
        public string FileName { get; set; }

        [Option("DotVersion", HelpText = ".Net Version number")]
        public string DotVersion { get; set; }

        [Option('d', "dir", HelpText = "Source Root Folder")]
        public string SourceCheckRootFolder { get; set; }

        public void Run()
        {
            //  foreach (var fileName in sourceFileList)
            {
                //  Stage6aProjectSetDotVersion(fileName, version);  
                //TODO
                // Set the Correct version of different DLL


                //var project = new Project(fileName);

                //var references = project.GetItems("Reference");
                //foreach (ProjectItem reference in references)
                //{
                //    foreach (var rule in rules)
                //    {
                //        // Need to handle name that include other names etc
                //        // Castle.ActiveRecord and Castle
                //        // NHibernate NHibernate.ByteCode.Castle

                //        // Does 
                //        if (rule.RemoveAllMetaData && reference.UnevaluatedInclude.Contains(rule.Name + ","))
                //        {
                //            reference.UnevaluatedInclude = rule.Name;
                //        }
                //    }
                //}
                //if (project.IsDirty) Console.WriteLine($"Changed: {fileName}");
                //project.Save();
            }
        }
    }
}