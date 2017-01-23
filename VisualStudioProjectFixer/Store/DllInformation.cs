using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Serilog;

namespace VisualStudioProjectFixer.Store
{
    // Singleton static class, so the files are only check once per execution of the application 
    public sealed class DllInformation
    {
        private static readonly Lazy<DllInformation> Lazy = new Lazy<DllInformation>(() => new DllInformation());
        public static DllInformation Instance => Lazy.Value;

        // Key=dllName Value=StrongName and ProcessArch
        private readonly Dictionary<string, string> dllInfo = new Dictionary<string, string>();

        public string GetDllInfo(string dllName, Options options)
        {
            if (!dllInfo.ContainsKey(dllName))
                dllInfo[dllName] = GetDllLine(dllName, options);
            return dllInfo[dllName];
        }

        private string GetDllLine(string dllName, Options options)
        {
            const string dllPath = @"C:\CurrentBuild\Dev\Bin";
            var fileName = Path.Combine(dllPath, $"{dllName}.dll");
            if (!File.Exists(fileName))
            {
                var fileNameExe = Path.Combine(dllPath, $"{dllName}.exe");
                if (!File.Exists(fileNameExe))
                {
                    Log.Error($"File {fileName} does not exist, aborting");
                    if(options.ContinueProcessing) return null;
                    throw new Exception($"File {fileName} does not exist, aborting");
                }
                fileName = fileNameExe;
            }

            var an = AssemblyName.GetAssemblyName(fileName);
            // ProcessorArchitecture;
            //   https://msdn.microsoft.com/en-us/library/system.reflection.processorarchitecture(v=vs.110).aspx
            //    Member name Description
            // Amd64  A 64-bit AMD processor only.
            // Arm    An ARM processor.
            // IA64   A 64-bit Intel processor only.
            // MSIL   Neutral with respect to processor and bits-per-word.
            // None   An unknown or unspecified combination of processor and bits-per-word.
            // X86    A 32-bit Intel processor, either native or in the Windows on Windows environment on a 64-bit platform (WOW64).
            var pa = an.ProcessorArchitecture.ToString();
            if (pa.Contains("86")) pa = pa.ToLower();

            var fullName = an.FullName;
            if (!an.Flags.HasFlag(AssemblyNameFlags.PublicKey))
            {
                var spilt = fullName.Split(',');
                fullName = $"{spilt[0].Trim()}, {spilt[1].Trim()}, {spilt[2].Trim()}";
            }

            var line = an.ProcessorArchitecture == ProcessorArchitecture.None
                ? $"{fullName}"
                : $"{fullName}, processorArchitecture={pa}";
            return line;
        }
    }
}