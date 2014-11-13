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


	using Chunker = opennlp.tools.chunker.Chunker;
	using AbstractBottomUpParser = opennlp.tools.parser.AbstractBottomUpParser;
	using Parse = opennlp.tools.parser.Parse;
	using POSTagger = opennlp.tools.postag.POSTagger;
	using opennlp.tools.util;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;

	public class ShallowParseCorefEnhancerStream : FilterObjectStream<RawCorefSample, RawCorefSample>
	{

	  private readonly POSTagger posTagger;
	  private readonly Chunker chunker;

	  public ShallowParseCorefEnhancerStream(POSTagger posTagger, Chunker chunker, ObjectStream<RawCorefSample> samples) : base(samples)
	  {
		this.posTagger = posTagger;
		this.chunker = chunker;
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

		  foreach (var sentence in sentences)
		  {

			Parse p = FullParseCorefEnhancerStream.createIncompleteParse(sentence);
			p.Type = AbstractBottomUpParser.TOP_NODE;

			Parse[] parseTokens = p.Children;

			// construct incomplete parse here ..
			string[] tags = posTagger.tag(sentence);

			for (int i = 0; i < parseTokens.Length; i++)
			{
			  p.insert(new Parse(p.Text, parseTokens[i].Span, tags[i], 1d, parseTokens[i].HeadIndex));
			}

			// insert tags into incomplete parse
			Span[] chunks = chunker.chunkAsSpans(sentence, tags);

			foreach (Span chunk in chunks)
			{
			  if ("NP".Equals(chunk.Type))
			  {
				p.insert(new Parse(p.Text, new Span(0,0), chunk.Type, 1d, p.HeadIndex));
			  }
			}

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