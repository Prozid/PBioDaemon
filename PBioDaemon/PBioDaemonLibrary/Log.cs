using System;
using System.Xml.Linq;

namespace PBioDaemonLibrary
{
	public class Log
	{
		private DateTime FechaSimulacion;
		private String Texto;

		public Log(DateTime fechaLog, String texto) 
		{
			FechaSimulacion = fechaLog;
			Texto = texto;
		}

		public XDocument ToXML()
		{
			XDocument xml = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("LogSimulacion",
					new XElement("FechaSimulacion", this.FechaSimulacion),
					new XElement("Texto", this.Texto))
			);
			return xml;
		}
	}
}

