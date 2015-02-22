using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace PBioDaemonLibrary
{
	public class Proceso
	{
		Guid IdProceso;
		int Pid;
		XDocument Xml;
		String Datos;
		Guid IdEstado;

		public Proceso (Guid idProceso, int pid, XDocument xml, String datos, Guid idEstado)
		{
			this.IdProceso = idProceso;
			this.Pid = pid;
			this.Xml = xml;
			this.Datos = datos;
			this.IdEstado = idEstado;
		}

		public static Guid Create(XDocument xml, String data)
		{
			Guid idProcess,idState;
			String cs = ConfigurationManager.ConnectionStrings["db"].ToString();

			// Obtenemos un nuevo GUID para almacenar el proceso en la BD.
			using(MySqlConnection conn = new MySqlConnection(cs))
			{				
				string qGUID = "SELECT uuid() as id";

				conn.Open();
				MySqlCommand comm = new MySqlCommand(qGUID,conn);
				MySqlDataReader myReader = comm.ExecuteReader();

				if(myReader.Read())
				{
					idProcess = myReader.GetGuid("id");
				}
				else
				{
					throw new Exception("Error: Cannot get a new id on database");
				}
				myReader.Close();
			}

			// Obtenemos el estado 'Waiting' provisionalmente hasta que lancemos la simulacion.
			Console.WriteLine("[SERVER SOCKET] Reading default state.");

			idState = Estado.GetIdStateByName("Wait");

			// Creamos el proceso en base de datos
			using (MySqlConnection conn = new MySqlConnection(cs))
			{
				string qSaveProcess = "INSERT INTO Proceso " +
					"(IdProceso, Xml, Datos, Estado_IdEstado)"   +
					" VALUES ( '"                   +
					idProcess.ToString()    + "','" +
					xml 		            + "','" +
					data                    + "','" +
					idState.ToString() 	    + "')";

				conn.Open();
				MySqlCommand comm = new MySqlCommand(qSaveProcess,conn);
				comm.ExecuteNonQuery();
			}

			Console.WriteLine("[SERVER SOCKET] Process logged.");

			return idProcess;
		}

		public static List<Guid> GetIdProcessRunning()
		{
			String cs = ConfigurationManager.ConnectionStrings["db"].ToString();
			List<Guid> processes = new List<Guid>();
			Guid idRunningState = Estado.GetIdStateByName("Run");

			using (MySqlConnection conn = new MySqlConnection(cs))
			{
				String qProcessesRunning = "SELECT * FROM Proceso WHERE Estado_IdEstado = '" + idRunningState +"'";

				conn.Open();
				MySqlCommand comm = new MySqlCommand(qProcessesRunning, conn);
				MySqlDataReader reader = comm.ExecuteReader();

				while (reader.Read())
				{
					processes.Add(reader.GetGuid("IdProceso"));
				}
				reader.Close();
			}

			return processes;   

		}
	}
}

