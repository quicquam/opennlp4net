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

namespace opennlp.tools.cmdline.tokenizer
{


	using Detokenizer = opennlp.tools.tokenize.Detokenizer;
	using DictionaryDetokenizer = opennlp.tools.tokenize.DictionaryDetokenizer;
	using WhitespaceTokenizer = opennlp.tools.tokenize.WhitespaceTokenizer;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	public sealed class DictionaryDetokenizerTool : BasicCmdLineTool
	{

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " detokenizerDictionary";
		  }
	  }

	  public override void run(string[] args)
	  {


		if (args.Length != 1)
		{
		  Console.WriteLine(Help);
		}
		else
		{

		  Detokenizer detokenizer = new DictionaryDetokenizer((new DetokenizationDictionaryLoader()).load(new File(args[0])));

		  ObjectStream<string> tokenizedLineStream = new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput));

		  PerformanceMonitor perfMon = new PerformanceMonitor(System.err, "sent");
		  perfMon.start();

		  try
		  {
			string tokenizedLine;
			while ((tokenizedLine = tokenizedLineStream.read()) != null)
			{

			  // white space tokenize line
			  string[] tokens = WhitespaceTokenizer.INSTANCE.tokenize(tokenizedLine);

			  Console.WriteLine(detokenizer.detokenize(tokens, null));

			  perfMon.incrementCounter();
			}
		  }
		  catch (IOException e)
		  {
			CmdLineUtil.handleStdinIoError(e);
		  }

		  perfMon.stopAndPrintFinalResult();
		}
	  }
	}

}