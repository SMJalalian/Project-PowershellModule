using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Get, "UntrustUser")]
    public class GetUntrustUserCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory =true, ValueFromPipeline =true)]
        public List<PSLocalUser> UserList { get; set; }
        [Parameter(Position = 1,Mandatory =true,ValueFromPipelineByPropertyName =true)]
        public string[] TrustList { get; set; }        

        //************************ PS Methodes *********************************

        protected override void ProcessRecord()
        {
            GetUntrustUser(UserList, TrustList);
        }

        //************************ Global Methodes *********************************

        public List<PSLocalUser>  GetUntrustUser (List<PSLocalUser> localUsers, string[] trustList)
        {
            List<PSLocalUser> result = new List<PSLocalUser>();
            try
            {

                foreach (PSLocalUser user in localUsers)
                {
                    bool Trust = false;
                    foreach (string s in trustList)
                    {
                        if (s.ToLower() == user.Name.ToLower())
                        {
                            Trust = true;
                            break;
                        }
                    }
                    if (!Trust)
                    {
                        WriteObject(user);
                        result.Add(user);
                    }                        
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on finding untrusted users ... !!!", ErrorCategory.OpenError, localUsers));
                throw;
            } 
           return result;
        }
    }
}
