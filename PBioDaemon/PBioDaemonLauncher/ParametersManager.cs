using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using PBioDaemonLibrary;

namespace PBioDaemonLauncher
{
	public class ParametersManager
	{
		// Lista con los distintos tipos de parámetros
		private List<IParameter> _listParameters;

		// Lista con las lineas de parámetros que compondrán el archivo txt
		List<String> parametersFile = new List<string>();



		private List<String> ToList()
		{
			List<String> list = new List<String>();

			if (this._listParameters != null)
			{
				foreach (IParameter parameter in this._listParameters)
				{
					list.AddRange(parameter.ToList());
				}
			}
			else
				list.AddRange(GeneralParameters.Defaults().ToList());


			return list;
		}

		public void SaveToFile()
		{
			using (StreamWriter sr = new StreamWriter("idProcess.txt"))
			{
				foreach (String line in parametersFile)
				{
					sr.WriteLine(line);
				}
			}
		}
	}


	public class ParametersLoader
	{
		private LauncherConfiguration config;
		private Dictionary<String, String> _clasificationParameters;
		private Dictionary<String, String> _selectionParameters;
		private Guid _idSimulation;


		public ParametersLoader(String clasificationParameters, String selectionParameters, Guid idSimulation)
		{
			config = new LauncherConfiguration ();
			_clasificationParameters = (clasificationParameters == null) ? null : ParametersFromStringToDictionary(clasificationParameters);
			_selectionParameters = (selectionParameters == null) ? null : ParametersFromStringToDictionary(selectionParameters);
			_idSimulation = idSimulation;
		}

		private Dictionary<String, String> ParametersFromStringToDictionary(String parameters)
		{
			Dictionary<String, String> parametersProcessed = new Dictionary<String, String>();
			if (parameters != ""){
				String[] parameterRaw = parameters.Split(';');

				foreach (String p in parameterRaw)
				{
					parametersProcessed.Add(p.Split('=')[0], p.Split('=')[1]);
				}
			}

			return parametersProcessed;
		}

		public void Merge()
		{
			// Abrimos el archivo de parámatros por defecto.
			String[] lines = System.IO.File.ReadAllLines(Path.Combine(config.FOLDER_INI, config.FILENAME_PARAMETERS));


			// Actualizamos con los parámetros de clasificación seleccionados
			if (_clasificationParameters != null)
			{
				foreach (KeyValuePair<String, String> item in _clasificationParameters)
				{
					if (lines.Where(l => l.Contains(item.Key)).Count() > 0)
					{
						String temp = lines.Where(l => l.Contains(item.Key)).Single();
						temp = item.Key + "\t" + item.Value;
					}
				}
			}

			// Actualizamos con los parámetros de selección seleccionados
			if (_selectionParameters != null)
			{
				foreach (KeyValuePair<String, String> item in _selectionParameters)
				{
					if (lines.Where(l => l.Contains(item.Key)).Count() > 0)
					{
						String temp = lines.Where(l => l.Contains(item.Key)).Single();
						temp = item.Key + "\t" + item.Value;
					}
				}
			}


			// Creamos carpeta con el ID de la simulación
			// ../ini/guid_simulation
			String pathIniSimulation = System.IO.Path.Combine(config.FOLDER_INI, _idSimulation.ToString());
			System.IO.Directory.CreateDirectory(pathIniSimulation);

			// Guardamos el archivo con los parámetros modificados
			// ../ini/guid_simulation/parameters.txt
			System.IO.File.WriteAllLines(System.IO.Path.Combine(pathIniSimulation, config.FILENAME_PARAMETERS), lines);
		}

		public void SetData(String data)
		{
			// Obtenemos la ruta dónde tenemos que almacenar los datos que requiere MATLAB
			// Dicha ruta se encuentra en: pathDataSet/idSimulation/input_dataset.arff
			String pathDatasetSimulation = System.IO.Path.Combine(config.FOLDER_DATASETS, _idSimulation.ToString());

			// Creamos la carpeta
			System.IO.Directory.CreateDirectory(pathDatasetSimulation);

			// Obtenemos el writer para escribir los datos
			StreamWriter writer = System.IO.File.CreateText(System.IO.Path.Combine(pathDatasetSimulation, config.FILENAME_DATASET));


			// Escribimos los datos
			writer.WriteLine(data);
			writer.Flush();
			writer.Close();
		}

		public void CreateResultsFolder()
		{
			// Obtenemos la ruta dónde Matlab guardará los resultados.
			String pathResults = System.IO.Path.Combine(config.FOLDER_RESULTS, _idSimulation.ToString());

			// Creamos la carpeta
			System.IO.Directory.CreateDirectory(pathResults);
		}

		public void RemoveParametersAndData()
		{
			String pathIni = Path.Combine(config.FOLDER_INI,_idSimulation.ToString());
			String pathDataSet = Path.Combine(config.FOLDER_DATASETS,_idSimulation.ToString());
			String pathResults = Path.Combine(config.FOLDER_RESULTS,_idSimulation.ToString());

			if (Directory.Exists(pathIni)) Directory.Delete(pathIni, true);
			if (Directory.Exists(pathDataSet)) Directory.Delete(pathDataSet, true);
			if (Directory.Exists(pathResults)) Directory.Delete(pathResults,true); 
		}

		public bool ExistResults()
		{
			String path = Path.Combine(config.FOLDER_RESULTS, _idSimulation.ToString());
			DirectoryInfo infoDir = new DirectoryInfo(path);

			if (infoDir.GetFiles(config.FILENAME_RESULTS).Length > 0)
				return true;
			else
				return false;
		}

		public bool ExistError()
		{
			String path = Path.Combine(config.FOLDER_RESULTS, _idSimulation.ToString());
			DirectoryInfo infoDir = new DirectoryInfo(path);

			if (infoDir.GetFiles(config.FILENAME_ERROR).Length > 0)
				return true;
			else
				return false;
		}


		/* Formato archivo de salida de Matlab y R:
         * 
         * nameGenesSolution: n1_n2_ ... 
         * idGenesSolution: id1_id2_ ... 
         * numgenes: n
         * accuracy_medio: acm
         * accuracy_std: acs
         * sensitivity_medio: sm
         * sensitivity_std: ss
         * specificity_medio: sm
         * specificity_std
         * nameGenes: n1 n2
         * idGenes: id1 id2
         * accuracyxgenes:(gen:ac +- std)(gen2: ac +- std) ... (gen,gen2,gen3,gen4: ac +- std)
         */
		public Resultado GetResultsFromTextFile()
		{
			try
			{
				String path = Path.Combine(Path.Combine(config.FOLDER_RESULTS, _idSimulation.ToString()), config.FILENAME_RESULTS);
				Resultado result = new Resultado();


				List<String> lines = new List<String>();
				String line = "";


				using (StreamReader sr = new StreamReader(path))
				{
					while (line != null)
					{
						line = sr.ReadLine();
						if (line != null)
							lines.Add(line);
					}
				}

				if (lines.Count == 12)
				{
					result.NombreGenesSolucion 	= lines[0].Split(':')[1];
					result.IdGenesSolucion 	= lines[1].Split(':')[1];
					result.NumGenes 			= Int32.Parse(lines[2].Split(':')[1]);
					result.Accuracy_Media 		= Double.Parse(lines[3].Split(':')[1]);
					result.Accuracy_Std 		= Double.Parse(lines[4].Split(':')[1]);
					result.Sensitivity_Media 	= Double.Parse(lines[5].Split(':')[1]);
					result.Sensitivity_Std 	= Double.Parse(lines[6].Split(':')[1]);
					result.Specificity_Media 	= Double.Parse(lines[7].Split(':')[1]);
					result.Specificity_Std 	= Double.Parse(lines[8].Split(':')[1]);
					result.NombreGenes 			= lines[9].Split(':')[1];
					result.IdGenes 			= lines[10].Split(':')[1];
					result.AccuracyXGenes 		= lines[11].Split(new Char[]{':'},2)[1];
				}

				/* Obtenemos la informacion de las lineas del texto
                results.GenNames = lines.Where(l => l.StartsWith("NombreGenes")).SingleOrDefault().Split(':')[1];
                results.GenId = lines.Where(l => l.StartsWith("IdGenes")).SingleOrDefault().Split(':')[1];
                results.NumGens = Int32.Parse(lines.Where(l => l.StartsWith("numGenes")).SingleOrDefault().Split(':')[1]);
                results.AccuracyAverage = Double.Parse(lines.Where(l => l.StartsWith("accuracy_medio")).SingleOrDefault().Split(':')[1]);
                results.AccuracyStd = Double.Parse(lines.Where(l=> l.StartsWith("accuracy_std")).SingleOrDefault().Split(':')[1]);
                results.PrecisionAverage = Double.Parse(lines.Where(l => l.StartsWith("precision_medio")).SingleOrDefault().Split(':')[1]);
                results.PrecisionStd = Double.Parse(lines.Where(l => l.StartsWith("precision_std")).SingleOrDefault().Split(':')[1]);
                results.RecallAverage = Double.Parse(lines.Where(l => l.StartsWith("recall_medio")).SingleOrDefault().Split(':')[1]);
                results.RecallStd = Double.Parse(lines.Where(l => l.StartsWith("recall_std")).SingleOrDefault().Split(':')[1]);
                results.AccuracyByGen = lines.Where(l => l.StartsWith("accuracyxgenes")).SingleOrDefault().Split(':')[1];
                */
				// Devolvemos el objeto resultados con la informacion cargada

				return result;
			}
			catch (Exception e)
			{
				throw e;
			}            
		}

		public StreamReader GetErrorFile()
		{
			String path = Path.Combine(Path.Combine(config.FOLDER_RESULTS, _idSimulation.ToString()), config.FILENAME_ERROR);

			return File.OpenText(path);
		}

		public String PathIni { get { return config.FOLDER_INI; } set { config.FOLDER_INI = value; } }
		public String PathDataSet { get { return config.FOLDER_DATASETS; } set { config.FOLDER_DATASETS = value; } }


	}

}

