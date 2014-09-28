using System;
using System.Collections.Generic;
/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
using j4n.Exceptions;

namespace opennlp.model
{


	using QNTrainer = opennlp.maxent.quasinewton.QNTrainer;
	using PerceptronTrainer = opennlp.perceptron.PerceptronTrainer;
	using SimplePerceptronSequenceTrainer = opennlp.perceptron.SimplePerceptronSequenceTrainer;

	public class TrainUtil
	{

	  public const string ALGORITHM_PARAM = "Algorithm";

	  public const string MAXENT_VALUE = "MAXENT";
	  public const string MAXENT_QN_VALUE = "MAXENT_QN_EXPERIMENTAL";
	  public const string PERCEPTRON_VALUE = "PERCEPTRON";
	  public const string PERCEPTRON_SEQUENCE_VALUE = "PERCEPTRON_SEQUENCE";


	  public const string CUTOFF_PARAM = "Cutoff";
	  private const int CUTOFF_DEFAULT = 5;

	  public const string ITERATIONS_PARAM = "Iterations";
	  private const int ITERATIONS_DEFAULT = 100;

	  public const string DATA_INDEXER_PARAM = "DataIndexer";
	  public const string DATA_INDEXER_ONE_PASS_VALUE = "OnePass";
	  public const string DATA_INDEXER_TWO_PASS_VALUE = "TwoPass";


	  private static string getStringParam(IDictionary<string, string> trainParams, string key, string defaultValue, IDictionary<string, string> reportMap)
	  {

		string valueString = trainParams[key];

		if (valueString == null)
		{
		  valueString = defaultValue;
		}

		if (reportMap != null)
		{
		  reportMap[key] = valueString;
		}

		return valueString;
	  }

	  private static int getIntParam(IDictionary<string, string> trainParams, string key, int defaultValue, IDictionary<string, string> reportMap)
	  {

		string valueString = trainParams[key];

		if (valueString != null)
		{
		  return Convert.ToInt32(valueString);
		}
		else
		{
		  return defaultValue;
		}
	  }

	  private static double getDoubleParam(IDictionary<string, string> trainParams, string key, double defaultValue, IDictionary<string, string> reportMap)
	  {

		string valueString = trainParams[key];

		if (valueString != null)
		{
		  return Convert.ToDouble(valueString);
		}
		else
		{
		  return defaultValue;
		}
	  }

	  private static bool getBooleanParam(IDictionary<string, string> trainParams, string key, bool defaultValue, IDictionary<string, string> reportMap)
	  {

		string valueString = trainParams[key];

		if (valueString != null)
		{
		  return Convert.ToBoolean(valueString);
		}
		else
		{
		  return defaultValue;
		}
	  }

	  public static bool isValid(IDictionary<string, string> trainParams)
	  {

		// TODO: Need to validate all parameters correctly ... error prone?!

		string algorithmName = trainParams[ALGORITHM_PARAM];

		if (algorithmName != null && !(MAXENT_VALUE.Equals(algorithmName) || MAXENT_QN_VALUE.Equals(algorithmName) || PERCEPTRON_VALUE.Equals(algorithmName) || PERCEPTRON_SEQUENCE_VALUE.Equals(algorithmName)))
		{
		  return false;
		}

		try
		{
		  string cutoffString = trainParams[CUTOFF_PARAM];
		  if (cutoffString != null)
		  {
			  Convert.ToInt32(cutoffString);
		  }

		  string iterationsString = trainParams[ITERATIONS_PARAM];
		  if (iterationsString != null)
		  {
			  Convert.ToInt32(iterationsString);
		  }
		}
		catch (NumberFormatException)
		{
		  return false;
		}

		string dataIndexer = trainParams[DATA_INDEXER_PARAM];

		if (dataIndexer != null)
		{
		  if (!("OnePass".Equals(dataIndexer) || "TwoPass".Equals(dataIndexer)))
		  {
			return false;
		  }
		}

		// TODO: Check data indexing ... 

		return true;
	  }



	  // TODO: Need a way to report results and settings back for inclusion in model ...

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static AbstractModel train(EventStream events, java.util.Map<String, String> trainParams, java.util.Map<String, String> reportMap) throws java.io.IOException
	  public static AbstractModel train(EventStream events, IDictionary<string, string> trainParams, IDictionary<string, string> reportMap)
	  {

		if (!isValid(trainParams))
		{
			throw new System.ArgumentException("trainParams are not valid!");
		}

		if (isSequenceTraining(trainParams))
		{
		  throw new System.ArgumentException("sequence training is not supported by this method!");
		}

		string algorithmName = getStringParam(trainParams, ALGORITHM_PARAM, MAXENT_VALUE, reportMap);

		int iterations = getIntParam(trainParams, ITERATIONS_PARAM, ITERATIONS_DEFAULT, reportMap);

		int cutoff = getIntParam(trainParams, CUTOFF_PARAM, CUTOFF_DEFAULT, reportMap);

		bool sortAndMerge;

		if (MAXENT_VALUE.Equals(algorithmName) || MAXENT_QN_VALUE.Equals(algorithmName))
		{
		  sortAndMerge = true;
		}
		else if (PERCEPTRON_VALUE.Equals(algorithmName))
		{
		  sortAndMerge = false;
		}
		else
		{
		  throw new IllegalStateException("Unexpected algorithm name: " + algorithmName);
		}

		HashSumEventStream hses = new HashSumEventStream(events);

		string dataIndexerName = getStringParam(trainParams, DATA_INDEXER_PARAM, DATA_INDEXER_TWO_PASS_VALUE, reportMap);

		DataIndexer indexer = null;

		if (DATA_INDEXER_ONE_PASS_VALUE.Equals(dataIndexerName))
		{
		  indexer = new OnePassDataIndexer(hses, cutoff, sortAndMerge);
		}
		else if (DATA_INDEXER_TWO_PASS_VALUE.Equals(dataIndexerName))
		{
		  indexer = new TwoPassDataIndexer(hses, cutoff, sortAndMerge);
		}
		else
		{
		  throw new IllegalStateException("Unexpected data indexer name: " + dataIndexerName);
		}

		AbstractModel model;
		if (MAXENT_VALUE.Equals(algorithmName))
		{

		  int threads = getIntParam(trainParams, "Threads", 1, reportMap);

		  model = opennlp.maxent.GIS.trainModel(iterations, indexer, true, false, null, 0, threads);
		}
		else if (MAXENT_QN_VALUE.Equals(algorithmName))
		{
		  int m = getIntParam(trainParams, "numOfUpdates", QNTrainer.DEFAULT_M, reportMap);
		  int maxFctEval = getIntParam(trainParams, "maxFctEval", QNTrainer.DEFAULT_MAX_FCT_EVAL, reportMap);
		  model = (new QNTrainer(m, maxFctEval, true)).trainModel(indexer);
		}
		else if (PERCEPTRON_VALUE.Equals(algorithmName))
		{
		  bool useAverage = getBooleanParam(trainParams, "UseAverage", true, reportMap);

		  bool useSkippedAveraging = getBooleanParam(trainParams, "UseSkippedAveraging", false, reportMap);

		  // overwrite otherwise it might not work
		  if (useSkippedAveraging)
		  {
			useAverage = true;
		  }

		  double stepSizeDecrease = getDoubleParam(trainParams, "StepSizeDecrease", 0, reportMap);

		  double tolerance = getDoubleParam(trainParams, "Tolerance", PerceptronTrainer.TOLERANCE_DEFAULT, reportMap);

		  PerceptronTrainer perceptronTrainer = new PerceptronTrainer();
		  perceptronTrainer.SkippedAveraging = useSkippedAveraging;

		  if (stepSizeDecrease > 0)
		  {
			perceptronTrainer.StepSizeDecrease = stepSizeDecrease;
		  }

		  perceptronTrainer.Tolerance = tolerance;

		  model = perceptronTrainer.trainModel(iterations, indexer, cutoff, useAverage);
		}
		else
		{
		  throw new IllegalStateException("Algorithm not supported: " + algorithmName);
		}

		if (reportMap != null)
		{
            reportMap["Training-Eventhash"] = hses.calculateHashSum().ToString("X"); // 16 Java : i.e. Hex
		}

		return model;
	  }

	  /// <summary>
	  /// Detects if the training algorithm requires sequence based feature generation
	  /// or not.
	  /// </summary>
	  public static bool isSequenceTraining(IDictionary<string, string> trainParams)
	  {
		return PERCEPTRON_SEQUENCE_VALUE.Equals(trainParams[ALGORITHM_PARAM]);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static AbstractModel train(SequenceStream events, java.util.Map<String, String> trainParams, java.util.Map<String, String> reportMap) throws java.io.IOException
	  public static AbstractModel train(SequenceStream<Event> events, IDictionary<string, string> trainParams, IDictionary<string, string> reportMap)
	  {

		if (!isValid(trainParams))
		{
		  throw new System.ArgumentException("trainParams are not valid!");
		}

		if (!isSequenceTraining(trainParams))
		{
		  throw new System.ArgumentException("Algorithm must be a sequence algorithm!");
		}

		int iterations = getIntParam(trainParams, ITERATIONS_PARAM, ITERATIONS_DEFAULT, reportMap);
		int cutoff = getIntParam(trainParams, CUTOFF_PARAM, CUTOFF_DEFAULT, reportMap);

		bool useAverage = getBooleanParam(trainParams, "UseAverage", true, reportMap);

		return (new SimplePerceptronSequenceTrainer()).trainModel(iterations, events, cutoff,useAverage);
	  }
	}

}