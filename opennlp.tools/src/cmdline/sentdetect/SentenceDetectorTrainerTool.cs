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

using System.IO;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace opennlp.tools.cmdline.sentdetect
{


	using TrainUtil = opennlp.model.TrainUtil;
	using opennlp.tools.cmdline;
	using TrainingToolParams = opennlp.tools.cmdline.@params.TrainingToolParams;
	using TrainerToolParams = opennlp.tools.cmdline.sentdetect.SentenceDetectorTrainerTool.TrainerToolParams;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using SentenceDetectorFactory = opennlp.tools.sentdetect.SentenceDetectorFactory;
	using SentenceDetectorME = opennlp.tools.sentdetect.SentenceDetectorME;
	using SentenceModel = opennlp.tools.sentdetect.SentenceModel;
	using SentenceSample = opennlp.tools.sentdetect.SentenceSample;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	public sealed class SentenceDetectorTrainerTool : AbstractTrainerTool<SentenceSample, TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams
	  {
	  }

	  public SentenceDetectorTrainerTool() : base(typeof(SentenceSample), typeof(TrainerToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "trainer for the learnable sentence detector";
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static opennlp.tools.dictionary.Dictionary loadDict(java.io.File f) throws java.io.IOException
	  internal static Dictionary loadDict(Jfile f)
	  {
		Dictionary dict = null;
		if (f != null)
		{
		  CmdLineUtil.checkInputFile("abb dict", f);
		  dict = new Dictionary(new FileInputStream(f));
		}
		return dict;
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		mlParams = CmdLineUtil.loadTrainingParameters(@params.Params, false);

		if (mlParams != null)
		{
		  if (TrainUtil.isSequenceTraining(mlParams.Settings))
		  {
			throw new TerminateToolException(1, "Sequence training is not supported!");
		  }
		}

		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(@params.Iterations.Value, @params.Cutoff.Value);
		}

		Jfile modelOutFile = @params.Model;
		CmdLineUtil.checkOutputFile("sentence detector model", modelOutFile);

		char[] eos = null;
		if (@params.EosChars != null)
		{
		  eos = @params.EosChars.ToCharArray();
		}

		SentenceModel model;

		try
		{
		  Dictionary dict = loadDict(@params.AbbDict);
		  SentenceDetectorFactory sdFactory = SentenceDetectorFactory.create(@params.Factory, @params.Lang, true, dict, eos);
		  model = SentenceDetectorME.train(@params.Lang, sampleStream, sdFactory, mlParams);
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

		CmdLineUtil.writeModel("sentence detector", modelOutFile, model);
	  }
	}

}