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
using j4n.IO.File;
using opennlp.tools.cmdline.postag;
using opennlp.tools.parser;
using opennlp.tools.postag;

namespace opennlp.tools.cmdline.parser
{
    // user should train with the POS tool
	public sealed class TaggerModelReplacerTool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "replaces the tagger model in a parser model";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " parser.model tagger.model";
		  }
	  }

	  public override void run(string[] args)
	  {

		if (args.Length != 2)
		{
		  Console.WriteLine(Help);
		}
		else
		{

          Jfile parserModelInFile = new Jfile(args[0]);
		  ParserModel parserModel = (new ParserModelLoader()).load(parserModelInFile);

          Jfile taggerModelInFile = new Jfile(args[1]);
		  POSModel taggerModel = (new POSModelLoader()).load(taggerModelInFile);

		  ParserModel updatedParserModel = parserModel.updateTaggerModel(taggerModel);

		  CmdLineUtil.writeModel("parser", parserModelInFile, updatedParserModel);
		}
	  }
	}

}