using System;
using System.ServiceProcess;

namespace PBioDaemon
{
	public partial class PBioDaemon : ServiceBase
	{
		private System.Diagnostics.EventLog pbioEventLog;

		public PBioDaemon()
		{
			//this.AutoLog = false;

			this.pbioEventLog = new System.Diagnostics.EventLog();
			((System.ComponentModel.ISupportInitialize)(this.pbioEventLog)).BeginInit();

			if (!System.Diagnostics.EventLog.SourceExists("PBioSource"))
			{
				System.Diagnostics.EventLog.CreateEventSource(
					"PBioSource", "PBioDaemonLog");
			}
			// Configuramos el registro de eventos del servicio
			pbioEventLog.Source = "PBioSource";
			pbioEventLog.Log = "PBioDaemonLog";

		}

		protected override void OnStart(string[] args)
		{
			Console.WriteLine("Start");
			pbioEventLog.WriteEntry("[INFO] Starting P-Bio daemon...");
			pbioEventLog.WriteEntry("[INFO] Starting threads...");
			PBioDaemonListener runnerDaemonListener = new PBioDaemonListener();
			runnerDaemonListener.Run();
		}

		protected override void OnStop()
		{
			pbioEventLog.WriteEntry("[INFO] Stopping P-Bio daemon...");
		}

		public static void Main()
		{
			System.ServiceProcess.ServiceBase.Run(new PBioDaemon());
		}
	}
}

