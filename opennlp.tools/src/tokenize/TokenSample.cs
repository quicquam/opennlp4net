using System;
using System.Collections.Generic;
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


namespace opennlp.tools.tokenize
{


	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// A <seealso cref="TokenSample"/> is text with token spans.
	/// </summary>
	public class TokenSample
	{

	  public const string DEFAULT_SEPARATOR_CHARS = "<SPLIT>";

	  private readonly string separatorChars = DEFAULT_SEPARATOR_CHARS;

	  private readonly string text;

	  private readonly IList<Span> tokenSpans;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="text"> the text which contains the tokens. </param>
	  /// <param name="tokenSpans"> the spans which mark the begin and end of the tokens. </param>
	  public TokenSample(string text, Span[] tokenSpans)
	  {

		if (text == null)
		{
		  throw new System.ArgumentException("text must not be null!");
		}

		if (tokenSpans == null)
		{
		  throw new System.ArgumentException("tokenSpans must not be null! ");
		}

		this.text = text;
		this.tokenSpans = Collections.unmodifiableList(new List<Span>(Arrays.asList(tokenSpans)));

		foreach (Span tokenSpan in tokenSpans)
		{
		  if (tokenSpan.Start < 0 || tokenSpan.Start > text.Length || tokenSpan.End > text.Length || tokenSpan.End < 0)
		  {
			throw new System.ArgumentException("Span " + tokenSpan.ToString() + " is out of bounds, text length: " + text.Length + "!");
		  }
		}
	  }

	  public TokenSample(Detokenizer detokenizer, string[] tokens)
	  {

		StringBuilder sentence = new StringBuilder();

		Detokenizer_DetokenizationOperation[] operations = detokenizer.detokenize(tokens);

		IList<Span> mergedTokenSpans = new List<Span>();

		for (int i = 0; i < operations.Length; i++)
		{

		  bool isSeparateFromPreviousToken = i > 0 && !isMergeToRight(operations[i - 1]) && !isMergeToLeft(operations[i]);

		  if (isSeparateFromPreviousToken)
		  {
			sentence.Append(' ');
		  }

		  int beginIndex = sentence.Length;
		  sentence.Append(tokens[i]);
		  mergedTokenSpans.Add(new Span(beginIndex, sentence.Length));
		}

		text = sentence.ToString();
		tokenSpans = Collections.unmodifiableList(mergedTokenSpans);
	  }

	  private bool isMergeToRight(Detokenizer_DetokenizationOperation operation)
	  {
		return Detokenizer_DetokenizationOperation.MERGE_TO_RIGHT.Equals(operation) || Detokenizer_DetokenizationOperation.MERGE_BOTH.Equals(operation);
	  }

	  private bool isMergeToLeft(Detokenizer_DetokenizationOperation operation)
	  {
		return Detokenizer_DetokenizationOperation.MERGE_TO_LEFT.Equals(operation) || Detokenizer_DetokenizationOperation.MERGE_BOTH.Equals(operation);
	  }

	  /// <summary>
	  /// Retrieves the text.
	  /// </summary>
	  public virtual string Text
	  {
		  get
		  {
			return text;
		  }
	  }

	  /// <summary>
	  /// Retrieves the token spans.
	  /// </summary>
	  public virtual Span[] TokenSpans
	  {
		  get
		  {
			return tokenSpans.ToArray();
		  }
	  }

	  public override string ToString()
	  {

		StringBuilder sentence = new StringBuilder();

		int lastEndIndex = -1;
		foreach (Span token in tokenSpans)
		{

		  if (lastEndIndex != -1)
		  {

			// If there are no chars between last token
			// and this token insert the separator chars
			// otherwise insert a space

			string separator = "";
			if (lastEndIndex == token.Start)
			{
			  separator = separatorChars;
			}
			else
			{
			  separator = " ";
			}

			sentence.Append(separator);
		  }

		  sentence.Append(token.getCoveredText(text));

		  lastEndIndex = token.End;
		}

		return sentence.ToString();
	  }

	  private static void addToken(StringBuilder sample, IList<Span> tokenSpans, string token, bool isNextMerged)
	  {

		int tokenSpanStart = sample.Length;
		sample.Append(token);
		int tokenSpanEnd = sample.Length;

		tokenSpans.Add(new Span(tokenSpanStart, tokenSpanEnd));

		if (!isNextMerged)
		{
			sample.Append(" ");
		}
	  }

	  public static TokenSample parse(string sampleString, string separatorChars)
	  {

		if (sampleString == null)
		{
			throw new System.ArgumentException("sampleString must not be null!");
		}
		if (separatorChars == null)
		{
			throw new System.ArgumentException("separatorChars must not be null!");
		}

		Span[] whitespaceTokenSpans = WhitespaceTokenizer.INSTANCE.tokenizePos(sampleString);

		// Pre-allocate 20% for newly created tokens
		IList<Span> realTokenSpans = new List<Span>((int)(whitespaceTokenSpans.Length * 1.2d));

		StringBuilder untaggedSampleString = new StringBuilder();

		foreach (Span whiteSpaceTokenSpan in whitespaceTokenSpans)
		{
		  string whitespaceToken = whiteSpaceTokenSpan.getCoveredText(sampleString).ToString();

		  bool wasTokenReplaced = false;

		  int tokStart = 0;
		  int tokEnd = -1;
		  while ((tokEnd = whitespaceToken.IndexOf(separatorChars, tokStart, StringComparison.Ordinal)) > -1)
		  {

			string token = whitespaceToken.Substring(tokStart, tokEnd - tokStart);

			addToken(untaggedSampleString, realTokenSpans, token, true);

			tokStart = tokEnd + separatorChars.Length;
			wasTokenReplaced = true;
		  }

		  if (wasTokenReplaced)
		  {
			// If the token contains the split chars at least once
			// a span for the last token must still be added
			string token = whitespaceToken.Substring(tokStart);

			addToken(untaggedSampleString, realTokenSpans, token, false);
		  }
		  else
		  {
			// If it does not contain the split chars at lest once
			// just copy the original token span

			addToken(untaggedSampleString, realTokenSpans, whitespaceToken, false);
		  }
		}

		return new TokenSample(untaggedSampleString.ToString(), realTokenSpans.ToArray());
	  }

	  public override bool Equals(object obj)
	  {
		if (this == obj)
		{
		  return true;
		}
		else if (obj is TokenSample)
		{
		  TokenSample a = (TokenSample) obj;

		  return Text.Equals(a.Text) && Arrays.Equals(TokenSpans, a.TokenSpans);
		}
		else
		{
		  return false;
		}
	  }
	}

}