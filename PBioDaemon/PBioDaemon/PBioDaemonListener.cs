using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using PBioDaemonLibrary;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;

namespace PBioDaemon
{
	class PBioDaemonConfiguration
	{
		public String CONNECTION_STRING;
		public String SERVICE_IP; // IP del servicio en Windows
		public int SERVICE_PORT;   // Puerto en el que escucha el servicio Windows
		public int DAEMON_PORT; // Puerto en el que escucha el demonio Linux

		public PBioDaemonConfiguration(){
			SERVICE_IP = ConfigurationManager.AppSettings["service_ip"].ToString();
			SERVICE_PORT = int.Parse(ConfigurationManager.AppSettings["service_port"].ToString());
			DAEMON_PORT = int.Parse(ConfigurationManager.AppSettings["daemon_port"].ToString());
		}
	}

	public class PBioDaemonListener
	{
		private PBioDaemonConfiguration config;

		// State object for reading client data asynchronously
		private class StateObject {
			// Client  socket.
			public Socket workSocket = null;
			// Size of receive buffer.
			public const int BufferSize = 1024;
			// Receive buffer.
			public byte[] buffer = new byte[BufferSize];
			// Received data string.
			public StringBuilder sb = new StringBuilder();  
		}

		private static ManualResetEvent allDone = new ManualResetEvent(false);

		public PBioDaemonListener ()
		{
			config = new PBioDaemonConfiguration ();
		}

		public void Run()
		{
			// ZONA DECLARACIÓN DE VARIABLES
			Socket listener;

			// INICIALIZACIÓN
			// Chequeamos los errores que se hayan producido y lanzamos las simulaciones de nuevo
			this.CheckSimulationsErrors();

			// Inicializamos la conexión
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, config.DAEMON_PORT);

			listener = new Socket(
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp);

			try
			{
				Console.WriteLine("[SERVER SOCKET] Ready to init.");
				// Vinculamos el socket al IPEndPoint
				listener.Bind(ipep);
				// Escuchamos conexiones entrantes
				listener.Listen(100);
				Console.WriteLine("[SERVER SOCKET] Initialized.");

				while (true)
				{
					allDone.Reset();

					// Iniciamos un socket asíncrono para escuchar conexiones
					Console.WriteLine("[SERVER SOCKET] Waiting for a connection...");
					listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

					// Esperamos a que la conexión desbloquee el socket (no estoy seguro, mirar MSDN: http://msdn.microsoft.com/es-es/library/fx6588te(v=vs.80).aspx)
					allDone.WaitOne();
				}	
			}
			catch (SocketException se)
			{
				Console.WriteLine("{0} error: {1}", se.Message, se.ErrorCode);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		// Prepara un objeto StateObject dónde recibiremos los datos
		private static void AcceptCallback(IAsyncResult ar)
		{
			// Signal the main thread to continue.
			allDone.Set();

			Console.WriteLine ("[SERVER SOCKET] New connection.");

			// Get the socket that handles the client request.
			Socket listener = (Socket)ar.AsyncState;
			Socket handler = listener.EndAccept(ar);

			// Create the state object.
			StateObject state = new StateObject();
			state.workSocket = handler;
			handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
				new AsyncCallback(ReadCallback), state);
		}

		// Recepción de datos
		private static void ReadCallback(IAsyncResult ar)
		{
			String content = String.Empty;
			
			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			StateObject state = (StateObject)ar.AsyncState;
			Socket handler = state.workSocket;

			// Read data from the client socket. 
			int bytesRead = handler.EndReceive(ar);

			if (bytesRead > 0) {
				// There  might be more data, so store the data received so far.
				state.sb.Append (Encoding.ASCII.GetString (state.buffer, 0, bytesRead));

				// Obtenemos los datos recibidos
				content = state.sb.ToString ();


				if (content.IndexOf("<PBIOEOF>") > -1) { 
					// All the data has been read from the 
					// client. Display it on the console.
					Console.WriteLine ("[SERVER SOCKET] Data received.");
					Console.WriteLine ("[SERVER SOCKET] Read {0} bytes from socket. \n", content.Length);

					// Checksum
					var sha = new System.Security.Cryptography.SHA256Managed ();
					byte[] byte_checksum = sha.ComputeHash (Encoding.ASCII.GetBytes (content));
					String checksum = BitConverter.ToString (byte_checksum).Replace ("-", String.Empty);

					int endTagIndex = content.IndexOf("<PBIOEOF>");
					content = content.Remove(endTagIndex);

					try {
						// Parseamos el XML
						XDocument datosSimulacion = XDocument.Parse (content);

						// Obtenemos los datos
						String data = datosSimulacion.Root.Element ("Datos").Value;
						datosSimulacion.Root.Element ("Datos").Remove ();

						// Insertamos la nueva simulacion en la base de datos.
						Guid idProcess = Proceso.Create (datosSimulacion, data);

						Console.WriteLine ("[SERVER SOCKET] XML saved to hard disk.");

						// Lanzamos la simulacion
						LaunchProcess (idProcess);

						// Echo the data back to the client.
						Send (state.workSocket, checksum);
					} catch (Exception e) {
						Console.WriteLine ("[SERVER SOCKET] Error launching process: " + e.Message);
						Send (state.workSocket, "<error>" + content.Length.ToString ());
					}
				} 
				else {
					handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
						new AsyncCallback(ReadCallback), state);
				}
			}
		}

		private static void Send(Socket handler, String data)
		{
			// Convert the string data to byte data using ASCII encoding.
			String content = data + "<PBIOEOF>";
			byte[] byteData = Encoding.ASCII.GetBytes(content);

			// Begin sending the data to the remote device.
			handler.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), handler);
		}

		private static void SendCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket handler = (Socket)ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(ar);
				Console.WriteLine("Sent " + bytesSent + " bytes to service.");

				handler.Shutdown(SocketShutdown.Both);
				handler.Close();

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		private static void LaunchProcess (Guid idProcess)
		{
			// Creamos proceso SRUN para ejecutar de forma distribuida 
			// el proceso que lanzara la simulacion, recogera los datos y los
			// devolvera al servicio Windows
			System.Diagnostics.ProcessStartInfo psf = new System.Diagnostics.ProcessStartInfo();
			System.Diagnostics.Process proc = new System.Diagnostics.Process();

			/*
			 * Cluster launch
			 */
			//psf.FileName = "srun" ;
			//psf.Arguments = "-N1 'pbio_launcher.exe' " + idProcess.ToString();

			/*
			 * Dev launch
			 */
			psf.FileName = "/home/dani/Proyecto/PBioDaemon/PBioDaemon/PBioDaemonLauncher/bin/Debug/PBioDaemonLauncher.exe" ;
			psf.Arguments = idProcess.ToString();

			proc.StartInfo = psf;

			Console.WriteLine("[SERVER SOCKET] " + psf.FileName + " " + psf.Arguments); 
			proc.Start();       // Ejecutamos y seguimos.

		}

		private void CheckSimulationsErrors ()
		{
			/* Posibles estados de la simulación
	         * - ToRun
			 * - Run
	         * - Wait
	         * - Terminate
	         * - Error
	         * 
	         * - Primero realizamos un chequeo inicial para ejecutar de nuevo simulaciones que se encuentran en Running o Error al inicio.
	         * 
	         * */ 
			// Obtenemos las simulaciones que se quedaron lanzadas
			List<Guid> processRunningOrWaiting = Proceso.GetIdProcessRunningOrWaiting ();

			if (processRunningOrWaiting.Count > 0) {
				Console.WriteLine ("[SERVER SOCKET] {0} simulations was running before init the server.", processRunningOrWaiting.Count);

				// Iteramos en la lista de simulaciones, actualizamos el estado y las lanzamos
				foreach (Guid idProcess in processRunningOrWaiting) {
					Console.WriteLine ("[SERVER SOCKET] Rescued: {0}", idProcess.ToString());
					Estado.Update ("ToRun", idProcess);
					LaunchProcess(idProcess);
				} 
			}        
		}
	}
}

