using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace PBioDaemonLibrary
{
	public class PBioDaemonDB
	{
		private String connectionString;
		/* 
		 * CONSTRUCTORES
		 */
		public PBioDaemonDB(String cs)
		{
			if(cs == null || cs == "")
				throw new Exception("Connection string empty");

			connectionString = cs;			
		}

		/*
		 * METODOS
		 */ 
		public Guid NewProcess(XDocument xml, String data)
		{
			Guid idProcess,idState;


			// Obtenemos un nuevo GUID para almacenar la simulacion en la BD.
			using(MySqlConnection conn = new MySqlConnection(connectionString))
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

			idState = GetIdStateByName("Wait");

			// Insertamos la simulacion
			using (MySqlConnection conn = new MySqlConnection(connectionString))
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

		public Guid NewResult(Guid idProcess, Guid idSimulation, XDocument xml)
		{
			Guid idResult;

			// Obtenemos un nuevo GUID para almacenar la simulacion en la BD.
			using(MySqlConnection conn = new MySqlConnection(connectionString))
			{				
				string qGUID = "SELECT uuid() as id";

				conn.Open();
				MySqlCommand comm = new MySqlCommand(qGUID,conn);
				MySqlDataReader myReader = comm.ExecuteReader();

				if(myReader.Read())
				{
					idResult = myReader.GetGuid("id");
				}
				else
				{
					throw new Exception("Error: Cannot get a new id on database");
				}
				myReader.Close();

				// Insertamos los resultados
				string qSaveResult = "INSERT INTO Resultado " +
					"(IdResultado, Xml, Proceso_IdProceso, IdSimulacion)"   +
					" VALUES ( '"                   +
					idResult.ToString()    	+ "','" +
					xml 		            + "','" +
					idProcess.ToString() 	+ "'," +
					idSimulation.ToString() + "')";

				comm = new MySqlCommand(qSaveResult,conn);
				comm.ExecuteNonQuery();
			}

			return idResult;
		}

		public void UpdateStateProcess(string state, Guid idProcess)
		{
			using(MySqlConnection conn = new MySqlConnection(connectionString))
			{
				string idState = "";

				//	Seleccionamos idState
				string qState = "SELECT IdEstado FROM Estado WHERE Nombre = '"+state+"'";

				conn.Open();
				MySqlCommand myCommand = new MySqlCommand(qState,conn);
				MySqlDataReader myReader = myCommand.ExecuteReader();

				if(myReader.Read())
				{
					idState = myReader.GetString("IdEstado");
				}
				myReader.Close();
				// Actualizamos el estado del proceso
				string uProcessState = "UPDATE Proceso SET Estado_IdEstado = '"+idState+"' WHERE IdProceso = '"+idProcess.ToString()+"'";
				myCommand = new MySqlCommand(uProcessState,conn);
				myCommand.ExecuteNonQuery();
				conn.Close();
			}
		}

		public XDocument GetXMLResults(Guid idProcess)

		{
			XDocument xml;
			String sXml;

			using (MySqlConnection conn = new MySqlConnection(connectionString)) 
			{
				MySqlCommand myCommand;
				MySqlDataReader myReader;

				// Obtenemos los datos de la BD.
				string qsProcess = "SELECT * FROM Proceso WHERE IdProceso = '" + idProcess.ToString() + "'";

				conn.Open();
				myCommand = new MySqlCommand (qsProcess, conn);
				myReader = myCommand.ExecuteReader();


				if(!myReader.Read())
					throw new Exception("Error: Cannot retrive the process data.");

				// Recogemos los resultados.
				sXml = myReader.GetString("Xml");
				myReader.Close();
			}

			// Parseamos el string a documento XML.
			xml = XDocument.Parse(sXml);

			return xml;
		}

		public List<Guid> GetIdSimulationsRunning()
		{
			List<Guid> simulations = new List<Guid>();
			Guid idRunning = GetIdStateByName("Run");

			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				String qSimRunning = "SELECT * FROM Proceso WHERE Estado_IdEstado = '" + idRunning +"'";

				conn.Open();
				MySqlCommand comm = new MySqlCommand(qSimRunning, conn);
				MySqlDataReader reader = comm.ExecuteReader();

				while (reader.Read())
				{
					simulations.Add(reader.GetGuid("IdProceso"));
				}
				reader.Close();
			}

			return simulations;   

		}

		public Simulacion GetSimulationById(Guid idSimulation)
		{
			Simulacion simulation = null;

			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				String qSim = "SELECT * FROM Proceso WHERE IdProceso = '" + idSimulation + "'";

				conn.Open();
				MySqlCommand comm = new MySqlCommand(qSim, conn);
				MySqlDataReader reader = comm.ExecuteReader();

				while (reader.Read())
				{
					simulation = Simulacion.DeserializeFromXML(reader.GetString("Xml"));
					simulation.Datos = reader.GetString("Datos");

				}
				reader.Close();
			}

			return simulation;
		}

		public Guid GetIdStateByName(String nameState)
		{
			Guid idState;
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					string qState = "SELECT IdEstado FROM Estado WHERE Nombre = '" + nameState + "'";

					conn.Open();
					MySqlCommand myCommand = new MySqlCommand(qState, conn);
					MySqlDataReader myReader = myCommand.ExecuteReader();

					if (myReader.Read())
					{
						idState = myReader.GetGuid("IdEstado");
						Console.WriteLine("[SERVER SOCKET] IdEstado: " + idState);
					}
					else
					{
						idState = Guid.Empty;
					}
					myReader.Close();
				}
			}
			catch (Exception e)
			{
				throw e;
				//Console.WriteLine("[ERROR] " + e.InnerException.Message);
			}
			return idState;
		}
	}
}

