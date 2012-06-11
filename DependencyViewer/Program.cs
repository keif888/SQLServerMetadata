using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CommandLine;

namespace DependencyViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            DependencyArguments dependencyArguments = new DependencyArguments();
            if (CommandLine.Parser.ParseArguments(args, dependencyArguments))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrom(dependencyArguments.depDb));
            }
            else
            {
                MessageBox.Show(CommandLine.Parser.ArgumentsUsage(typeof(DependencyArguments)),
                    "Dependency Viewer Usage");
            }

        }
    }

    class DependencyArguments
    {
        [Argument(ArgumentType.AtMostOnce, HelpText = "ADO.Net SqlConnection compatible connection string to dependency database location.", DefaultValue = "Server=localhost;database=SSIS_Meta;Integrated Security=SSPI;")]
        public string depDb = "Server=localhost;database=SSIS_Meta;Integrated Security=SSPI;";
    }

}
