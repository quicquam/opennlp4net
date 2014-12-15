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

using System.Collections.Generic;
using opennlp.tools.chunker;
using opennlp.tools.parser;
using opennlp.tools.postag;
using opennlp.tools.util;

namespace opennlp.tools.formats.muc
{
    public class ShallowParseCorefEnhancerStream : FilterObjectStream<RawCorefSample, RawCorefSample>
	{

	  private readonly POSTagger posTagger;
	  private readonly Chunker chunker;

	  public ShallowParseCorefEnhancerStream(POSTagger posTagger, Chunker chunker, ObjectStream<RawCorefSample> samples) : base(samples)
	  {
		this.posTagger = posTagger;
		this.chunker = chunker;
	  }

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