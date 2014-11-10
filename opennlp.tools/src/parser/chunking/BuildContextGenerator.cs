using System;
using System.Collections.Generic;
using System.Linq;

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

namespace opennlp.tools.parser.chunking
{


	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using StringList = opennlp.tools.util.StringList;

	/// <summary>
	/// Class to generator predictive contexts for deciding how constituents should be combined together.
	/// </summary>
	public class BuildContextGenerator : AbstractContextGenerator
	{

	  private Dictionary dict;
	  private string[] unigram;
	  private string[] bigram;
	  private string[] trigram;

	  /// <summary>
	  /// Creates a new context generator for making decisions about combining constitients togehter.
	  /// 
	  /// </summary>
	  public BuildContextGenerator() : base()
	  {
		zeroBackOff = false;
		useLabel = true;
	  }

	  public BuildContextGenerator(Dictionary dict) : this()
	  {
		this.dict = dict;
		unigram = new string[1];
		bigram = new string[2];
		trigram = new string[3];
	  }

	  public virtual string[] getContext(object o)
	  {
		object[] @params = (object[]) o;
		return getContext((Parse[]) @params[0], (int) @params[1]);
	  }

	  /// <summary>
	  /// Returns the predictive context used to determine how constituent at the specified index
	  /// should be combined with other contisuents. </summary>
	  /// <param name="constituents"> The constituents which have yet to be combined into new constituents. </param>
	  /// <param name="index"> The index of the constituent whcihi is being considered. </param>
	  /// <returns> the context for building constituents at the specified index. </returns>
	  public virtual string[] getContext(Parse[] constituents, int index)
	  {
		IList<string> features = new List<string>(100);
		int ps = constituents.Length;

		// cons(-2), cons(-1), cons(0), cons(1), cons(2)
		// cons(-2)
		Parse p_2 = null;
		Parse p_1 = null;
		Parse p0 = null;
		Parse p1 = null;
		Parse p2 = null;

		ICollection<Parse> punct1s = null;
		ICollection<Parse> punct2s = null;
		ICollection<Parse> punct_1s = null;
		ICollection<Parse> punct_2s = null;

		if (index - 2 >= 0)
		{
		  p_2 = constituents[index - 2];
		}
		if (index - 1 >= 0)
		{
		  p_1 = constituents[index - 1];
		  punct_2s = p_1.PreviousPunctuationSet;
		}
		p0 = constituents[index];
		punct_1s = p0.PreviousPunctuationSet;
		punct1s = p0.NextPunctuationSet;

		if (index + 1 < ps)
		{
		  p1 = constituents[index + 1];
		  punct2s = p1.NextPunctuationSet;
		}
		if (index + 2 < ps)
		{
		  p2 = constituents[index + 2];
		}

		bool u_2 = true;
		bool u_1 = true;
		bool u0 = true;
		bool u1 = true;
		bool u2 = true;
		bool b_2_1 = true;
		bool b_10 = true;
		bool b01 = true;
		bool b12 = true;
		bool t_2_10 = true;
		bool t_101 = true;
		bool t012 = true;

		if (dict != null)
		{

		  if (p_2 != null)
		  {
			unigram[0] = p_2.Head.CoveredText;
			u_2 = dict.contains(new StringList(unigram));
		  }

		  if (p2 != null)
		  {
			unigram[0] = p2.Head.CoveredText;
			u2 = dict.contains(new StringList(unigram));
		  }

		  unigram[0] = p0.Head.CoveredText;
		  u0 = dict.contains(new StringList(unigram));

		  if (p_2 != null && p_1 != null)
		  {
			bigram[0] = p_2.Head.CoveredText;
			bigram[1] = p_1.Head.CoveredText;
			b_2_1 = dict.contains(new StringList(bigram));

			trigram[0] = p_2.Head.CoveredText;
			trigram[1] = p_1.Head.CoveredText;
			trigram[2] = p0.Head.CoveredText;
			t_2_10 = dict.contains(new StringList(trigram));
		  }
		  if (p_1 != null && p1 != null)
		  {
			trigram[0] = p_1.Head.CoveredText;
			trigram[1] = p0.Head.CoveredText;
			trigram[2] = p1.Head.CoveredText;
			t_101 = dict.contains(new StringList(trigram));
		  }
		  if (p_1 != null)
		  {
			unigram[0] = p_1.Head.CoveredText;
			u_1 = dict.contains(new StringList(unigram));

			//extra check for 2==null case
			b_2_1 = b_2_1 && u_1 & u_2;
			t_2_10 = t_2_10 && u_1 & u_2 & u0;
			t_101 = t_101 && u_1 & u0 && u1;

			bigram[0] = p_1.Head.CoveredText;
			bigram[1] = p0.Head.CoveredText;
			b_10 = dict.contains(new StringList(bigram)) && u_1 && u0;
		  }
		  if (p1 != null && p2 != null)
		  {
			bigram[0] = p1.Head.CoveredText;
			bigram[1] = p2.Head.CoveredText;
			b12 = dict.contains(new StringList(bigram));

			trigram[0] = p0.Head.CoveredText;
			trigram[1] = p1.Head.CoveredText;
			trigram[2] = p2.Head.CoveredText;
			t012 = dict.contains(new StringList(trigram));
		  }
		  if (p1 != null)
		  {
			unigram[0] = p1.Head.CoveredText;
			u1 = dict.contains(new StringList(unigram));

			//extra check for 2==null case
			b12 = b12 && u1 && u2;
			t012 = t012 && u1 && u2 && u0;
			t_101 = t_101 && u0 && u_1 && u1;

			bigram[0] = p0.Head.CoveredText;
			bigram[1] = p1.Head.CoveredText;
			b01 = dict.contains(new StringList(bigram));
			b01 = b01 && u0 && u1;
		  }
		}

		string consp_2 = cons(p_2, -2);
		string consp_1 = cons(p_1, -1);
		string consp0 = cons(p0, 0);
		string consp1 = cons(p1, 1);
		string consp2 = cons(p2, 2);

		string consbop_2 = consbo(p_2, -2);
		string consbop_1 = consbo(p_1, -1);
		string consbop0 = consbo(p0, 0);
		string consbop1 = consbo(p1, 1);
		string consbop2 = consbo(p2, 2);

		Cons c_2 = new Cons(consp_2,consbop_2,-2,u_2);
		Cons c_1 = new Cons(consp_1,consbop_1,-1,u_1);
		Cons c0 = new Cons(consp0,consbop0,0,u0);
		Cons c1 = new Cons(consp1,consbop1,1,u1);
		Cons c2 = new Cons(consp2,consbop2,2,u2);

		//default
		features.Add("default");
		//first constituent label
		//features.add("fl="+constituents[0].getLabel());

		// features.add("stage=cons(i)");
		// cons(-2), cons(-1), cons(0), cons(1), cons(2)
		if (u0)
		{
			features.Add(consp0);
		}
		features.Add(consbop0);

		if (u_2)
		{
			features.Add(consp_2);
		}
		features.Add(consbop_2);
		if (u_1)
		{
			features.Add(consp_1);
		}
		features.Add(consbop_1);
		if (u1)
		{
			features.Add(consp1);
		}
		features.Add(consbop1);
		if (u2)
		{
			features.Add(consp2);
		}
		features.Add(consbop2);

		//cons(0),cons(1)
		cons2(features,c0,c1,punct1s,b01);
		//cons(-1),cons(0)
		cons2(features,c_1,c0,punct_1s,b_10);
		//features.add("stage=cons(0),cons(1),cons(2)");
		cons3(features,c0,c1,c2,punct1s,punct2s,t012,b01,b12);
		cons3(features,c_2,c_1,c0,punct_2s,punct_1s,t_2_10,b_2_1,b_10);
		cons3(features,c_1,c0,c1,punct_1s,punct1s,t_101,b_10,b01);
		//features.add("stage=other");
		string p0Tag = p0.Type;
		if (p0Tag.Equals("-RRB-"))
		{
		  for (int pi = index - 1; pi >= 0; pi--)
		  {
			Parse p = constituents[pi];
			if (p.Type.Equals("-LRB-"))
			{
			  features.Add("bracketsmatch");
			  break;
			}
			if (p.Label.StartsWith(Parser.START, StringComparison.Ordinal))
			{
			  break;
			}
		  }
		}
		if (p0Tag.Equals("-RCB-"))
		{
		  for (int pi = index - 1; pi >= 0; pi--)
		  {
			Parse p = constituents[pi];
			if (p.Type.Equals("-LCB-"))
			{
			  features.Add("bracketsmatch");
			  break;
			}
			if (p.Label.StartsWith(Parser.START, StringComparison.Ordinal))
			{
			  break;
			}
		  }
		}
		if (p0Tag.Equals("''"))
		{
		  for (int pi = index - 1; pi >= 0; pi--)
		  {
			Parse p = constituents[pi];
			if (p.Type.Equals("``"))
			{
			  features.Add("quotesmatch");
			  break;
			}
			if (p.Label.StartsWith(Parser.START, StringComparison.Ordinal))
			{
			  break;
			}
		  }
		}
		if (p0Tag.Equals("'"))
		{
		  for (int pi = index - 1; pi >= 0; pi--)
		  {
			Parse p = constituents[pi];
			if (p.Type.Equals("`"))
			{
			  features.Add("quotesmatch");
			  break;
			}
			if (p.Label.StartsWith(Parser.START, StringComparison.Ordinal))
			{
			  break;
			}
		  }
		}
		if (p0Tag.Equals(","))
		{
		  for (int pi = index - 1; pi >= 0; pi--)
		  {
			Parse p = constituents[pi];
			if (p.Type.Equals(","))
			{
			  features.Add("iscomma");
			  break;
			}
			if (p.Label.StartsWith(Parser.START, StringComparison.Ordinal))
			{
			  break;
			}
		  }
		}
		if (p0Tag.Equals(".") && index == ps - 1)
		{
		  for (int pi = index - 1; pi >= 0; pi--)
		  {
			Parse p = constituents[pi];
			if (p.Label.StartsWith(Parser.START, StringComparison.Ordinal))
			{
			  if (pi == 0)
			  {
				features.Add("endofsentence");
			  }
			  break;
			}
		  }
		}
		return (features.ToArray());
	  }
	}

}