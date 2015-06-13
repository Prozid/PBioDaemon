using System;
using System.Xml.Linq;
using PBioDaemonLibrary;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

namespace PBioDaemonLauncher
{
	public class SimulationLauncher
	{
		private const String ResultsFileTag = "<Results>";
		private const String ErrorFileTag = "<Error>";
		private Simulacion simulation;
		private LauncherConfiguration config;
		private ParametersLoader parametersLoader;
		private Guid idProcess;

		public SimulationLauncher(Guid idProcess)
		{
			try
			{
				this.idProcess = idProcess;
				this.config = new LauncherConfiguration();
				this.simulation = Simulacion.GetSimulation(idProcess);
				this.parametersLoader = new ParametersLoader(simulation.ParametrosClasificacion, simulation.ParametrosSeleccion, simulation.IdSimulacion);
				this.parametersLoader.Merge();
				//this.parametersLoader.CreateResultsFolder();
				this.parametersLoader.SetData(simulation.Datos);
			}
			catch (Exception e)
			{
				throw e;
			}
		}
			
		public void Run()
		{				
			// Establecemos a "Running" el proceso.
			Estado.Update("Run", idProcess);	

			// Lanzamiento de la ejecución		
			System.Diagnostics.ProcessStartInfo psf = new System.Diagnostics.ProcessStartInfo();
			System.Diagnostics.Process proc = new System.Diagnostics.Process();


			// RUN ON LINUX
			psf.FileName = config.PATH_MATLAB;
			String mat_mainPath = config.FOLDER_INI +'/';
			String mat_method = "LDA";
			String mat_dataset = simulation.IdSimulacion.ToString();
			String mat_guid = simulation.IdSimulacion.ToString();

			//psf.Arguments = "-nodisplay -r \"bateriaGAmain('" + mat + "','LDA','" + simulation.IdSimulacion.ToString() + "','" + simulation.IdSimulacion.ToString() + "')\";quit";
			psf.Arguments = String.Format("-nodisplay -r \"bateriaGAmain('{0}','{1}','{2}','{3}')\";quit", 
				mat_mainPath, mat_method, mat_dataset, mat_guid);

			// RUN ON WINDOWS
			//psf.FileName = "matlab";
			// bateriaGAmain(pathParametersInitials, metodoClasificacion, dataset, idSimulacion
			//psf.Arguments = "-nodesktop -nosplash -r \"bateriaGAmain('" + parametersLoader.PathIni + "','LDA','West_ER',"+ simulacion.IdSimulacion + ")\";quit";
			//psf.Arguments = "-nodesktop -nosplash -r \"bateriaGAmain('" + parametersLoader.PathIni + "','LDA','" + simulacion.IdSimulacion.ToString() + "','" + simulacion.IdSimulacion.ToString() + "')\";quit";

			proc.Exited += new EventHandler(proc_Exited);   // Añadimos un evento para cuando finalice.
			proc.StartInfo = psf;

			Console.Write("[" + simulation.IdSimulacion.ToString() + "]Launching MATLAB. Please wait...");

			// Guardamos fecha inicio
			DateTime fechaInicioSimulacion = DateTime.Now;
			proc.Start();       // Ejecutamos
			proc.WaitForExit(); // Esperamos su finalización


			Console.Write("[" + simulation.IdSimulacion.ToString() + "] Matlab process finished.");

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
					resultadosXML.Save(simulation.IdSimulacion.ToString() + ".xml");

					// Almacenar los resultados en BD
					Resultado.Create(idProcess, simulation.IdSimulacion, resultadosXML);

					// Incluimos la ID de la simulación en el XML
					resultadosXML.Root.Add(new XElement("IdSimulacion", simulation.IdSimulacion));

					// Incluimos el <PBIOEOF> al final del archivo para que el servicio sepa cuando termina nuestro archivo y pasamos a bytes
					sBytes = Encoding.ASCII.GetBytes(resultadosXML.ToString()+ "<PBIOEOF>");

					// Lanzamos sendResults para enviar los resultados al servicio Windows
					SendResults(sBytes);                    

					// Ejecución finalizada, establecemos como finalizada
					Estado.Update("Terminate", idProcess);
					Console.WriteLine("[" + simulation.IdSimulacion.ToString() + "] Finished");

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
					// Mandar un UNKNOWN_ERROR<PBIOEOF>
					errorContent = "UNKNOWN_ERROR";
				}

				// Creamos XML con el Log
				Log log = new Log(
					DateTime.Now,
					errorContent);

				XDocument errorXML = log.ToXML();

				// Incluimos la ID de la simulación en el XML
				errorXML.Root.Add(new XElement("IdSimulacion", simulation.IdSimulacion));

				sBytes = Encoding.ASCII.GetBytes(errorXML.ToString()+"<PBIOEOF>");
				SendResults(sBytes);
				Estado.Update("Error", idProcess);
			}            
		}

		private void SendResults(Byte[] byteData)
		{
			try
			{
				// Send data
				Console.WriteLine("[SEND SIMULATION] Try to connect to " + config.SERVICE_IP + ":" + config.SERVICE_PORT + "...");

				String response_checksum = PBioSocketClient.StartClient(config.SERVICE_IP, config.SERVICE_PORT, byteData);
				var sha = new SHA256Managed();
				byte[] byte_checksum = sha.ComputeHash(byteData);
				String checksum = BitConverter.ToString(byte_checksum).Replace("-", String.Empty);

				// Checksum
				Console.WriteLine("[SEND SIMULATION]Checksum: " + checksum + " Response checksum: " + response_checksum);
				if (response_checksum != null && response_checksum == checksum)
				{
					// Mostramos confirmación
					Console.WriteLine("[SEND SIMULATION]Confirmation: Sended: checksum");
					// Establecemos a "Completed" la simulación
					// TODO: Estado completed no existe. Revisar si quitar o qué.
					Estado.Update("Terminate", idProcess);
				} else {
					Console.WriteLine("[SEND SIMULATION] Send failed.");
					// TODO: Ver que hacer si ha habido un error.
					Estado.Update("Error", idProcess);
				}
			}
			catch (System.TimeoutException error)
			{
				Console.WriteLine("[SEND SIMULATION] Timeout finished: " + error);
				Estado.Update("Error", idProcess);
			}
			catch (SocketException se)
			{
				Console.WriteLine("[SEND SIMULATION] Cannot connect to remote host: " + se.Message);
				Estado.Update("Error", idProcess);
			}
			catch (Exception error)
			{
				Console.WriteLine("[SEND SIMULATION] Unknown error: " + error);
				Estado.Update("Error", idProcess);
			}
		}

		private void proc_Exited(object sender, EventArgs e)
		{
			Console.WriteLine("[P] Process finished.");
		}
	}

	// State object for receiving data from remote device.
	public class StateObject
	{
		// Client socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 256;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();
	}
	/// ENVÍO ASÍNCRONO
	/// http://msdn.microsoft.com/es-es/library/bew39x2a(v=vs.80).aspx
	class PBioSocketClient
	{
		//private static System.Diagnostics.EventLog PBioEventLog = PBioEventLogger.initLogger();
		// ManualResetEvent instances signal completion.
		private static ManualResetEvent connectDone = new ManualResetEvent(false);
		private static ManualResetEvent sendDone = new ManualResetEvent(false);
		private static ManualResetEvent receiveDone = new ManualResetEvent(false);
		// The response from the remote device.
		private static String response = String.Empty;
		public static string StartClient(String ip, int port, byte[] byteData) {
			try
			{
				// Connect to a remote device.
				// Establish the remote endpoint for the socket.
				IPAddress ipAddress = IPAddress.Parse(ip);
				IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
				// Create a TCP/IP socket.
				Socket client = new Socket(AddressFamily.InterNetwork,
					SocketType.Stream, ProtocolType.Tcp);
				// Connect to the remote endpoint.
				client.BeginConnect(remoteEP,
					new AsyncCallback(ConnectCallback), client);
				connectDone.WaitOne();
				// Send test data to the remote device.
				Send(client, byteData);
				sendDone.WaitOne();
				// Receive the response from the remote device.
				Receive(client);
				receiveDone.WaitOne();
				// Write the response to the console.
				Console.WriteLine("Response received : " + response);
				// Release the socket.
				client.Shutdown(SocketShutdown.Both);
				client.Close();
			}
			catch (Exception e)
			{
				throw e;
			}
			return response;
		}
		private static void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket client = (Socket)ar.AsyncState;
				// Complete the connection.
				client.EndConnect(ar);
				Console.WriteLine("Socket connected to " +
					client.RemoteEndPoint.ToString());
				// Signal that the connection has been made.
				connectDone.Set();
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		private static void Receive(Socket client)
		{
			try
			{
				// Create the state object.
				StateObject state = new StateObject();
				state.workSocket = client;
				// Begin receiving the data from the remote device.
				client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback(ReceiveCallback), state);
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		private static void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the state object and the client socket
				// from the asynchronous state object.
				StateObject state = (StateObject)ar.AsyncState;
				Socket client = state.workSocket;
				// Read data from the remote device.
				int bytesRead = client.EndReceive(ar);
				if (bytesRead > 0)
				{
					// There might be more data, so store the data received so far.
					state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
					// Get the rest of the data.
					client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
						new AsyncCallback(ReceiveCallback), state);
				}
				else
				{
					// All the data has arrived; put it in response.
					if (state.sb.Length > 1)
					{
						response = state.sb.ToString();
					}
					// Signal that all bytes have been received.
					receiveDone.Set();
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		private static void Send(Socket client, byte[] byteData)
		{
			Console.WriteLine("Begin to send");
			// Begin sending the data to the remote device.
			client.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), client);
		}
		private static void SendCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket client = (Socket)ar.AsyncState;
				// Complete sending the data to the remote device.
				int bytesSent = client.EndSend(ar);
				Console.WriteLine("Sent " +bytesSent + " bytes to server.");
				// Signal that all bytes have been sent.
				sendDone.Set();
			}
			catch (Exception e)
			{
				throw e;
			}
		}
	}


}

