using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.ServiceModel;
using System.Text;

namespace Service
{
    public class ServiceManagement : IServiceManagement
    {
        public byte[] ClientPublicKey { get; set; }
        public byte[] ClientIV { get; set; }
        public DiffieHellman diffieHellman = new DiffieHellman();
        public Dictionary<string, ServiceHost> hosts = new Dictionary<string, ServiceHost>();


        [PrincipalPermission(SecurityAction.Demand, Role = "ExchangeSessionKey")]
        public byte[] Connect(byte[] publicKey, byte[] iv)
        {
            ClientPublicKey = publicKey;
            ClientIV = iv;

            Console.WriteLine("[ CLIENT CONNECTED ]\n");
            var sessionId = OperationContext.Current.SessionId;
            Console.WriteLine(sessionId);

            return diffieHellman.PublicKey;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public bool RunService(byte[] ip, byte[] port, byte[] protocol)
        {
            string decryptedIp = diffieHellman.Decrypt(ClientPublicKey, ip, ClientIV);
            string decryptedPort = diffieHellman.Decrypt(ClientPublicKey, port, ClientIV);
            string decryptedProtocol = diffieHellman.Decrypt(ClientPublicKey, protocol, ClientIV);

            if (decryptedProtocol.ToLower().Equals("tcp"))
            {
                decryptedProtocol = "net.tcp";
            }
            else
            {
                Data.blackListProtocol.Add(decryptedProtocol);
                ValidInput("protocol=" + decryptedProtocol);
            }

            if (decryptedIp.ToLower().Equals("localhost"))
            {
                decryptedIp = "127.0.0.1";
            }


            if (Blacklisted(decryptedPort, decryptedProtocol)) 
                return false;

            NetTcpBinding binding = new NetTcpBinding();
            string address = $"{decryptedProtocol}://{decryptedIp}:{decryptedPort}/TestService";

            if (hosts.ContainsKey(address))
            {
                Console.WriteLine($"Service already runs on address [{address}]");
                return false;
            }

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(TestService));

            host.AddServiceEndpoint(typeof(ITest), binding, address);

            host.Open();

            hosts.Add(address, host);

            Console.WriteLine("Service run on port " + decryptedPort);

            return true;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Admin")]
        public bool StopService(byte[] ip, byte[] port, byte[] protocol)
        {
            string decryptedIp = diffieHellman.Decrypt(ClientPublicKey, ip, ClientIV);
            string decryptedPort = diffieHellman.Decrypt(ClientPublicKey, port, ClientIV);
            string decryptedProtocol = diffieHellman.Decrypt(ClientPublicKey, protocol, ClientIV);

            if (decryptedProtocol.ToLower().Equals("tcp"))
            {
                decryptedProtocol = "net.tcp";
            }
            else
            {
                return false;
            }

            if (decryptedIp.ToLower().Equals("localhost"))
            {
                decryptedIp = "127.0.0.1";
            }

            string address = $"{decryptedProtocol}://{decryptedIp}:{decryptedPort}/TestService";


            if (hosts.ContainsKey(address))
            {
                hosts[address].Close();
                hosts.Remove(address);
                Console.WriteLine("Service stopped on port " + decryptedPort);
                return true;
            }

            return false;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Admin")]
        public void AddItemToBlackList(string type, string value)
        {
            switch (type)
            {
                case "protocol":
                    Data.blackListProtocol.Add(value);
                    ValidInput("protocol=" + value.ToString());
                    break;
                case "port":
                    Data.blackListPort.Add(value);
                    ValidInput("port=" + value.ToString());
                    break;
            }
        }

        public bool Blacklisted(string port, string protocol)
        {
            if (Data.blackListPort.Contains(port) || Data.blackListProtocol.Contains(protocol))
                return true;

            return false;
        }

        public void ValidInput(string input)
        {
            lock (Data.fileChecksum)
            {
                byte[] tempHash = Data.Checksum();
                bool write = true;
                for (int i = 0; i < Data.fileChecksum.Length; i++)
                {
                    if (Data.fileChecksum[i] != tempHash[i])
                    {
                        write = false;
                    }
                }

                if (write)
                {
                    using (StreamWriter sw = new StreamWriter("blacklist.txt", true))
                    {
                        sw.WriteLine(input);
                    }

                    Data.fileChecksum = Data.Checksum();
                }
            }  
        }
    }
}
