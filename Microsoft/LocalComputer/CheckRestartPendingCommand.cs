using System;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;

namespace Microsoft.LocalComputer
{
    //[Cmdlet(VerbsCommon.Get, "RestartPending")]
    public class CheckRestartPendingCommand : MyLocalComputer
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ComputerName { get; set; }

        [Parameter(Position = 1)]
        public string ErrorLog { get; set; }

        //************************ PS Methodes *********************************
        protected override void BeginProcessing()
        {
            if (ComputerName == null)
                ComputerName = Environment.MachineName;
        }

        protected override void ProcessRecord()
        {
            CheckRestartPending(ComputerName);
        }

        //************************ Global Methodes *********************************

        public bool CheckRestartPending(string computer)
        {

            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager");
                var key1 = Registry.LocalMachine.GetValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\RunLevelValidate");
                var x2 = Registry.LocalMachine.GetSubKeyNames();
                Console.Read();

            }
            catch
            {

            }

            return true;
        }
    }
}
