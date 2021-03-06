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

namespace opennlp.tools.cmdline.coref
{

	using opennlp.tools.cmdline;
	using TrainingToolParams = opennlp.tools.cmdline.@params.TrainingToolParams;
	using TrainerToolParams = opennlp.tools.cmdline.coref.CoreferencerTrainerTool.TrainerToolParams;
	using CorefSample = opennlp.tools.coref.CorefSample;
	using CorefTrainer = opennlp.tools.coref.CorefTrainer;

	public class CoreferencerTrainerTool : AbstractTrainerTool<CorefSample, TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams
	  {
	  }

	  public CoreferencerTrainerTool() : base(typeof(CorefSample), typeof(TrainerToolParams))
	  {
	  }

	  public override void run(string format, string[] args)
	  {

		base.run(format, args);

		try
		{
		  CorefTrainer.train(@params.Model.ToString(), sampleStream, true, true);
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while reading training data or indexing data: " + e.Message, e);
		}
	  }

	}

}