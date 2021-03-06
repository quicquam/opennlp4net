﻿using System;
using System.Collections.Generic;
using opennlp.tools.cmdline.chunker;
using opennlp.tools.cmdline.coref;
using opennlp.tools.cmdline.dictionary;
using opennlp.tools.cmdline.doccat;
using opennlp.tools.cmdline.namefind;
using opennlp.tools.cmdline.parser;
using opennlp.tools.cmdline.postag;
using opennlp.tools.cmdline.sentdetect;
using opennlp.tools.cmdline.tokenizer;
using Version = opennlp.tools.util.Version;
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


namespace opennlp.tools.cmdline
{
    public sealed class CLI
	{

	  public const string CMD = "opennlp";

	  private static IDictionary<string, CmdLineTool> toolLookupMap;

	  static CLI()
	  {
		toolLookupMap = new Dictionary<string, CmdLineTool>();

		IList<CmdLineTool> tools = new List<CmdLineTool>();

		// Document Categorizer
		tools.Add(new DoccatTool());
		tools.Add(new DoccatTrainerTool());
		tools.Add(new DoccatConverterTool());

		// Dictionary Builder
		tools.Add(new DictionaryBuilderTool());

		// Tokenizer
		tools.Add(new SimpleTokenizerTool());
		tools.Add(new TokenizerMETool());
		tools.Add(new TokenizerTrainerTool());
		tools.Add(new TokenizerMEEvaluatorTool());
		tools.Add(new TokenizerCrossValidatorTool());
		tools.Add(new TokenizerConverterTool());
		tools.Add(new DictionaryDetokenizerTool());

		// Sentence detector
		tools.Add(new SentenceDetectorTool());
		tools.Add(new SentenceDetectorTrainerTool());
		tools.Add(new SentenceDetectorEvaluatorTool());
		tools.Add(new SentenceDetectorCrossValidatorTool());
		tools.Add(new SentenceDetectorConverterTool());

		// Name Finder
		tools.Add(new TokenNameFinderTool());
		tools.Add(new TokenNameFinderTrainerTool());
		tools.Add(new TokenNameFinderEvaluatorTool());
		tools.Add(new TokenNameFinderCrossValidatorTool());
		tools.Add(new TokenNameFinderConverterTool());
		tools.Add(new CensusDictionaryCreatorTool());


		// POS Tagger
		tools.Add(new POSTaggerTool());
		tools.Add(new POSTaggerTrainerTool());
		tools.Add(new POSTaggerEvaluatorTool());
		tools.Add(new POSTaggerCrossValidatorTool());
		tools.Add(new POSTaggerConverterTool());

		// Chunker
		tools.Add(new ChunkerMETool());
		tools.Add(new ChunkerTrainerTool());
		tools.Add(new ChunkerEvaluatorTool());
		tools.Add(new ChunkerCrossValidatorTool());
		tools.Add(new ChunkerConverterTool());

		// Parser
		tools.Add(new ParserTool());
		tools.Add(new ParserTrainerTool()); // trains everything
		tools.Add(new ParserConverterTool()); // trains everything
		tools.Add(new BuildModelUpdaterTool()); // re-trains  build model
		tools.Add(new CheckModelUpdaterTool()); // re-trains  build model
		tools.Add(new TaggerModelReplacerTool());

		// Coreferencer
		tools.Add(new CoreferencerTool());
		tools.Add(new CoreferencerTrainerTool());
		tools.Add(new CoreferenceConverterTool());

		foreach (CmdLineTool tool in tools)
		{
		  toolLookupMap[tool.Name] = tool;
		}
	  }

	  /// <returns> a set which contains all tool names </returns>
	  public static HashSet<string> ToolNames
	  {
		  get
		  {
              return new HashSet<string>(toolLookupMap.Keys);
		  }
	  }

	  private static void usage()
	  {
		Console.Write("OpenNLP " + Version.currentVersion().ToString() + ". ");
		Console.WriteLine("Usage: " + CMD + " TOOL");
		Console.WriteLine("where TOOL is one of:");

		// distance of tool name from line start
		int numberOfSpaces = -1;
		foreach (string toolName in toolLookupMap.Keys)
		{
		  if (toolName.Length > numberOfSpaces)
		  {
			numberOfSpaces = toolName.Length;
		  }
		}
		numberOfSpaces = numberOfSpaces + 4;

		foreach (CmdLineTool tool in toolLookupMap.Values)
		{

		  Console.Write("  " + tool.Name);

		  for (int i = 0; i < Math.Abs(tool.Name.Length - numberOfSpaces); i++)
		  {
			Console.Write(" ");
		  }

		  Console.WriteLine(tool.ShortDescription);
		}

		Console.WriteLine("All tools print help when invoked with help parameter");
		Console.WriteLine("Example: opennlp SimpleTokenizer help");
	  }
        /*
	  public static void Main(string[] args)
	  {

		if (args.Length == 0)
		{
		  usage();
		  Environment.Exit(0);
		}

		string[] toolArguments = new string[args.Length - 1];
		Array.Copy(args, 1, toolArguments, 0, toolArguments.Length);

		string toolName = args[0];

		//check for format
		string formatName = StreamFactoryRegistry.DEFAULT_FORMAT;
		int idx = toolName.IndexOf(".", StringComparison.Ordinal);
		if (-1 < idx)
		{
		  formatName = toolName.Substring(idx + 1);
		  toolName = toolName.Substring(0, idx);
		}
		CmdLineTool tool = toolLookupMap[toolName];

		try
		{
		  if (null == tool)
		  {
			throw new TerminateToolException(1, "Tool " + toolName + " is not found.");
		  }

		  if ((0 == toolArguments.Length && tool.hasParams()) || 0 < toolArguments.Length && "help".Equals(toolArguments[0]))
		  {
			  if (tool is TypedCmdLineTool)
			  {
				Console.WriteLine(((TypedCmdLineTool) tool).getHelp(formatName));
			  }
			  else if (tool is BasicCmdLineTool)
			  {
				Console.WriteLine(tool.Help);
			  }

			  Environment.Exit(0);
		  }

		  if (tool is TypedCmdLineTool)
		  {
			((TypedCmdLineTool) tool).run(formatName, toolArguments);
		  }
		  else if (tool is BasicCmdLineTool)
		  {
			if (-1 == idx)
			{
			  ((BasicCmdLineTool) tool).run(toolArguments);
			}
			else
			{
			  throw new TerminateToolException(1, "Tool " + toolName + " does not support formats.");
			}
		  }
		  else
		  {
			throw new TerminateToolException(1, "Tool " + toolName + " is not supported.");
		  }
		}
		catch (TerminateToolException e)
		{

		  if (e.Message != null)
		  {
			Console.Error.WriteLine(e.Message);
		  }

		  if (e.InnerException != null)
		  {
			Console.Error.WriteLine(e.InnerException.Message);
			e.InnerException.printStackTrace(Console.Error);
		  }

		  Environment.Exit(e.Code);
		}
	  } */
	} 

}