using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft
{    
    public class MyMicrosoft : Cmdlet
    {
        [Parameter()]
        public SwitchParameter AutomationLog { set; get; }

        public string LogCategory { get; private set; } = "Microsoft";

        public string SQLServerName { get; set; }
        public string SQLServerInstance { get; set; }
        public string SQLDB { get; set; }
        public string AssemblyFolder { get; set; }

        public string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }

    public class PSEventLog
    {
        public string Host { get; set; }
        public string Subject { get; set; }
        public string SubjectFullName { get; set; }
        public DateTime Date { get; set; }
        public long EventID { get; set; }
        public string Type { get; set; }        
        public string Description { get; set; }

        public List<string[]> SplitLogMessage (EventLogEntry log)
        {
            List<string[]> result = new List<string[]>();
            StringReader strReader = new StringReader(log.Message.ToString());
            while (true)
            {
                string aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    string[] parts = aLine.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    result.Add(parts);
                }
                else
                    break;
            }
            return result;
        }
    }

    public class PSGroupEventLog : PSEventLog
    {
        public string Member { get; set; }
        public string Group { get; set; }
    }

    public class PSUserEventLog : PSEventLog
    {
        public string User { get; set; }
        public string Action { get; set; }
    }

}
