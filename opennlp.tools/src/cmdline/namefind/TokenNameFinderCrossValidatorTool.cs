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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.namefind;
using opennlp.tools.util.eval;
using opennlp.tools.util.model;

namespace opennlp.tools.cmdline.namefind
{
    public sealed class TokenNameFinderCrossValidatorTool : AbstractCrossValidatorTool<NameSample, TokenNameFinderCrossValidatorTool.CVToolParams>
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

		mlParams = CmdLineUtil.loadTrainingParameters(parameters.Params, false);
		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(parameters.Iterations.Value, parameters.Cutoff.Value);
		}

		sbyte[] featureGeneratorBytes = TokenNameFinderTrainerTool.openFeatureGeneratorBytes(parameters.Featuregen);

		IDictionary<string, object> resources = TokenNameFinderTrainerTool.loadResources(parameters.Resources);

		IList<EvaluationMonitor<NameSample>> listeners = new List<EvaluationMonitor<NameSample>>();
		if (parameters.Misclassified.Value)
		{
		  listeners.Add(new NameEvaluationErrorListener());
		}
		TokenNameFinderDetailedFMeasureListener detailedFListener = null;
		if (parameters.DetailedF.Value)
		{
		  detailedFListener = new TokenNameFinderDetailedFMeasureListener();
		  listeners.Add(detailedFListener);
		}

		TokenNameFinderCrossValidator validator;
		try
		{
		  validator = new TokenNameFinderCrossValidator(parameters.Lang, parameters.Type, mlParams, featureGeneratorBytes, resources, listeners.ToArray() as TokenNameFinderEvaluationMonitor[]);
		  validator.evaluate(sampleStream, parameters.Folds.Value);
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