using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Service
{
    public class Program
    {
        public static List<string> blackListIp = new List<string>();
        public static List<string> blackListPort = new List<string>();
        public static List<string> blackListProtocol = new List<string>();
        public static byte[] fileChecksum = null;

        static void Main(string[] args)
        {
            ReadBlackListFile();
            fileChecksum = Checksum();

            CheckBlacklistTxt();

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
                                blackListProtocol.Add(value);
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

        public static byte[] Checksum()
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead("blacklist.txt"))
                    {
                        return md5.ComputeHash(stream);
                    }
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public static void CheckBlacklistTxt()
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    lock (fileChecksum)
                    {
                        byte[] help = Checksum();
                        for (int i = 0; i < Program.fileChecksum.Length; i++)
                        {
                            if (Program.fileChecksum[i] != help[i])
                            {
                                Console.WriteLine("Unauthorised blacklist file corrupted, Admin reaction REQUIRED!!!");
                                break;
                            }
                        }
                    }
                }
            });

            thread.Start();
        }
    }
}
