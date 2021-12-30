using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Service
{
    public class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data();
            data.ReadBlackListFile();
            data.CheckBlacklistTxt();

            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/ServiceManagement";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(ServiceManagement));

            host.AddServiceEndpoint(typeof(IServiceManagement), binding, address);

            host.Open();

            Console.WriteLine("Service process run by user: " + WindowsIdentity.GetCurrent().Name);
            Console.WriteLine("Service up and running...");

            Console.ReadLine();
        }
    }
}
