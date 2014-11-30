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
using j4n.IO.File;
using j4n.IO.Reader;
using j4n.Serialization;

namespace opennlp.tools.cmdline.postag
{


	using POSModel = opennlp.tools.postag.POSModel;
	using POSSample = opennlp.tools.postag.POSSample;
	using POSTaggerME = opennlp.tools.postag.POSTaggerME;
	using WhitespaceTokenizer = opennlp.tools.tokenize.WhitespaceTokenizer;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	public sealed class POSTaggerTool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "learnable part of speech tagger";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " model < sentences";
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

		  POSModel model = (new POSModelLoader()).load(new Jfile(args[0]));

		  POSTaggerME tagger = new POSTaggerME(model);

		  ObjectStream<string> lineStream = new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput));

		  PerformanceMonitor perfMon = new PerformanceMonitor(System.err, "sent");
		  perfMon.start();

		  try
		  {
			string line;
			while ((line = lineStream.read()) != null)
			{

			  string[] whitespaceTokenizerLine = WhitespaceTokenizer.INSTANCE.tokenize(line);
			  string[] tags = tagger.tag(whitespaceTokenizerLine);

			  POSSample sample = new POSSample(whitespaceTokenizerLine, tags);
			  Console.WriteLine(sample.ToString());

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