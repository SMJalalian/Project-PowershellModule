using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Principal;
using System.DirectoryServices;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Get, "NAUser")]
    public class GetUserCommand : MyLocalComputer
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }
        
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ComputerName { get; set;}        

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            if (Name == null)
                GetUser(ComputerName);
            else
            {
                foreach (string user in Name)
                    GetUser(ComputerName, user);
            }
        }

        //************************ Global Methodes *********************************

        public List<PSLocalUser> GetUser(string computer, string user = null)
        {
            List<PSLocalUser> result = new List<PSLocalUser>();
            if (user == null)
            {
                try
                {
                    DirectoryEntry targetMachine = new DirectoryEntry("WinNT://" + computer);
                    foreach (DirectoryEntry item in targetMachine.Children)
                    {
                        PSLocalUser temp = new PSLocalUser();
                        if (item.SchemaClassName == "User")
                        {
                            temp.ObjectClass = item.SchemaClassName;
                            temp.Name = item.Name;
                            temp.FullName = Convert.ToString(item.Properties["FullName"].Value);
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
                            if (((int)item.Properties["UserFlags"].Value & 0x0002) != 0x0002)
                                temp.Enabled = true;
                            else
                                temp.Enabled = false;
                            WriteObject(temp);
                            result.Add(temp);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Error on getting local user(s) list ... !!!", ErrorCategory.OpenError, computer));
                }
            }
            else
            {
                try
                {
                    DirectoryEntry targetMachine = new DirectoryEntry("WinNT://" + computer);
                    foreach (DirectoryEntry item in targetMachine.Children)
                    {
                        PSLocalUser temp = new PSLocalUser();
                        if (item.SchemaClassName == "User" && item.Name.ToLower() == user.ToLower())
                        {
                            temp.ObjectClass = item.SchemaClassName;
                            temp.Name = item.Name;
                            temp.FullName = Convert.ToString(item.Properties["FullName"].Value);
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
                            if (((int)item.Properties["UserFlags"].Value & 0x0002) != 0x0002)
                                temp.Enabled = true;
                            else
                                temp.Enabled = false;
                            WriteObject(temp);
                            result.Add(temp);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Error on getting local user ... !!!", ErrorCategory.OpenError, user));
                }
            }
            return result;
        }
    }
}
