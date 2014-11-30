using System;
using System.Collections.Generic;
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
using System.Linq;

namespace opennlp.tools.cmdline.namefind
{


	using opennlp.tools.cmdline;
	using CVToolParams = opennlp.tools.cmdline.namefind.TokenNameFinderCrossValidatorTool.CVToolParams;
	using CVParams = opennlp.tools.cmdline.@params.CVParams;
	using DetailedFMeasureEvaluatorParams = opennlp.tools.cmdline.@params.DetailedFMeasureEvaluatorParams;
	using NameSample = opennlp.tools.namefind.NameSample;
	using TokenNameFinderCrossValidator = opennlp.tools.namefind.TokenNameFinderCrossValidator;
	using TokenNameFinderEvaluationMonitor = opennlp.tools.namefind.TokenNameFinderEvaluationMonitor;
	using opennlp.tools.util.eval;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	public sealed class TokenNameFinderCrossValidatorTool : AbstractCrossValidatorTool<NameSample, CVToolParams>
	{
	    public interface CVToolParams : TrainingParams, CVParams, DetailedFMeasureEvaluatorParams
	  {
	  }

	  public TokenNameFinderCrossValidatorTool() : base(typeof(NameSample), typeof(CVToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "K-fold cross validator for the learnable Name Finder";
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

		sbyte[] featureGeneratorBytes = TokenNameFinderTrainerTool.openFeatureGeneratorBytes(@params.Featuregen);

		IDictionary<string, object> resources = TokenNameFinderTrainerTool.loadResources(@params.Resources);

		IList<EvaluationMonitor<NameSample>> listeners = new List<EvaluationMonitor<NameSample>>();
		if (@params.Misclassified.Value)
		{
		  listeners.Add(new NameEvaluationErrorListener());
		}
		TokenNameFinderDetailedFMeasureListener detailedFListener = null;
		if (@params.DetailedF.Value)
		{
		  detailedFListener = new TokenNameFinderDetailedFMeasureListener();
		  listeners.Add(detailedFListener);
		}

		TokenNameFinderCrossValidator validator;
		try
		{
		  validator = new TokenNameFinderCrossValidator(@params.Lang, @params.Type, mlParams, featureGeneratorBytes, resources, listeners.ToArray() as TokenNameFinderEvaluationMonitor[]);
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

		Console.WriteLine("done");

		Console.WriteLine();

		if (detailedFListener == null)
		{
		  Console.WriteLine(validator.FMeasure);
		}
		else
		{
		  Console.WriteLine(detailedFListener.ToString());
		}
	  }
	}

}