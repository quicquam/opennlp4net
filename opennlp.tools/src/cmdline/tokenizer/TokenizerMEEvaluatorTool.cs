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
using j4n.IO.File;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.tokenize;

namespace opennlp.tools.cmdline.tokenizer
{
    public sealed class TokenizerMEEvaluatorTool : AbstractEvaluatorTool<TokenSample, TokenizerMEEvaluatorTool.EvalToolParams>
	{
	  public class EvalToolParams : EvaluatorParams
	  {
	      public Jfile Model { get; private set; }
	      public bool? Misclassified { get; private set; }
	  }

	  public TokenizerMEEvaluatorTool() : base(typeof(TokenSample), typeof(EvalToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "evaluator for the learnable tokenizer";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		TokenizerModel model = (new TokenizerModelLoader()).load(parameters.Model);

		TokenizerEvaluationMonitor misclassifiedListener = null;
		if (parameters.Misclassified.Value)
		{
		  misclassifiedListener = new TokenEvaluationErrorListener();
		}

		TokenizerEvaluator evaluator = new TokenizerEvaluator(new opennlp.tools.tokenize.TokenizerME(model), misclassifiedListener);

		Console.Write("Evaluating ... ");

		try
		{
		  evaluator.evaluate(sampleStream);
		}
		catch (IOException e)
		{
		  Console.Error.WriteLine("failed");
		  throw new TerminateToolException(-1, "IO error while reading test data: " + e.Message, e);
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

		Console.WriteLine(evaluator.FMeasure);
	  }
	}

}