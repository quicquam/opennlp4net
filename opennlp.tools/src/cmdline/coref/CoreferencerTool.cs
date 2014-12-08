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
using j4n.IO.Reader;
using opennlp.tools.coref;
using opennlp.tools.coref.mention;
using opennlp.tools.nonjava.extensions;
using opennlp.tools.parser;
using opennlp.tools.util;
using Parse = opennlp.tools.parser.Parse;
using Parser = opennlp.tools.parser.chunking.Parser;
using TreebankLinker = opennlp.tools.lang.english.TreebankLinker;

namespace opennlp.tools.cmdline.coref
{
    public class CoreferencerTool : BasicCmdLineTool
	{

	  internal class CorefParse
	  {
		  private readonly CoreferencerTool outerInstance;


		internal IDictionary<Parse, int?> parseMap;
		internal IList<Parse> parses;

		public CorefParse(CoreferencerTool outerInstance, IList<Parse> parses, DiscourseEntity[] entities)
		{
			this.outerInstance = outerInstance;
		  this.parses = parses;
		  parseMap = new Dictionary<Parse, int?>();
		  for (int ei = 0,en = entities.Length;ei < en;ei++)
		  {
			if (entities[ei].NumMentions > 1)
			{
			  for (IEnumerator<MentionContext> mi = entities[ei].Mentions; mi.MoveNext();)
			  {
				MentionContext mc = mi.Current;
				Parse mentionParse = ((DefaultParse) mc.Parse).Parse;
				parseMap[mentionParse] = ei + 1;
			  }
			}
		  }
		}

		public virtual void show()
		{
		  for (int pi = 0,pn = parses.Count;pi < pn;pi++)
		  {
			Parse p = parses[pi];
			show(p);
			Console.WriteLine();
		  }
		}

		internal virtual void show(Parse p)
		{
		  int start;
		  start = p.Span.Start;
		  if (!p.Type.Equals(Parser.TOK_NODE))
		  {
			Console.Write("(");
			Console.Write(p.Type);
			if (parseMap.ContainsKey(p))
			{
			  Console.Write("#" + parseMap[p]);
			}
			//System.out.print(p.hashCode()+"-"+parseMap.containsKey(p));
			Console.Write(" ");
		  }
		  Parse[] children = p.Children;
		  for (int pi = 0,pn = children.Length;pi < pn;pi++)
		  {
			Parse c = children[pi];
			Span s = c.Span;
			if (start < s.Start)
			{
			  Console.Write(p.Text.Substring(start, s.Start - start));
			}
			show(c);
			start = s.End;
		  }
		  Console.Write(p.Text.Substring(start, p.Span.End - start));
		  if (!p.Type.Equals(Parser.TOK_NODE))
		  {
			Console.Write(")");
		  }
		}
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "learnable noun phrase coreferencer";
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

		  TreebankLinker treebankLinker;
		  try
		  {
			treebankLinker = new TreebankLinker(args[0], LinkerMode.TEST);
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "Failed to load all coreferencer models!", e);
		  }

		  ObjectStream<string> lineStream = new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput()));

          PerformanceMonitor perfMon = new PerformanceMonitor(Console.Error, "parses");
		  perfMon.start();

		  try
		  {

			int sentenceNumber = 0;
			IList<Mention> document = new List<Mention>();
			IList<Parse> parses = new List<Parse>();

			string line;
			while ((line = lineStream.read()) != null)
			{

			  if (line.Equals(""))
			  {
				DiscourseEntity[] entities = treebankLinker.getEntities(document.ToArray());
				//showEntities(entities);
				(new CorefParse(this, parses,entities)).show();
				sentenceNumber = 0;
				document.Clear();
				parses.Clear();
			  }
			  else
			  {
				Parse p = Parse.parseParse(line, (HeadRules)null);
				parses.Add(p);
				Mention[] extents = treebankLinker.MentionFinder.getMentions(new DefaultParse(p,sentenceNumber));
				//construct new parses for mentions which don't have constituents.
				for (int ei = 0,en = extents.Length;ei < en;ei++)
				{
				  //Console.Error.println("PennTreebankLiner.main: "+ei+" "+extents[ei]);

				  if (extents[ei].Parse == null)
				  {
					//not sure how to get head index, but its not used at this point.
					Parse snp = new Parse(p.Text,extents[ei].Span,"NML",1.0,0);
					p.insert(snp);
					extents[ei].Parse = new DefaultParse(snp,sentenceNumber);
				  }

				}
				document.AddRange(extents);
				sentenceNumber++;
			  }

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

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " model_directory < parses";
		  }
	  }
	}

}