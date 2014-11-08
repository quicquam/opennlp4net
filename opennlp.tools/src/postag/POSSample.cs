using System;
using System.Collections.Generic;
using System.Linq;
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
using opennlp.nonjava.helperclasses;


namespace opennlp.tools.postag
{


	using WhitespaceTokenizer = opennlp.tools.tokenize.WhitespaceTokenizer;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;

	/// <summary>
	/// Represents an pos-tagged sentence.
	/// </summary>
	public class POSSample
	{

	  private IList<string> sentence;

	  private IList<string> tags;

	  private readonly string[][] additionalContext;

	  public POSSample(string[] sentence, string[] tags) : this(sentence, tags, null)
	  {
	  }

	  public POSSample(IList<string> sentence, IList<string> tags) : this(sentence, tags, null)
	  {
	  }

	  public POSSample(IList<string> sentence, IList<string> tags, string[][] additionalContext)
	  {
		this.sentence = sentence;
		this.tags = tags;

		checkArguments();
		string[][] ac;
		if (additionalContext != null)
		{
		  ac = new string[additionalContext.Length][];

		  for (int i = 0; i < additionalContext.Length; i++)
		  {
			ac[i] = new string[additionalContext[i].Length];
			Array.Copy(additionalContext[i], 0, ac[i], 0, additionalContext[i].Length);
		  }
		}
		else
		{
		  ac = null;
		}
		this.additionalContext = ac;
	  }

	  public POSSample(string[] sentence, string[] tags, string[][] additionalContext) : this(sentence.ToList(), tags.ToList(), additionalContext)
	  {
	  }

	  private void checkArguments()
	  {
		if (sentence.Count != tags.Count)
		{
		  throw new System.ArgumentException("There must be exactly one tag for each token. tokens: " + sentence.Count + ", tags: " + tags.Count);
		}

		  if (sentence.Contains(null))
		  {
			throw new System.ArgumentException("null elements are not allowed in sentence tokens!");
		  }
		  if (tags.Contains(null))
		  {
			throw new System.ArgumentException("null elements are not allowed in tags!");
		  }
	  }

	  public virtual string[] Sentence
	  {
		  get
		  {
			return sentence.ToArray();
		  }
	  }

	  public virtual string[] Tags
	  {
		  get
		  {
			return tags.ToArray();
		  }
	  }

	  public virtual string[][] AddictionalContext
	  {
		  get
		  {
			return this.additionalContext;
		  }
	  }

	  public override string ToString()
	  {

		StringBuilder result = new StringBuilder();

		for (int i = 0; i < Sentence.Length; i++)
		{
		  result.Append(Sentence[i]);
		  result.Append('_');
		  result.Append(Tags[i]);
		  result.Append(' ');
		}

		if (result.Length > 0)
		{
		  // get rid of last space
		  result.Length = result.Length - 1;
		}

		return result.ToString();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static POSSample parse(String sentenceString) throws opennlp.tools.util.InvalidFormatException
	  public static POSSample parse(string sentenceString)
	  {

		string[] tokenTags = WhitespaceTokenizer.INSTANCE.tokenize(sentenceString);

		string[] sentence = new string[tokenTags.Length];
		string[] tags = new string[tokenTags.Length];

		for (int i = 0; i < tokenTags.Length; i++)
		{
		  int split = tokenTags[i].LastIndexOf("_", StringComparison.Ordinal);

		  if (split == -1)
		  {
			throw new InvalidFormatException("Cannot find \"_\" inside token '" + tokenTags[i] + "'!");
		  }

		  sentence[i] = tokenTags[i].Substring(0, split);
		  tags[i] = tokenTags[i].Substring(split + 1);
		}

		return new POSSample(sentence, tags);
	  }

	  public override bool Equals(object obj)
	  {
		if (this == obj)
		{
		  return true;
		}
		else if (obj is POSSample)
		{
		  POSSample a = (POSSample) obj;

		  return Arrays.Equals(Sentence, a.Sentence) && Arrays.Equals(Tags, a.Tags);
		}
		else
		{
		  return false;
		}
	  }
	}

}