using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;

namespace VisualStudioProjectFixer.Scripts
{
    // TODO NOT TESTED, NOT ENABLE
    [Verb("ChangeCSTargetTov14", HelpText = "")]
    public class ChangeCSTargetTov14 : Options
    {
        public void Run()
        {
            List<string> sourceFileList = Helper.GetProjectFiles(RootFolder, Config.GetSourceSearchPatterns);
            foreach (var filepath in sourceFileList)
            {
                if (filepath.Contains(".csproj"))
                {
                    string text = File.ReadAllText(filepath);
                    text =
                        text.Replace(
                            "Project=\"$(MSBuildExtensionsPath)\\Microsoft\\VisualStudio\\v9.0\\WebApplications\\Microsoft.WebApplication.targets\"",
                            "Project=\"$(MSBuildExtensionsPath)\\Microsoft\\VisualStudio\\v14.0\\WebApplications\\Microsoft.WebApplication.targets\"");
                    File.WriteAllText(filepath, text, Encoding.UTF8);
                }
            }
        }
    }
}