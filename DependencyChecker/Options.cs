using System.Collections.Generic;
using System.Text;
using NConsole;

namespace DependencyChecker
{
    public interface IOptions
    {
        string SourceDirectory { get; }
        bool Help { get; }

        string Mode { get; }
    }


    internal class Options : IOptions
    {
        [CommandLineArgument("h", Description = "Show usage", Required = false)]
        public bool? HelpArg { get; set; }

        [CommandLineArgument(@"dir", Description = "Root source directory (default: $Dev_Src)", Required = true)]
        public string SourceDirectory { get; set; }


        [CommandLineArgument(@"mode",
            Description = "Mode usage /mode:[CheckMakeFiles,CheckProjectFiles,CheckCompliance,FormatMakeFiles]",
            Required = true)]
        public string Mode { get; set; }


        public bool Help
        {
            get { return HelpArg.GetValueOrDefault(false); }
        }

        #region Functions

        public string PrintOptions()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("TODO HELP:").AppendLine();
            //sb.AppendFormat("Database:    {0}", Database).AppendLine();
            //if (UseIntegratedSecurity)
            //{
            //    sb.AppendLine("Using integrated security");
            //}
            //else
            //{
            //    sb.AppendFormat("User:        {0}", User).AppendLine();
            //    sb.AppendFormat("Password:    {0}", Password).AppendLine();
            //}
            //sb.AppendFormat("Batch Calc:   {0}", BatchCalc.ToString()).AppendLine();
            //if (this.BatchCalc == BatchEndMethod.Define)
            //{
            //    sb.AppendFormat("Batch Start: {0}", BatchStart).AppendLine();
            //    sb.AppendFormat("Batch End:   {0}", BatchEnd).AppendLine();
            //}
            //sb.AppendFormat("max dop:     {0}", MaxDop.ToString());
            //if (Force)
            //{
            //    sb.AppendLine();
            //    sb.Append("Validation is ignored (force option). This is dangerous!");
            //}
            return sb.ToString();
        }

        #endregion

        #region Static

        public static IOptions ParseOptions(string[] args)
        {
            var ops = new CommandLineParser<Options>().ParseArguments(args);
            if (Validate(ops))
            {
                return ops;
            }
            return new Options {HelpArg = true};
        }

        public static string GetUsage()
        {
            return new CommandLineParser<Options>().Usage;
        }

        private static bool Validate(Options ops)
        {
            //Shitty way of creating a Mode OPtions list.  Need to work out how to use Nconsole correctly or add this feature
            var modeOptions = new List<string>
            {
                "CheckMakeFiles".ToLower(),
                "CheckProjectFiles".ToLower(),
                "CheckCompliance".ToLower(),
                "FormatMakeFiles".ToLower()
            };
            if (!modeOptions.Contains(ops.Mode.ToLower())) return false;


            return true;
        }

        #endregion
    }
}