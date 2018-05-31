using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProjectFixer.Utility;
using ProjectFixer.VisualStudioFile;
using Serilog;

namespace ProjectFixer.Data
{
    internal class StoreCsFiles
    {
        private const string StoreFile = "StoreCSharpFile.json";
        public List<VisualStudioCSharpFile> Files { get; private set; }

        public void Build(Options options)
        {
            using (new LoggingTimer("Building Store C# Files"))
            {
                BuildStore(options);
            }
        }

        private void BuildStore(Options options)
        {
            Files = GetPreBuild(options) ?? BuildFileInfo(options);
        }

        private List<VisualStudioCSharpFile> GetPreBuild(Options options)
        {
            Log.Debug($"Running {MethodBase.GetCurrentMethod().Name}");
            if (!Helper.PreProcessedObject(StoreFile, options)) return null;

            var files = JsonSerialization.ReadFromJsonFile<List<VisualStudioCSharpFile>>(StoreFile);
            CheckFiles(new List<IVisualStudioFile>(files));
            return files;
        }

        private List<VisualStudioCSharpFile> BuildFileInfo(Options options)
        {
            // Step 2 Read all Visual Studio Files 
            options.SearchPatterns = new[] { "*.csproj" };
            // Find and limit return set to what is required
            var files = Helper.FindFiles(options).Where(VisualStudioFileHelper.IncludeFile).ToList();

            var vsFiles = new List<VisualStudioCSharpFile>();

            Parallel.ForEach(files, options.ParallelOption, (file) => vsFiles.Add(new VisualStudioCSharpFile(file)));

            // Scan Visual Studio Files For Internal References to TSD and Other Reference DLL
            Parallel.ForEach(vsFiles, options.ParallelOption, (vsFile) => vsFile.ScanFileForReferences());

            Helper.PreProcessedFileSave(StoreFile, vsFiles);

            return vsFiles;
        }

        public void CheckFiles(List<IVisualStudioFile> vsFiles)
        {
            if (vsFiles.Any(vsFile => vsFile == null))
            {
                throw new Exception("VSFile null, try deleting json files Aborting");
            }
        }
    }
}
