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
using j4n.Serialization;

namespace opennlp.tools.formats.muc
{


	using CorefSample = opennlp.tools.coref.CorefSample;
	using DefaultParse = opennlp.tools.coref.mention.DefaultParse;
	using Mention = opennlp.tools.coref.mention.Mention;
	using MentionFinder = opennlp.tools.coref.mention.MentionFinder;
	using PTBHeadFinder = opennlp.tools.coref.mention.PTBHeadFinder;
	using PTBMentionFinder = opennlp.tools.coref.mention.PTBMentionFinder;
	using CorefMention = opennlp.tools.formats.muc.MucCorefContentHandler.CorefMention;
	using Parse = opennlp.tools.parser.Parse;
	using opennlp.tools.util;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// The mention insert is responsible to insert the mentions from the training data
	/// into the parse trees.
	/// </summary>
	public class MucMentionInserterStream : FilterObjectStream<RawCorefSample, CorefSample>
	{

	  private static HashSet<string> entitySet = new HashSet<string>(Arrays.asList(DefaultParse.NAME_TYPES));

	  private readonly MentionFinder mentionFinder;

	  protected internal MucMentionInserterStream(ObjectStream<RawCorefSample> samples) : base(samples)
	  {

		mentionFinder = PTBMentionFinder.getInstance(PTBHeadFinder.Instance);
	  }

	  private static Span getMinSpan(Parse p, CorefMention mention)
	  {
		string min = mention.min;

		if (min != null)
		{

		  int startOffset = p.ToString().IndexOf(min, StringComparison.Ordinal);
		  int endOffset = startOffset + min.Length;

		  Parse[] tokens = p.TagNodes;

		  int beginToken = -1;
		  int endToken = -1;

		  for (int i = 0; i < tokens.Length; i++)
		  {
			if (tokens[i].Span.Start == startOffset)
			{
			  beginToken = i;
			}

			if (tokens[i].Span.End == endOffset)
			{
			  endToken = i + 1;
			  break;
			}
		  }

		  if (beginToken != -1 && endToken != -1)
		  {
			return new Span(beginToken, endToken);
		  }
		}

		return null;
	  }

	  public static bool addMention(int id, Span mention, Parse[] tokens)
	  {

		bool failed = false;

		Parse startToken = tokens[mention.Start];
		Parse endToken = tokens[mention.End - 1];
		Parse commonParent = startToken.getCommonParent(endToken);

		if (commonParent != null)
		{
	//      Span mentionSpan = new Span(startToken.getSpan().getStart(), endToken.getSpan().getEnd());

		  if (entitySet.Contains(commonParent.Type))
		  {
			commonParent.Parent.Type = "NP#" + id;
		  }
		  else if (commonParent.Type.Equals("NML"))
		  {
			commonParent.Type = "NML#" + id;
		  }
		  else if (commonParent.Type.Equals("NP"))
		  {
			commonParent.Type = "NP#" + id;
		  }
		  else
		  {
			Console.WriteLine("Inserting mention failed: " + commonParent.Type + " Failed id: " + id);
			failed = true;
		  }
		}
		else
		{
		  throw new System.ArgumentException("Tokens must always have a common parent!");
		}

		return !failed;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.coref.CorefSample read() throws java.io.IOException
	  public override CorefSample read()
	  {

		RawCorefSample sample = samples.read();

		if (sample != null)
		{

		  IList<Parse> mentionParses = new List<Parse>();

		  IList<CorefMention[]> allMentions = sample.Mentions;
		  IList<Parse> allParses = sample.Parses;

		  for (int si = 0; si < allMentions.Count; si++)
		  {
			CorefMention[] mentions = allMentions[si];
			Parse p = allParses[si];

			foreach (Mention extent in mentionFinder.getMentions(new DefaultParse(p, si)))
			{
			  if (extent.Parse == null)
			  {
				// not sure how to get head index
				Parse snp = new Parse(p.Text,extent.Span,"NML",1.0,0);
				p.insert(snp);
			  }
			}

			Parse[] tokens = p.TagNodes;

			foreach (CorefMention mention in mentions)
			{
			  Span min = getMinSpan(p, mention);

			  if (min == null)
			  {
				min = mention.span;
			  }

			  addMention(mention.id, min, tokens);
			}

			p.show();

			mentionParses.Add(p);
		  }

		  return new CorefSample(mentionParses);
		}
		else
		{
		  return null;
		}
	  }
	}

}