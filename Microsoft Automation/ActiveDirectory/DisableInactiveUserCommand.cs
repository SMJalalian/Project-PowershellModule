using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;
using Microsoft.Tools;

namespace Microsoft.ActiveDirectory
{
    [Cmdlet(VerbsLifecycle.Disable, "NAInactiveUser")]
    public class DisableInactiveUserCommand : MyActiveDirectory
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string ComputerName { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public string SearchBase { get; set; }

        [Parameter(Position = 2, Mandatory = true)]
        public int InactiveDays { get; set; }

        [Parameter(Position = 3)]
        public string DestOU { get; set; } = null;

        [Parameter(Position = 4)]
        public PSCredential Credential { get; set; } = null;

        //************************ PS Methodes *********************************

        protected override void ProcessRecord()
        {
            DisableInactiveUser(ComputerName, InactiveDays, SearchBase, DestOU, Credential);
        }

        //************************ Global Methodes *********************************

        public List<DomainUser> DisableInactiveUser(string domainName, int InactiveDays, string searchBase = null, string newLocation = null, PSCredential credential = null)
        {
            LogWriter log = new LogWriter();
            List<DomainUser> result = new List<DomainUser>();
            PrincipalContext targetDomain = null;
            DirectoryEntry source = null;
            DirectoryEntry destination = null;
            try
            {
                if (credential != null)
                {
                    targetDomain = new PrincipalContext(ContextType.Domain, domainName, searchBase,
                                                                       credential.UserName, SecureStringToString(credential.Password));
                }
                else
                {
                    targetDomain = new PrincipalContext(ContextType.Domain, domainName, searchBase);
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, " Error on connecting to directory  ... !!!", ErrorCategory.OpenError, targetDomain));
                throw;
            }

            try
            {
                PrincipalSearchResult<UserPrincipal> users = UserPrincipal.FindByLogonTime(targetDomain, (DateTime.Now).AddDays(-InactiveDays), MatchType.LessThanOrEquals);
                foreach (UserPrincipal account in users)
                {
                    account.Enabled = false;
                    account.Save();
                    if (newLocation != null && credential != null)
                    {
                        source = new DirectoryEntry("LDAP://" + account.DistinguishedName,
                                                    credential.UserName, SecureStringToString(credential.Password));
                        destination = new DirectoryEntry("LDAP://" + newLocation,
                                                         credential.UserName, SecureStringToString(credential.Password));
                        source.MoveTo(destination);
                        source.Close();
                        destination.Close();
                    }
                    else if (newLocation != null && credential == null)
                    {
                        source = new DirectoryEntry("LDAP://" + account.DistinguishedName);
                        destination = new DirectoryEntry("LDAP://" + newLocation);
                        source.MoveTo(destination);
                        source.Close();
                        destination.Close();
                    }
                    DomainUser temp = new DomainUser(account);
                    result.Add(temp);
                }

                string output = string.Format("{0} number(s) of user is disabled by network automation task ...\n", result.Count);
                output += "-----------------------------\n";
                foreach (var item in result)
                    output += item.Name + "\n";
                WriteObject(output);

                if (AutomationLog)
                    log.InsertLog("Information", LogCategory, LogService, this.GetType().Name, output);
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, " Error occured on process  ... !!!", ErrorCategory.OpenError, log));
                throw;
            }
            return result;
        }
    }
}
