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

namespace opennlp.tools.cmdline.chunker
{


	using ChunkSample = opennlp.tools.chunker.ChunkSample;
	using ChunkerFactory = opennlp.tools.chunker.ChunkerFactory;
	using ChunkerME = opennlp.tools.chunker.ChunkerME;
	using ChunkerModel = opennlp.tools.chunker.ChunkerModel;
	using opennlp.tools.cmdline;
	using TrainerToolParams = opennlp.tools.cmdline.chunker.ChunkerTrainerTool.TrainerToolParams;
	using TrainingToolParams = opennlp.tools.cmdline.@params.TrainingToolParams;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	public class ChunkerTrainerTool : AbstractTrainerTool<ChunkSample, TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams
	  {
	  }

	  public ChunkerTrainerTool() : base(typeof(ChunkSample), typeof(TrainerToolParams))
	  {
	  }

	  public override string Name
	  {
		  get
		  {
			return "ChunkerTrainerME";
		  }
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "trainer for the learnable chunker";
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

		File modelOutFile = @params.Model;
		CmdLineUtil.checkOutputFile("sentence detector model", modelOutFile);

		ChunkerModel model;
		try
		{
		  ChunkerFactory chunkerFactory = ChunkerFactory.create(@params.Factory);
		  model = ChunkerME.train(@params.Lang, sampleStream, mlParams, chunkerFactory);
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

		CmdLineUtil.writeModel("chunker", modelOutFile, model);
	  }
	}

}