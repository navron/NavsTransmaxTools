using System;
using CommandLine;
using Microsoft.Build.Evaluation;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("RemoveVersionNumbers", HelpText = "Remove meta data from references")]
    public class RemoveVersionNumbers
    {
        [Option('f', "file", HelpText = "CS Project File")]
        public string FileName { get; set; }

        public void Run()
        {
            // Set the Correct version of different DLL
            if (!FileName.Contains("csproj")) return;
            var project = new Project(FileName);

            var references = project.GetItems("Reference");
            foreach (ProjectItem reference in references)
            {
                foreach (var rule in Config.GetReferenceRules)
                {
                    // Need to handle name that include other names etc
                    // Castle.ActiveRecord and Castle
                    // NHibernate NHibernate.ByteCode.Castle

                    // Does 
                    if (rule.RemoveAllMetaData && reference.UnevaluatedInclude.Contains(rule.Name + ","))
                    {
                        reference.UnevaluatedInclude = rule.Name;
                    }
                }
            }
            if (project.IsDirty) Console.WriteLine($"Changed: {FileName}");
            project.Save();
        }
    }
}