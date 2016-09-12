using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ProjectFileFixer
{
    public class RunPowershell
    { 
        public static void ExecutePowerShellCommand(string sourcepath)
        {
            try
            {
                using (var runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();
                    using (var pipeline = runspace.CreatePipeline())
                    {
                        Command command = new Command("./UpgradeToVS2015AndDotnet4.6.ps1", true, true);

                        command.Parameters.Add(new CommandParameter(null, sourcepath));

                        pipeline.Commands.Add(command);

                        pipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                        
                        pipeline.Input.Write("\n");

                        var psresults = pipeline.Invoke();

                        foreach (var item in psresults)
                        {
                            Console.WriteLine(item);
                        }
                    }
                    runspace.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
