using System;

/*
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

namespace opennlp.tools.cmdline.postag
{


	using TrainUtil = opennlp.model.TrainUtil;
	using opennlp.tools.cmdline;
	using TrainingToolParams = opennlp.tools.cmdline.@params.TrainingToolParams;
	using TrainerToolParams = opennlp.tools.cmdline.postag.POSTaggerTrainerTool.TrainerToolParams;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using MutableTagDictionary = opennlp.tools.postag.MutableTagDictionary;
	using POSModel = opennlp.tools.postag.POSModel;
	using POSSample = opennlp.tools.postag.POSSample;
	using POSTaggerFactory = opennlp.tools.postag.POSTaggerFactory;
	using POSTaggerME = opennlp.tools.postag.POSTaggerME;
	using TagDictionary = opennlp.tools.postag.TagDictionary;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using TrainingParameters = opennlp.tools.util.TrainingParameters;
	using ModelType = opennlp.tools.util.model.ModelType;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	public sealed class POSTaggerTrainerTool : AbstractTrainerTool<POSSample, TrainerToolParams>
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

		mlParams = CmdLineUtil.loadTrainingParameters(@params.Params, true);
		if (mlParams != null && !TrainUtil.isValid(mlParams.Settings))
		{
		  throw new TerminateToolException(1, "Training parameters file '" + @params.Params + "' is invalid!");
		}

		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(@params.Iterations.Value, @params.Cutoff.Value);
		  mlParams.put(TrainingParameters.ALGORITHM_PARAM, getModelType(@params.Type).ToString());
		}

		File modelOutFile = @params.Model;
		CmdLineUtil.checkOutputFile("pos tagger model", modelOutFile);

		Dictionary ngramDict = null;

		int? ngramCutoff = @params.Ngram;

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
		  postaggerFactory = POSTaggerFactory.create(@params.Factory, ngramDict, null);
		}
		catch (InvalidFormatException e)
		{
		  throw new TerminateToolException(-1, e.Message, e);
		}

		if (@params.Dict != null)
		{
		  try
		  {
			postaggerFactory.TagDictionary = postaggerFactory.createTagDictionary(@params.Dict);
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "IO error while loading POS Dictionary: " + e.Message, e);
		  }
		}

		if (@params.TagDictCutoff != null)
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
			  POSTaggerME.populatePOSDictionary(sampleStream, (MutableTagDictionary)dict, @params.TagDictCutoff.Value);
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
		  model = POSTaggerME.train(@params.Lang, sampleStream, mlParams, postaggerFactory);
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
		ModelType model;
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
		else
		{
		  model = null;
		}
		return model;
	  }
	}

}