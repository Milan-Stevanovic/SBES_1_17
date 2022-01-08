using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;

namespace Audit
{
    class Program
    {
        static void Main(string[] args)
        {
			// DoS Attack detector for audit component
			DoSAttackDetector detector = new DoSAttackDetector();
			detector.DoSTrackerDetection();

			/// srvCertCN.SubjectName should be set to the service's username. .NET WindowsIdentity class provides information about Windows user running the given process
			string srvCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

			NetTcpBinding binding = new NetTcpBinding();
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

			string address = "net.tcp://localhost:11000/Audit";
			ServiceHost host = new ServiceHost(typeof(AuditImplementation));
			host.AddServiceEndpoint(typeof(IAudit), binding, address);

			host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
			host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

			///Custom validation mode enables creation of a custom validator - CustomCertificateValidator
			host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
			host.Credentials.ClientCertificate.Authentication.CustomCertificateValidator = new ServiceCertValidator();

			///If CA doesn't have a CRL associated, WCF blocks every client because it cannot be validated
			host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

			///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
			host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

			ServiceSecurityAuditBehavior newAudit = new ServiceSecurityAuditBehavior();
			newAudit.AuditLogLocation = AuditLogLocation.Application;
			newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;

			host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
			host.Description.Behaviors.Add(newAudit);

			try
			{
				host.Open();
				Console.WriteLine("Audit process run by user: " + WindowsIdentity.GetCurrent().Name);
				Console.WriteLine("Press <enter> to stop ...");
				Console.ReadLine();
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] {0}", e.Message);
				Console.WriteLine("[StackTrace] {0}", e.StackTrace);
			}
			finally
			{
				host.Close();
			}
		}
    }
}
