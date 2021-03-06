﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using j4n.IO.File;
using opennlp.model;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.dictionary;
using opennlp.tools.postag;
using opennlp.tools.util;
using opennlp.tools.util.model;

namespace opennlp.tools.cmdline.postag
{
    public sealed class POSTaggerTrainerTool : AbstractTrainerTool<POSSample, POSTaggerTrainerTool.TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams
	  {
	  }

	  public POSTaggerTrainerTool() : base(typeof(POSSample), typeof(TrainerToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "trains a model for the part-of-speech tagger";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		mlParams = CmdLineUtil.loadTrainingParameters(parameters.Params, true);
		if (mlParams != null && !TrainUtil.isValid(mlParams.getSettings()))
		{
		  throw new TerminateToolException(1, "Training parameters file '" + parameters.Params + "' is invalid!");
		}

		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(parameters.Iterations.Value, parameters.Cutoff.Value);
		  mlParams.put(TrainingParameters.ALGORITHM_PARAM, getModelType(parameters.Type).ToString());
		}

		Jfile modelOutFile = parameters.Model;
		CmdLineUtil.checkOutputFile("pos tagger model", modelOutFile);

		Dictionary ngramDict = null;

		int? ngramCutoff = parameters.Ngram;

		if (ngramCutoff != null)
		{
		  Console.Error.Write("Building ngram dictionary ... ");
		  try
		  {
			ngramDict = POSTaggerME.buildNGramDictionary(sampleStream, ngramCutoff.Value);
			sampleStream.reset();
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "IO error while building NGram Dictionary: " + e.Message, e);
		  }
		  Console.Error.WriteLine("done");
		}

		POSTaggerFactory postaggerFactory = null;
		try
		{
		  postaggerFactory = POSTaggerFactory.create(parameters.Factory, ngramDict, null);
		}
		catch (InvalidFormatException e)
		{
		  throw new TerminateToolException(-1, e.Message, e);
		}

		if (parameters.Dict != null)
		{
		  try
		  {
			postaggerFactory.TagDictionary = postaggerFactory.createTagDictionary(parameters.Dict);
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "IO error while loading POS Dictionary: " + e.Message, e);
		  }
		}

		if (parameters.TagDictCutoff != null)
		{
		  try
		  {
			TagDictionary dict = postaggerFactory.TagDictionary;
			if (dict == null)
			{
			  dict = postaggerFactory.createEmptyTagDictionary();
			  postaggerFactory.TagDictionary = dict;
			}
			if (dict is MutableTagDictionary)
			{
			  POSTaggerME.populatePOSDictionary(sampleStream, (MutableTagDictionary)dict, parameters.TagDictCutoff.Value);
			}
			else
			{
			  throw new System.ArgumentException("Can't extend a POSDictionary that does not implement MutableTagDictionary.");
			}
			sampleStream.reset();
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "IO error while creating/extending POS Dictionary: " + e.Message, e);
		  }
		}

		POSModel model;
		try
		{
		  model = POSTaggerME.train(parameters.Lang, sampleStream, mlParams, postaggerFactory);
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while reading training data or indexing data: " + e.Message, e);
		}
		finally
		{
		  try
		  {
			sampleStream.close();
		  }
		  catch (IOException)
		  {
			// sorry that this can fail
		  }
		}

		CmdLineUtil.writeModel("pos tagger", modelOutFile, model);
	  }

	  internal static ModelType getModelType(string modelString)
	  {
		ModelType model = ModelType.MAXENT;
		if (modelString == null)
		{
		  modelString = "maxent";
		}

		if (modelString.Equals("maxent"))
		{
		  model = ModelType.MAXENT;
		}
		else if (modelString.Equals("perceptron"))
		{
		  model = ModelType.PERCEPTRON;
		}
		else if (modelString.Equals("perceptron_sequence"))
		{
		  model = ModelType.PERCEPTRON_SEQUENCE;
		}

		return model;
	  }
	}

}