﻿using System.Collections.Generic;
using System.Text;

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


namespace opennlp.tools.parser
{


	/// <summary>
	/// Abstract class containing many of the methods used to generate contexts for parsing.
	/// </summary>
	public abstract class AbstractContextGenerator
	{

	  protected internal const string EOS = "eos";

	  protected internal bool zeroBackOff;
	  /// <summary>
	  /// Set of punctuation to be used in generating features. </summary>
	  protected internal HashSet<string> punctSet;
	  protected internal bool useLabel;

	  /// <summary>
	  /// Creates punctuation feature for the specified punctuation at the specified index based on the punctuation mark. </summary>
	  /// <param name="punct"> The punctuation which is in context. </param>
	  /// <param name="i"> The index of the punctuation with relative to the parse. </param>
	  /// <returns> Punctuation feature for the specified parse and the specified punctuation at the specfied index. </returns>
	  protected internal virtual string punct(Parse punct, int i)
	  {
		StringBuilder feat = new StringBuilder(5);
		feat.Append(i).Append("=");
		feat.Append(punct.CoveredText);
		return feat.ToString();
	  }

	  /// <summary>
	  /// Creates punctuation feature for the specified punctuation at the specfied index based on the punctuation's tag. </summary>
	  /// <param name="punct"> The punctuation which is in context. </param>
	  /// <param name="i"> The index of the punctuation relative to the parse. </param>
	  /// <returns> Punctuation feature for the specified parse and the specified punctuation at the specfied index. </returns>
	  protected internal virtual string punctbo(Parse punct, int i)
	  {
		StringBuilder feat = new StringBuilder(5);
		feat.Append(i).Append("=");
		feat.Append(punct.Type);
		return (feat.ToString());
	  }

	  protected internal virtual string cons(Parse p, int i)
	  {
		StringBuilder feat = new StringBuilder(20);
		feat.Append(i).Append("=");
		if (p != null)
		{
		  if (useLabel && i < 0)
		  {
			feat.Append(p.Label).Append("|");
		  }
		  feat.Append(p.Type).Append("|").Append(p.Head.CoveredText);
		}
		else
		{
		  feat.Append(EOS);
		}
		return (feat.ToString());
	  }

	  protected internal virtual string consbo(Parse p, int i) //cons back-off
	  {
		StringBuilder feat = new StringBuilder(20);
		feat.Append(i).Append("*=");
		if (p != null)
		{
		  if (useLabel && i < 0)
		  {
			feat.Append(p.Label).Append("|");
		  }
		  feat.Append(p.Type);
		}
		else
		{
		  feat.Append(EOS);
		}
		return (feat.ToString());
	  }

	  /// <summary>
	  /// Generates a string representing the grammar rule production that the specified parse
	  /// is starting.  The rule is of the form p.type -> c.children[0..n].type. </summary>
	  /// <param name="p"> The parse which stats teh production. </param>
	  /// <param name="includePunctuation"> Whether punctuation should be included in the production. </param>
	  /// <returns> a string representing the grammar rule production that the specified parse
	  /// is starting. </returns>
	  protected internal virtual string production(Parse p, bool includePunctuation)
	  {
		StringBuilder production = new StringBuilder(20);
		production.Append(p.Type).Append("->");
		Parse[] children = AbstractBottomUpParser.collapsePunctuation(p.Children,punctSet);
		for (int ci = 0; ci < children.Length; ci++)
		{
		  production.Append(children[ci].Type);
		  if (ci + 1 != children.Length)
		  {
			production.Append(",");
			ICollection<Parse> nextPunct = children[ci].NextPunctuationSet;
			if (includePunctuation && nextPunct != null)
			{
			  //TODO: make sure multiple punctuation comes out the same
			  for (IEnumerator<Parse> pit = nextPunct.GetEnumerator(); pit.MoveNext();)
			  {
				Parse punct = pit.Current;
				production.Append(punct.Type).Append(",");
			  }
			}
		  }
		}
		return production.ToString();
	  }

	  protected internal virtual void cons2(IList<string> features, Cons c0, Cons c1, ICollection<Parse> punct1s, bool bigram)
	  {
		if (punct1s != null)
		{
		  for (IEnumerator<Parse> pi = punct1s.GetEnumerator(); pi.MoveNext();)
		  {
			Parse p = pi.Current;
	//        String punct = punct(p,c1.index);
			string punctbo = punctbo(p,c1.index <= 0 ? c1.index - 1 : c1.index);

			//punctbo(1);
			features.Add(punctbo);
			if (c0.index == 0) //TODO look at removing case
			{
			  //cons(0)punctbo(1)
			  if (c0.unigram)
			  {
				  features.Add(c0.cons + "," + punctbo);
			  }
			  features.Add(c0.consbo + "," + punctbo);
			}
			if (c1.index == 0) //TODO look at removing case
			{
			  //punctbo(1)cons(1)
			  if (c1.unigram)
			  {
				  features.Add(punctbo + "," + c1.cons);
			  }
			  features.Add(punctbo + "," + c1.consbo);
			}

			//cons(0)punctbo(1)cons(1)
			if (bigram)
			{
				features.Add(c0.cons + "," + punctbo + "," + c1.cons);
			}
			if (c1.unigram)
			{
				features.Add(c0.consbo + "," + punctbo + "," + c1.cons);
			}
			if (c0.unigram)
			{
				features.Add(c0.cons + "," + punctbo + "," + c1.consbo);
			}
			features.Add(c0.consbo + "," + punctbo + "," + c1.consbo);
		  }
		}
		else
		{
		  //cons(0),cons(1)
		  if (bigram)
		  {
			  features.Add(c0.cons + "," + c1.cons);
		  }
		  if (c1.unigram)
		  {
			  features.Add(c0.consbo + "," + c1.cons);
		  }
		  if (c0.unigram)
		  {
			  features.Add(c0.cons + "," + c1.consbo);
		  }
		  features.Add(c0.consbo + "," + c1.consbo);
		}
	  }

	  /// <summary>
	  /// Creates cons features involving the 3 specified nodes and adds them to the specified feature list. </summary>
	  /// <param name="features"> The list of features. </param>
	  /// <param name="c0"> The first node. </param>
	  /// <param name="c1"> The second node. </param>
	  /// <param name="c2"> The third node. </param>
	  /// <param name="punct1s"> The punctuation between the first and second node. </param>
	  /// <param name="punct2s"> The punctuation between the second and third node. </param>
	  /// <param name="trigram"> Specifies whether lexical tri-gram features between these nodes should be generated. </param>
	  /// <param name="bigram1"> Specifies whether lexical bi-gram features between the first and second node should be generated. </param>
	  /// <param name="bigram2"> Specifies whether lexical bi-gram features between the second and third node should be generated. </param>
	  protected internal virtual void cons3(IList<string> features, Cons c0, Cons c1, Cons c2, ICollection<Parse> punct1s, ICollection<Parse> punct2s, bool trigram, bool bigram1, bool bigram2)
	  {
		//  features.add("stage=cons(0),cons(1),cons(2)");
		if (punct1s != null)
		{
		  if (c0.index == -2)
		  {
			for (IEnumerator<Parse> pi = punct1s.GetEnumerator(); pi.MoveNext();)
			{
			  Parse p = pi.Current;
	//          String punct = punct(p,c1.index);
			  string punctbo = punctbo(p,c1.index <= 0 ? c1.index - 1 : c1.index);
			  //punct(-2)
			  //TODO consider changing
			  //features.add(punct);

			  //punctbo(-2)
			  features.Add(punctbo);
			}
		  }
		}
		if (punct2s != null)
		{
		  if (c2.index == 2)
		  {
			for (IEnumerator<Parse> pi = punct2s.GetEnumerator(); pi.MoveNext();)
			{
			  Parse p = pi.Current;
	//          String punct = punct(p,c2.index);
			  string punctbo = punctbo(p,c2.index <= 0 ? c2.index - 1 : c2.index);
			  //punct(2)
			  //TODO consider changing
			  //features.add(punct);

			  //punctbo(2)
			  features.Add(punctbo);
			}
		  }
		  if (punct1s != null)
		  {
			//cons(0),punctbo(1),cons(1),punctbo(2),cons(2)
			for (IEnumerator<Parse> pi2 = punct2s.GetEnumerator(); pi2.MoveNext();)
			{
			  string punctbo2 = punctbo(pi2.Current,c2.index <= 0 ? c2.index - 1 : c2.index);
			  for (IEnumerator<Parse> pi1 = punct1s.GetEnumerator(); pi1.MoveNext();)
			  {
				string punctbo1 = punctbo(pi1.Current,c1.index <= 0 ? c1.index - 1 : c1.index);
				if (trigram)
				{
					features.Add(c0.cons + "," + punctbo1 + "," + c1.cons + "," + punctbo2 + "," + c2.cons);
				}

				if (bigram2)
				{
					features.Add(c0.consbo + "," + punctbo1 + "," + c1.cons + "," + punctbo2 + "," + c2.cons);
				}
				if (c0.unigram && c2.unigram)
				{
					features.Add(c0.cons + "," + punctbo1 + "," + c1.consbo + "," + punctbo2 + "," + c2.cons);
				}
				if (bigram1)
				{
					features.Add(c0.cons + "," + punctbo1 + "," + c1.cons + "," + punctbo2 + "," + c2.consbo);
				}

				if (c2.unigram)
				{
					features.Add(c0.consbo + "," + punctbo1 + "," + c1.consbo + "," + punctbo2 + "," + c2.cons);
				}
				if (c1.unigram)
				{
					features.Add(c0.consbo + "," + punctbo1 + "," + c1.cons + "," + punctbo2 + "," + c2.consbo);
				}
				if (c0.unigram)
				{
					features.Add(c0.cons + "," + punctbo1 + "," + c1.consbo + "," + punctbo2 + "," + c2.consbo);
				}

				features.Add(c0.consbo + "," + punctbo1 + "," + c1.consbo + "," + punctbo2 + "," + c2.consbo);
				if (zeroBackOff)
				{
				  if (bigram1)
				  {
					  features.Add(c0.cons + "," + punctbo1 + "," + c1.cons + "," + punctbo2);
				  }
				  if (c1.unigram)
				  {
					  features.Add(c0.consbo + "," + punctbo1 + "," + c1.cons + "," + punctbo2);
				  }
				  if (c0.unigram)
				  {
					  features.Add(c0.cons + "," + punctbo1 + "," + c1.consbo + "," + punctbo2);
				  }
				  features.Add(c0.consbo + "," + punctbo1 + "," + c1.consbo + "," + punctbo2);
				}
			  }
			}
		  }
		  else //punct1s == null
		  {
			//cons(0),cons(1),punctbo(2),cons(2)
			for (IEnumerator<Parse> pi2 = punct2s.GetEnumerator(); pi2.MoveNext();)
			{
			  string punctbo2 = punctbo(pi2.Current,c2.index <= 0 ? c2.index - 1 : c2.index);
			  if (trigram)
			  {
				  features.Add(c0.cons + "," + c1.cons + "," + punctbo2 + "," + c2.cons);
			  }

			  if (bigram2)
			  {
				  features.Add(c0.consbo + "," + c1.cons + "," + punctbo2 + "," + c2.cons);
			  }
			  if (c0.unigram && c2.unigram)
			  {
				  features.Add(c0.cons + "," + c1.consbo + "," + punctbo2 + "," + c2.cons);
			  }
			  if (bigram1)
			  {
				  features.Add(c0.cons + "," + c1.cons + "," + punctbo2 + "," + c2.consbo);
			  }

			  if (c2.unigram)
			  {
				  features.Add(c0.consbo + "," + c1.consbo + "," + punctbo2 + "," + c2.cons);
			  }
			  if (c1.unigram)
			  {
				  features.Add(c0.consbo + "," + c1.cons + "," + punctbo2 + "," + c2.consbo);
			  }
			  if (c0.unigram)
			  {
				  features.Add(c0.cons + "," + c1.consbo + "," + punctbo2 + "," + c2.consbo);
			  }

			  features.Add(c0.consbo + "," + c1.consbo + "," + punctbo2 + "," + c2.consbo);

			  if (zeroBackOff)
			  {
				if (bigram1)
				{
					features.Add(c0.cons + "," + c1.cons + "," + punctbo2);
				}
				if (c1.unigram)
				{
					features.Add(c0.consbo + "," + c1.cons + "," + punctbo2);
				}
				if (c0.unigram)
				{
					features.Add(c0.cons + "," + c1.consbo + "," + punctbo2);
				}
				features.Add(c0.consbo + "," + c1.consbo + "," + punctbo2);
			  }
			}
		  }
		}
		else
		{
		  if (punct1s != null)
		  {
			//cons(0),punctbo(1),cons(1),cons(2)
			for (IEnumerator<Parse> pi1 = punct1s.GetEnumerator(); pi1.MoveNext();)
			{
			  string punctbo1 = punctbo(pi1.Current,c1.index <= 0 ? c1.index - 1 : c1.index);
			  if (trigram)
			  {
				  features.Add(c0.cons + "," + punctbo1 + "," + c1.cons + "," + c2.cons);
			  }

			  if (bigram2)
			  {
				  features.Add(c0.consbo + "," + punctbo1 + "," + c1.cons + "," + c2.cons);
			  }
			  if (c0.unigram && c2.unigram)
			  {
				  features.Add(c0.cons + "," + punctbo1 + "," + c1.consbo + "," + c2.cons);
			  }
			  if (bigram1)
			  {
				  features.Add(c0.cons + "," + punctbo1 + "," + c1.cons + "," + c2.consbo);
			  }

			  if (c2.unigram)
			  {
				  features.Add(c0.consbo + "," + punctbo1 + "," + c1.consbo + "," + c2.cons);
			  }
			  if (c1.unigram)
			  {
				  features.Add(c0.consbo + "," + punctbo1 + "," + c1.cons + "," + c2.consbo);
			  }
			  if (c0.unigram)
			  {
				  features.Add(c0.cons + "," + punctbo1 + "," + c1.consbo + "," + c2.consbo);
			  }

			  features.Add(c0.consbo + "," + punctbo1 + "," + c1.consbo + "," + c2.consbo);

			  //zero backoff case covered by cons(0)cons(1)
			}
		  }
		  else
		  {
			//cons(0),cons(1),cons(2)
			if (trigram)
			{
				features.Add(c0.cons + "," + c1.cons + "," + c2.cons);
			}

			if (bigram2)
			{
				features.Add(c0.consbo + "," + c1.cons + "," + c2.cons);
			}
			if (c0.unigram && c2.unigram)
			{
				features.Add(c0.cons + "," + c1.consbo + "," + c2.cons);
			}
			if (bigram1)
			{
				features.Add(c0.cons + "," + c1.cons + "," + c2.consbo);
			}

			if (c2.unigram)
			{
				features.Add(c0.consbo + "," + c1.consbo + "," + c2.cons);
			}
			if (c1.unigram)
			{
				features.Add(c0.consbo + "," + c1.cons + "," + c2.consbo);
			}
			if (c0.unigram)
			{
				features.Add(c0.cons + "," + c1.consbo + "," + c2.consbo);
			}

			features.Add(c0.consbo + "," + c1.consbo + "," + c2.consbo);
		  }
		}
	  }

	  /// <summary>
	  /// Generates features for nodes surrounding a completed node of the specified type. </summary>
	  /// <param name="node"> A surrounding node. </param>
	  /// <param name="i"> The index of the surrounding node with respect to the completed node. </param>
	  /// <param name="type"> The type of the completed node. </param>
	  /// <param name="punctuation"> The punctuation adjacent and between the specified surrounding node. </param>
	  /// <param name="features"> A list to which features are added. </param>
	  protected internal virtual void surround(Parse node, int i, string type, ICollection<Parse> punctuation, IList<string> features)
	  {
		StringBuilder feat = new StringBuilder(20);
		feat.Append("s").Append(i).Append("=");
		if (punctuation != null)
		{
		  for (IEnumerator<Parse> pi = punctuation.GetEnumerator(); pi.MoveNext();)
		  {
			Parse punct = pi.Current;
			if (node != null)
			{
			  feat.Append(node.Head.CoveredText).Append("|").Append(type).Append("|").Append(node.Type).Append("|").Append(punct.Type);
			}
			else
			{
			  feat.Append(type).Append("|").Append(EOS).Append("|").Append(punct.Type);
			}
			features.Add(feat.ToString());

			feat.Length = 0;
			feat.Append("s").Append(i).Append("*=");
			if (node != null)
			{
			  feat.Append(type).Append("|").Append(node.Type).Append("|").Append(punct.Type);
			}
			else
			{
			  feat.Append(type).Append("|").Append(EOS).Append("|").Append(punct.Type);
			}
			features.Add(feat.ToString());

			feat.Length = 0;
			feat.Append("s").Append(i).Append("*=");
			feat.Append(type).Append("|").Append(punct.Type);
			features.Add(feat.ToString());
		  }
		}
		else
		{
		  if (node != null)
		  {
			feat.Append(node.Head.CoveredText).Append("|").Append(type).Append("|").Append(node.Type);
		  }
		  else
		  {
			feat.Append(type).Append("|").Append(EOS);
		  }
		  features.Add(feat.ToString());
		  feat.Length = 0;
		  feat.Append("s").Append(i).Append("*=");
		  if (node != null)
		  {
			feat.Append(type).Append("|").Append(node.Type);
		  }
		  else
		  {
			feat.Append(type).Append("|").Append(EOS);
		  }
		  features.Add(feat.ToString());
		}
	  }

	  /// <summary>
	  /// Produces features to determine whether the specified child node is part of
	  /// a complete constituent of the specified type and adds those features to the
	  /// specfied list. </summary>
	  /// <param name="child"> The parse node to consider. </param>
	  /// <param name="i"> A string indicating the position of the child node. </param>
	  /// <param name="type"> The type of constituent being built. </param>
	  /// <param name="features"> List to add features to. </param>
	  protected internal virtual void checkcons(Parse child, string i, string type, IList<string> features)
	  {
		StringBuilder feat = new StringBuilder(20);
		feat.Append("c").Append(i).Append("=").Append(child.Type).Append("|").Append(child.Head.CoveredText).Append("|").Append(type);
		features.Add(feat.ToString());
		feat.Length = 0;
		feat.Append("c").Append(i).Append("*=").Append(child.Type).Append("|").Append(type);
		features.Add(feat.ToString());
	  }

	  protected internal virtual void checkcons(Parse p1, Parse p2, string type, IList<string> features)
	  {
		StringBuilder feat = new StringBuilder(20);
		feat.Append("cil=").Append(type).Append(",").Append(p1.Type).Append("|").Append(p1.Head.CoveredText).Append(",").Append(p2.Type).Append("|").Append(p2.Head.CoveredText);
		features.Add(feat.ToString());
		feat.Length = 0;
		feat.Append("ci*l=").Append(type).Append(",").Append(p1.Type).Append(",").Append(p2.Type).Append("|").Append(p2.Head.CoveredText);
		features.Add(feat.ToString());
		feat.Length = 0;
		feat.Append("cil*=").Append(type).Append(",").Append(p1.Type).Append("|").Append(p1.Head.CoveredText).Append(",").Append(p2.Type);
		features.Add(feat.ToString());
		feat.Length = 0;
		feat.Append("ci*l*=").Append(type).Append(",").Append(p1.Type).Append(",").Append(p2.Type);
		features.Add(feat.ToString());
	  }

	  /// <summary>
	  /// Populates specified nodes array with left-most right frontier
	  /// node with a unique head. If the right frontier doesn't contain
	  /// enough nodes, then nulls are placed in the array elements. </summary>
	  /// <param name="rf"> The current right frontier. </param>
	  /// <param name="nodes"> The array to be populated. </param>
	  protected internal virtual void getFrontierNodes(IList<Parse> rf, Parse[] nodes)
	  {
		int leftIndex = 0;
		int prevHeadIndex = -1;

		for (int fi = 0;fi < rf.Count;fi++)
		{
		  Parse fn = rf[fi];
		  int headIndex = fn.HeadIndex;
		  if (headIndex != prevHeadIndex)
		  {
			nodes[leftIndex] = fn;
			leftIndex++;
			prevHeadIndex = headIndex;
			if (leftIndex == nodes.Length)
			{
			  break;
			}
		  }
		}
		for (int ni = leftIndex;ni < nodes.Length;ni++)
		{
		  nodes[ni] = null;
		}
	  }

	}

}