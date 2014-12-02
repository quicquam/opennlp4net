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
using System.Linq;
using j4n.IO.File;
using j4n.Lang;
using opennlp.tools.namefind;
using opennlp.tools.nonjava.extensions;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.console.formats.muc
{
    public class MucNameContentHandler : SgmlParser.ContentHandler
	{

	  private const string ENTITY_ELEMENT_NAME = "ENAMEX";
	  private const string TIME_ELEMENT_NAME = "TIMEX";
	  private const string NUM_ELEMENT_NAME = "NUMEX";

	  private static readonly HashSet<string> NAME_ELEMENT_NAMES;

	  private static readonly HashSet<string> EXPECTED_TYPES;

	  static MucNameContentHandler()
	  {
		HashSet<string> types = new HashSet<string>();

		types.Add("PERSON");
		types.Add("ORGANIZATION");
		types.Add("LOCATION");
		types.Add("DATE");
		types.Add("TIME");
		types.Add("MONEY");
		types.Add("PERCENT");

		EXPECTED_TYPES = types;

		HashSet<string> nameElements = new HashSet<string>();
		nameElements.Add(ENTITY_ELEMENT_NAME);
		nameElements.Add(TIME_ELEMENT_NAME);
		nameElements.Add(NUM_ELEMENT_NAME);
		NAME_ELEMENT_NAMES = nameElements;
	  }

	  private readonly Tokenizer tokenizer;
	  private readonly IList<NameSample> storedSamples;

	  internal bool isInsideContentElement = false;
	  private readonly IList<string> text = new List<string>();
	  private bool isClearAdaptiveData = false;
	  private readonly Stack<Span> incompleteNames = new Stack<Span>();

	  private IList<Span> names = new List<Span>();

	  public MucNameContentHandler(Tokenizer tokenizer, IList<NameSample> storedSamples)
	  {
		this.tokenizer = tokenizer;
		this.storedSamples = storedSamples;
	  }

	  public override void startElement(string name, IDictionary<string, string> attributes)
	  {

		if (MucElementNames.DOC_ELEMENT.Equals(name))
		{
		  isClearAdaptiveData = true;
		}

		if (MucElementNames.CONTENT_ELEMENTS.Contains(name))
		{
		  isInsideContentElement = true;
		}

		if (NAME_ELEMENT_NAMES.Contains(name))
		{

		  string nameType = attributes["TYPE"];

		  if (!EXPECTED_TYPES.Contains(nameType))
		  {
			throw new InvalidFormatException("Unknown timex, numex or namex type: " + nameType + ", expected one of " + EXPECTED_TYPES);
		  }

		  incompleteNames.Push(new Span(text.Count, text.Count, nameType.ToLower(Locale.ENGLISH)));
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

		if (NAME_ELEMENT_NAMES.Contains(name))
		{
		  Span nameSpan = incompleteNames.Pop();
		  nameSpan = new Span(nameSpan.Start, text.Count, nameSpan.Type);
		  names.Add(nameSpan);
		}

		if (MucElementNames.CONTENT_ELEMENTS.Contains(name))
		{
		  storedSamples.Add(new NameSample(text.ToArray(), names.ToArray(), isClearAdaptiveData));

		  if (isClearAdaptiveData)
		  {
			isClearAdaptiveData = false;
		  }

		  text.Clear();
		  names.Clear();
		  isInsideContentElement = false;
		}
	  }
	}

}