using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Service
{
    public class Data
    {
        public static List<string> blackListPort = null;
        public static List<string> blackListProtocol = null;
        public static byte[] fileChecksum = null;

        public Data()
        {
            blackListPort = new List<string>();
            blackListProtocol = new List<string>();
            fileChecksum = Checksum();
        }

        public void ReadBlackListFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader("blacklist.txt"))
                {
                    string line = null;
                    while (!String.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        string[] parts = line.Split('=');
                        string key = parts[0];
                        string value = parts[1];
                        switch (key)
                        {
                            case "port":
                                blackListPort.Add(value);
                                break;
                            case "protocol":
                                blackListProtocol.Add(value);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public static byte[] Checksum()
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead("blacklist.txt"))
                    {
                        return md5.ComputeHash(stream);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public void CheckBlacklistTxt()
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    lock (fileChecksum)
                    {
                        byte[] help = Checksum();
                        for (int i = 0; i < fileChecksum.Length; i++)
                        {
                            if (fileChecksum[i] != help[i])
                            {
                                Console.WriteLine("Unauthorised blacklist file corrupted, Admin reaction REQUIRED!!!");
                                break;
                            }
                        }
                    }
                }
            });

            thread.Start();
        }
    }
}
