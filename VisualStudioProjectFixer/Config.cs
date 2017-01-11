using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudioProjectFixer
{
    public static class Config
    {
        public class ReferenceRules
        {
            public string Name;
            public bool RemoveAllMetaData; // i.e Version, Culture, PublicKeyToken, and processorArchitecture"
        }

        public static string[] GetSourceSearchPatterns
        {
            get { return new string[] {"*.csproj", "*.config", "*.Config"}; }
        }

        public static List<ReferenceRules> GetReferenceRules => new List<ReferenceRules>
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