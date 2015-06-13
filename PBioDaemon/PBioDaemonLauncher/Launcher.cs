using System;

namespace PBioDaemonLauncher
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			SimulationLauncher launcher;
			Guid idProcess;

			try
			{
				idProcess = new Guid(args[0]);
				Console.WriteLine(idProcess.ToString());
				launcher = new SimulationLauncher(idProcess);
				launcher.Run();
			} 
			catch (Exception e){
				Console.WriteLine ("[PBioDaemonLauncher]"+ e.Message);
				System.Threading.Thread.Sleep(20000);
			}
		}
	}
}
