using System;
using System.Management.Automation;
using System.DirectoryServices;
using System.Collections;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Remove, "NAGroupMember")]
    public class RemoveGroupMemberCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline =true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Member { get; set; }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        public string ComputerName { get; set; }

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            foreach (string grp in Name)
            {
                foreach (string  mbr in Member)
                    RemoveGroupMembers(ComputerName, grp, mbr, true);
            }            
        }

        //************************ Global Methodes *********************************

        public bool RemoveGroupMembers(string computer, string group, string memberToRemove, bool showReport=false)
        {
            DirectoryEntry targetMachine = new DirectoryEntry("WinNT://" + computer + ",Computer");
            DirectoryEntry grp = targetMachine.Children.Find(group, "group");
            bool FindTag = false;
            foreach (object Member in (IEnumerable)grp.Invoke("Members"))
            {
                DirectoryEntry MemberEntry = new DirectoryEntry(Member);
                if (MemberEntry.Name == memberToRemove)
                {
                    grp.Invoke("Remove", new[] { MemberEntry.Path });
                    grp.CommitChanges();
                    FindTag = true;
                    if (showReport)
                        WriteObject(string.Format("Object ({0}) successfully removed from ({1}) Group.", memberToRemove, group));
                    return true;
                }                                  
            }
            if (!FindTag && showReport)
                WriteVerbose(string.Format("Object ({0}) does not exist in ({1}) Group.", memberToRemove, group));
            return false;
        }
    }
}
