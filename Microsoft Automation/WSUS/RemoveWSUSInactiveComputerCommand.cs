using System;
using Microsoft.UpdateServices.Administration;
using System.Management.Automation;
using Microsoft.Tools;


namespace Microsoft.WSUS
{
    [Cmdlet(VerbsCommon.Remove, "NAWSUSInactiveComputer")]
    public class RemoveWSUSInactiveComputerCommand : MyWSUS
    {
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string ComputerName { get; set; }

        [Parameter(Position = 1)]
        public int PortNumber { get; set; } = 8530;

        [Parameter(Position = 2)]
        public bool IsSecure { get; set; } = false;

        [Parameter(Position = 3)]
        public int InactiveDays { get; set; } = 7;

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
            RemoveWSUSInactiveComputers(ComputerName, PortNumber, IsSecure, InactiveDays);
        }

        //************************ Global Methodes *********************************

        public bool RemoveWSUSInactiveComputers(string computer, int portNumber, bool isSecure, int inactiveDays)
        {
            LogWriter log = new LogWriter();
            int i = 0;
            try
            {
                IUpdateServer wsus = AdminProxy.GetUpdateServer(computer, isSecure, portNumber);
                ComputerTargetCollection allComputers = wsus.GetComputerTargets();                
                foreach (IComputerTarget computerTarget in allComputers)
                {
                    if (computerTarget.LastReportedStatusTime < DateTime.Now.AddDays(-inactiveDays))
                    {
                        WriteVerbose("Computer " + computerTarget.FullDomainName + " is deleted ...");
                        computerTarget.Delete();
                        i++;
                    }
                }
                WriteVerbose(i.ToString() + " number(s) of computer is deleted by cleanup task ");
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on connecting to WSUS server ... !!!", ErrorCategory.OpenError,false));
                if (AutomationLog)
                    log.InsertLog("Error", LogCategory, LogService, this.GetType().Name, "An error had beed occured in wsus computers cleanup task ..");
                return false;
            }

            if (AutomationLog)
                log.InsertLog("Information", LogCategory, LogService, this.GetType().Name, i.ToString() + " number(s) of computer is deleted by network automation task.");

            return true;
        }
    }
}
