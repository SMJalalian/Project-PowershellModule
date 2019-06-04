using System;
using System.Management.Automation;
using System.DirectoryServices;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Remove, "NAUser")]
    public class RemoveUserCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ComputerName { get; set; }

        [Parameter(Position = 2)]
        public SwitchParameter Force { get; set; }

        private bool yesToAll;
        private bool noToAll;

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            foreach (string user in Name)
                RemoveUser(ComputerName, user);

        }

        //************************ Global Methodes *********************************

        public bool RemoveUser(string computer, string user)
        {
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + computer);
            try
            {                
                DirectoryEntries objects = localDirectory.Children;
                try
                {
                    DirectoryEntry usr = objects.Find(user);
                    if (Force || ShouldContinue(user + " will be removed from computer. Are you sure?", "Remove Confirmation ...", ref yesToAll, ref noToAll))
                        objects.Remove(usr);
                    WriteObject(string.Format("'{0}' is removed successfully from computer '{1}.' ", user, computer));
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, user + " could not be removed from computer or user does not exist  ... !!!", ErrorCategory.OpenError, user));
                    return false;
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on listing user(s) information ... !!!", ErrorCategory.OpenError, Name));
                return false;
            }
            return true;
        }
    }
}
