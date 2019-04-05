using System;
using System.Data.SqlClient;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.Tools
{
    class LogWriter : MyTools
    {
        //**************************************** Constructure
        #region Constructure
        public LogWriter()
        {
            string[] information;
            AssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = AssemblyFolder + "\\Config.inf";

            if (File.Exists(configFile))
            {
                information = File.ReadAllLines(configFile);
                foreach (string line in information)
                {
                    string[] temp = line.Split('=');
                    temp[0] = temp[0].Trim();
                    temp[1] = temp[1].Trim();
                    if (temp[0] == "SQLServerName") SQLServerName = temp[1];
                    else if (temp[0] == "SQLServerInstance") SQLServerInstance = temp[1];
                    else if (temp[0] == "SQLDB") SQLDB = temp[1];
                }
            }
            else
            {
                SQLServerName = null;
                SQLServerInstance = null;
                SQLDB = null;
            }
        }
        #endregion
        //**************************************** Methods
        #region Methods
        public void InsertLog(string severity, string category, string service, string cmdlet, string description = "")
        {
            SqlConnection connection = new SqlConnection();
            
            //Connecting to database ...
            try
            {
                connection.ConnectionString =
                            "Data Source=" + SQLServerName + @"\" + SQLServerInstance + ";" +
                            "Initial Catalog=" + SQLDB + " ;" +
                            "Integrated Security=SSPI;";
                connection.Open();
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on connecting to database ... !!!", ErrorCategory.ConnectionError, connection));
                throw;
            }

            //Insert log to database ...
            SqlCommand cmd = new SqlCommand("INSERT INTO G_LogInfo (Severity, Category, Service, Cmdlet, DateTime, Description) VALUES (@Severity, @Category, @Service, @Cmdlet, @DateTime, @Description)", connection);
            try
            {                
                cmd.Parameters.AddWithValue("@Severity", severity);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.Parameters.AddWithValue("@Service", service);
                cmd.Parameters.AddWithValue("@Cmdlet", cmdlet);
                cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Error on insert log to database ... !!!", ErrorCategory.WriteError, cmd));
                throw;
            }
        }
        #endregion
    }
}
