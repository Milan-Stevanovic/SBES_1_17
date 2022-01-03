using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Manager
{
    public class Audit : IDisposable
    {
		private static EventLog customLog = null;
		const string SourceName = "Audit";
		const string LogName = "Application";

		static Audit()
		{
			try
			{
				if (!EventLog.SourceExists(SourceName))
				{
					EventLog.CreateEventSource(SourceName, LogName);
				}
				customLog = new EventLog(LogName, Environment.MachineName, SourceName);
			}
			catch (Exception e)
			{
				customLog = null;
				Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
			}
		}

		public static void ConnectSuccess(string username)
		{
			if (customLog != null)
			{
				string ConnectSuccess = AuditEvents.ConnectSuccess;
				string message = String.Format(ConnectSuccess, username);
				customLog.WriteEntry(message);
			}
			else
			{
				throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.", (int)AuditEventTypes.ConnectSuccess));
			}
		}

		public static void RunServiceSuccess(string username)
		{
			if (customLog != null)
			{
				string RunServiceSuccess = AuditEvents.RunServiceSuccess;
				string message = String.Format(RunServiceSuccess, username);
				customLog.WriteEntry(message);
			}
			else
			{
				throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.", (int)AuditEventTypes.RunServiceSuccess));
			}
		}

		public static void RunServiceFailure(string username)
        {
			if(customLog != null)
            {
				string RunServiceFailure = AuditEvents.RunServiceFailure;
				string message = String.Format(RunServiceFailure, username);
				customLog.WriteEntry(message);
            }
			else
            {
				throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.", (int)AuditEventTypes.RunServiceFailure));
			}
        }

		public static void DoSAttackDetected(string username)
		{
			if (customLog != null)
			{
				string DoSAttackDetected = AuditEvents.DoSAttackDetected;
				string message = String.Format(DoSAttackDetected, username);
				customLog.WriteEntry(message);
			}
			else
			{
				throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.", (int)AuditEventTypes.DoSAttackDetected));
			}
		}

		public static void BlacklistFileChanged()
		{
			if (customLog != null)
			{
				string message = AuditEvents.BlacklistFileChanged;
				customLog.WriteEntry(message);
			}
			else
			{
				throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.", (int)AuditEventTypes.BlacklistFileChanged));
			}
		}

		public void Dispose()
		{
			if (customLog != null)
			{
				customLog.Dispose();
				customLog = null;
			}
		}
	}
}
