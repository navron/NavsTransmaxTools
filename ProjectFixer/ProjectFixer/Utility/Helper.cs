using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;

namespace ProjectFixer.Utility
{
    internal static class Helper
    {
        internal static List<string> FindFiles(Options options)
        {
            // If file is specifier then return a list with just that file in it
            if (!string.IsNullOrEmpty(options.SingleFile))
            {
                Console.WriteLine($"Processing Single file:{options.SingleFile}");
                var ext = Path.GetExtension(options.SingleFile);
                return options.SearchPatterns.Any(p => p.Contains(ext, StringComparison.OrdinalIgnoreCase)) ? new List<string> { options.SingleFile } : new List<string>();
            }

            if (!Directory.Exists(options.Folder))
                throw new Exception($"Folder {options.Folder} does not exist -- aborting");

            if (options.SearchPatterns == null)
                throw new Exception($"Programming error, SearchPatterns is null -- aborting");

            // Scan the Search Patterns in Parallel for all files matching the required
            var files = options.SearchPatterns.AsParallel()
                .SelectMany(searchPattern => Directory.EnumerateFiles(options.Folder, searchPattern, SearchOption.AllDirectories))
                .ToList();

            var knownProblemsListToRemove = new List<string>();
            knownProblemsListToRemove.Add(@"DBErrorLogger\install.mak");

            var limitedFiles = new List<string>();
            foreach (var file in files)
            {
                var found = false;
                foreach (var badString in knownProblemsListToRemove)
                {
                    if (file.Contains(badString)) found = true;
                    if (file.Contains("/ait/")) found = true;
                }
                if (!found)
                    limitedFiles.Add(file);
            }

            Log.Information("Pattern: {Patterns} Total Files: {TotalFiles} Limited: {LimitedFiles}",
                                options.SearchPatterns, files.Count, limitedFiles.Count);

            return limitedFiles;
        }

        // Returns True if the Call can used the Previous built Object File
        public static bool PreProcessedObject(string fileName, Options options)
        {
            // If Reuse flag is false then return file, 
            // If the file does not exist then return false to force rebuild of file
            if (!options.ReuseTemporaryFiles || !File.Exists(fileName))
            {
                Log.Debug($"Rebuild Method:{fileName}", ConsoleColor.Yellow);
                return false;
            }

            // Otherwise Return true to load file
            Log.Debug($"Load Predefined Method:{fileName}", ConsoleColor.Green);
            return true;
        }

        public static void PreProcessedFileSave(string fileName, object saveObject)
        {
            // if PreProcessedFolder is empty then don't save
            if (string.IsNullOrEmpty(fileName)) return;
            if(File.Exists(fileName)) File.Delete(fileName);

            JsonSerialization.WriteToJsonFile(fileName, saveObject);
        }
    }
}
