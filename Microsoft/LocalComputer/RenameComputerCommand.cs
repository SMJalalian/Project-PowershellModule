using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Security;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Rename, "NAComputer")]
    public class RenameComputerCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory =true)]
        [ValidateNotNullOrEmpty]
        public string ComputerName { get; set; }
        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string NewName { get; set; }
        [Parameter(Position = 2)]
        public PSCredential DomainCredential { get; set; }
        [Parameter(Position = 3)]
        public PSCredential LocalCredential { get; set; }
        [Parameter(Position = 4)]
        public SwitchParameter Restart;

        //************************ PS Methodes *********************************

        protected override void ProcessRecord()
        {
            RenameComputer(ComputerName, NewName, DomainCredential, LocalCredential);
        }

        //************************ Global Methodes *********************************

        public bool RenameComputer( string computerName, string newName, PSCredential domainCredential =null, PSCredential localCredential=null)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            bool flag = false;
            try
            {
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.FileName = "netdom.exe";
                if (domainCredential != null && localCredential != null)
                {
                    startInfo.Arguments = "renamecomputer " + computerName + " /newname:" + newName +
                                          "  /userD:" + domainCredential.UserName + "  /passwordD:" + SecureStringToString(domainCredential.Password) +
                                          "  /userO:" + localCredential.UserName + "  /passwordO:" + SecureStringToString(localCredential.Password) +
                                          " /Force";
                }
                else if (domainCredential != null)
                {
                    startInfo.Arguments = "renamecomputer " + computerName + " /newname:" + newName +
                                          "  /userD:" + domainCredential.UserName + "  /passwordD:" + SecureStringToString(domainCredential.Password) +
                                          " /Force";
                }
                else if (localCredential != null)
                {
                    startInfo.Arguments = "renamecomputer " + computerName + " /newname:" + newName +
                                          "  /userO:" + localCredential.UserName + "  /passwordO:" + SecureStringToString(localCredential.Password) +
                                          " /Force";
                }
                else
                    startInfo.Arguments = "renamecomputer " + computerName + " /newname:" + newName +" /Force";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    WriteObject(line);
                    if (line == "The command completed successfully.")
                        flag = true;
                }
                if (flag && Restart)
                {
                    WriteObject("The computer will be restarted to complete actions ... ");
                    startInfo.FileName = "shutdown.exe";
                    startInfo.Arguments = String.Format(@"-r -m \\{0} -t 1 -f", computerName);
                    process.StartInfo = startInfo;
                    process.Start();
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Failed to rename target computer ... !!!", ErrorCategory.OpenError, e.Source));
                return false;
            }

            if (flag)
                return true;
            else
                return false;
        }
    }
}
