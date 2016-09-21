using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CheckStoreProcCase
{
    // Notes
    // 2133 SQL Files 
    // 25510 Files to check (thats a lot to check)
    // About 30mins to run on a machine with an SSD
    // Could make it faster and parallel but so what, run once every few years.
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (args.Length < 1) throw new Exception("No Stage to do");

            new Program().Execute(args[0]);
        }

        public void Execute(string stage)
        {
            // Stage 0 Build Store Procedure list to check. This is every SQL file in the db\proc folder 
            // Not check multipliable folders 

            const string sqlCheckFolder = @"C:\Dev\db\streams\procs";
            const string sqlCheckListFile = @"C:\Temp\sqlCheckList.txt";

            const string sourceCheckRootFolder = @"C:\Dev\";
            const string sourceCheckListFile = @"C:\Temp\sourceCheckList.txt";
            string[] sourceSearchPatterns = { "*.mak", "*.sql", "*.xml", "*.cs", "*.h", "*.cpp", "*.csproj", "*.vcxproj", "*.py" };

            const string incorrectListFile = @"C:\Temp\incorrectListFile.txt";

            var sqlCheckList = new List<string>();
            var sourceFileList = new List<string>();
            var incorrectFileList = new List<string>();

            // Stage Rules
            if (stage.Contains("5")) stage = stage + "34";
            if (stage.Contains("7")) stage = stage + "36";

            // Run Stages
            if (stage.Contains("1"))
            {
                Stage1BuildStoreProcedureList(sqlCheckFolder, sqlCheckListFile);
            }
            if (stage.Contains("2"))
            {
                Stage2BuildSourceFileList(sourceCheckRootFolder, sourceCheckListFile, sourceSearchPatterns);
            }
            if (stage.Contains("3"))
            {
                sqlCheckList = Stage3GetStoreProcedureList(sqlCheckListFile);
            }
            if (stage.Contains("4"))
            {
                sourceFileList = Stage4GetSourceFileList(sourceCheckListFile);
            }
            if (stage.Contains("5")) // Must do 3 and 4
            {
                Stage5CheckForIncorrectCase(sqlCheckList, sourceFileList, incorrectListFile);
            }
            if (stage.Contains("6"))
            {
                incorrectFileList = Stage6GetIncorrectFileList(incorrectListFile);
            }
            if (stage.Contains("7")) // Must do 3 and 6
            {
                Stage7ChangeCaseForIncorrectFiles(sqlCheckList, incorrectFileList);
            }
        }

        void Stage1BuildStoreProcedureList(string sqlFolder, string outputFileName)
        {
            var files = Directory.EnumerateFiles(sqlFolder, "*.sql");
            var sqllist = new List<string>();
            foreach (var file in files)
            {
                var t = file.TrimEnd(".sql".ToCharArray());
                var t2 = t.Remove(0, sqlFolder.Length + 1);

                sqllist.Add(t2);
            }
            File.WriteAllLines(outputFileName, sqllist);
        }

        List<string> Stage3GetStoreProcedureList(string fileName)
        {
            return File.ReadLines(fileName).ToList();
        }

        void Stage2BuildSourceFileList(string rootFolder, string outputFileName, string[] searchPatterns)
        {
            var files = searchPatterns.AsParallel()
                            .SelectMany(searchPattern => Directory.EnumerateFiles(rootFolder, searchPattern, SearchOption.AllDirectories));
            File.WriteAllLines(outputFileName, files);
        }

        List<string> Stage4GetSourceFileList(string fileName)
        {
            return File.ReadLines(fileName).ToList();
        }

        void Stage5CheckForIncorrectCase(List<string> sqlCheckList, List<string> sourceFileList, string incorrectListFile)
        {
            var filesToChange = sourceFileList.AsParallel()
                        .Where(f => Stage5bCheckFile(f, sqlCheckList))
                        .Select(f => f);

            File.WriteAllLines(incorrectListFile, filesToChange);
        }

        bool Stage5bCheckFile(string fileName, List<string> sqlCheckList)
        {
            var text = File.ReadAllText(fileName);
            foreach (string storeProc in sqlCheckList)
            {
                int index = text.IndexOf(storeProc, 0, StringComparison.OrdinalIgnoreCase);
                while (index >= 0)
                {
                    // check for correctness at "index"
                    var incorrectValue = text.Substring(index, storeProc.Length);

                    // One found, return true, test done
                    if (incorrectValue != storeProc) return true; 
                    
                    if (index + storeProc.Length > text.Length) break;
                    index = text.IndexOf(storeProc, index + storeProc.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        List<string> Stage6GetIncorrectFileList(string incorrectListFile)
        {
            return File.ReadLines(incorrectListFile).ToList();
        }

        // to do make the parallel, nope should not be that many
        void Stage7ChangeCaseForIncorrectFiles(List<string> sqlCheckList, List<string> incorrectListFile)
        {
            //Should make the outer for each Parallel
            foreach (string file in incorrectListFile)
            {
                var text = File.ReadAllText(file);
                foreach (string storeProc in sqlCheckList)
                {
                    int index = text.IndexOf(storeProc, 0, StringComparison.OrdinalIgnoreCase);
                    while (index >= 0)
                    {
                        // check for correctness at "index"
                        var incorrectValue = text.Substring(index, storeProc.Length);
                        bool test = incorrectValue == storeProc;
                        if (!test)
                        {
                            text = text.Replace(incorrectValue, storeProc);
                        }
                        if (index + storeProc.Length > text.Length) break;
                        index = text.IndexOf(storeProc, index + storeProc.Length, StringComparison.OrdinalIgnoreCase);
                    }
                }
                File.WriteAllText(file, text);
            }
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

