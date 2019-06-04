using System;
using Microsoft.Tools;
using Microsoft.UpdateServices.Administration;
using System.Management.Automation;

namespace Microsoft.WSUS
{
    [Cmdlet(VerbsCommon.Clear, "NAWSUSData")]
    public class ClearWSUSDataCommand : MyWSUS
    {

        [Parameter(Position = 0,ValueFromPipeline =true)]
        public string ComputerName { get; set; }

        [Parameter(Position = 1)]
        public int PortNumber { get; set; } = 8530;

        [Parameter(Position = 2)]
        public bool IsSecure { get; set; } = false;

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (IsSecure && PortNumber == 8530)
                PortNumber = 8531;
            if (ComputerName == null)
                ComputerName = System.Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            ClearWSUSData(ComputerName, PortNumber, IsSecure);
        }

        //************************ Global Methodes *********************************

        public bool ClearWSUSData(string computer, int portNumber, bool isSecure)
        {
            string output = "";
            LogWriter log = new LogWriter();
            CleanupResults result;

            try
            {
                IUpdateServer wsus = AdminProxy.GetUpdateServer(computer, isSecure, portNumber);
                ICleanupManager Manager = wsus.GetCleanupManager();
                CleanupScope Clean_Scope = new CleanupScope();

                Clean_Scope.DeclineSupersededUpdates = true;
                Clean_Scope.DeclineExpiredUpdates = true;
                Clean_Scope.CleanupObsoleteComputers = true;
                Clean_Scope.CleanupObsoleteUpdates = true;
                Clean_Scope.CleanupUnneededContentFiles = true;
                Clean_Scope.CompressUpdates = true;
                result = Manager.PerformCleanup(Clean_Scope);
                output = "Number of Superseded Updates Declined = " + result.SupersededUpdatesDeclined.ToString() + "\n";
                output += "Number of Obsolete Computers Deleted = " + result.ObsoleteComputersDeleted.ToString() + "\n";
                output += "Number of Obsolete Updates Deleted = " + result.ObsoleteUpdatesDeleted.ToString() + "\n";
                output += "Number of Expired Updates Declined = " + result.ExpiredUpdatesDeclined.ToString() + "\n";
                output += "Number of Updates Compressed = " + result.UpdatesCompressed.ToString() + "\n\n";
                output += "Amount of Cleanup Size = " + result.DiskSpaceFreed.ToString();
                WriteVerbose(output);
            }
            catch (Exception e)
            {
                output = e.Message;
                if (AutomationLog)
                    log.InsertLog("Error", LogCategory,LogService, this.GetType().Name, output);
                return false;
            }

            if (AutomationLog)
                log.InsertLog("Information", LogCategory, LogService, this.GetType().Name, output);

            return true;
        }
    }
}
