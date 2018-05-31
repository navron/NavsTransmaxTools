using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine;
using ProjectFixer.Data;
using ProjectFixer.Scripts;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ProjectFixer
{

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



    class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            // Log to Console, The application is run in a batch file that will pipe the contents to an log file
            var levelSwitch = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Information };
            Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch) //or Information 
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            // Get all classes with command line Verb Attribute
            var verbTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
                             let testAttribute = Attribute.GetCustomAttribute(type, typeof(VerbAttribute))
                             where testAttribute != null
                             select type);
            // Parser the command line
            var map = Parser.Default.ParseArguments(args, verbTypes.ToArray());
            // Change Log level if needed
            map.WithParsed<Options>(options => { if (options.Verbose) levelSwitch.MinimumLevel = LogEventLevel.Verbose; });

            map.WithParsed<StoreTest>(options => options.Run())
                .WithParsed<FormatMakeFiles>(options => options.Run())
                .WithParsed<FixDependencies>(options => options.Run())
                .WithParsed<FixMakeFileCaseFromVisualStudio>(options => options.Run())
                .WithParsed<MatchMfProjectDependencyCaseToMfProject>(options => options.Run())
                .WithParsed<CircularDependeyCheck>(options => options.Run())
                .WithParsed<ReduceDependency>(options => options.Run())
                
                .WithParsed<ListMissingMakeProjects>(options => options.Run())
                .WithParsed<FixMakeFileHeader>(options => options.Run())
                .WithParsed<RemovedMakeProject>(options => options.Run())
                .WithParsed<MakeFileChangeCase>(options => options.Run())
                .WithParsed<ListCaseProblems>(options => options.Run())

                .WithNotParsed(HelpFooter);
        }

        static void HelpFooter(IEnumerable<Error> errs)
        {
            Console.WriteLine("Make File Fixer");
            Console.WriteLine("This program is design to fix different common problems that Transmax has with its build system");
        }

        /// <summary>
        /// Handles any exception that was done thrown and wasn't catch downstream. 
        /// </summary>
        /// <param name="sender">The object that sent the exception</param>
        /// <param name="e">The unhanded exception.</param>
        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Error($"Application exiting with error: {exception?.Message ?? "UnknownReason"}");
            Log.Error(exception?.StackTrace);
            Environment.Exit(-1);
        }
    }
}
