using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Manager
{
    public enum AuditEventTypes
    {
        ConnectSuccess = 0,
        RunServiceSuccess = 1,
        RunServiceFailure = 2,
        DoSAttackDetected = 3,
        BlacklistFileChanged = 4
    }

    public class AuditEvents
    {
        private static ResourceManager resourceManager = null;
        private static object resourceLock = new object();

        private static ResourceManager ResourceMgr
        {
            get
            {
                lock (resourceLock)
                {
                    if (resourceManager == null)
                    {
                        resourceManager = new ResourceManager
                            (typeof(AuditEventFile).ToString(),
                            Assembly.GetExecutingAssembly());
                    }
                    return resourceManager;
                }
            }
        }

        public static string ConnectSuccess
        {
            get { return ResourceMgr.GetString(AuditEventTypes.ConnectSuccess.ToString()); }
        }

        public static string RunServiceSuccess
        {
            get { return ResourceMgr.GetString(AuditEventTypes.RunServiceSuccess.ToString()); }
        }

        public static string RunServiceFailure
        {
            get { return ResourceMgr.GetString(AuditEventTypes.RunServiceFailure.ToString()); }
        }

        public static string DoSAttackDetected
        {
            get { return ResourceMgr.GetString(AuditEventTypes.DoSAttackDetected.ToString()); }
        }

        public static string BlacklistFileChanged
        {
            get { return ResourceMgr.GetString(AuditEventTypes.BlacklistFileChanged.ToString()); }
        }
    }
}
