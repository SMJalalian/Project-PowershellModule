using System;
using System.Management.Automation;
using System.DirectoryServices;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsLifecycle.Enable, "NAUser")]
    public class EnableUserCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }
        
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
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
                EnableUser(ComputerName, user);
        }

        //************************ Global Methodes *********************************

        public void EnableUser(string computer, string user)
        {
            DirectoryEntry usr = new DirectoryEntry("WinNT://" + computer + "/" + user);
            try
            {
                usr.InvokeSet("AccountDisabled", false);
                usr.CommitChanges();
                WriteObject("'" + user + "' is enabled successfully on computer " + computer + " .");
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, user + " could not be enabled ... !!!", ErrorCategory.OpenError, user));
            }
        }
    }
}
