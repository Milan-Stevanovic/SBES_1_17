using Common;
using System;
using System.Collections.Generic;
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

        public void RunService(byte[] ip, byte[] port, byte[] protocol)
        {
            string decryptedIp = diffieHellman.Decrypt(ClientPublicKey, ip, ClientIV);
            string decryptedPort = diffieHellman.Decrypt(ClientPublicKey, port, ClientIV);
            string decryptedProtocol = diffieHellman.Decrypt(ClientPublicKey, protocol, ClientIV);

            if(decryptedProtocol.ToLower().Equals("tcp"))
            {
                decryptedProtocol = "net.tcp";
            }
            else
            {
                return;
            }

            // TODO: Check BLACKLIST Configuration
            // TODO: Return value must be BOOL

            NetTcpBinding binding = new NetTcpBinding();
            string address = $"{decryptedProtocol}://{decryptedIp}:{decryptedPort}/TestService";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(TestService));

            host.AddServiceEndpoint(typeof(ITest), binding, address);

            host.Open();

            Console.WriteLine("Service run on port " + decryptedPort);
        }
    }
}
