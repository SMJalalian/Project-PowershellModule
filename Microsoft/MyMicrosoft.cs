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
}
