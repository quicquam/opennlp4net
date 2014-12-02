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
using j4n.IO.File;
using j4n.IO.InputStream;
using opennlp.console.cmdline.@params;
using opennlp.model;
using opennlp.tools.dictionary;
using opennlp.tools.tokenize;
using opennlp.tools.util.model;

namespace opennlp.console.cmdline.tokenizer
{
    public sealed class TokenizerTrainerTool : AbstractTrainerTool<TokenSample, TokenizerTrainerTool.TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams
	  {
	  }

	  public TokenizerTrainerTool() : base(typeof(TokenSample), typeof(TrainerToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "trainer for the learnable tokenizer";
		  }
	  }

	  internal static Dictionary loadDict(Jfile f)
	  {
		Dictionary dict = null;
		if (f != null)
		{
		  CmdLineUtil.checkInputFile("abb dict", f);
		  dict = new Dictionary(new FileInputStream(f));
		}
		return dict;
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		mlParams = CmdLineUtil.loadTrainingParameters(@params.Params, false);

		if (mlParams != null)
		{
		  if (!TrainUtil.isValid(mlParams.getSettings()))
		  {
			throw new TerminateToolException(1, "Training parameters file '" + @params.Params + "' is invalid!");
		  }

		  if (TrainUtil.isSequenceTraining(mlParams.getSettings()))
		  {
			throw new TerminateToolException(1, "Sequence training is not supported!");
		  }
		}

		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(@params.Iterations.Value, @params.Cutoff.Value);
		}

		Jfile modelOutFile = @params.Model;
		CmdLineUtil.checkOutputFile("tokenizer model", modelOutFile);

		TokenizerModel model;
		try
		{
		  Dictionary dict = loadDict(@params.AbbDict);

		  TokenizerFactory tokFactory = TokenizerFactory.create(@params.Factory, @params.Lang, dict, @params.AlphaNumOpt.Value, null);
		  model = opennlp.tools.tokenize.TokenizerME.train(sampleStream, tokFactory, mlParams);

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

		CmdLineUtil.writeModel("tokenizer", modelOutFile, model);
	  }
	}
}