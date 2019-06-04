using System;
using System.Management.Automation;
using System.Security.Principal;
using System.DirectoryServices;
using System.Collections.Generic;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Get, "NAGroup")]
    public class GetGroupCommand : MyLocalComputer
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
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
            if (Name == null)
                GetGroup(ComputerName);
            else
            {
                foreach (string group in Name)
                    GetGroup(ComputerName, group);
            }
        }

        //************************ Global Methodes *********************************

        public List<PSLocalGroup> GetGroup(string computer, string group = null)
        {
            List<PSLocalGroup> result = new List<PSLocalGroup>();
            DirectoryEntry targetMachine = new DirectoryEntry("WinNT://" + computer);
            if (group == null)
            {                
                try
                {                    
                    foreach (DirectoryEntry item in targetMachine.Children)
                    {
                        if (item.SchemaClassName == "Group")
                        {
                            PSLocalGroup temp = new PSLocalGroup();
                            temp.Name = item.Name;
                            temp.ObjectClass = "Group";
                            try
                            {
                                NTAccount f = new NTAccount(item.Name);
                                SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                                temp.SID = s;
                            }
                            catch
                            {
                                temp.SID = null;
                            }
                            temp.Description = Convert.ToString(item.Properties["Description"].Value);
                            WriteObject(temp);
                            result.Add(temp);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Error on getting local group(s) list ... !!!", ErrorCategory.OpenError, computer));
                }
            }
            else
            {
                try
                {
                    foreach (DirectoryEntry item in targetMachine.Children)
                    {
                        if (item.SchemaClassName == "Group" && item.Name.ToLower() == group.ToLower())
                        {
                            PSLocalGroup temp = new PSLocalGroup();
                            temp.Name = item.Name;
                            temp.ObjectClass = "Group";
                            try
                            {
                                NTAccount f = new NTAccount(item.Name);
                                SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                                temp.SID = s;
                            }
                            catch
                            {
                                temp.SID = null;
                            }
                            temp.Description = Convert.ToString(item.Properties["Description"].Value);
                            WriteObject(temp);
                            result.Add(temp);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Error on getting local group(s) list", ErrorCategory.OpenError, group));
                }                
            }
            return result;
        }
    }
}
