using System;
using System.Management.Automation;
using System.DirectoryServices;
using System.Collections;
using System.Collections.Generic;


namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Remove, "NAUnauthorizeGroupMember")]
    public class RemoveUnauthorizeGroupMemberCommand : MyLocalComputer
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string ComputerName { get; set; }
        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }
        [Parameter(Position = 2, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] AuthorizeList { get; set; } = null;
        [Parameter(Position = 3)]
        public SwitchParameter JustShow;

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            foreach (string grp in Name)
                RemoveUnauthorizeGroupMember(ComputerName, grp, AuthorizeList, JustShow);
        }

        //************************ Global Methodes *********************************

        public bool RemoveUnauthorizeGroupMember ( string computer, string group, string[] authorizeList, SwitchParameter justShow)
        {
            DirectoryEntry targetMachine = new DirectoryEntry("WinNT://" + computer);
            try
            {
                foreach (DirectoryEntry item in targetMachine.Children)
                {
                    if ((item.SchemaClassName == "Group") && (item.Name == group))
                    {
                        object MembersList = item.Invoke("members", null);
                        foreach (object grpMember in (IEnumerable)MembersList)
                        {
                            DirectoryEntry member = new DirectoryEntry(grpMember);
                            int pos = Array.IndexOf(authorizeList, member.Name);
                            if ((pos < 0))
                            {
                                if (!justShow)
                                {
                                    item.Invoke("Remove", new[] { member.Path });
                                    item.CommitChanges();
                                    WriteObject(string.Format("Object ({0}) successfully removed from ({1}) Group.", member.Name, group));
                                }
                                else
                                    WriteWarning(string.Format("{0} will be removed from ({1}) group", member.Name, group));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Objects can not be removed from targte group ... !!!", ErrorCategory.OpenError, targetMachine));
                return false;               
            }
            
            return true;
        }
    }
}