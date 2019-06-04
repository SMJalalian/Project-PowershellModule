using System;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security;
using System.Runtime.InteropServices;
using System.DirectoryServices.AccountManagement;

namespace Microsoft.ActiveDirectory
{
    public class MyActiveDirectory : MyMicrosoft
    {


        //********************* Properties ******************************
        public string LogService { get; private set; } = "Active Directory";
        public Forest CurrentForest { get; private set; }
        public Domain CurrentDomain { get; private set; }
        public DomainControllerCollection DCs_List { get; private set; }
        public string DomainName { get; private set; }
        public int DC_Numbers { get; private set; }

        //********************* Constructor ******************************

        public MyActiveDirectory()
        {
            try
            {
                CurrentForest = Forest.GetCurrentForest();
                CurrentDomain = Domain.GetCurrentDomain();
                DomainName = CurrentDomain.Name;
                DCs_List = CurrentDomain.FindAllDomainControllers();
                DC_Numbers = DCs_List.Count;
            }
            catch
            {
                CurrentForest = null;
                CurrentDomain = null;
                DomainName = null;
                DCs_List = null;
                DC_Numbers = 0;
            }
        }

        //********************* Methods ******************************

        public List<SearchResult> GetADComputers(string SearchBase)
        {
            DirectoryEntry Entry = new DirectoryEntry("LDAP://" + SearchBase, "SM.Jalalian", "MBoa6569*");
            DirectorySearcher MySearcher = new DirectorySearcher(Entry);
            List<SearchResult> Result = new List<SearchResult>();
            MySearcher.Filter = ("(objectClass=computer)");
            MySearcher.SizeLimit = int.MaxValue;
            MySearcher.PageSize = int.MaxValue;
            foreach (SearchResult resEnt in MySearcher.FindAll())
            {
                Result.Add(resEnt);
            }
            return Result;
        }

        public List<SearchResult> GetADUsers(string SearchBase)
        {
            DirectoryEntry Entry = new DirectoryEntry("LDAP://" + SearchBase);
            DirectorySearcher MySearcher = new DirectorySearcher(Entry);
            List<SearchResult> Result = new List<SearchResult>();
            MySearcher.Filter = ("(objectClass=user)");
            MySearcher.SizeLimit = int.MaxValue;
            MySearcher.PageSize = int.MaxValue;
            foreach (SearchResult resEnt in MySearcher.FindAll())
            {
                Result.Add(resEnt);
            }
            return Result;
        }

        public string GetUserFullName(string userLogonName)
        {
            string result = "";
            PrincipalContext targetDomain = null;
            targetDomain = new PrincipalContext(ContextType.Domain, DomainName);     
            PrincipalSearcher searcher = new PrincipalSearcher(new UserPrincipal(targetDomain));

            foreach (var adObject in searcher.FindAll())
            {
                DirectoryEntry de = adObject.GetUnderlyingObject() as DirectoryEntry;
                if ((de.Properties["sAMAccountName"].Value).ToString() == userLogonName)
                {
                    result = (de.Properties["displayName"].Value).ToString();
                    break;
                }
            }
            return result;
        }
    }

    public class DomainComputer
    {
        public string DistinguishedName { get; set; }
        public bool? Enabled { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }
        public Guid? ObjectGUID { get; set; }
        public string SamAccountName { get; set; }
        public System.Security.Principal.SecurityIdentifier SID { get; set; }
        public string UserPrincipalName { get; set; }

        public DomainComputer ()
        {

        }

        public DomainComputer(ComputerPrincipal computer)
        {
            DistinguishedName = computer.DistinguishedName;
            Enabled = computer.Enabled;
            Name = computer.Name;
            ObjectClass = computer.StructuralObjectClass;
            ObjectGUID = computer.Guid;
            SamAccountName = computer.SamAccountName;
            SID = computer.Sid;
            UserPrincipalName = computer.UserPrincipalName;
        }
    }

    public class DomainUser
    {
        public string DistinguishedName { get; set; }
        public bool? Enabled { get; set; }
        public string GivenName { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }
        public Guid? ObjectGUID { get; set; }
        public string SamAccountName { get; set; }
        public System.Security.Principal.SecurityIdentifier SID { get; set; }
        public string Surname { get; set; }
        public string UserPrincipalName { get; set; }

        public DomainUser()
        {

        }

        public DomainUser(UserPrincipal user)
        {
            DistinguishedName = user.DistinguishedName;
            Enabled = user.Enabled;
            GivenName = user.GivenName;
            Name = user.Name;
            ObjectClass = user.StructuralObjectClass;
            ObjectGUID = user.Guid;
            SamAccountName = user.SamAccountName;
            SID = user.Sid;
            Surname = user.Surname;
            UserPrincipalName = user.UserPrincipalName;
        }
    }
}
