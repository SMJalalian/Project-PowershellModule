using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Management.Automation;
using System.DirectoryServices;
using System.Collections;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Get, "NAGroupMember")]
    public class GetGroupMemberCommand : MyLocalComputer
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }
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
            GetGroupMembers(ComputerName, Name);
        }

        //************************ Global Methodes *********************************

        public List<PSLocalMember> GetGroupMembers( string computer , string group)
        {
            List<PSLocalMember> result = new List<PSLocalMember>();
            DirectoryEntry targetMachine = new DirectoryEntry("WinNT://" + computer + ",Computer");
            DirectoryEntry gp = targetMachine.Children.Find(group, "group");
            object members = gp.Invoke("members", null);
            try
            {
                foreach (object groupMember in (IEnumerable)members)
                {
                    PSLocalMember temp = new PSLocalMember();
                    DirectoryEntry member = new DirectoryEntry(groupMember);
                    temp.Name = member.Name;
                    temp.MemberOf = group;
                    temp.ObjectClass = member.SchemaClassName;
                    temp.Parent = member.Parent.Path;
                    try
                    {
                        NTAccount f = new NTAccount(member.Name);
                        SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                        temp.SID = s;
                    }
                    catch
                    {
                        temp.SID = null;
                    }
                    WriteObject(temp);
                    result.Add(temp);
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on getting local group member(s) ... !!!", ErrorCategory.OpenError, group));
            }
            
            return result;
        }
    }
}
