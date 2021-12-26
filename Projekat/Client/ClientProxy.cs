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

        public void AddItemToBlackList(string type, string value)
        {
            try
            {
                factory.AddItemToBlackList(type, value);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
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

        public bool RunService(byte[] ip, byte[] port, byte[] protocol)
        {
            try
            {
                return factory.RunService(ip, port, protocol);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }

        public bool StopService(byte[] ip, byte[] port, byte[] protocol)
        {
            try
            {
                return factory.StopService(ip, port, protocol);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }
    }
}
