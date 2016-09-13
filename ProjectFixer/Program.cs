using System;
using CommandLine;

namespace ProjectFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            // (1) default singleton
            var result = Parser.Default.ParseArguments<BaseOptions>(args);

         //   result.Tag == ParserResultType.Parsed


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
