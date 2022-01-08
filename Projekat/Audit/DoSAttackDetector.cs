using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Collections.Specialized;

namespace Audit
{
    public class DoSAttackDetector
    {
        // Dos Attack
        // time interval and number of faild requests allowed
        public int allowedNumberOfDosAttacks = Int32.Parse(ConfigurationManager.AppSettings.Get("allowedNumberOfDosAttacks"));
        public int dosInterval = Int32.Parse(ConfigurationManager.AppSettings.Get("dosInterval"));

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
                string message;
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
                                    Manager.Audit.DoSAttackDetected(user);
                                    message = $"[ EVENT LOG ] [ INFO ] DoS attack detected by user \'{user}\'.";                                    
                                    dosTracker[user] = 0;
                                    Console.WriteLine(message);
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
