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
using opennlp.tools.cmdline.parameters;
using opennlp.tools.dictionary;
using opennlp.tools.tokenize;
using opennlp.tools.util.eval;
using opennlp.tools.util.model;

namespace opennlp.tools.cmdline.tokenizer
{
    public sealed class TokenizerCrossValidatorTool : AbstractCrossValidatorTool<TokenSample, TokenizerCrossValidatorTool.CVToolParams>
	{
	    public interface CVToolParams : CVParams, TrainingParams
	  {
	  }

	  public TokenizerCrossValidatorTool() : base(typeof(TokenSample), typeof(CVToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "K-fold cross validator for the learnable tokenizer";
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

		TokenizerCrossValidator validator;

		TokenizerEvaluationMonitor listener = null;
		if (parameters.Misclassified.Value)
		{
		  listener = new TokenEvaluationErrorListener();
		}

		try
		{
		  Dictionary dict = TokenizerTrainerTool.loadDict(parameters.AbbDict);

		  TokenizerFactory tokFactory = TokenizerFactory.create(parameters.Factory, parameters.Lang, dict, parameters.AlphaNumOpt.Value, null);
		  validator = new TokenizerCrossValidator(mlParams, tokFactory, listener);

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

		FMeasure result = validator.FMeasure;

		Console.WriteLine(result.ToString());
	  }
	}

}