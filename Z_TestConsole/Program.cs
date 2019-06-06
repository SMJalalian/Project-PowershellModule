using System;

namespace Z_TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.File.Copy(@"C:\Workspaces\DevOps\SMJalalian\Network Automation\Z_Manifest Module\Network Automation.psd1",
                                @"C:\Program Files\PowerShell\6\Modules\Network Automation\Network Automation.psd1", true);
            Console.WriteLine("Hello World!");
            //Console.ReadLine();
        }
    }
}
