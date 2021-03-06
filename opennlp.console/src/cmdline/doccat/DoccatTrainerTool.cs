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

namespace opennlp.tools.cmdline.doccat
{


	using opennlp.tools.cmdline;
	using TrainerToolParams = opennlp.tools.cmdline.doccat.DoccatTrainerTool.TrainerToolParams;
	using TrainingToolParams = opennlp.tools.cmdline.@params.TrainingToolParams;
	using DoccatModel = opennlp.tools.doccat.DoccatModel;
	using DocumentCategorizerME = opennlp.tools.doccat.DocumentCategorizerME;
	using DocumentSample = opennlp.tools.doccat.DocumentSample;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	public class DoccatTrainerTool : AbstractTrainerTool<DocumentSample, TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams
	  {
	  }

	  public DoccatTrainerTool() : base(typeof(DocumentSample), typeof(TrainerToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "trainer for the learnable document categorizer";
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

		CmdLineUtil.checkOutputFile("document categorizer model", modelOutFile);

		DoccatModel model;
		try
		{
		  model = DocumentCategorizerME.train(@params.Lang, sampleStream, mlParams);
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

		CmdLineUtil.writeModel("document categorizer", modelOutFile, model);
	  }
	}

}