using System;
using System.Configuration;

namespace PBioDaemonLauncher
{
	public class LauncherConfiguration
	{
		public String CONNECTION_STRING;
		public String SERVICE_IP; // IP del servicio en Windows
		public int SERVICE_PORT;   // Puerto en el que escucha el servicio Windows
		public int DAEMON_PORT; // Puerto en el que escucha el demonio Linux

		public String PATH; // Path dónde se almacena los datos y parámetros que necesita MATLAB
		public String FOLDER_INI;
		public String FOLDER_DATASETS;
		public String FOLDER_RESULTS;
		public String FILENAME_PARAMETERS;
		public String FILENAME_DATASET;
		public String FILENAME_RESULTS;
		public String FILENAME_ERROR;

		public String PATH_MATLAB;

		public LauncherConfiguration ()
		{
			this.CONNECTION_STRING = ConfigurationManager.ConnectionStrings["db"].ToString();
			this.SERVICE_IP = ConfigurationManager.AppSettings["service_ip"].ToString();
			this.SERVICE_PORT = int.Parse(ConfigurationManager.AppSettings["service_port"].ToString());
			this.DAEMON_PORT = int.Parse(ConfigurationManager.AppSettings["daemon_port"].ToString());

			this.PATH = ConfigurationManager.AppSettings["datapath"].ToString();
			this.FOLDER_INI = ConfigurationManager.AppSettings["folder_ini"].ToString();
			this.FOLDER_DATASETS = ConfigurationManager.AppSettings["folder_datasets"].ToString();
			this.FOLDER_RESULTS = ConfigurationManager.AppSettings["folder_results"].ToString();
			this.FILENAME_PARAMETERS = ConfigurationManager.AppSettings["filename_parameters"].ToString();
			this.FILENAME_DATASET = ConfigurationManager.AppSettings["filename_dataset"].ToString();
			this.FILENAME_RESULTS = ConfigurationManager.AppSettings["filename_results"].ToString();
			this.FILENAME_ERROR = ConfigurationManager.AppSettings["filename_error"].ToString();

			this.PATH_MATLAB = ConfigurationManager.AppSettings["path_matlab"].ToString();
		}
	}
}

