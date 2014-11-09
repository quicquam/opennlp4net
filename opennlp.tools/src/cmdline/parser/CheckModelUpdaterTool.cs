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

namespace opennlp.tools.cmdline.parser
{

	using AbstractModel = opennlp.model.AbstractModel;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using Parse = opennlp.tools.parser.Parse;
	using ParserEventTypeEnum = opennlp.tools.parser.ParserEventTypeEnum;
	using ParserModel = opennlp.tools.parser.ParserModel;
	using Parser = opennlp.tools.parser.chunking.Parser;
	using ParserEventStream = opennlp.tools.parser.chunking.ParserEventStream;
	using opennlp.tools.util;

	// trains a new check model ...
	public sealed class CheckModelUpdaterTool : ModelUpdaterTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "trains and updates the check model in a parser model";
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected opennlp.tools.parser.ParserModel trainAndUpdate(opennlp.tools.parser.ParserModel originalModel, opennlp.tools.util.ObjectStream<opennlp.tools.parser.Parse> parseSamples, ModelUpdaterParams parameters) throws java.io.IOException
	  protected internal override ParserModel trainAndUpdate(ParserModel originalModel, ObjectStream<Parse> parseSamples, ModelUpdaterParams parameters)
	  {

		  Dictionary mdict = ParserTrainerTool.buildDictionary(parseSamples, originalModel.HeadRules, parameters.Cutoff.Value);

		  parseSamples.reset();

		  // TODO: Maybe that should be part of the ChunkingParser ...
		  // Training build
		  Console.WriteLine("Training check model");
		  opennlp.model.EventStream bes = new ParserEventStream(parseSamples, originalModel.HeadRules, ParserEventTypeEnum.CHECK, mdict);
		  AbstractModel checkModel = Parser.train(bes, parameters.Iterations.Value, parameters.Cutoff.Value);

		  parseSamples.close();

		  return originalModel.updateCheckModel(checkModel);
	  }
	}

}