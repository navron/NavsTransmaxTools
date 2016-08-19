using System;
using System.Diagnostics;
using DependencyChecker.Checkers;

namespace DependencyChecker
{
    /// <summary>
    /// DependencyChecker is an build tool program for checking common problems with mak files and csproj files
    /// </summary>    
    internal class Program
    {
        private static void Main(string[] args)
        {
            var listener = new MyConsoleTraceListener();
            Trace.Listeners.Add(listener);

            IOptions options = null;

            try
            {
                options = Options.ParseOptions(args);
            }
            catch
            {
                Trace.TraceError("Error parsing arguments");
                Console.WriteLine(Options.GetUsage()); //use the actual console to do this
                Environment.Exit(1);
            }

            try
            {
                // Ok this is a little crap way of using options, its because NConsole either doesn't do what I want or I don't know how to use it.
                if (options.Mode == "CheckMakeFiles" || options.Mode == "CheckCompliance")
                {
                    var makeFileChecker = new MakeFileChecker(options);
                    makeFileChecker.CheckMakeFilesForDependencies();
                }
                if (options.Mode == "CheckProjectFiles" || options.Mode == "CheckCompliance")
                {
                    var projectFileChecker = new ProjectFileChecker(options);
                    projectFileChecker.CheckProjectFilesForCircularDependencies();
                }
                if (options.Mode == "FormatMakeFiles")
                {
                    // TODO: 
                    throw new ArgumentException(" This feature has not been created yet");
                }
                if (options.Mode == "CheckCompliance")
                {
                    // TODO: Feature not done.
                    // Check that the make files are in the same format as the above (FormatMakeFiles) function would write it in
                }

                if (listener.ErrorDetected)
                {
                    Environment.Exit(1);
                }
            }
            catch (StopException stopException)
            {
                // Its ok there should of been a Trace Warning outputted, just exit
                Environment.Exit(1);
            }
            catch (Exception exception)
            {
                var message = string.Format("Exception.Message: {0}", exception.Message);
                if (exception.InnerException != null)
                {
                    message = message + Environment.NewLine + string.Format("InnerException.Message: {0}", exception.InnerException.Message);
                }
                message = message + Environment.NewLine + string.Format("Exception.StackTrace: {0}", exception.StackTrace);
                Trace.TraceError(message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// The checkers will throw this to stop the checking, Just print out the message and exit the program nicely 
        /// </summary>
        public class StopException : Exception
        {

        }

        private class MyConsoleTraceListener : ConsoleTraceListener
        {
            public bool ErrorDetected;

            public override void Write(string message)
            {
                //this section is just used for writing message meta data, we'll use colour instead
                if (message.Contains("Information"))
                {
                    Console.ResetColor();
                }
                else if (message.Contains("Error"))
                {
                    ErrorDetected = true;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("ERROR: "); // Capitalise for begin pick up via TeamCity
                }
                else if (message.Contains("Warning"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("Warning: ");
                }
            }

            public override void WriteLine(string message)
            {
                //enforce max length
                const int maxMessageLength = 1000;

                if (message != null && message.Length > maxMessageLength)
                {
                    message = message.Substring(0, maxMessageLength - 3) + "...";
                }

                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }
}