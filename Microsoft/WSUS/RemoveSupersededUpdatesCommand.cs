using System;
using Microsoft.UpdateServices.Administration;
using System.Management.Automation;
using Microsoft.Tools;

namespace Microsoft.WSUS
{
    [Cmdlet(VerbsCommon.Remove, "NASupersededUpdates")]
    public class RemoveSupersededUpdatesCommand : MyWSUS
    {

        [Parameter(Position = 0, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string ComputerName { get; set; }
        [Parameter(Position = 1)]
        public int PortNumber { get; set; }
        
        [Parameter(Position = 2)]
        public int ExclusionPeriod { get; set; } = 0;
        
        [Parameter(Position = 3)]
        public SwitchParameter IsSecure;
        
        [Parameter(Position = 4)]
        public SwitchParameter SkipDecline;
        
        [Parameter(Position = 5)]
        public SwitchParameter DeclineLastLevelOnly;

        //************************ PS Methodes *********************************

        protected override void ProcessRecord()
        {
            RemoveSupersededUpdates(ComputerName, PortNumber, IsSecure, SkipDecline, DeclineLastLevelOnly, ExclusionPeriod);
        }

        //************************ Global Methodes *********************************

        public bool RemoveSupersededUpdates ( string computerName, int portNumber, SwitchParameter isSecure, SwitchParameter skipDecline, SwitchParameter declineLastLevelOnly, int exclusionPeriod = 0)
        {
            LogWriter log = new LogWriter();
            string outputLog = "";
            IUpdateServer updateServer;
            UpdateCollection allUpdates;
            DateTime today = DateTime.Today;

            if (skipDecline && declineLastLevelOnly)
            {                
                outputLog += "Using SkipDecline and DeclineLastLevelOnly switches together is not allowed.\n";
                if (AutomationLog)
                    log.InsertLog("Warning", LogCategory, LogService, this.GetType().Name, outputLog);
                WriteObject(outputLog);
                return false;
            }

            try
            {
                if (isSecure)
                    outputLog += "Connecting to WSUS server UpdateServer on Port " + portNumber.ToString() + "using SSL... ";
                else
                    outputLog += "Connecting to WSUS server UpdateServer on Port " + portNumber.ToString() + " ... ";
                updateServer = AdminProxy.GetUpdateServer(computerName, isSecure, portNumber);
            }
            catch (Exception ex)
            {
                outputLog += "Failed to connect.\n";
                outputLog += "Error: " + ex.Message;
                outputLog += "Please make sure that WSUS Admin Console is installed on this machine\n\n";
                updateServer = null;
                if (AutomationLog)
                    log.InsertLog("Error", LogCategory, LogService, this.GetType().Name, outputLog);
                WriteObject(outputLog);
                return false;
            }

            if (updateServer == null)
            {
                if (AutomationLog)
                    log.InsertLog("Error", LogCategory, LogService, this.GetType().Name, outputLog);
                return false;
            }

            outputLog += "Connected.\n";
            int countAllUpdates = 0;
            int countSupersededAll = 0;
            int countSupersededLastLevel = 0;
            int countSupersededExclusionPeriod = 0;
            int countSupersededLastLevelExclusionPeriod = 0;
            int countDeclined = 0;
            outputLog += "Getting a list of all updates... ";
            try
            {
                allUpdates = updateServer.GetUpdates();
            }
            catch (Exception ex)
            {
                outputLog += "Failed to get updates.\n";
                outputLog += "Error: " + ex.Message;                
                outputLog += "If this operation timed out, please decline the superseded updates from the WSUS Console manually.\n";                
                if (AutomationLog)
                    log.InsertLog("Error", LogCategory, LogService, this.GetType().Name, outputLog);
                WriteObject(outputLog);
                return false;
            }
            outputLog += "Done\n";
            outputLog += "Parsing the list of updates... ";

            foreach (IUpdate update in allUpdates)
            {
                countAllUpdates++;

                if (update.IsDeclined)
                {
                    countDeclined++;
                }

                if (!update.IsDeclined && update.IsSuperseded)
                {
                    countSupersededAll++;
                    if (!update.HasSupersededUpdates)
                    {
                        countSupersededLastLevel++;
                    }
                    if (update.CreationDate <= today.AddDays(-exclusionPeriod))
                    {
                        countSupersededExclusionPeriod++;
                        if (!update.HasSupersededUpdates)
                        {
                            countSupersededLastLevelExclusionPeriod++;
                        }
                    }

                }
            }
            outputLog += "Done.\n";
            outputLog += "List of superseded updates: outSupersededList\n";
            outputLog += "\n";
            outputLog += "Summary:";
            outputLog += "========\n";
            outputLog += "All Updates = " + countAllUpdates.ToString() + "\n";
            outputLog += "Any Except Declined = " + (countAllUpdates - countDeclined) + "\n";
            outputLog += "All Superseded Updates = " + countSupersededAll.ToString() + "\n";
            outputLog += "    Superseded Updates (Intermediate) = " + (countSupersededAll - countSupersededLastLevel).ToString() + "\n";
            outputLog += "    Superseded Updates (Last Level) = " + countSupersededLastLevel.ToString() + "\n";
            outputLog += "    Superseded Updates (Older than ExclusionPeriod days) = " + countSupersededExclusionPeriod.ToString() + "\n";
            outputLog += "    Superseded Updates (Last Level Older than ExclusionPeriod days) = " + countSupersededLastLevelExclusionPeriod.ToString() + "\n";
            outputLog += "\n";

            int i = 0;

            if (!skipDecline)
            {
                outputLog += "SkipDecline flag is set to SkipDecline. Continuing with declining updates\n";
                int updatesDeclined = 0;

                if (declineLastLevelOnly)
                {
                    outputLog += "DeclineLastLevel is set to True. Only declining last level superseded updates.\n";
                    foreach (IUpdate update in allUpdates)
                    {
                        if (!update.IsDeclined && update.IsSuperseded && !update.HasSupersededUpdates)
                        {
                            if (update.CreationDate <= today.AddDays(-exclusionPeriod))
                            {
                                i++;
                                int percentComplete = Convert.ToInt32(((updatesDeclined / countSupersededAll) * 100));
                                try
                                {
                                    update.Decline();
                                    updatesDeclined++;
                                }
                                catch (Exception ex)
                                {
                                    outputLog += "Failed to decline update (update.Id.UpdateId.Guid). Error: " + ex.Message + "\n";
                                    if (AutomationLog)
                                        log.InsertLog("Error", LogCategory, LogService, this.GetType().Name, outputLog);
                                }
                            }
                        }
                    }
                }
                else
                {
                    outputLog += "DeclineLastLevel is set to False. Declining all superseded updates.\n";
                    foreach (IUpdate update in allUpdates)
                    {
                        if (!update.IsDeclined && update.IsSuperseded)
                        {
                            if (update.CreationDate <= today.AddDays(-exclusionPeriod))
                            {
                                i++;
                                int percentComplete = Convert.ToInt32(((updatesDeclined / countSupersededAll) * 100));
                                try
                                {
                                    update.Decline();
                                    updatesDeclined++;
                                }
                                catch (Exception ex)
                                {
                                    outputLog += "Failed to decline update (update.Id.UpdateId.Guid). Error:" + ex.Message + "\n";
                                    if (AutomationLog)
                                        log.InsertLog("Error", LogCategory, LogService, this.GetType().Name, outputLog);
                                }
                            }
                        }
                    }
                }
                outputLog += "Declined updatesDeclined updates.\n";
                if (updatesDeclined != 0)
                    outputLog += "Backed up list of superseded updates to outSupersededListBackup\n";
                else
                    outputLog += "SkipDecline flag is set to SkipDecline. Skipped declining updates\n";
                outputLog += "\n";
                outputLog += "Done\n";
                outputLog += "\n";
            }
            if (AutomationLog)
                log.InsertLog("Information", LogCategory, LogService, this.GetType().Name, outputLog);
            WriteObject(outputLog);
            return true;
        }

    }
}
