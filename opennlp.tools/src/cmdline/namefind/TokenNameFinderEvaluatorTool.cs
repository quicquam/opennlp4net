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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using opennlp.tools.cmdline.@params;
using opennlp.tools.namefind;
using opennlp.tools.util;
using opennlp.tools.util.eval;

namespace opennlp.tools.cmdline.namefind
{
    public sealed class TokenNameFinderEvaluatorTool : AbstractEvaluatorTool<NameSample, TokenNameFinderEvaluatorTool.EvalToolParams>
	{
	    public interface EvalToolParams : EvaluatorParams, DetailedFMeasureEvaluatorParams
	  {
	  }

	  public TokenNameFinderEvaluatorTool() : base(typeof(NameSample), typeof(EvalToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "Measures the performance of the NameFinder model with the reference data";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		TokenNameFinderModel model = (new TokenNameFinderModelLoader()).load(@params.Model);

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

		TokenNameFinderEvaluator evaluator = new TokenNameFinderEvaluator(new NameFinderME(model), listeners.ToArray() as TokenNameFinderEvaluationMonitor[]);

		PerformanceMonitor monitor = new PerformanceMonitor("sent");

		ObjectStream<NameSample> measuredSampleStream = new ObjectStreamAnonymousInnerClassHelper(this, monitor);

		monitor.startAndPrintThroughput();

		try
		{
		  evaluator.evaluate(measuredSampleStream);
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
			measuredSampleStream.close();
		  }
		  catch (IOException)
		  {
			// sorry that this can fail
		  }
		}

		monitor.stopAndPrintFinalResult();

		Console.WriteLine();

		if (detailedFListener == null)
		{
		  Console.WriteLine(evaluator.FMeasure);
		}
		else
		{
		  Console.WriteLine(detailedFListener.ToString());
		}
	  }

	  private class ObjectStreamAnonymousInnerClassHelper : ObjectStream<NameSample>
	  {
		  private readonly TokenNameFinderEvaluatorTool outerInstance;

		  private PerformanceMonitor monitor;

		  public ObjectStreamAnonymousInnerClassHelper(TokenNameFinderEvaluatorTool outerInstance, PerformanceMonitor monitor)
		  {
			  this.outerInstance = outerInstance;
			  this.monitor = monitor;
		  }


		  public override NameSample read()
		  {
			monitor.incrementCounter();
			return outerInstance.sampleStream.read();
		  }

          public override void reset()
		  {
			outerInstance.sampleStream.reset();
		  }

          public override void close()
		  {
			outerInstance.sampleStream.close();
		  }
	  }
	}

}