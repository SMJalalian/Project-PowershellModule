using System;
using System.Management.Automation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.ActiveDirectory;

namespace Microsoft.LocalComputer
{
    [Cmdlet(VerbsCommon.Get, "NAEventLog")]
    public class GetEventLogCommand : MyLocalComputer
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ComputerName { get; set; }

        [ValidateSet("GroupChanges", "ClearSecurityLog", "UserChanges", IgnoreCase = true)]
        [Parameter(Position = 1,Mandatory =true)]
        public string LogType { get; set; }

        [Parameter(Position = 2)]
        public int Period { get; set; } = 5;
        
        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;                  
        }

        protected override void ProcessRecord()
        {
            switch (LogType)
            {
                case "ClearSecurityLog":
                    GetClearSecurityLog(ComputerName);
                    break;
                case "GroupChanges":
                    GetGroupChangeEventLog(ComputerName, Period);
                    break;
                case "UserChanges":
                    GetUserChangeSecurityLog(ComputerName,Period);
                    break;
            }
        }

        //************************ Global Methodes *********************************

        public List<PSEventLog> GetClearSecurityLog(string computer)
        {
            List<PSEventLog> result = new List<PSEventLog>();
            EventLogEntryCollection allLogs;
            try
            {
                EventLog targetLog = new EventLog("Security", computer);
                allLogs = targetLog.Entries;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on connecting to target machine ... ", ErrorCategory.OpenError, computer));
                throw;
            }

            foreach (EventLogEntry log in allLogs)
            {
                if (log.InstanceId == 1102)
                {
                    MyActiveDirectory AD = new MyActiveDirectory();
                    PSEventLog temp = new PSEventLog();
                    List<string[]> logOutput = temp.SplitLogMessage(log);
                    temp.Host = computer;
                    try
                    {
                        SecurityIdentifier sid = new SecurityIdentifier(logOutput[2][1].ToString());
                        NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
                        temp.Subject = string.Format(ntAccount.ToString());
                    }
                    catch (Exception)
                    {
                        temp.Subject = logOutput[2][1].ToString();
                    }
                    try
                    {
                        temp.SubjectFullName = AD.GetUserFullName(temp.Subject.Substring(temp.Subject.IndexOf("\\") + 1));
                    }
                    catch (Exception)
                    {
                        temp.SubjectFullName = "";
                    }
                    temp.Type = "Security";
                    temp.Date = log.TimeGenerated;
                    temp.EventID = log.InstanceId;
                    temp.Description = "Security logs were removed by particular subject ...";
                    WriteObject(temp);
                    result.Add(temp);
                    break;
                }
            }
            return result;
        }
        public List<PSGroupEventLog> GetGroupChangeEventLog (string computer, int period = 5)
        {            
            List<PSGroupEventLog> result = new List<PSGroupEventLog>();
            EventLogEntryCollection allLogs;
            try
            {
                EventLog targetLog = new EventLog("Security", computer);
                allLogs = targetLog.Entries;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on connecting to target machine ... ", ErrorCategory.OpenError, computer));
                throw;
            }

            foreach (EventLogEntry log in allLogs)
            {
                if ((log.InstanceId == 4732 || log.InstanceId == 4733) && (log.TimeGenerated >= (DateTime.Now).AddDays(-period)))
                {
                    MyActiveDirectory AD = new MyActiveDirectory();
                    PSGroupEventLog temp = new PSGroupEventLog();
                    List<string[]> logOutput = temp.SplitLogMessage(log);
                    temp.Host = computer;
                    temp.Subject = logOutput[5][1].ToString() + @"\" + logOutput[4][1].ToString();
                    try
                    {
                        temp.SubjectFullName = AD.GetUserFullName(logOutput[4][1].ToString());
                    }
                    catch (Exception)
                    {
                        temp.SubjectFullName = "";
                    }
                    try
                    {
                        SecurityIdentifier sid = new SecurityIdentifier(logOutput[9][1].ToString());
                        NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
                        temp.Member = string.Format(ntAccount.ToString());
                    }
                    catch (Exception)
                    {
                        temp.Member = logOutput[9][1].ToString();
                    }
                    temp.Type = "Security";
                    temp.Group = logOutput[14][1].ToString();
                    temp.Date = log.TimeGenerated;
                    temp.EventID = log.InstanceId;                    
                    if (log.InstanceId == 4732)
                        temp.Description = "Member was added to the particular group ...";
                    else
                        temp.Description = "Member was removed from the particular group ...";

                    WriteObject(temp);
                    result.Add(temp);
                }
            }
            return result;      
        }     
        public List<PSUserEventLog> GetUserChangeSecurityLog(string computer, int period = 5)
        {
            List<PSUserEventLog> result = new List<PSUserEventLog>();
            EventLogEntryCollection allLogs;
            try
            {
                EventLog targetLog = new EventLog("Security", computer);
                allLogs = targetLog.Entries;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on connecting to target machine ... ", ErrorCategory.OpenError, computer));
                throw;
            }

            foreach (EventLogEntry log in allLogs)
            {
                if ((log.InstanceId == 4720 || log.InstanceId == 4726 || log.InstanceId == 4738) && (log.TimeGenerated >= (DateTime.Now).AddDays(-period)))
                {
                    MyActiveDirectory AD = new MyActiveDirectory();
                    PSUserEventLog temp = new PSUserEventLog();
                    List<string[]> logOutput = temp.SplitLogMessage(log);
                    temp.Host = computer;
                    temp.User = logOutput[10][1].ToString();
                    try
                    {
                        SecurityIdentifier sid = new SecurityIdentifier(logOutput[3][1].ToString());
                        NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
                        temp.Subject = string.Format(ntAccount.ToString());
                    }
                    catch (Exception)
                    {
                        temp.Subject = logOutput[3][1].ToString();
                    }
                    try
                    {
                        temp.SubjectFullName = AD.GetUserFullName(temp.Subject.Substring(temp.Subject.IndexOf("\\") + 1));
                    }
                    catch (Exception)
                    {
                        temp.SubjectFullName = "";
                    }
                    temp.Type = "Security";
                    temp.Date = log.TimeGenerated;
                    temp.EventID = log.InstanceId;
                    if (log.InstanceId == 4720)
                    {
                        temp.Action = "Create";
                        temp.Description = "The user was created on particular computer ...";
                    }
                    else if (log.InstanceId == 4726)
                    {
                        temp.Action = "Delete";
                        temp.Description = "The user was deleted on particular computer ...";
                    }
                    else
                    {
                        temp.Action = "Change";
                        temp.Description = "The user was changed on particular computer ...";
                    }
                    WriteObject(temp);
                    result.Add(temp);
                }
            }
            return result;
        }
    }
}
