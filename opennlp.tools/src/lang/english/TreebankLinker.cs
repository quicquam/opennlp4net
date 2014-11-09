using System;
using System.Collections.Generic;

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

namespace opennlp.tools.lang.english
{


	using DefaultLinker = opennlp.tools.coref.DefaultLinker;
	using DiscourseEntity = opennlp.tools.coref.DiscourseEntity;
	using Linker = opennlp.tools.coref.Linker;
	using LinkerMode = opennlp.tools.coref.LinkerMode;
	using DefaultParse = opennlp.tools.coref.mention.DefaultParse;
	using Mention = opennlp.tools.coref.mention.Mention;
	using MentionContext = opennlp.tools.coref.mention.MentionContext;
	using PTBMentionFinder = opennlp.tools.coref.mention.PTBMentionFinder;
	using Parse = opennlp.tools.parser.Parse;
	using Parser = opennlp.tools.parser.chunking.Parser;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// This class perform coreference for treebank style parses.
	/// It will only perform coreference over constituents defined in the trees and
	/// will not generate new constituents for pre-nominal entities or sub-entities in
	/// simple coordinated noun phrases.  This linker requires that named-entity information also be provided.
	/// This information can be added to the parse using the -parse option with EnglishNameFinder.
	/// </summary>
	/// @deprecated will be removed soon! 
	[Obsolete("will be removed soon!")]
	public class TreebankLinker : DefaultLinker
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TreebankLinker(String project, opennlp.tools.coref.LinkerMode mode) throws java.io.IOException
	  public TreebankLinker(string project, LinkerMode mode) : base(project,mode)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TreebankLinker(String project, opennlp.tools.coref.LinkerMode mode, boolean useDiscourseModel) throws java.io.IOException
	  public TreebankLinker(string project, LinkerMode mode, bool useDiscourseModel) : base(project,mode,useDiscourseModel)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TreebankLinker(String project, opennlp.tools.coref.LinkerMode mode, boolean useDiscourseModel, double fixedNonReferentialProbability) throws java.io.IOException
	  public TreebankLinker(string project, LinkerMode mode, bool useDiscourseModel, double fixedNonReferentialProbability) : base(project,mode,useDiscourseModel,fixedNonReferentialProbability)
	  {
	  }

	  protected internal override void initMentionFinder()
	  {
		mentionFinder = PTBMentionFinder.getInstance(headFinder);
	  }

	  private static void showEntities(DiscourseEntity[] entities)
	  {
		for (int ei = 0,en = entities.Length;ei < en;ei++)
		{
		 Console.WriteLine(ei + " " + entities[ei]);
		}
	  }

	  /// <summary>
	  /// Identitifies corefernce relationships for parsed input passed via standard in. </summary>
	  /// <param name="args"> The model directory. </param>
	  /// <exception cref="IOException"> when the model directory can not be read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
	  public static void Main(string[] args)
	  {
		if (args.Length == 0)
		{
		  Console.Error.WriteLine("Usage: TreebankLinker model_directory < parses");
		  Environment.Exit(1);
		}
		BufferedReader @in;
		int ai = 0;
		string dataDir = args[ai++];
		if (ai == args.Length)
		{
		  @in = new BufferedReader(new InputStreamReader(Console.OpenStandardInput));
		}
		else
		{
		  @in = new BufferedReader(new FileReader(args[ai]));
		}
		Linker treebankLinker = new TreebankLinker(dataDir,LinkerMode.TEST);
		int sentenceNumber = 0;
		IList<Mention> document = new List<Mention>();
		IList<Parse> parses = new List<Parse>();
		for (string line = @in.readLine();null != line;line = @in.readLine())
		{
		  if (line.Equals(""))
		  {
			DiscourseEntity[] entities = treebankLinker.getEntities(document.ToArray());
			//showEntities(entities);
			(new CorefParse(parses,entities)).show();
			sentenceNumber = 0;
			document.Clear();
			parses.Clear();
		  }
		  else
		  {
			Parse p = Parse.parseParse(line);
			parses.Add(p);
			Mention[] extents = treebankLinker.MentionFinder.getMentions(new DefaultParse(p,sentenceNumber));
			//construct new parses for mentions which don't have constituents.
			for (int ei = 0,en = extents.Length;ei < en;ei++)
			{
			  //System.err.println("PennTreebankLiner.main: "+ei+" "+extents[ei]);

			  if (extents[ei].Parse == null)
			  {
				//not sure how to get head index, but its not used at this point.
				Parse snp = new Parse(p.Text,extents[ei].Span,"NML",1.0,0);
				p.insert(snp);
				extents[ei].Parse = new DefaultParse(snp,sentenceNumber);
			  }

			}
			document.AddRange(Arrays.asList(extents));
			sentenceNumber++;
		  }
		}
		if (document.Count > 0)
		{
		  DiscourseEntity[] entities = treebankLinker.getEntities(document.ToArray());
		  //showEntities(entities);
		  (new CorefParse(parses,entities)).show();
		}
	  }
	}

	internal class CorefParse
	{

	  private IDictionary<Parse, int?> parseMap;
	  private IList<Parse> parses;

	  public CorefParse(IList<Parse> parses, DiscourseEntity[] entities)
	  {
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
			  //System.err.println("CorefParse: "+mc.getParse().hashCode()+" -> "+ (ei+1));
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

	  private void show(Parse p)
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

}