﻿using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Microsoft.Build.Evaluation;
using ProjectFileFixer;

namespace VisualStudioProjectFixer.Scripts
{
    [Verb("RemoveDotNet35WebConfig", HelpText = "")]
    public class RemoveDotNet35WebConfig : Options
    {
        public void Run()
        {
            List<string> sourceFileList = Helper.GetProjectFiles(SourceCheckRootFolder, Config.GetSourceSearchPatterns);

            foreach (var filepath in sourceFileList)
            {
                if (filepath.Contains(".config") || filepath.Contains(".Config"))
                {
                    string text = File.ReadAllText(filepath);
                    text = text.Replace("3.5", "4.0");
                    File.WriteAllText(filepath, text);
                }
            }
        }

        private static void UpgradeProjectsToVS2015(string sourcepath)
        {
            RunPowershell.ExecutePowerShellCommand(sourcepath);
        }

        /// <summary>
        /// Set all executables to 64bit
        /// </summary>
        /// <param name="sourceFileList"></param>
        private static void RemovePerfer32Bit(List<string> sourceFileList)
        {
            foreach (var file in sourceFileList)
            {
                if (file.Contains(".csproj"))
                {
                    var project = new Project(file);

                    var outputtype = project.GetProperty("OutputType");

                    if (outputtype.EvaluatedValue.Contains("Exe"))
                    {
                        project.SetProperty("Prefer32Bit", "false");
                    }

                    if (project.IsDirty) Console.WriteLine($"Changed: {file}");

                    project.Save();
                }
            }
        }

        static void ProjectRemoveMetaData(List<string> sourceFileList)
        {
            // Process each file in parallel
            //Parallel.ForEach(sourceFileList, Stage2aProjectRemoveMetaData);

            foreach (var fileName in sourceFileList)
            {
                //    Stage2aProjectRemoveMetaData(fileName);
            }
        }
    }
}