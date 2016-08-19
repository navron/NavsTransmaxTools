using System;
using System.IO;
using System.Linq;

namespace BadExample
{
    /// <summary>
    /// Read a grade file in and sort the contents
    /// </summary>
    /// <remarks>language C#6</remarks>
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // if (!args.Any()) return;
            new Program().Execute(args[0]);
        }

        /// <summary>
        /// The execution function that handles the flow of the program. 
        /// </summary>
        /// <param name="fileName">The sample file name. </param>
        public void Execute(string fileName)
        {
            // Read the file contents creating an grade Tuple of <lastname, firstname, grade>
            var grades = File.ReadLines(fileName).Select(line => line.Split(','))
                .Select(t => new Tuple<string, string, int>(t[0], t[1], int.Parse(t[2])));

            // Sort by Grade (descending, then last/first ascending)
            // Requires double the memory usage for copying results, modem computer system can handle whole schools grades in this format
            // Sort in parallel for performance
            // Names should always be compared using case sensitive means 
            grades = grades.AsParallel().OrderByDescending(tuple => tuple.Item3)
                .ThenBy(tuple => tuple.Item1, StringComparer.InvariantCultureIgnoreCase)
                .ThenBy(tuple => tuple.Item2, StringComparer.InvariantCultureIgnoreCase);

            // Write the sorted contents
            var outputFileName = Path.Combine(Path.GetDirectoryName(fileName),$"{Path.GetFileNameWithoutExtension(fileName)}-graded.txt");
            File.WriteAllLines(outputFileName, grades.Select(t => $"{t.Item1},{t.Item2},{t.Item3}"));
        }

        /// <summary>
        /// Handles any exception that was done thrown and wasn't catch downstream. 
        /// </summary>
        /// <param name="sender">The object that sent the exception</param>
        /// <param name="e">The unhandled exception. </param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Application exiting with error:", exception?.Message ?? "UnknownReason");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(-1);
        }
    }
}
