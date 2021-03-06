﻿using System;
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
using j4n.Serialization;

namespace opennlp.tools.cmdline.namefind
{


	using opennlp.tools.cmdline;
	using EvalToolParams = opennlp.tools.cmdline.namefind.TokenNameFinderEvaluatorTool.EvalToolParams;
	using DetailedFMeasureEvaluatorParams = opennlp.tools.cmdline.@params.DetailedFMeasureEvaluatorParams;
	using EvaluatorParams = opennlp.tools.cmdline.@params.EvaluatorParams;
	using NameFinderME = opennlp.tools.namefind.NameFinderME;
	using NameSample = opennlp.tools.namefind.NameSample;
	using TokenNameFinderEvaluationMonitor = opennlp.tools.namefind.TokenNameFinderEvaluationMonitor;
	using TokenNameFinderEvaluator = opennlp.tools.namefind.TokenNameFinderEvaluator;
	using TokenNameFinderModel = opennlp.tools.namefind.TokenNameFinderModel;
	using opennlp.tools.util;
	using opennlp.tools.util.eval;

	public sealed class TokenNameFinderEvaluatorTool : AbstractEvaluatorTool<NameSample, EvalToolParams>
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

		IList<EvaluationMonitor<NameSample>> listeners = new LinkedList<EvaluationMonitor<NameSample>>();
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

		TokenNameFinderEvaluator evaluator = new TokenNameFinderEvaluator(new NameFinderME(model), listeners.ToArray());

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


		  public virtual NameSample read()
		  {
			monitor.incrementCounter();
			return outerInstance.sampleStream.read();
		  }

		  public virtual void reset()
		  {
			outerInstance.sampleStream.reset();
		  }

		  public virtual void close()
		  {
			outerInstance.sampleStream.close();
		  }
	  }
	}

}