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
        List<string> blackListIp = new List<string>();
        List<string> blackListPort = new List<string>();
        List<string> blacListProtocol = new List<string>();
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
            // Check if user has 'ExchangeSessionKey' role
            // TODO: Return value must be BOOL
            // Exchange Session Keys ??

            return diffieHellman.PublicKey;
        }

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
                return false;
            }

            if (decryptedIp.ToLower().Equals("localhost"))
            {
                decryptedIp = "127.0.0.1";
            }

            // TODO: Check BLACKLIST Configuration
            // TODO: Return value must be BOOL

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


        public void AddItemToBlackList(string type, string value)
        {
            using (StreamWriter sw = new StreamWriter("blacklist.txt", true))
            {
                switch (type)
                {
                    case "ip":
                        blackListIp.Add(value);
                        sw.WriteLine("ip=" + value.ToString());
                        break;
                    case "protocol":
                        blacListProtocol.Add(value);
                        sw.WriteLine("protocol=" + value.ToString());
                        break;
                    case "port":
                        blackListPort.Add(value);
                        sw.WriteLine("port=" + value.ToString());
                        break;
                }
            }
        }
    }
}
