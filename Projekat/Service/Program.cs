using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;

namespace Service
{
    class Program
    {
        public static List<string> blackListIp = new List<string>();
        public static List<string> blackListPort = new List<string>();
        public static List<string> blacListProtocol = new List<string>();

        static void Main(string[] args)
        {
            ReadBlackListFile();

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

        public static void ReadBlackListFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader("blacklist.txt"))
                {
                    string line = null;
                    while (!String.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        string[] parts = line.Split('=');
                        string key = parts[0];
                        string value = parts[1];
                        switch (key)
                        {
                            case "ip":
                                blackListIp.Add(value);
                                break;
                            case "port":
                                blackListPort.Add(value);
                                break;
                            case "protocol":
                                blacListProtocol.Add(value);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
