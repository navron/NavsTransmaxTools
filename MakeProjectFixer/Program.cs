using System;
using System.Collections.Generic;
using ColorConsole;
using CommandLine;

/**********************************************************************************
 * 
 * Tasks:
 *  Format Make Files:
 *      Line Length
 *      Sort Order
 *      Match Case in Header to Project
 *      Match Case from Visual Studio Project File to Make File
 *      Make Header has all Make Projects in the file in the Header
 *      Make Directory File has all make files in the folder
 *  
 *  Format Visual Studio Files
 *      Check Case of TSD Reference to Reference Project Name (C#)
 * 
 *  Make File Dependency Check
 *      Check Visual Studio (C#) TSD References are in Make Project Dependency list
 *      Check Visual Studio (CPP) TSD Include files are in Make Project Dependency list
 *      Circular Dependency Check
 *      Reduce Dependency List
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * *********************************************************************************/


namespace MakeProjectFixer
{
    class Program
    {
        
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Parser.Default.ParseArguments<MakeFileFormatter,
                                            MakeFileScanForErrors,
                                            MakeFileDependencyChecker,
                                            MatchMakeFileAndVisualStudioProjectCase,
                                            MatchMakeProjectDependencyCaseToMakeProjectName,
                                            MakeFileCircularDependencyCheck
                                           >(args)
                .WithParsed<MakeFileFormatter>(options => options.Run())
                .WithParsed<MakeFileScanForErrors>(options => options.Run())
                .WithParsed<MakeFileDependencyChecker>(options => options.Run())
                .WithParsed<MatchMakeFileAndVisualStudioProjectCase>(options => options.Run())
                .WithParsed<MatchMakeProjectDependencyCaseToMakeProjectName>(options => options.Run())
                .WithParsed<MakeFileCircularDependencyCheck>(options => options.Run())
                .WithNotParsed(CommandLineNotParsed);
        }

        static void CommandLineNotParsed(IEnumerable<Error> errs)
        {
            Console.WriteLine("CommandLineNotParsed");
        }

        /// <summary>
        /// Handles any exception that was done thrown and wasn't catch downstream. 
        /// </summary>
        /// <param name="sender">The object that sent the exception</param>
        /// <param name="e">The unhanded exception. </param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var foregroundColor = Console.ForegroundColor;
            var exception = e.ExceptionObject as Exception;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Application exiting with error: {exception?.Message ?? "UnknownReason"}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(exception?.StackTrace);
            Console.ForegroundColor = foregroundColor;
            Environment.Exit(-1);
        }
    }
}
