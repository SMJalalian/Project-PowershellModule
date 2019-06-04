using System;
using System.Management;
using System.Management.Automation;
using System.Linq;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Get, "NAComputerInformation")]
    public class GetComputerInformationCommand : MyLocalComputer
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
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
            GetComputerInformation(ComputerName);
        }

        //************************ Global Methodes *********************************

        public PSLocalComputer GetComputerInformation (string computer)
        {
            PSLocalComputer result = new PSLocalComputer();
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;

            ManagementScope scope = new ManagementScope(String.Format(@"\\{0}\root\cimv2", computer), options);
            scope.Connect();

            #region OSInfo
            try
            {
                ObjectQuery osQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                ManagementObjectSearcher osSearcher = new ManagementObjectSearcher(scope, osQuery);
                foreach (ManagementObject WmiObject in osSearcher.Get())
                {
                    result.Name = WmiObject["csname"].ToString();
                    result.OperatingSystem = WmiObject["Caption"].ToString();
                    result.OSDirectory = WmiObject["WindowsDirectory"].ToString();
                    Version.TryParse((WmiObject["Version"]).ToString(),out Version ver);
                    result.Version = ver;
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on getting OS information ... !!!", ErrorCategory.OpenError, scope));
                throw;
            }

            #endregion

            #region Physical Memory
            try
            {
                ObjectQuery ramQuery = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
                ManagementObjectSearcher ramSearcher = new ManagementObjectSearcher(scope, ramQuery);
                UInt64 ramCapacity = 0;
                foreach (ManagementObject WmiObject in ramSearcher.Get())
                    ramCapacity += (UInt64)WmiObject["Capacity"];
                result.TotalMemory = ramCapacity / (1024 * 1024);
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on getting physical memory information ... !!!", ErrorCategory.OpenError, scope));
                throw;
            }
            #endregion

            #region Physical Disks
            try
            {
                ObjectQuery diskQuery = new ObjectQuery("SELECT * FROM Win32_DiskDrive");
                ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher(scope, diskQuery);
                UInt64 diskCapacity = 0;
                foreach (ManagementObject WmiObject in diskSearcher.Get())
                    diskCapacity += Convert.ToUInt64(WmiObject["Size"]) / 1073741824;
                result.TotalDiskSpace = diskCapacity;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on getting physical disk(s) information ... !!!", ErrorCategory.OpenError, scope));
                throw;
            }
            #endregion

            #region Logged in Users
            try
            {
                ObjectQuery processQuery = new ObjectQuery("Select * From Win32_Process Where Name = 'explorer.exe'");
                ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher(scope, processQuery);
                foreach (ManagementObject WmiObject in diskSearcher.Get())
                {
                    object[] argList = { string.Empty, string.Empty };
                    var returnVal = Convert.ToInt32(WmiObject.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                        (result.LoggedInUser).Add(string.Format(argList[1] + "\\" + argList[0]));                   
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on getting Logged in user information ... !!!", ErrorCategory.OpenError, scope));
                throw;
            }
            #endregion

            WriteObject(result);
            return result;
        }

        public string GetProcessOwner(int processId)
        {
            var query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectCollection processList;

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                processList = searcher.Get();

            foreach (var mo in processList.OfType<ManagementObject>())
            {
                object[] argList = { string.Empty, string.Empty };
                var returnVal = Convert.ToInt32(mo.InvokeMethod("GetOwner", argList));

                if (returnVal == 0)
                    return argList[1] + "\\" + argList[0];
            }
            return "NO OWNER";
        }
    }
}
