using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace PBioDaemonLibrary
{
	public class Estado
	{
		Guid IdEstado;
		String Nombre;
		String NombreCorto;

		public Estado (Guid idEstado, String nombre, String nombreCorto)
		{
			this.IdEstado = idEstado;
			this.Nombre = nombre;
			this.NombreCorto = nombreCorto;
		}

		public static void Update(string state, Guid idProcess)
		{
			String cs = ConfigurationManager.ConnectionStrings["db"].ToString();

			using(MySqlConnection conn = new MySqlConnection(cs))
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

		public static Guid GetIdStateByName(String nameState)
		{
			String cs = ConfigurationManager.ConnectionStrings["db"].ToString();

			Guid idState;
			try
			{
				using (MySqlConnection conn = new MySqlConnection(cs))
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

