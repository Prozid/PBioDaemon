using System;
using System.Xml.Linq;

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
	}
}

