using System;
using System.Configuration;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace PBioDaemonLibrary
{
	[Serializable]
	public class Resultado
	{

		public String NombreGenesSolucion { get; set; }
		public String IdGenesSolucion { get; set; }
		public int NumGenes { get; set; }
		public Double Accuracy_Media { get; set; }
		public Double Accuracy_Std { get; set; }
		public Double Sensitivity_Media { get; set; }
		public Double Sensitivity_Std { get; set; }
		public Double Specificity_Media { get; set; }
		public Double Specificity_Std{ get; set; }
		public String NombreGenes { get; set; }
		public String IdGenes { get; set; }
		public String AccuracyXGenes { get; set; }
		public DateTime FechaLanzamiento { get; set; }
		public DateTime FechaFinalizacion { get; set; }

		public Resultado ()
		{

		}

		public XDocument ToXML()
		{
			XDocument xml = new XDocument(
				new XDeclaration("1.0","utf-8","yes"),
				new XElement("Resultado",
					new XElement("NombreGenesSolucion", this.NombreGenesSolucion),
					new XElement("IdGenesSolucion", this.IdGenesSolucion),
					new XElement("NumGenes", this.NumGenes),
					new XElement("Accuracy_Media", this.Accuracy_Media),
					new XElement("Accuracy_Std", this.Accuracy_Std),
					new XElement("Sensitivity_Media", this.Sensitivity_Media),
					new XElement("Sensitivity_Std", this.Sensitivity_Std),
					new XElement("Specificity_Media", this.Specificity_Media),
					new XElement("Specificity_Std", this.Specificity_Std),
					new XElement("NombreGenes", this.NombreGenes),
					new XElement("IdGenes", this.IdGenes),
					new XElement("AccuracyXGenes", this.AccuracyXGenes),
					new XElement("FechaLanzamiento", this.FechaLanzamiento),
					new XElement("FechaFinalizacion", this.FechaFinalizacion),
					new XElement("Duracion", (this.FechaFinalizacion - this.FechaLanzamiento))
				)
			);

			return xml;			
		}

		public static Guid Create(Guid idProcess, Guid idSimulation, XDocument xml)
		{
			String cs = ConfigurationManager.ConnectionStrings["db"].ToString();
			Guid idResult;

			// Obtenemos un nuevo GUID para almacenar la simulacion en la BD.
			using(MySqlConnection conn = new MySqlConnection(cs))
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
					idProcess.ToString() 	+ "','" +
					idSimulation.ToString() + "')";

				comm = new MySqlCommand(qSaveResult,conn);
				comm.ExecuteNonQuery();
			}


			return idResult;
		}

		public static XDocument GetXML(Guid idProcess)
		{
			String cs = ConfigurationManager.ConnectionStrings["db"].ToString();
			XDocument xml;
			String sXml;

			using (MySqlConnection conn = new MySqlConnection(cs)) 
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
	}
}

