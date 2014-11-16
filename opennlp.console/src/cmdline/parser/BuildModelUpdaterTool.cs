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
using j4n.Serialization;

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

	public sealed class BuildModelUpdaterTool : ModelUpdaterTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "trains and updates the build model in a parser model";
		  }
	  }

	  protected internal override ParserModel trainAndUpdate(ParserModel originalModel, ObjectStream<Parse> parseSamples, ModelUpdaterParams parameters)
	  {

		  Dictionary mdict = ParserTrainerTool.buildDictionary(parseSamples, originalModel.HeadRules, parameters.Cutoff.Value);

		  parseSamples.reset();

		  // TODO: training individual models should be in the chunking parser, not here
		  // Training build
		  Console.WriteLine("Training builder");
		  opennlp.model.EventStream bes = new ParserEventStream(parseSamples, originalModel.HeadRules, ParserEventTypeEnum.BUILD, mdict);
		  AbstractModel buildModel = Parser.train(bes, parameters.Iterations.Value, parameters.Cutoff.Value);

		  parseSamples.close();

		  return originalModel.updateBuildModel(buildModel);
	  }
	}

}