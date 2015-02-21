using System;
using System.Collections.Generic;

namespace PBioDaemonLauncher
{
	public enum ValidationStrategy { holdout, bootstrap, leaveoneout, kfold }

	public class GeneralParameters: IParameter
	{
		private const String HEADER_LINE = "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%";

		private ValidationStrategy VALIDATION_STRATEGY;
		public int NUM_BOXES { get; set; }
		public int NUM_REPETITIONS_CLASSIFIER { get; set; }
		public int NUM_REPETITIONS_HOLD { get; set; }
		public int NUM_REPETITIONS_KFOLD { get; set; }
		public int NUM_ITERACIONES { get; set; }
		public int NUM_BOOSTRAP { get; set; }
		public double PERCENT_TEST { get; set; }

		public GeneralParameters()
		{
			VALIDATION_STRATEGY = ValidationStrategy.holdout;
			NUM_BOXES = 10;
			NUM_REPETITIONS_CLASSIFIER = 1;
			NUM_REPETITIONS_HOLD = 10;
			NUM_REPETITIONS_KFOLD = 5;
			NUM_ITERACIONES = 1;
			NUM_BOOSTRAP = 50;
			PERCENT_TEST = 0.4;
		}

		public static GeneralParameters Defaults()
		{
			GeneralParameters generalParameters = new GeneralParameters();
			generalParameters.VALIDATION_STRATEGY = ValidationStrategy.holdout;
			generalParameters.NUM_BOXES = 10;
			generalParameters.NUM_REPETITIONS_CLASSIFIER = 1;
			generalParameters.NUM_REPETITIONS_HOLD = 10;
			generalParameters.NUM_REPETITIONS_KFOLD = 5;
			generalParameters.NUM_ITERACIONES = 1;
			generalParameters.NUM_BOOSTRAP = 50;
			generalParameters.PERCENT_TEST = 0.4;

			return generalParameters;
		}


		public List<String> ToList()
		{
			List<String> listLines = new List<String>();

			listLines.Add(HEADER_LINE);
			listLines.Add("% GENERAL");
			listLines.Add(HEADER_LINE);
			listLines.Add("VALIDATION_STRATEGY" + VALIDATION_STRATEGY.ToString());
			listLines.Add("NUM_BOXES " + NUM_BOXES);
			listLines.Add("NUM_REPETITIONS_CLASSIFIER" + NUM_REPETITIONS_CLASSIFIER);
			listLines.Add("NUM_REPETITIONS_HOLD" + NUM_REPETITIONS_HOLD);
			listLines.Add("NUM_REPETITIONS_KFOLD" + NUM_REPETITIONS_KFOLD);
			listLines.Add("NUM_ITERACIONES" + NUM_ITERACIONES);
			listLines.Add("NUM_BOOSTRAP" + NUM_BOOSTRAP);
			listLines.Add("PERCENT_TEST" + PERCENT_TEST);     

			return listLines;
		}
	}
}

