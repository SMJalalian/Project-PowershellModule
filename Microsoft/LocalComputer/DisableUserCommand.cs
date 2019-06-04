using System;
using System.Management.Automation;
using System.DirectoryServices;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsLifecycle.Disable, "NAUser")]
    public class DisableUserCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }
        
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ComputerName { get; set; }

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            foreach (string user in Name)
                DisableUser(ComputerName, user);                
        }

        //************************ Global Methodes *********************************

        public bool DisableUser( string computer , string user)
        {
            DirectoryEntry usr = new DirectoryEntry("WinNT://" + computer + "/" + user);
            try
            {
                usr.InvokeSet("AccountDisabled", true);
                usr.CommitChanges();
                WriteObject("'" + user + "' is disabled successfully on computer " + computer + " .");
                return true;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, user + " could not be disabled ... !!!", ErrorCategory.OpenError, user));
                return false;
            }
        }
    }
}
