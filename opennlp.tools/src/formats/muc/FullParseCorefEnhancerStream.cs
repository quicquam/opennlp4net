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

namespace opennlp.tools.formats.muc
{


	using AbstractBottomUpParser = opennlp.tools.parser.AbstractBottomUpParser;
	using Parse = opennlp.tools.parser.Parse;
	using Parser = opennlp.tools.parser.Parser;
	using opennlp.tools.util;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;

	public class FullParseCorefEnhancerStream : FilterObjectStream<RawCorefSample, RawCorefSample>
	{

	  private readonly Parser parser;

	  public FullParseCorefEnhancerStream(Parser parser, ObjectStream<RawCorefSample> samples) : base(samples)
	  {
		this.parser = parser;
	  }

	  internal static Parse createIncompleteParse(string[] tokens)
	  {

		// produce text
		Span[] tokenSpans = new Span[tokens.Length];
		StringBuilder textBuilder = new StringBuilder();

		for (int i = 0; i < tokens.Length; i++)
		{

		  if (textBuilder.Length > 0)
		  {
			textBuilder.Append(' ');
		  }

		  int startOffset = textBuilder.Length;
		  textBuilder.Append(tokens[i]);
		  tokenSpans[i] = new Span(startOffset, textBuilder.Length);
		}

		string text = textBuilder.ToString();

		Parse p = new Parse(text, new Span(0, text.Length), AbstractBottomUpParser.INC_NODE, 0, 0);

		for (int i = 0; i < tokenSpans.Length; i++)
		{
		  Span tokenSpan = tokenSpans[i];
		  p.insert(new Parse(text, new Span(tokenSpan.Start, tokenSpan.End), AbstractBottomUpParser.TOK_NODE, 0, i));
		}

		return p;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RawCorefSample read() throws java.io.IOException
	  public override RawCorefSample read()
	  {

		RawCorefSample sample = samples.read();

		if (sample != null)
		{

		  IList<Parse> enhancedParses = new List<Parse>();

		  IList<string[]> sentences = sample.Texts;

		  for (int i = 0; i < sentences.Count; i++)
		  {

			string[] sentence = sentences[i];

			Parse incompleteParse = createIncompleteParse(sentence);
			Parse p = parser.parse(incompleteParse);

			// What to do when a parse cannot be found ?!

			enhancedParses.Add(p);
		  }

		  sample.Parses = enhancedParses;

		  return sample;
		}
		else
		{
		  return null;
		}
	  }
	}

}