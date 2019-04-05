using System;
using System.Management.Automation;
using System.Data.SqlClient;
using Microsoft.Tools;

namespace Microsoft.WSUS
{
    [Cmdlet(VerbsCommon.Optimize, "NAWSUSDatabase")]
    public class OptimizeWSUSDatabaseCommand : MyWSUS
    {
        [Parameter(Position = 0, ValueFromPipeline =true)]
        [ValidateNotNullOrEmpty]
        public string ComputerName { get; set; }
       
        [Parameter(Position = 1, Mandatory =true)]
        [ValidateNotNullOrEmpty]
        public string DBName { get; set; }

        //************************ PS Methodes *********************************

        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            OptimizeWSUSDatabase(ComputerName, DBName);
        }

        //************************ Global Methodes *********************************

        public bool OptimizeWSUSDatabase (string computer, string dBName)
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
                SQLCMD.CommandText = SQLQuery;
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
                Log.InsertLog("Information", LogCategory, LogService, this.GetType().Name, "For WSUS database optimizing, it had been reindexed ... ");

            return true;
        }
    }
}
