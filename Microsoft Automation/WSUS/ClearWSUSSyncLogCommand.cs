using System;
using System.Management.Automation;
using System.Data.SqlClient;
using Microsoft.Tools;

namespace Microsoft.WSUS
{
    [Cmdlet(VerbsCommon.Clear, "NAWSUSSyncLog")]
    public class ClearWSUSSyncLogCommand : MyWSUS
    {
        [Parameter(Position = 0, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string ComputerName { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string DBName { get; set; }

        [Parameter(Position = 1)]
        public int Days { get; set; } = 20;


        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            ClaerWSUSSyncLog(ComputerName, DBName, Days);
        }

        //************************ Global Methodes *********************************

        public bool ClaerWSUSSyncLog(string computer, string dBName, int days)
        {
            LogWriter Log = new LogWriter();
            try
            {
                string SQLQuery = Properties.Resources.WSUS_DatabaseRe_Index;

                SqlConnection Connection = new SqlConnection();
                SqlCommand SQLCMD = new SqlCommand();
                SqlDataReader Reader;
                Connection.ConnectionString = "Data Source=" + computer + ";" +
                                              "Initial Catalog=" + dBName + ";" +
                                              "Integrated Security=SSPI;";
                SQLCMD.CommandText = "DELETE FROM tbEventInstance WHERE EventNamespaceID = 2 AND EVENTID IN(381, 382, 384, 386, 387, 389)" +
                                     " AND TimeAtServer < '" + (DateTime.Now.AddDays(-days)).ToShortDateString() + " 00:00:00'";
                SQLCMD.Connection = Connection;
                SQLCMD.CommandTimeout = 300;
                Connection.Open();
                WriteVerbose("Connection is opened successfully ...");
                Reader = SQLCMD.ExecuteReader();
                WriteVerbose("Command is executed successfully ...");
                Connection.Close();
                WriteVerbose("Connection is closed successfully ...");
            }
            catch (Exception e)
            {
                if (AutomationLog)
                    Log.InsertLog("Error", LogCategory, LogService, this.GetType().Name, e.Message);
                return false;
            }

            if (AutomationLog)
                Log.InsertLog("Information", LogCategory, LogService, this.GetType().Name, "For WSUS database optimizing, logs which are older than " + days + " day(s) are removed from database ... ");

            return true;
        }
    }
}
