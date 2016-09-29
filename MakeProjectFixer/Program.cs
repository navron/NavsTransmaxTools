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

            Parser.Default.ParseArguments<MakeFormat, MakeScanErrors, MakeDependencyCheck,
                                          MakeDependencyAllocator>(args)
                .WithParsed<MakeFormat>(RunMakeFormater)
                .WithParsed<MakeScanErrors>(RunMakeScanErrors)
                .WithParsed<MakeDependencyCheck>(RunMakeDependencyCheck)
                .WithParsed<MakeDependencyAllocator>(options => options.Run())
                .WithNotParsed(CommandLineNotParsed);
        }

        static void RunMakeFormater(MakeOptions options)
        {
            var files = Helper.FindFiles(options);
            Parallel.ForEach(files, (file) =>
            {
                if (options.Verbose) Console.WriteLine($"Formatting {file}");
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);
                make.WriteFile(options.LineLength, options.SortProject);
            });
        }
        static void RunMakeDependencyCheck(MakeDependencyCheck options)
        {

        }
        static void RunMakeScanErrors(MakeScanErrors options)
        {
            var files = Helper.FindFiles(options);
            Parallel.ForEach(files, (file) =>
            {
                if (options.Verbose) Console.WriteLine($"Scanning {file}");
                var make = new MakeFile.MakeFile();
                make.ReadFile(file);
                var found = make.ScanForErrors(options.FixErrors);
                if (found && options.FixErrors)
                {
                    make.WriteFile(options.LineLength, options.SortProject);
                }
            });
        }

        static void CommandLineNotParsed(IEnumerable<Error> errs)
        {

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
