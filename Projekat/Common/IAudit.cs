using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Common
{
    [ServiceContract]
    public interface IAudit
    {
        [OperationContract]
        void LogEvent(int auditCode, string username);
    }
}
