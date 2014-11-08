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
using System.Linq;
using j4n.Lang;


namespace opennlp.tools.postag
{


	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using Cache = opennlp.tools.util.Cache;
	using StringList = opennlp.tools.util.StringList;

	/// <summary>
	/// A context generator for the POS Tagger.
	/// </summary>
	public class DefaultPOSContextGenerator : POSContextGenerator
	{

	  protected internal readonly string SE = "*SE*";
	  protected internal readonly string SB = "*SB*";
	  private const int PREFIX_LENGTH = 4;
	  private const int SUFFIX_LENGTH = 4;

	  private static Pattern hasCap = Pattern.compile("[A-Z]");
	  private static Pattern hasNum = Pattern.compile("[0-9]");

	  private Cache contextsCache;
	  private object wordsKey;

	  private Dictionary dict;
	  private string[] dictGram;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="dict"> </param>
	  public DefaultPOSContextGenerator(Dictionary dict) : this(0,dict)
	  {
	  }

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="cacheSize"> </param>
	  /// <param name="dict"> </param>
	  public DefaultPOSContextGenerator(int cacheSize, Dictionary dict)
	  {
		this.dict = dict;
		dictGram = new string[1];
		if (cacheSize > 0)
		{
		  contextsCache = new Cache(cacheSize);
		}
	  }
	  protected internal static string[] getPrefixes(string lex)
	  {
		string[] prefs = new string[PREFIX_LENGTH];
		for (int li = 0, ll = PREFIX_LENGTH; li < ll; li++)
		{
		  prefs[li] = lex.Substring(0, Math.Min(li + 1, lex.Length));
		}
		return prefs;
	  }

	  protected internal static string[] getSuffixes(string lex)
	  {
		string[] suffs = new string[SUFFIX_LENGTH];
		for (int li = 0, ll = SUFFIX_LENGTH; li < ll; li++)
		{
		  suffs[li] = lex.Substring(Math.Max(lex.Length - li - 1, 0));
		}
		return suffs;
	  }

	  public virtual string[] getContext(int index, string[] sequence, string[] priorDecisions, object[] additionalContext)
	  {
		return getContext(index,sequence,priorDecisions);
	  }

	  /// <summary>
	  /// Returns the context for making a pos tag decision at the specified token index given the specified tokens and previous tags. </summary>
	  /// <param name="index"> The index of the token for which the context is provided. </param>
	  /// <param name="tokens"> The tokens in the sentence. </param>
	  /// <param name="tags"> The tags assigned to the previous words in the sentence. </param>
	  /// <returns> The context for making a pos tag decision at the specified token index given the specified tokens and previous tags. </returns>
	  public virtual string[] getContext(int index, object[] tokens, string[] tags)
	  {
		string next, nextnext, lex, prev, prevprev;
		string tagprev, tagprevprev;
		tagprev = tagprevprev = null;
		next = nextnext = lex = prev = prevprev = null;

		lex = tokens[index].ToString();
		if (tokens.Length > index + 1)
		{
		  next = tokens[index + 1].ToString();
		  if (tokens.Length > index + 2)
		  {
			nextnext = tokens[index + 2].ToString();
		  }
		  else
		  {
			nextnext = SE; // Sentence End
		  }

		}
		else
		{
		  next = SE; // Sentence End
		}

		if (index - 1 >= 0)
		{
		  prev = tokens[index - 1].ToString();
		  tagprev = tags[index - 1];

		  if (index - 2 >= 0)
		  {
			prevprev = tokens[index - 2].ToString();
			tagprevprev = tags[index - 2];
		  }
		  else
		  {
			prevprev = SB; // Sentence Beginning
		  }
		}
		else
		{
		  prev = SB; // Sentence Beginning
		}
		string cacheKey = index + tagprev + tagprevprev;
		if (contextsCache != null)
		{
		  if (wordsKey == tokens)
		  {
			string[] cachedContexts = (string[]) contextsCache[cacheKey];
			if (cachedContexts != null)
			{
			  return cachedContexts;
			}
		  }
		  else
		  {
			contextsCache.Clear();
			wordsKey = tokens;
		  }
		}
		IList<string> e = new List<string>();
		e.Add("default");
		// add the word itself
		e.Add("w=" + lex);
		dictGram[0] = lex;
		if (dict == null || !dict.contains(new StringList(dictGram)))
		{
		  // do some basic suffix analysis
		  string[] suffs = getSuffixes(lex);
		  for (int i = 0; i < suffs.Length; i++)
		  {
			e.Add("suf=" + suffs[i]);
		  }

		  string[] prefs = getPrefixes(lex);
		  for (int i = 0; i < prefs.Length; i++)
		  {
			e.Add("pre=" + prefs[i]);
		  }
		  // see if the word has any special characters
		  if (lex.IndexOf('-') != -1)
		  {
			e.Add("h");
		  }

		  if (hasCap.matcher(lex).find())
		  {
			e.Add("c");
		  }

		  if (hasNum.matcher(lex).find())
		  {
			e.Add("d");
		  }
		}
		// add the words and pos's of the surrounding context
		if (prev != null)
		{
		  e.Add("p=" + prev);
		  if (tagprev != null)
		  {
			e.Add("t=" + tagprev);
		  }
		  if (prevprev != null)
		  {
			e.Add("pp=" + prevprev);
			if (tagprevprev != null)
			{
			  e.Add("t2=" + tagprevprev + "," + tagprev);
			}
		  }
		}

		if (next != null)
		{
		  e.Add("n=" + next);
		  if (nextnext != null)
		  {
			e.Add("nn=" + nextnext);
		  }
		}
		string[] contexts = e.ToArray();
		if (contextsCache != null)
		{
		  contextsCache[cacheKey] = contexts;
		}
		return contexts;
	  }

	}

}