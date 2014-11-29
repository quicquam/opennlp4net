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
using j4n.Exceptions;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.Reader;
using j4n.Serialization;
using opennlp.tools.parser;

namespace opennlp.tools.cmdline.parser
{


	using TrainUtil = opennlp.model.TrainUtil;
	using opennlp.tools.cmdline;
	using EncodingParameter = opennlp.tools.cmdline.@params.EncodingParameter;
	using TrainingToolParams = opennlp.tools.cmdline.@params.TrainingToolParams;
	using TrainerToolParams = opennlp.tools.cmdline.parser.ParserTrainerTool.TrainerToolParams;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using HeadRules = opennlp.tools.parser.HeadRules;
	using Parse = opennlp.tools.parser.Parse;
	using ParserModel = opennlp.tools.parser.ParserModel;
	using Parser = opennlp.tools.parser.chunking.Parser;
	using opennlp.tools.util;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	public sealed class ParserTrainerTool : AbstractTrainerTool<Parse, TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams, EncodingParameter
	  {
	  }

	  public ParserTrainerTool() : base(typeof(Parse), typeof(TrainerToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "trains the learnable parser";
		  }
	  }

	  internal static Dictionary buildDictionary(ObjectStream<Parse> parseSamples, HeadRules headRules, int cutoff)
	  {
		Console.Error.Write("Building dictionary ...");

		Dictionary mdict;
		try
		{
		  mdict = Parser.buildDictionary(parseSamples, headRules, cutoff);
		}
		catch (IOException e)
		{
		  Console.Error.WriteLine("Error while building dictionary: " + e.Message);
		  mdict = null;
		}
		Console.Error.WriteLine("done");

		return mdict;
	  }

	  internal static ParserType parseParserType(string typeAsString)
	  {
		var type = ParserType.UNKNOWN;
		if (!string.IsNullOrEmpty(typeAsString))
		{
		  type = ParserTypeHelper.parse(typeAsString);
            if(type == ParserType.UNKNOWN)
		        throw new TerminateToolException(1, "ParserType training parameter '" + typeAsString + "' is invalid!");		 
		}

		return type;
	  }

	  // TODO: Add param to train tree insert parser
	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		mlParams = CmdLineUtil.loadTrainingParameters(@params.Params, true);

		if (mlParams != null)
		{
		  if (!TrainUtil.isValid(mlParams.getSettings("build")))
		  {
			throw new TerminateToolException(1, "Build training parameters are invalid!");
		  }

		  if (!TrainUtil.isValid(mlParams.getSettings("check")))
		  {
			throw new TerminateToolException(1, "Check training parameters are invalid!");
		  }

		  if (!TrainUtil.isValid(mlParams.getSettings("attach")))
		  {
			throw new TerminateToolException(1, "Attach training parameters are invalid!");
		  }

		  if (!TrainUtil.isValid(mlParams.getSettings("tagger")))
		  {
			throw new TerminateToolException(1, "Tagger training parameters are invalid!");
		  }

		  if (!TrainUtil.isValid(mlParams.getSettings("chunker")))
		  {
			throw new TerminateToolException(1, "Chunker training parameters are invalid!");
		  }
		}

		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(@params.Iterations.Value, @params.Cutoff.Value);
		}

		Jfile modelOutFile = @params.Model;
		CmdLineUtil.checkOutputFile("parser model", modelOutFile);

		ParserModel model;
		try
		{

		  // TODO hard-coded language reference
		  HeadRules rules = new opennlp.tools.parser.lang.en.HeadRules(new InputStreamReader(new FileInputStream(@params.HeadRules), @params.Encoding));

		  var type = parseParserType(@params.ParserType);
		  if (@params.Fun.Value)
		  {
			  Parse.useFunctionTags(true);
		  }

		  if (ParserType.CHUNKING == type)
		  {
			model = Parser.train(@params.Lang, sampleStream, rules, mlParams);
		  }
		  else if (ParserType.TREEINSERT == type)
		  {
			model = opennlp.tools.parser.treeinsert.Parser.train(@params.Lang, sampleStream, rules, mlParams);
		  }
		  else
		  {
			throw new IllegalStateException();
		  }
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

		CmdLineUtil.writeModel("parser", modelOutFile, model);
	  }
	}

}