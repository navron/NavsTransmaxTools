using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MakeProjectFixer.VisualStudioFile
{
    //class ProjectFileCSharp

    //{
    //    public List<CsProjectFile> ProjectFiles { get; set; }


    //    private void ScanProjectFiles(Options options)
    //    {
    //        var excludedlist = new List<string>();
    //        excludedlist.Add("Tsd.Libraries.Common.Eventing"); // In code but not in build system, Need to Talk to MattH about this
    //        excludedlist.Add("ManagementConsole"); // Jono special, Its missing a number of files. Needs work, Not in build.

    //        // Only doing CS Project files for now, could easily include C++ projects
    //        var csfiles = Directory.EnumerateFiles(options.Folder, "*.csproj", SearchOption.AllDirectories).ToList();
    //        var vcfiles = Directory.EnumerateFiles(options.Folder, "*.vcxproj", SearchOption.AllDirectories).ToList();

    //        ProjectFiles = new List<CsProjectFile>();
    //        foreach (var file in csfiles)
    //        {
    //            // Don't want Unit Test in the list. Assume that these are ok
    //            if (file.Contains(@"\UnitTest")) continue;  // UnitTests and UnitTestSupport Folders

    //            // Don't want Test in the list. Assume that these are ok
    //            if (file.Contains(@"\test\")) continue;

    //            // Remove 3rdparty Project Files. On build machines the 3rdparty lib are check out to $src\lib\3rdparty and thus pick up
    //            if (file.Contains(@"\3rdparty\")) continue;

    //            // Exclude any known problems
    //            if (excludedlist.Any(s => file.Contains(s))) continue;

    //            ProjectFiles.Add(new CsProjectFile { FileName = file });
    //        }
    //        Console.WriteLine($"Project files scan done. {ProjectFiles.Count} project files");
    //    }
    //}
    //internal class CsProjectFile
    //{
    //    public CsProjectFile()
    //    {
    //        Walked = false;
    //        TsdRefenences = new List<string>();
    //        TsdRefenencesComplete = new List<string>();
    //        ProjectReference = new List<string>();
    //    }

    //    public string FileName { get; set; }
    //    public string AssemblyName { get; set; }

    //    // Tsd References used by this project
    //    public List<string> TsdRefenences { get; set; }

    //    // List of Tsd References including tsd References that those references use
    //    public List<string> TsdRefenencesComplete { get; private set; }

    //    // List of Project Reference
    //    public List<string> ProjectReference { get; private set; }

    //    public bool Walked { get; set; }
    //    public bool Checking { get; set; }
    //}
}
