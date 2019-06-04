using System;
using System.Collections.Generic;

namespace Microsoft.LocalComputer
{
    public class MyLocalComputer : MyMicrosoft
    {
        public string LogService { get; private set; } = "Local Computer";
    }
    //*******************************************************************************************
    public class PSLocalMember
    {
        public string Name { get; set; }
        public string MemberOf { get; set; }
        public string ObjectClass { get; set; }
        public string Parent { get; set; }
        public System.Security.Principal.SecurityIdentifier SID { get; set; }
    }
    //*******************************************************************************************
    public class PSLocalUser
    {
        public string Name { get; set; }        
        public string ObjectClass { get; set; }
        public bool Enabled { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public System.Security.Principal.SecurityIdentifier SID { get; set; }                
    }
    //*******************************************************************************************
    public class PSLocalGroup
    {
        public string Name { get; set; }
        public string ObjectClass { get; set; }        
        public string Description { get; set; }
        public System.Security.Principal.SecurityIdentifier SID { get; set; }
    }
    //*******************************************************************************************
    public class PSLocalComputer
    {
        public string Name { get; set; }        
        public string OperatingSystem { get; set; }
        public Version Version { get; set; }
        public UInt64 TotalMemory { get; set; }
        public UInt64 TotalDiskSpace { get; set; }
        public List<string> LoggedInUser { get; set; } = new List<string>();
        public string OSDirectory { get; set; }

    }
    //*******************************************************************************************
}