using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using Serilog;
using VisualStudioProjectFixer.Scripts;

namespace VisualStudioProjectFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Log to Console, The application is run in a batch file that will pipe the contents to an log file
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information() // Debug() or Information 
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Get all classes with command line Verb Attribute
            var verbTypes = (from type in Assembly.GetExecutingAssembly().GetTypes()
                             let testAttribute = Attribute.GetCustomAttribute(type, typeof(VerbAttribute))
                             where testAttribute != null
                             select type);
            // Parser the command line
            var map = Parser.Default.ParseArguments(args, verbTypes.ToArray());
            map.WithParsed<SetReferenceDllData>(action => action.Run())
                .WithParsed<RemoveMetaData>(action => action.Run())
                .WithParsed<MarkAsDirty>(action => action.Run())
                .WithParsed<RemoveDLL>(action => action.Run());
        }

        /// <summary>
        /// Handles any exception that was done thrown and wasn't catch downstream. 
        /// </summary>
        /// <param name="sender">The object that sent the exception</param>
        /// <param name="e">The unhanded exception. </param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Error($"Application exiting with error: {exception?.Message ?? "UnknownReason"}");
            Log.Error(exception?.StackTrace);
            Environment.Exit(-1);
        }
    }
}