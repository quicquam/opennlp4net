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
using j4n.Serialization;
using opennlp.tools.namefind;
using opennlp.tools.parser;
using opennlp.tools.util;

namespace opennlp.console.formats.muc
{
    /// <summary>
	/// Adds names to the Coref Sample Stream.
	/// </summary>
	public class NameFinderCorefEnhancerStream : FilterObjectStream<RawCorefSample, RawCorefSample>
	{

	  private TokenNameFinder[] nameFinders;
	  private string[] tags;

	  // TODO: Should be updated to use tag from span instead!
	  protected internal NameFinderCorefEnhancerStream(TokenNameFinder[] nameFinders, string[] tags, ObjectStream<RawCorefSample> samples) : base(samples)
	  {
		this.nameFinders = nameFinders;
		this.tags = tags;
	  }

	  public override RawCorefSample read()
	  {

		RawCorefSample sample = samples.read();

		if (sample != null)
		{

		  foreach (TokenNameFinder namefinder in nameFinders)
		  {
			namefinder.clearAdaptiveData();
		  }

		  IList<Parse> parses = new List<Parse>();

		  foreach (Parse p in sample.Parses)
		  {

			Parse[] parseTokens = p.TagNodes;
			string[] tokens = new string[parseTokens.Length];

			for (int i = 0; i < tokens.Length; i++)
			{
			  tokens[i] = parseTokens[i].ToString();
			}

			for (int i = 0; i < nameFinders.Length; i++)
			{
			  Span[] names = nameFinders[i].find(tokens);
			  Parse.addNames(tags[i], names, parseTokens);
			}

			parses.Add(p);
		  }

		  sample.Parses = parses;

		  return sample;
		}
		else
		{
		  return null;
		}
	  }
	}

}