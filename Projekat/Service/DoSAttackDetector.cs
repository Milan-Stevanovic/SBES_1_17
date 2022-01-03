using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Service
{
    public class DoSAttackDetector
    {
        // Dos Attack
        // time interval and number of faild requests allowed
        public int allowedNumberOfDosAttacks = 5;
        public int dosInterval = 10;
        public static Dictionary<string, int> dosTracker;

        public DoSAttackDetector() 
        {
            dosTracker = new Dictionary<string, int>();
        }

        // Dos attack alarm
        public void DoSTrackerDetection()
        {
            var thread = new Thread(() =>
            {
                string user;
                while (true)
                {
                    for (int interval = 1; interval <= dosInterval; interval++)
                    { 
                        lock (dosTracker)
                        {
                            for(int i = 0; i < dosTracker.Count; i++)
                            {
                                user = (dosTracker.ElementAt(i)).Key;

                                if ((dosTracker.ElementAt(i)).Value > allowedNumberOfDosAttacks)
                                {
                                    Program.auditProxy.LogEvent((int)AuditEventTypes.DoSAttackDetected, user);
                                    dosTracker[user] = 0;
                                    Console.WriteLine($" [DoS Attack Detected] by user: '{0}'", user);
                                }

                                // only on last piace of interval reset value to zero
                                if (interval == dosInterval)
                                    dosTracker[user] = 0;
                            }
                        }
                        Thread.Sleep(1000);
                    }
                }
            });

            thread.Start();
        }
    }
}
