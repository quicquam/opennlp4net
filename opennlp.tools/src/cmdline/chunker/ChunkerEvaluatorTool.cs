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
using opennlp.tools.chunker;
using opennlp.tools.cmdline.@params;
using opennlp.tools.util;
using opennlp.tools.util.eval;

namespace opennlp.tools.cmdline.chunker
{
    public sealed class ChunkerEvaluatorTool : AbstractEvaluatorTool<ChunkSample, ChunkerEvaluatorTool.EvalToolParams>
	{
	    public interface EvalToolParams : EvaluatorParams, DetailedFMeasureEvaluatorParams
	  {
	  }

	  public ChunkerEvaluatorTool() : base(typeof(ChunkSample), typeof(EvalToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "Measures the performance of the Chunker model with the reference data";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		ChunkerModel model = (new ChunkerModelLoader()).load(@params.Model);

		IList<EvaluationMonitor<ChunkSample>> listeners = new List<EvaluationMonitor<ChunkSample>>();
		ChunkerDetailedFMeasureListener detailedFMeasureListener = null;
		if (@params.Misclassified.Value)
		{
		  listeners.Add(new ChunkEvaluationErrorListener());
		}
		if (@params.DetailedF.Value)
		{
		  detailedFMeasureListener = new ChunkerDetailedFMeasureListener();
		  listeners.Add(detailedFMeasureListener);
		}

        ChunkerEvaluator evaluator = new ChunkerEvaluator(new ChunkerME(model, ChunkerME.DEFAULT_BEAM_SIZE), listeners.ToArray() as ChunkerEvaluationMonitor[]);

		PerformanceMonitor monitor = new PerformanceMonitor("sent");

		ObjectStream<ChunkSample> measuredSampleStream = new ObjectStreamAnonymousInnerClassHelper(this, monitor);

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

		if (detailedFMeasureListener == null)
		{
		  Console.WriteLine(evaluator.FMeasure);
		}
		else
		{
		  Console.WriteLine(detailedFMeasureListener.ToString());
		}
	  }

	  private class ObjectStreamAnonymousInnerClassHelper : ObjectStream<ChunkSample>
	  {
		  private readonly ChunkerEvaluatorTool outerInstance;

		  private PerformanceMonitor monitor;

		  public ObjectStreamAnonymousInnerClassHelper(ChunkerEvaluatorTool outerInstance, PerformanceMonitor monitor)
		  {
			  this.outerInstance = outerInstance;
			  this.monitor = monitor;
		  }


		  public override ChunkSample read()
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