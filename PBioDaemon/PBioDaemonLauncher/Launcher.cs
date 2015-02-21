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
				launcher = new SimulationLauncher(idProcess);
				launcher.Run();
			} 
			catch (Exception e){
				Console.WriteLine ("[SimulationLauncher]"+ e.Message);
				System.Threading.Thread.Sleep(20000);
			}
		}
	}
}
