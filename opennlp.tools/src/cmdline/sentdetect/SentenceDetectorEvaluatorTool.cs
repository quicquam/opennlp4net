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
using opennlp.tools.sentdetect;

namespace opennlp.tools.cmdline.sentdetect
{
    public sealed class SentenceDetectorEvaluatorTool : AbstractEvaluatorTool<SentenceSample, SentenceDetectorEvaluatorTool.EvalToolParams>
	{
	  public class EvalToolParams : EvaluatorParams
	  {
	      public Jfile Model { get; private set; }
          [OptionalAttribute]
          public bool? Misclassified { get; private set; }
	  }

	  public SentenceDetectorEvaluatorTool() : base(typeof(SentenceSample), typeof(EvalToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "evaluator for the learnable sentence detector";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		SentenceModel model = (new SentenceModelLoader()).load(parameters.Model);

		SentenceDetectorEvaluationMonitor errorListener = null;
		if (parameters.Misclassified.Value)
		{
		  errorListener = new SentenceEvaluationErrorListener();
		}

		SentenceDetectorEvaluator evaluator = new SentenceDetectorEvaluator(new SentenceDetectorME(model), errorListener);

		Console.Write("Evaluating ... ");
		try
		{
		evaluator.evaluate(sampleStream);
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

		Console.Error.WriteLine("done");

		Console.WriteLine();

		Console.WriteLine(evaluator.FMeasure);
	  }
	}

}