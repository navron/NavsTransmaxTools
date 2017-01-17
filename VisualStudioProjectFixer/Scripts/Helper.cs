using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace VisualStudioProjectFixer.Scripts
{
    /// <summary>
    /// Helper class for common tasks
    /// </summary>
    static class Helper
    {
        /// <summary>
        /// Get all files matching the search pattern 
        /// </summary>
        internal static List<string> GetProjectFiles(string rootFolder, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                .SelectMany(
                    searchPattern => Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
            return files.ToList();
        }

        internal static bool CheckCSharpeFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Log.Error($"File does not exist {fileName}");
                return false;
            }
            if (fileName.ToLower().Contains(@"\test\"))
            {
                Log.Information($"Skipping Test file: {fileName}");
                return false;
            }
            //if (fileName.ToLower().Contains(@".ait."))
            //{
            //    Log.Information($"Skipping AIT file: {fileName}");
            //    return false;
            //}
            if (fileName.ToLower().Contains(@"unittests\"))
            {
                // Stupid code that is still in source control and not used
                if (!Path.GetDirectoryName(fileName).EndsWith("UnitTests")) return false;
            }
            // Projects that are not built in the system and that need work to compile
            var projectFilesNotBuilt = new[]
            {
                "RoadSegmentConverter", "IisSetupIntegrationTests", "DrawingTestHarness", "TGPWebService",
                "WebService.IntegrationTests"
            };
            if (projectFilesNotBuilt.Any(fileName.Contains)) return false;

            return true;
        }
    }
}