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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using j4n.IO.File;
using j4n.IO.Reader;
using opennlp.tools.namefind;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.tools.cmdline.namefind
{
    public sealed class TokenNameFinderTool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "learnable name finder";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " model1 model2 ... modelN < sentences";
		  }
	  }

	  public override void run(string[] args)
	  {

		if (args.Length == 0)
		{
		  Console.WriteLine(Help);
		}
		else
		{

		  NameFinderME[] nameFinders = new NameFinderME[args.Length];

		  for (int i = 0; i < nameFinders.Length; i++)
		  {
			TokenNameFinderModel model = (new TokenNameFinderModelLoader()).load(new Jfile(args[i]));
			nameFinders[i] = new NameFinderME(model);
		  }

		  ObjectStream<string> untokenizedLineStream = new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput()));

		  PerformanceMonitor perfMon = new PerformanceMonitor(Console.Error, "sent");
		  perfMon.start();

		  try
		  {
			string line;
			while ((line = untokenizedLineStream.read()) != null)
			{
			  string[] whitespaceTokenizerLine = WhitespaceTokenizer.INSTANCE.tokenize(line);

			  // A new line indicates a new document,
			  // adaptive data must be cleared for a new document

			  if (whitespaceTokenizerLine.Length == 0)
			  {
				foreach (NameFinderME nameFinder in nameFinders)
				{
				  nameFinder.clearAdaptiveData();
				}
			  }

			  IList<Span> names = new List<Span>();

			  foreach (TokenNameFinder nameFinder in nameFinders)
			  {
			    var spans = nameFinder.find(whitespaceTokenizerLine);
			      foreach (var span in spans)
			      {
                      names.Add(span);
			      }				
			  }

			  // Simple way to drop intersecting spans, otherwise the
			  // NameSample is invalid
			  Span[] reducedNames = NameFinderME.dropOverlappingSpans(names.ToArray());

			  NameSample nameSample = new NameSample(whitespaceTokenizerLine, reducedNames, false);

			  Console.WriteLine(nameSample.ToString());

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