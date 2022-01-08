using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;

namespace Service
{
    public class Program
    {
        public static IAudit auditProxy = null;
        public static bool flagShutdown = false;
        static void Main(string[] args)
        {
            Data data = new Data();
            data.ReadBlackListFile();
            data.CheckBlacklistTxt();
            //DoSAttackDetector detector = new DoSAttackDetector();
            //detector.DoSTrackerDetection();
            auditProxy = ConnectAudit();

            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/ServiceManagement";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(ServiceManagement));

            host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            host.AddServiceEndpoint(typeof(IServiceManagement), binding, address);

            host.Open();


            Console.WriteLine("Service process run by user: " + WindowsIdentity.GetCurrent().Name);
            Console.WriteLine("Service up and running...");

            while (true)
            {
                // if file corrupted, shutdown server
                if (flagShutdown)
                { 
                    host.Close();
                    Console.WriteLine("Service shutdown...");
                    break;
                }

                Thread.Sleep(1000);
            }
            Console.ReadLine();
        }

        static WCFAuditClient ConnectAudit()
        {
            /// Define the expected service certificate. It is required to establish cmmunication using certificates.
            string srvCertCN = "wcfaudit";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:11000/Audit"),
                                      new X509CertificateEndpointIdentity(srvCert));

            return new WCFAuditClient(binding, address);
        }
    }
}
