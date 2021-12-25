using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Client
{
    public class ClientProxy : ChannelFactory<IServiceManagement>, IServiceManagement, IDisposable
    {
        IServiceManagement factory;

        public ClientProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public byte[] Connect(byte[] publicKey, byte[] iv)
        {
            byte[] serverPublicKey = null;
            try
            {
                serverPublicKey = factory.Connect(publicKey, iv);
                Console.WriteLine("Connect allowed!\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }

            return serverPublicKey;
        }

        public void RunService(byte[] ip, byte[] port, byte[] protocol)
        {
            try
            {
                factory.RunService(ip, port, protocol);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
    }
}
