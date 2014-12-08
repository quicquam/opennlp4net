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
using System.IO;
using j4n.IO.File;
using j4n.IO.Reader;
using opennlp.tools.chunker;
using opennlp.tools.postag;
using opennlp.tools.util;

namespace opennlp.tools.cmdline.chunker
{
    public class ChunkerMETool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "learnable chunker";
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
		  ChunkerModel model = (new ChunkerModelLoader()).load(new Jfile(args[0]));

		  ChunkerME chunker = new ChunkerME(model, ChunkerME.DEFAULT_BEAM_SIZE);

		  ObjectStream<string> lineStream = new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput()));

          PerformanceMonitor perfMon = new PerformanceMonitor(Console.Error, "sent");
		  perfMon.start();

		  try
		  {
			string line;
			while ((line = lineStream.read()) != null)
			{

			  POSSample posSample;
			  try
			  {
				posSample = POSSample.parse(line);
			  }
			  catch (InvalidFormatException)
			  {
				Console.Error.WriteLine("Invalid format:");
				Console.Error.WriteLine(line);
				continue;
			  }

			  string[] chunks = chunker.chunk(posSample.Sentence, posSample.Tags);

			  Console.WriteLine((new ChunkSample(posSample.Sentence, posSample.Tags, chunks)).nicePrint());

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