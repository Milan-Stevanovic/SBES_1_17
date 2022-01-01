using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audit
{
    public class AuditImplementation : IAudit
    {
        public void LogEvent(int code, string username)
        {
            string message = null;
            switch(code)
            {
                case 0:
                    try
                    {
                        Manager.Audit.ConnectSuccess(username);
                        message = $"[ EVENT LOG ] [ INFO ] User \'{username}\' connected.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 1:
                    try
                    {
                        Manager.Audit.RunServiceSuccess(username);
                        message = $"[ EVENT LOG ] [ SUCCESS ] User \'{username}\' successfully run service.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 2:
                    try
                    {
                        Manager.Audit.RunServiceFailure(username);
                        message = $"[ EVENT LOG ] [ FAILURE ] User \'{username}\' failed to run service.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 3:
                    try
                    {
                        Manager.Audit.DoSAttackDetected(username);
                        message = $"[ EVENT LOG ] [ INFO ] DoS attack detected by user \'{username}\'.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 4:
                    try
                    {
                        Manager.Audit.BlacklistFileChanged();
                        message = $"[ EVENT LOG ] [ INFO ] \'blacklist.txt\' file corrupted.";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
            }
            Console.WriteLine(message);
        }
    }
}
