using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/ServiceManagement";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            Console.WriteLine("Client process run by user: " + WindowsIdentity.GetCurrent().Name);

            EndpointAddress endpointAddress = new EndpointAddress(new Uri(address), EndpointIdentity.CreateUpnIdentity("wcfService"));

            using (ClientProxy proxy = new ClientProxy(binding, endpointAddress))
            {
                proxy.Connect();

                // TODO: put login in inf. while loop
                Console.Write("Enter IP      : ");
                string ip = Console.ReadLine();
                Console.Write("Enter PORT    : ");
                string port = Console.ReadLine();
                Console.Write("Enter PROTOCOL: ");
                string protocol = Console.ReadLine();

                proxy.RunService(ip.Trim(), port.Trim(), protocol.Trim());

                // TODO: Separate TestConnection to individual methon
                // Test method from test service
                string testAddress = $"net.tcp://{ip}:{port}/TestService";
                EndpointAddress testEndpointAddress = new EndpointAddress(new Uri(testAddress), EndpointIdentity.CreateUpnIdentity("wcfTestService"));
                ChannelFactory<ITest> testFactory = new ChannelFactory<ITest>(binding);
                ITest testProxy = testFactory.CreateChannel(testEndpointAddress);

                testProxy.TestConnection();

                Console.WriteLine("[ CLIENT ] Service run successfully!\n");
            }

            Console.ReadLine();
        }
    }
}
