using System;
using System.Collections.Generic;
using CommandLine;
using Microsoft.Build.Evaluation;

namespace VisualStudioProjectFixer.Scripts
{
    // Don't used  We want the Version Numbers
    [Verb("RemoveVersionNumbersDONTUSE", HelpText = "Remove meta data from references")]
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
                foreach (var rule in GetReferenceRules)
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

        private static List<ReferenceRules> GetReferenceRules => new List<ReferenceRules>
        {
            new ReferenceRules {Name = "log4net", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Castle.ActiveRecord", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Castle.Components.Validator", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Castle.Core", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Castle.DynamicProxy2", RemoveAllMetaData = true},
            new ReferenceRules
            {
                Name = "Castle.Facilities.ActiveRecordIntegration",
                RemoveAllMetaData = true
            },
            new ReferenceRules
            {
                Name = "Castle.Facilities.AutomaticTransactionManagement",
                RemoveAllMetaData = true
            },
            new ReferenceRules {Name = "Castle.MicroKernel", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Castle.Services.Transaction", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Castle.Windsor", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Rhino.Mocks", RemoveAllMetaData = true},
            new ReferenceRules {Name = "nunit.framework", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Iesi.Collections", RemoveAllMetaData = true},
            new ReferenceRules {Name = "Caliburn.Micro", RemoveAllMetaData = true},
            new ReferenceRules {Name = "PresentationUI", RemoveAllMetaData = true},
            new ReferenceRules {Name = "NHibernate", RemoveAllMetaData = true},
            new ReferenceRules {Name = "NHibernate.ByteCode.Castle", RemoveAllMetaData = true},
            new ReferenceRules {Name = "NVelocity", RemoveAllMetaData = true},
            new ReferenceRules {Name = "CabLib", RemoveAllMetaData = true},
            new ReferenceRules {Name = "aspdu.net", RemoveAllMetaData = true},
            new ReferenceRules {Name = "NConsole", RemoveAllMetaData = false},
            new ReferenceRules {Name = "System", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Configuration", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Core", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Data", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Data.DataSetExtensions", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Data.SqlServerCe", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Drawing", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.EnterpriseServices", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Management.Automation", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.ServiceModel", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Web", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Web.Mobile", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Web.Services", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Xml", RemoveAllMetaData = true},
            new ReferenceRules {Name = "System.Xml.Linq", RemoveAllMetaData = true},
        };

    }
}