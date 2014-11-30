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
using System.IO;

namespace opennlp.tools.cmdline.sentdetect
{

	using opennlp.tools.cmdline;
	using EvaluatorParams = opennlp.tools.cmdline.@params.EvaluatorParams;
	using EvalToolParams = opennlp.tools.cmdline.sentdetect.SentenceDetectorEvaluatorTool.EvalToolParams;
	using SentenceDetectorEvaluationMonitor = opennlp.tools.sentdetect.SentenceDetectorEvaluationMonitor;
	using SentenceDetectorEvaluator = opennlp.tools.sentdetect.SentenceDetectorEvaluator;
	using SentenceDetectorME = opennlp.tools.sentdetect.SentenceDetectorME;
	using SentenceModel = opennlp.tools.sentdetect.SentenceModel;
	using SentenceSample = opennlp.tools.sentdetect.SentenceSample;

	public sealed class SentenceDetectorEvaluatorTool : AbstractEvaluatorTool<SentenceSample, EvalToolParams>
	{
	    public interface EvalToolParams : EvaluatorParams
	  {
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

		SentenceModel model = (new SentenceModelLoader()).load(@params.Model);

		SentenceDetectorEvaluationMonitor errorListener = null;
		if (@params.Misclassified.Value)
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