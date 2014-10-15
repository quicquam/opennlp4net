using System;
using System.Collections.Generic;
using System.IO;
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
using j4n.Lang;
using opennlp.nonjava.helperclasses;

namespace opennlp.tools.namefind
{


	using WhitespaceTokenizer = opennlp.tools.tokenize.WhitespaceTokenizer;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// Class for holding names for a single unit of text.
	/// </summary>
	public class NameSample
	{

	  private readonly IList<string> sentence;
	  private readonly IList<Span> names;
	  private readonly string[][] additionalContext;
	  private readonly bool isClearAdaptiveData;

	  /// <summary>
	  /// The a default type value when there is no type in training data. </summary>
	  public const string DEFAULT_TYPE = "default";

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="sentence"> training sentence </param>
	  /// <param name="names"> </param>
	  /// <param name="additionalContext"> </param>
	  /// <param name="clearAdaptiveData"> if true the adaptive data of the 
	  ///     feature generators is cleared </param>
	  public NameSample(string[] sentence, Span[] names, string[][] additionalContext, bool clearAdaptiveData)
	  {

		if (sentence == null)
		{
		  throw new System.ArgumentException("sentence must not be null!");
		}

		if (names == null)
		{
		  names = new Span[0];
		}

		this.sentence = sentence.ToList();
		this.names = names.ToList();

		if (additionalContext != null)
		{
		  this.additionalContext = new string[additionalContext.Length][];

		  for (int i = 0; i < additionalContext.Length; i++)
		  {
			this.additionalContext[i] = new string[additionalContext[i].Length];
			Array.Copy(additionalContext[i], 0, this.additionalContext[i], 0, additionalContext[i].Length);
		  }
		}
		else
		{
		  this.additionalContext = null;
		}
		isClearAdaptiveData = clearAdaptiveData;

		// TODO: Check that name spans are not overlapping, otherwise throw exception
	  }

	  public NameSample(string[] sentence, Span[] names, bool clearAdaptiveData) : this(sentence, names, null, clearAdaptiveData)
	  {
	  }

	  public virtual string[] Sentence
	  {
		  get
		  {
			return sentence.ToArray();
		  }
	  }

	  public virtual Span[] Names
	  {
		  get
		  {
			return names.ToArray();
		  }
	  }

	  public virtual string[][] AdditionalContext
	  {
		  get
		  {
			return additionalContext;
		  }
	  }

	  public virtual bool ClearAdaptiveDataSet
	  {
		  get
		  {
			return isClearAdaptiveData;
		  }
	  }

	  public override bool Equals(object obj)
	  {

		if (this == obj)
		{
		  return true;
		}
		else if (obj is NameSample)
		{
		  NameSample a = (NameSample) obj;

		  return Arrays.Equals(Sentence, a.Sentence) && Arrays.Equals(Names, a.Names) && Arrays.Equals(AdditionalContext, a.AdditionalContext) && ClearAdaptiveDataSet == a.ClearAdaptiveDataSet;
		}
		else
		{
		  return false;
		}

	  }

	  public override string ToString()
	  {
		StringBuilder result = new StringBuilder();

		// If adaptive data must be cleared insert an empty line
		// before the sample sentence line
		if (ClearAdaptiveDataSet)
		{
		  result.Append("\n");
		}

		for (int tokenIndex = 0; tokenIndex < sentence.Count; tokenIndex++)
		{
		  // token

		  foreach (Span name in names)
		  {
			if (name.Start == tokenIndex)
			{
			  // check if nameTypes is null, or if the nameType for this specific
			  // entity is empty. If it is, we leave the nameType blank.
			  if (name.Type == null)
			  {
				result.Append(NameSampleDataStream.START_TAG).Append(' ');
			  }
			  else
			  {
				result.Append(NameSampleDataStream.START_TAG_PREFIX).Append(name.Type).Append("> ");
			  }
			}

			if (name.End == tokenIndex)
			{
			  result.Append(NameSampleDataStream.END_TAG).Append(' ');
			}
		  }

		  result.Append(sentence[tokenIndex]).Append(' ');
		}

		if (sentence.Count > 1)
		{
		  result.Length = result.Length - 1;
		}

		foreach (Span name in names)
		{
		  if (name.End == sentence.Count)
		  {
			result.Append(' ').Append(NameSampleDataStream.END_TAG);
		  }
		}

		return result.ToString();
	  }

	  private static string errorTokenWithContext(string[] sentence, int index)
	  {

		StringBuilder errorString = new StringBuilder();

		// two token before
		if (index > 1)
		{
		  errorString.Append(sentence[index - 2]).Append(" ");
		}

		if (index > 0)
		{
		  errorString.Append(sentence[index - 1]).Append(" ");
		}

		// token itself
		errorString.Append("###");
		errorString.Append(sentence[index]);
		errorString.Append("###").Append(" ");

		// two token after
		if (index + 1 < sentence.Length)
		{
		  errorString.Append(sentence[index + 1]).Append(" ");
		}

		if (index + 2 < sentence.Length)
		{
		  errorString.Append(sentence[index + 2]);
		}

		return errorString.ToString();
	  }

	  private static readonly Pattern START_TAG_PATTERN = Pattern.compile("<START(:([^:>\\s]*))?>");

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static NameSample parse(String taggedTokens, boolean isClearAdaptiveData) throws java.io.IOException
	  public static NameSample parse(string taggedTokens, bool isClearAdaptiveData)
	  {
		return parse(taggedTokens, DEFAULT_TYPE, isClearAdaptiveData);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static NameSample parse(String taggedTokens, String defaultType, boolean isClearAdaptiveData) throws java.io.IOException
	  public static NameSample parse(string taggedTokens, string defaultType, bool isClearAdaptiveData)
	  {
		// TODO: Should throw another exception, and then convert it into an IOException in the stream
		string[] parts = WhitespaceTokenizer.INSTANCE.tokenize(taggedTokens);

		IList<string> tokenList = new List<string>(parts.Length);
		IList<Span> nameList = new List<Span>();

		string nameType = defaultType;
		int startIndex = -1;
		int wordIndex = 0;

		// we check if at least one name has the a type. If no one has, we will
		// leave the NameType property of NameSample null.
		bool catchingName = false;

		for (int pi = 0; pi < parts.Length; pi++)
		{
		  Matcher startMatcher = START_TAG_PATTERN.matcher(parts[pi]);
		  if (startMatcher.matches())
		  {
			if (catchingName)
			{
			  throw new IOException("Found unexpected annotation" + " while handling a name sequence: " + errorTokenWithContext(parts, pi));
			}
			catchingName = true;
			startIndex = wordIndex;
			string nameTypeFromSample = startMatcher.group(2);
			if (nameTypeFromSample != null)
			{
			  if (nameTypeFromSample.Length == 0)
			  {
				throw new IOException("Missing a name type: " + errorTokenWithContext(parts, pi));
			  }
			  nameType = nameTypeFromSample;
			}

		  }
		  else if (parts[pi].Equals(NameSampleDataStream.END_TAG))
		  {
			if (catchingName == false)
			{
			  throw new IOException("Found unexpected annotation: " + errorTokenWithContext(parts, pi));
			}
			catchingName = false;
			// create name
			nameList.Add(new Span(startIndex, wordIndex, nameType));

		  }
		  else
		  {
			tokenList.Add(parts[pi]);
			wordIndex++;
		  }
		}
		string[] sentence = tokenList.ToArray();
		Span[] names = nameList.ToArray();

		return new NameSample(sentence, names, isClearAdaptiveData);
	  }
	}

}