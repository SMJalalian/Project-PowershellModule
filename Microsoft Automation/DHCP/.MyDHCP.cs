using System.Management.Automation;

namespace Microsoft.DHCP
{
    public class MyDHCP : MyMicrosoft
    {
        public string LogService { get; private set; } = "DHCP Service";
    }
}
