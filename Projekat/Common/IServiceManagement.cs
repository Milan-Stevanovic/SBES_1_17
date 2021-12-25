using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Common
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IServiceManagement
    {
        [OperationContract]
        byte[] Connect(byte[] publicKey, byte[] iv);


        [OperationContract]
        void RunService(byte[] ip, byte[] port, byte[] protocol);
    }
}
