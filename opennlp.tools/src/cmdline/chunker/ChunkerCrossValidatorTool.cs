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
using opennlp.tools.util.eval;
using opennlp.tools.util.model;

namespace opennlp.tools.cmdline.chunker
{
    public sealed class ChunkerCrossValidatorTool : AbstractCrossValidatorTool<ChunkSample, ChunkerCrossValidatorTool.CVToolParams>
	{
	    public interface CVToolParams : TrainingParams, CVParams, DetailedFMeasureEvaluatorParams
	  {
	  }

	  public ChunkerCrossValidatorTool() : base(typeof(ChunkSample), typeof(CVToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "K-fold cross validator for the chunker";
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

		ChunkerCrossValidator validator;

		try
		{
		  ChunkerFactory chunkerFactory = ChunkerFactory.create(@params.Factory);

          validator = new ChunkerCrossValidator(@params.Lang, mlParams, chunkerFactory, listeners.ToArray() as ChunkerEvaluationMonitor[]);
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

		if (detailedFMeasureListener == null)
		{
		  FMeasure result = validator.FMeasure;
		  Console.WriteLine(result.ToString());
		}
		else
		{
		  Console.WriteLine(detailedFMeasureListener.ToString());
		}
	  }
	}

}