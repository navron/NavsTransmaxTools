﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MakeProjectFixer
{
    internal static class Helper
    {
        internal static List<string> FindFiles(Options options)
        {
            // If file is specifier then return a list with just that file in it
            if (!string.IsNullOrEmpty(options.SingleFile))
            {
                Console.WriteLine($"Processing Single file:{options.SingleFile}");
                return new List<string> { options.SingleFile };
            }

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
                }
                if(!found)
                    limitedFiles.Add(file);
            }

            Console.WriteLine($"Total Files: {files.Count} Limited: {limitedFiles.Count}");
            if (options.Verbose) limitedFiles.ForEach(Console.WriteLine);

            return limitedFiles;
        }
    }

    // from http://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/

    /// <summary>
    /// Functions for performing common Json Serialization operations.
    /// <para>Requires the Newtonsoft.Json assembly (Json.Net package in NuGet Gallery) to be referenced in your project.</para>
    /// <para>Only public properties and variables will be serialized.</para>
    /// <para>Use the [JsonIgnore] attribute to ignore specific public properties or variables.</para>
    /// <para>Object to be serialized must have a parameterless constructor.</para>
    /// </summary>
    public static class JsonSerialization
    {
        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite, Formatting.Indented);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                writer?.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                reader?.Close();
            }
        }
    }
}
