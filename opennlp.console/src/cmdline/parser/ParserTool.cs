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
using System.Text;
using j4n.IO.File;
using j4n.IO.Reader;
using j4n.Lang;
using j4n.Object;
using opennlp.tools.parser;
using opennlp.tools.util;

namespace opennlp.console.cmdline.parser
{
    public sealed class ParserTool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "performs full syntactic parsing";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " [-bs n -ap n -k n] model < sentences \n" + "-bs n: Use a beam size of n.\n" + "-ap f: Advance outcomes in with at least f% of the probability mass.\n" + "-k n: Show the top n parses.  This will also display their log-probablities.";
		  }
	  }

	  private static Pattern untokenizedParenPattern1 = Pattern.compile("([^ ])([({)}])");
	  private static Pattern untokenizedParenPattern2 = Pattern.compile("([({)}])([^ ])");

	  public static Parse[] parseLine(string line, opennlp.tools.parser.Parser parser, int numParses)
	  {
		line = untokenizedParenPattern1.matcher(line).replaceAll("$1 $2");
		line = untokenizedParenPattern2.matcher(line).replaceAll("$1 $2");
		StringTokenizer str = new StringTokenizer(line);
		StringBuilder sb = new StringBuilder();
		IList<string> tokens = new List<string>();
		while (str.hasMoreTokens())
		{
		  string tok = str.nextToken();
		  tokens.Add(tok);
		  sb.Append(tok).Append(" ");
		}
		string text = sb.ToString().Substring(0, sb.Length - 1);
		Parse p = new Parse(text, new Span(0, text.Length), AbstractBottomUpParser.INC_NODE, 0, 0);
		int start = 0;
		int i = 0;
		for (IEnumerator<string> ti = tokens.GetEnumerator(); ti.MoveNext(); i++)
		{
		  string tok = ti.Current;
		  p.insert(new Parse(text, new Span(start, start + tok.Length), AbstractBottomUpParser.TOK_NODE, 0,i));
		  start += tok.Length + 1;
		}
		Parse[] parses;
		if (numParses == 1)
		{
		  parses = new Parse[] {parser.parse(p)};
		}
		else
		{
		  parses = parser.parse(p,numParses);
		}
		return parses;
	  }

	  public override void run(string[] args)
	  {

		if (args.Length < 1)
		{
		  Console.WriteLine(Help);
		}
		else
		{

		  ParserModel model = (new ParserModelLoader()).load(new Jfile(args[args.Length - 1]));

		  int? beamSize = CmdLineUtil.getIntParameter("-bs", args);
		  if (beamSize == null)
		  {
			  beamSize = AbstractBottomUpParser.defaultBeamSize;
		  }

		  int? numParses = CmdLineUtil.getIntParameter("-k", args);
		  bool showTopK;
		  if (numParses == null)
		  {
			numParses = 1;
			showTopK = false;
		  }
		  else
		  {
			showTopK = true;
		  }

		  double? advancePercentage = CmdLineUtil.getDoubleParameter("-ap", args);

		  if (advancePercentage == null)
		  {
			advancePercentage = AbstractBottomUpParser.defaultAdvancePercentage;
		  }

		  opennlp.tools.parser.Parser parser = ParserFactory.create(model, beamSize.Value, advancePercentage.Value);

		  ObjectStream<string> lineStream = new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput()));

		  PerformanceMonitor perfMon = new PerformanceMonitor(Console.Error, "sent");
		  perfMon.start();

		  try
		  {
			string line;
			while ((line = lineStream.read()) != null)
			{
			  if (line.Length == 0)
			  {
				Console.WriteLine();
			  }
			  else
			  {
				Parse[] parses = parseLine(line, parser, numParses.Value);

				for (int pi = 0,pn = parses.Length;pi < pn;pi++)
				{
				  if (showTopK)
				  {
					Console.Write(pi + " " + parses[pi].Prob + " ");
				  }

				  parses[pi].show();

				  perfMon.incrementCounter();
				}
			  }
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