using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectFixer
{
    internal static class Helper
    {
        internal static List<string> FindFiles(Options options)
        {
            // If file is specifier then return a list with just that file in it
            if (!string.IsNullOrEmpty(options.File))
            {
                Console.WriteLine($"Processing Single file:{options.File}");
                return new List<string> { options.File };
            }

            // Scan the Search Patterns in Parallel for all files matching the required
            var files = options.SearchPatterns.AsParallel()
                .SelectMany(searchPattern => Directory.EnumerateFiles(options.Folder, searchPattern, SearchOption.AllDirectories))
                .ToList();

            Console.WriteLine($"Processing Multiple Files: {files.Count}");
            if (options.Verbose) files.ForEach(Console.WriteLine);

            return files;
        }
    }
}
