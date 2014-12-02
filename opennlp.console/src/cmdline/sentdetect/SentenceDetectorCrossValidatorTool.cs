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
using System;
using System.IO;
using opennlp.console.cmdline.@params;
using opennlp.tools.dictionary;
using opennlp.tools.sentdetect;
using opennlp.tools.util.eval;
using opennlp.tools.util.model;

namespace opennlp.console.cmdline.sentdetect
{
    public sealed class SentenceDetectorCrossValidatorTool : AbstractCrossValidatorTool<SentenceSample, SentenceDetectorCrossValidatorTool.CVToolParams>
	{
	    public interface CVToolParams : TrainingParams, CVParams
	  {
	  }

	  public SentenceDetectorCrossValidatorTool() : base(typeof(SentenceSample), typeof(CVToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "K-fold cross validator for the learnable sentence detector";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		mlParams = CmdLineUtil.loadTrainingParameters(@params.Params, false);
		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(@params.Iterations.Value, @params.Cutoff.Value);
		}

		SDCrossValidator validator;

		SentenceDetectorEvaluationMonitor errorListener = null;
		if (@params.Misclassified.Value)
		{
		  errorListener = new SentenceEvaluationErrorListener();
		}

		char[] eos = null;
		if (@params.EosChars != null)
		{
		  eos = @params.EosChars.ToCharArray();
		}

		try
		{
		  Dictionary abbreviations = SentenceDetectorTrainerTool.loadDict(@params.AbbDict);
		  SentenceDetectorFactory sdFactory = SentenceDetectorFactory.create(@params.Factory, @params.Lang, true, abbreviations, eos);
		  validator = new SDCrossValidator(@params.Lang, mlParams, sdFactory, errorListener);

		  validator.evaluate(sampleStream, @params.Folds.Value);
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

		FMeasure result = validator.FMeasure;

		Console.WriteLine(result.ToString());
	  }
	}

}