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
            bool connected = false;
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/ServiceManagement";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            Console.WriteLine("Client process run by user: " + WindowsIdentity.GetCurrent().Name);

            EndpointAddress endpointAddress = new EndpointAddress(new Uri(address), EndpointIdentity.CreateUpnIdentity("wcfService"));

            using (ClientProxy proxy = new ClientProxy(binding, endpointAddress))
            {

                DiffieHellman clientDiffieHellman = new DiffieHellman();
                byte[] serverPublicKey = null;
                while (true)
                {

                    switch (Menu())
                    {
                        case 1:
                            serverPublicKey = proxy.Connect(clientDiffieHellman.PublicKey, clientDiffieHellman.IV);
                            connected = true;
                            break;
                        case 2:
                            if (!connected)
                            {
                                Console.WriteLine("Please connect first!");
                                break;
                            }
                            Console.Write("Enter IP      : ");
                            string ip = Console.ReadLine().Trim();
                            Console.Write("Enter PORT    : ");
                            string port = Console.ReadLine().Trim();
                            Console.Write("Enter PROTOCOL: ");
                            string protocol = Console.ReadLine().Trim();

                            bool validRun = proxy.RunService(clientDiffieHellman.Encrypt(serverPublicKey, ip),
                                                clientDiffieHellman.Encrypt(serverPublicKey, port),
                                                clientDiffieHellman.Encrypt(serverPublicKey, protocol));
                            if (validRun)
                            {
                                string testAddress = $"net.tcp://{ip}:{port}/TestService";
                                EndpointAddress testEndpointAddress = new EndpointAddress(new Uri(testAddress), EndpointIdentity.CreateUpnIdentity("wcfTestService"));
                                ChannelFactory<ITest> testFactory = new ChannelFactory<ITest>(binding);
                                ITest testProxy = testFactory.CreateChannel(testEndpointAddress);

                                testProxy.TestConnection();

                                Console.WriteLine("[ CLIENT ] Service run successfully!\n");
                            }
                            else
                            {
                                Console.WriteLine("[ CLIENT ] Service falied to run!\n");
                            }
                            break;
                        case 3:
                            if (!connected)
                            {
                                Console.WriteLine("Please connect first!");
                                break;
                            }
                            Console.Write("Enter IP      : ");
                            string stopIp = Console.ReadLine().Trim();
                            Console.Write("Enter PORT    : ");
                            string stopPort = Console.ReadLine().Trim();
                            Console.Write("Enter PROTOCOL: ");
                            string stopProtocol = Console.ReadLine().Trim();

                            bool validStop = proxy.StopService(clientDiffieHellman.Encrypt(serverPublicKey, stopIp),
                                             clientDiffieHellman.Encrypt(serverPublicKey, stopPort),
                                             clientDiffieHellman.Encrypt(serverPublicKey, stopProtocol));
                            if (validStop)
                            {
                                Console.WriteLine("[ CLIENT ] Service stopped successfully!\n");
                            }
                            else
                            {
                                Console.WriteLine("[ CLIENT ] Service falied to stop!\n");
                            }
                            break;
                        case 4:
                            if (!connected)
                            {
                                Console.WriteLine("Please connect first!");
                                break;
                            }
                            Console.Write("Port to ban:");
                            string portBan = Console.ReadLine().Trim();
                            proxy.AddItemToBlackList("port", portBan);
                            break;
                        case 5:
                            if (!connected)
                            {
                                Console.WriteLine("Please connect first!");
                                break;
                            }
                            Console.Write("Protocol to ban:");
                            string protocolBan = Console.ReadLine().Trim();
                            proxy.AddItemToBlackList("protocol", protocolBan);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static int Menu()
        {
            int option = -1;
            do
            {
                Console.WriteLine("============ MENU ============");
                Console.WriteLine("[ 1 ] Connect");
                Console.WriteLine("[ 2 ] Run service");
                Console.WriteLine("[ 3 ] Stop service");
                Console.WriteLine("[ 4 ] Add port to blacklist");
                Console.WriteLine("[ 5 ] Add protocol to blacklist");
                Console.WriteLine("==============================");

                Console.Write("Choose option: ");
                option = Int32.Parse(Console.ReadLine());

            } while (option < 1 || option > 5);



            return option;
        }
    }
}
