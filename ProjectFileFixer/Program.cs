using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;

namespace ProjectFileFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (args.Length < 1) throw new Exception("No Stage to do");

            new Program().Execute(args[0]);
        }

        private void Execute(string stage)
        {
            string[] sourceSearchPatterns = {"*.csproj"};
            const string sourceCheckRootFolder = @"C:\Dev";

            var sourceFileList = new List<string>();

            // Stage Rules
            if (stage.Contains("2")) stage = stage + "1";

            // Run Stages
            if (stage.Contains("1"))
            {
                sourceFileList = Stage1FindProjectFiles(sourceCheckRootFolder, sourceSearchPatterns);
            }
            if (stage.Contains("2"))
            {
                Stage2ProjectRemoveMetaData(sourceFileList);
            }
        }

       
        List<string> Stage1FindProjectFiles(string rootFolder, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                .SelectMany(searchPattern => Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
            return files.ToList();
        }

        void Stage2ProjectRemoveMetaData(List<string> sourceFileList)
        {
            // Process each file in parallel
            Parallel.ForEach(sourceFileList, Stage2aProjectRemoveMetaData);
        }

        void Stage2aProjectRemoveMetaData(string fileName)
        {
            var project = new Project(fileName);
 
            var references = project.GetItems("Reference");
            foreach (ProjectItem reference in references)
            {
                var include = reference.EvaluatedInclude;
                var temp = include.Split(',');

                if (reference.HasMetadata(@"RequiredTargetFramework"))
                    reference.RemoveMetadata(@"RequiredTargetFramework");

                // Remove All SpecificVersion and HintPath
                if (reference.HasMetadata(@"SpecificVersion"))
                    reference.RemoveMetadata(@"SpecificVersion");

                if (reference.HasMetadata(@"HintPath"))
                    reference.RemoveMetadata(@"HintPath");

                if (temp[0].Contains(@"Tsd."))
                {
                   // May do something if an TSD project
                }
            }
            project.Save();
        }

        void Stage3SetVersion()
        {
            // Set the Correct version of different DLL

            //Log for net version is ..
            //Castle version is 
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Application exiting with error:{exception?.Message ?? "UnknownReason"}");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(-1);
        }
    }
}
