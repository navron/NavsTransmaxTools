using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;

namespace MakeProjectFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Parser.Default.ParseArguments < MakeFileFormat,
                                            MakeDependencyAllocator,
                                            MakeScanErrors
                                           >(args)
                .WithParsed<MakeFileFormat>(options => options.Run())
                .WithParsed<MakeScanErrors>(options => options.Run())
                .WithParsed<MakeDependencyAllocator>(options => options.Run())
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
