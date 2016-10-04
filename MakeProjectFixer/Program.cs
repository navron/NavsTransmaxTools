using System;
using System.Collections.Generic;
using ColorConsole;
using CommandLine;
using MakeProjectFixer.Checkers;

/**********************************************************************************
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
 *      Scan C# project files for TSD References
 *      TODO Scan C++ projects for #include files
 *      Check Case of TSD Reference to Reference Project Name (C#)
 * 
 * 
 *  Make File Dependency Check
 *   TODO   Check Visual Studio (C#) TSD References are in Make Project Dependency list
 *   TODO   Check Visual Studio (CPP) TSD Include files are in Make Project Dependency list
 *      Circular Dependency Check
 *   TODO   Reduce Dependency List
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
        public static readonly ConsoleWriter Console = new ConsoleWriter();


        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Parser.Default.ParseArguments<MakeFileFormatter,
                                            MakeFileScanForErrors,
                                            MkDependAllocation,
                                            MatchMakeFileAndVisualStudioProjectCase,
                                            MatchMakeProjectDependencyCaseToMakeProjectName,
                                            MkCircularDependCheck,
                                            VsFixCaseReference,
                                            MkReduceDepends
                                           >(args)
                .WithParsed<MakeFileFormatter>(options => options.Run())
                .WithParsed<MakeFileScanForErrors>(options => options.Run())
                .WithParsed<MkDependAllocation>(options => options.Run())
                .WithParsed<MatchMakeFileAndVisualStudioProjectCase>(options => options.Run())
                .WithParsed<MatchMakeProjectDependencyCaseToMakeProjectName>(options => options.Run())
                .WithParsed<MkCircularDependCheck>(options => options.Run())
                .WithParsed<VsFixCaseReference>(options => options.Run())
                .WithParsed<MkReduceDepends>(options => options.Run())

                .WithNotParsed(CommandLineNotParsed);
        }

        static void CommandLineNotParsed(IEnumerable<Error> errs)
        {
            System.Console.WriteLine("CommandLineNotParsed");
        }

        /// <summary>
        /// Handles any exception that was done thrown and wasn't catch downstream. 
        /// </summary>
        /// <param name="sender">The object that sent the exception</param>
        /// <param name="e">The unhanded exception. </param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var foregroundColor = System.Console.ForegroundColor;
            var exception = e.ExceptionObject as Exception;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Application exiting with error: {exception?.Message ?? "UnknownReason"}");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(exception?.StackTrace);
            System.Console.ForegroundColor = foregroundColor;
            Environment.Exit(-1);
        }
    }
}
