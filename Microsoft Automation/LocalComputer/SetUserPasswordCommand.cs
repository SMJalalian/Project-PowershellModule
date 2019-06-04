using System;
using System.Management.Automation;
using System.DirectoryServices;
using System.Security;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Set, "NAUserPassword")]
    public class SetUserPasswordCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ComputerName { get; set; }

        [Parameter(Position = 2)]
        public SecureString Password { get; set; }

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            foreach (string user in Name)
                SetUserPassword(user, ComputerName, Password);
        }

        //************************ Global Methodes *********************************

        public bool SetUserPassword (string name, string copmuterName, SecureString password)
        {
            try
            {
                DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + copmuterName);
                DirectoryEntries users = localDirectory.Children;
                DirectoryEntry user = users.Find(name);
                user.Invoke("SetPassword", SecureStringToString(password));
                WriteObject(string.Format("{0}'s Password on computer '{1}' is changed successfully.", name, copmuterName));
                return true;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Password can not be changed .. !! ", ErrorCategory.OpenError, name));
                return false;
            }
        }
    }
}
