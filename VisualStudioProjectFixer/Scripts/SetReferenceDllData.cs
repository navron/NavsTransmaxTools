using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Evaluation;
using Serilog;
using VisualStudioProjectFixer.Store;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("SetReferenceDLLData", HelpText = "")]
    public class SetReferenceDllData
    {
        [Option('d', "dir", HelpText = "Source Root Folder")]
        public string rootFolder { get; set; }

        [Option('f', "file", HelpText = "CS Project File")]
        public string FileName { get; set; }

        [Option('a', "all", HelpText = "Check all references")]
        public bool CheckAllReferences { get; set; }

        [Option('p', "parallel", HelpText = "Run in parallel mode")]
        public bool RunAsParallel { get; set; }

        public void Run()
        {
            if (string.IsNullOrEmpty(FileName) && string.IsNullOrEmpty(rootFolder))
            {
                Log.Error("Need either a file Name or Folder ");
                Environment.Exit(-1);
            }
            var files = new List<string>();
            if (!string.IsNullOrEmpty(FileName))
            {
                files.Add(FileName);
            }
            if (!string.IsNullOrEmpty(rootFolder))
            {
                files = Helper.GetProjectFiles(rootFolder, new[] { @"*.csproj" });
            }

            if (RunAsParallel)
                Parallel.ForEach(files, file => ProcessFile(file, CheckAllReferences));
            else
                files.ForEach(file => ProcessFile(file, CheckAllReferences));
        }

        private void ProcessFile(string fileName, bool checkAllReferences)
        {
            if (!File.Exists(fileName))
            {
                Log.Error($"File does not exist {fileName}");
                return;
            }
            if (fileName.ToLower().Contains(@"\test\"))
            {
                Log.Information($"Skipping Test file: {fileName}");
                return;
            }

            Log.Information($"Processing: {fileName}");

            var project = new Project(fileName);
            var references = project.GetItems("Reference");
            foreach (var reference in references)
            {
                var include = reference.EvaluatedInclude;
                var values = include.Split(',');
                if (values.Length == 1 || checkAllReferences)
                {
                    var dllName = values[0];
                    if (!UpdateThisDll(dllName))
                    {
                        Log.Debug($"SetReferenceDLLData Skipping {dllName}");
                        continue;
                    }
                    Log.Debug($"SetReferenceDLLData Fixing {dllName}");
                    var r = DllInformation.Instance.GetDllInfo(dllName);
                    if (!string.IsNullOrEmpty(r))
                    {
                        reference.UnevaluatedInclude = r;
                    }
                }
                else if (values.Length > 1)
                {
                    Log.Debug($"SetReferenceDLLData Not Touching {include}");
                }
            }

            if (!project.IsDirty) return;
            Log.Information($"Project Updated: {fileName}");
            project.Save();
        }

        bool UpdateThisDll(string dllName)
        {
            var dontChangeSystem = new[] { "System","WindowsBase", "WindowsFormsIntegration",
                                            "PresentationCore", "PresentationFramework", "PresentationUI" };
            if (dontChangeSystem.Contains(dllName)) return false;

            var dontChangeTsdTools = new[] { "DBManager", "SchemaManager", "ErrorUI2", "DBSync2",
                                              "DBCollections", "intersectionselector", "intreports", "TsdCop",
                                                "dbobj", "copydb"};
            if (dontChangeTsdTools.Contains(dllName)) return false;

            var dontChangeIngoreForNow = new[] { "DBCollections", "intersectionselector", "intreports",
                                                "Accessibility", "rebootfpclnt" , "rmclearfaultsclnt" };
            if (dontChangeIngoreForNow.Contains(dllName)) return false;

            var dontChangeNotInCurrentBuild = new[] { "MapInfo.CoreEngine", "MapInfo.CoreTypes", "MapInfo.Windows" };
            if (dontChangeNotInCurrentBuild.Contains(dllName)) return false;
            if (dllName.Contains("MapInfo.")) return false;

            var dontChangeOldTsdFiles = new[] { "Tsd.ResponsePlan.ApplicationServer.RPUpdater_1To2" ,
                                                "Tsd.Intersection.Workstation.ICAClient",
                                                "ExcelConnectionStringManager","ReportingServicesLoader",
                                                 "Tsd.EventReport.Workstation.esr","Tsd.VehicleDetector.Workstation.FMSDReportClient",
            "Tsd.Core.Workstation.FileUploader", "Tsd.TGP.ApplicationServer.TGPWebService", "Tsd.RampManagement.Workstation.RSMTrendReport",
            "Tsd.Presentation.Workstation.SXMain", "Tsd.PackageManagement.Workstation.PlatformFPPackagesRpt",
            "Tsd.StrategyManagement.Workstation.StimulusRankings", "Tsd.Intersection.ApplicationServer.IOGDDAL",
            "GlobalResourceIndexer", "Tsd.Movement.Workstation.MPMReport", "Tsd.Presentation.Workstation.sxmaptypes",
            "Tsd.Libraries.Workstation.LineUtil"};
            if (dontChangeOldTsdFiles.Contains(dllName)) return false;



            // These are incorrect, should be v3 on the end, there in test tools
            // var dontChangeJanus = new[] {  "Janus.Windows.Common", "Janus.Windows.GridEX" };
            //    if (dontChangeJanus.Contains(dllName)) return false;


            // .Net Framework
            var dontChangeDotNetFramework = new[] { "UIAutomationProvider", "UIAutomationTypes", "UIAutomationClient",
                                                    "Microsoft.CSharp", "Microsoft.VisualC","Microsoft.VisualBasic" ,"Microsoft.Build",
                                                    "CustomMarshalers", "ReachFramework",
                                                    "Microsoft.Build.Utilities.v4.0",   "Microsoft.SqlServer.Management.Sdk.Sfc",
                                                    "stdole",
                                                    "Microsoft.SqlServer.ConnectionInfo"};
            if (dontChangeDotNetFramework.Contains(dllName)) return false;

            if (dllName.Contains("Microsoft.")) return false; 

            if (dllName.Contains("System.")) return false;
            if (dllName.Contains("DVTel.")) return false;

            // TODO REMOVE only Temporary
            if (dllName.Contains("DevExpress.") && dllName.Contains("v15")) return false;
            
            // TODO REMOVE
            var dontChangeJanus = new[] { "Janus.Windows.Common", "Janus.Windows.GridEX" , "wsutil" };
            if (dontChangeJanus.Contains(dllName)) return false;


            return true;
        }
    }
}