using System;
using System.Xml.Linq;
using PBioDaemonLibrary;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace PBioDaemonLauncher
{
	public class SimulationLauncher
	{
		private const String ResultsFileTag = "<Results>";
		private const String ErrorFileTag = "<Error>";
		private Simulacion simulacion;
		private LauncherConfiguration config;
		private PBioDaemonDB db;
		private ParametersLoader parametersLoader;
		private Guid idProcess;

		public SimulationLauncher(Guid idProcess)
		{
			try
			{
				this.config = new LauncherConfiguration();
				this.db = new PBioDaemonDB(config.CONNECTION_STRING);
				this.simulacion = db.GetSimulationById(idProcess);
				this.parametersLoader = new ParametersLoader(simulacion.ParametrosClasificacion, simulacion.ParametrosSeleccion, simulacion.IdSimulacion);
				this.parametersLoader.Merge();
				this.parametersLoader.SetData(simulacion.Datos);
				this.idProcess = idProcess;
			}
			catch (Exception e)
			{
				throw e;
			}
		}
			
		public void Run()
		{				
			// Establecemos a "Running" el proceso.
			db.UpdateStateProcess("Run",simulacion.IdSimulacion);	

			// Lanzamiento de la ejecución		
			System.Diagnostics.ProcessStartInfo psf = new System.Diagnostics.ProcessStartInfo();
			System.Diagnostics.Process proc = new System.Diagnostics.Process();


			// RUN ON LINUX
			psf.FileName = config.PATH_MATLAB;
			psf.Arguments = "-nodisplay -r \"bateriaGAmain('" + config.FOLDER_INI + "','LDA','" + simulacion.IdSimulacion.ToString() + "','" + simulacion.IdSimulacion.ToString() + "')\";quit";

			// RUN ON WINDOWS
			//psf.FileName = "matlab";
			// bateriaGAmain(pathParametersInitials, metodoClasificacion, dataset, idSimulacion
			//psf.Arguments = "-nodesktop -nosplash -r \"bateriaGAmain('" + parametersLoader.PathIni + "','LDA','West_ER',"+ simulacion.IdSimulacion + ")\";quit";
			//psf.Arguments = "-nodesktop -nosplash -r \"bateriaGAmain('" + parametersLoader.PathIni + "','LDA','" + simulacion.IdSimulacion.ToString() + "','" + simulacion.IdSimulacion.ToString() + "')\";quit";

			proc.Exited += new EventHandler(proc_Exited);   // Añadimos un evento para cuando finalice.
			proc.StartInfo = psf;

			Console.Write("[" + simulacion.IdSimulacion.ToString() + "]Launching MATLAB. Please wait...");

			// Guardamos fecha inicio
			DateTime fechaInicioSimulacion = DateTime.Now;
			proc.Start();       // Ejecutamos
			proc.WaitForExit(); // Esperamos su finalización


			Console.Write("[" + simulacion.IdSimulacion.ToString() + "] Matlab process finished.");

			// Guardamos fecha de finalización
			DateTime fechaFinSimulacion = DateTime.Now;

			Byte[] sBytes = new Byte[1024];


			// Buscamos si se ha genereado el archivo con los resultados, o por el contrario se ha generado un archivo de error
			if (parametersLoader.ExistResults())
			{

				// La simulación ha acabado correctamente en MATLAB
				try
				{
					// Almacenamos resultados
					Resultado resultados = parametersLoader.GetResultsFromTextFile();
					resultados.FechaLanzamiento = fechaInicioSimulacion;
					resultados.FechaFinalizacion = fechaFinSimulacion;
					XDocument resultadosXML = resultados.ToXML();
					resultadosXML.Save(simulacion.IdSimulacion.ToString() + ".xml");

					// Almacenar los resultados en BD
					db.NewResult(idProcess, simulacion.IdSimulacion, resultadosXML);

					// Incluimos la ID de la simulación en el XML
					resultadosXML.Root.Add(new XElement("IdSimulacion", simulacion.IdSimulacion));

					// Incluimos el <EOF> al final del archivo para que el servicio sepa cuando termina nuestro archivo y pasamos a bytes
					sBytes = Encoding.ASCII.GetBytes(resultadosXML.ToString());

					// Lanzamos sendResults para enviar los resultados al servicio Windows
					SendResults(sBytes);                    

					// Ejecución finalizada, establecemos como finalizada
					db.UpdateStateProcess("Terminate", simulacion.IdSimulacion);
					Console.WriteLine("[" + simulacion.IdSimulacion.ToString() + "] Finished");

					// Eliminamos archivos de configuración
					// TODO Arreglar, no permite borrar los datos porque dice que lo tiene otro proceso
					parametersLoader.RemoveParametersAndData();
				}
				catch (Exception e)
				{
					Console.WriteLine("Problemas al recoger los resultados: " + e.Message);
				}
			}
			else
			{
				String errorContent = "";
				// Tratar el archivo de error
				if (parametersLoader.ExistError())
				{
					// TODO Test: Carga el archivo de error
					using (StreamReader sr = parametersLoader.GetErrorFile())
					{
						String s;
						while (( s = sr.ReadLine()) !=null){
							errorContent += s;
						}
					}
				}
				else
				{
					// Si no hay ni archivo de error ni nada:
					// Mandar un UNKNOWN_ERROR<EOF>
					errorContent = "UNKNOWN_ERROR";
				}

				// Creamos XML con el Log
				Log log = new Log(
					DateTime.Now,
					errorContent);

				XDocument errorXML = log.ToXML();

				// Incluimos la ID de la simulación en el XML
				errorXML.Root.Add(new XElement("IdSimulacion", simulacion.IdSimulacion));

				// TODO Test: Modificar SendResults para que acepte bytes y no un XML.
				sBytes = Encoding.ASCII.GetBytes(errorXML.ToString());

				SendResults(sBytes);

				db.UpdateStateProcess("Error", simulacion.IdSimulacion);
			}            
		}


		private void SendResults(Byte[] sBytes)
		{
			// Preparando conexión
			IPEndPoint ipep = new IPEndPoint (
				IPAddress.Parse (config.SERVICE_IP),
				config.SERVICE_PORT
			);

			Socket conexion = new Socket (
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp
			);

			// Configuramos el envio
			Console.WriteLine("[" + simulacion.IdSimulacion.ToString() + "] Starting to send results...");
			Byte[] rBytes = new Byte[1024];
			int raw;           

			// Envio
			try
			{
				// Conectamos con el servicio
				Console.WriteLine("[" + simulacion.IdSimulacion.ToString() + "] Try to connect...");
				conexion.Connect(ipep);
				Console.WriteLine("[" + simulacion.IdSimulacion.ToString() + "] Connected");

				// Enviamos datos
				conexion.Send(sBytes);

				// Esperamos confirmación
				Console.WriteLine("[" + simulacion.IdSimulacion.ToString() + "] Waiting answer...");
				raw = conexion.Receive(rBytes);


				Console.WriteLine("[" + simulacion.IdSimulacion.ToString() + "] Sended: " + sBytes.LongLength + " Received: " + rBytes.LongLength + "bytes");
				// Comprobamos confirmación
				// TODO Comprobar confirmación recepción resultados. Estudiar decisión en caso que veamos que no ha llegado bien.

				// Establecemos a "Completed" la simulación
				db.UpdateStateProcess("Completed",simulacion.IdSimulacion);
			}
			catch (Exception e)
			{
				Console.WriteLine("[" + simulacion.IdSimulacion.ToString() + "] Error: " + e.ToString());
			}			
		}

		private void proc_Exited(object sender, EventArgs e)
		{
			Console.WriteLine("[P] Process finished.");
		}
	}

}

