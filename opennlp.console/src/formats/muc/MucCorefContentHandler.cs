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
using opennlp.nonjava.helperclasses;
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.formats.muc
{


	using Tokenizer = opennlp.tools.tokenize.Tokenizer;
	using Span = opennlp.tools.util.Span;

	// Note:
	// Take care for special @ sign handling (identifies a table or something else that should be ignored)
    public class MucCorefContentHandler : SgmlParser.ContentHandler
	{
        public class CorefMention
	  {
		internal Span span;
		internal int id;
		internal string min;

		internal CorefMention(Span span, int id, string min)
		{
		  this.span = span;
		  this.id = id;
		  this.min = min;
		}
	  }

	  internal const string COREF_ELEMENT = "COREF";

	  private readonly Tokenizer tokenizer;
	  private readonly IList<RawCorefSample> samples;

	  internal bool isInsideContentElement = false;
	  private readonly IList<string> text = new List<string>();
	  private Stack<CorefMention> mentionStack = new Stack<CorefMention>();
	  private IList<CorefMention> mentions = new List<MucCorefContentHandler.CorefMention>();

	  private IDictionary<int?, int?> idMap = new Dictionary<int?, int?>();

	  private RawCorefSample sample;

	  internal MucCorefContentHandler(Tokenizer tokenizer, IList<RawCorefSample> samples)
	  {
		this.tokenizer = tokenizer;
		this.samples = samples;
	  }

	  /// <summary>
	  /// Resolve an id via the references to the root id.
	  /// </summary>
	  /// <param name="id"> the id or reference to be resolved
	  /// </param>
	  /// <returns> the resolved id or -1 if id cannot be resolved </returns>
	  private int resolveId(int id)
	  {

		int? refId = idMap[id];

		if (refId != null)
		{
		  if (id == refId)
		  {
			return id;
		  }
		  else
		  {
			return resolveId(refId.Value);
		  }
		}
		else
		{
		  return -1;
		}
	  }

	  public override void startElement(string name, IDictionary<string, string> attributes)
	  {

		if (MucElementNames.DOC_ELEMENT.Equals(name))
		{
		  idMap.Clear();
		  sample = new RawCorefSample(new List<string>(), new List<MucCorefContentHandler.CorefMention[]>());
		}

		if (MucElementNames.CONTENT_ELEMENTS.Contains(name))
		{
		  isInsideContentElement = true;
		}

		if (COREF_ELEMENT.Equals(name))
		{
		  int beginOffset = text.Count;

		  string idString = attributes["ID"];
		  string refString = attributes["REF"];

		  int id;
		  if (idString != null)
		  {
			id = Convert.ToInt32(idString); // might fail

			if (refString == null)
			{
			  idMap[id] = id;
			}
			else
			{
			  int @ref = Convert.ToInt32(refString);
			  idMap[id] = @ref;
			}
		  }
		  else
		  {
			id = -1;
			// throw invalid format exception ...
		  }

		  mentionStack.Push(new CorefMention(new Span(beginOffset, beginOffset), id, attributes["MIN"]));
		}
	  }

	  public override void characters(CharSequence chars)
	  {
		if (isInsideContentElement)
		{

		  string[] tokens = tokenizer.tokenize(chars.ToString());

		  text.AddRange(tokens);
		}
	  }

	  public override void endElement(string name)
	  {

		if (COREF_ELEMENT.Equals(name))
		{
		  CorefMention mention = mentionStack.Pop();
		  mention.span = new Span(mention.span.Start, text.Count);
		  mentions.Add(mention);
		}

		if (MucElementNames.CONTENT_ELEMENTS.Contains(name))
		{

		  sample.Texts.Add(text.ToArray());
		  sample.Mentions.Add(mentions.ToArray());

		  mentions.Clear();
		  text.Clear();
		  isInsideContentElement = false;
		}

		if (MucElementNames.DOC_ELEMENT.Equals(name))
		{
            // TODO fix array syntax
/*
		  foreach (CorefMention mentions[] in sample.Mentions)
		  {
			for (int i = 0; i < mentions.Length; i++)
			{
			  mentions[i].id = resolveId(mentions[i].id);
			}
		  }
            */
		  samples.Add(sample);
		}
	  }
	}

}