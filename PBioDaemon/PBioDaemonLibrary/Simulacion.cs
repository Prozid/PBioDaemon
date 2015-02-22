using System;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using MySql.Data.MySqlClient;

namespace PBioDaemonLibrary
{
	[Serializable]
	public class Simulacion
	{
		public System.Guid IdSimulacion { get; set; }
		public System.Guid IdProyecto { get; set; }
		public string Nombre { get; set; }
		public string Descripcion { get; set; }
		public System.DateTime FechaCreacionSimulacion { get; set; }
		public System.Guid IdMetodoSeleccion { get; set; }
		public System.Guid IdMetodoClasificacion { get; set; }
		public System.Guid IdEstadoSimulacion { get; set; }
		public string ParametrosSeleccion { get; set; }
		public string ParametrosClasificacion { get; set; }
		public string Usuario { get; set; }

		[XmlIgnoreAttribute]
		public String Datos { get; set; }

		public static Simulacion GetSimulation(Guid idProcess)
		{
			String cs = ConfigurationManager.ConnectionStrings["db"].ToString();
			Simulacion simulation = null;

			using (MySqlConnection conn = new MySqlConnection(cs))
			{
				String qSim = "SELECT * FROM Proceso WHERE IdProceso = '" + idProcess + "'";

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

		public static void Serialize(string file, Simulacion sim)
		{
			System.Xml.Serialization.XmlSerializer xs
			= new System.Xml.Serialization.XmlSerializer(sim.GetType());

			StreamWriter writer = File.CreateText(file);
			xs.Serialize(writer, sim);
			writer.Flush();
			writer.Close();
		}

		public static Simulacion Deserialize(string file)
		{
			Simulacion sim;
			try
			{
				System.Xml.Serialization.XmlSerializer xs
				= new System.Xml.Serialization.XmlSerializer(typeof(Simulacion));
				StreamReader reader = File.OpenText(file);
				sim = (Simulacion)xs.Deserialize(reader);
				reader.Close();
			}
			catch
			{
				sim = null;
			}
			return sim;
		}

		public static Simulacion DeserializeFromXML(string xmlString)
		{
			Simulacion sim;
			try
			{
				XmlSerializer xs
				= new XmlSerializer(typeof(Simulacion));
				XmlReader reader = XDocument.Parse(xmlString).Root.CreateReader();
				sim = (Simulacion)xs.Deserialize(reader);
			}
			catch(Exception e)
			{
				sim = null;
			}
			return sim;
		}
	}
}

