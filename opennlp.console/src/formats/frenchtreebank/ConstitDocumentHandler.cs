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
using saxlib.Interfaces;
namespace opennlp.tools.formats.frenchtreebank
{


	using AbstractBottomUpParser = opennlp.tools.parser.AbstractBottomUpParser;
	using Constituent = opennlp.tools.parser.Constituent;
	using Parse = opennlp.tools.parser.Parse;
	using Span = opennlp.tools.util.Span;


	internal class ConstitDocumentHandler : DefaultHandler
	{

	  private const string SENT_ELEMENT_NAME = "SENT";
	  private const string WORD_ELEMENT_NAME = "w";

	  private const string SENT_TYPE_NAME = "S";

	  private readonly IList<Parse> parses;

	  private bool insideSentenceElement;

	  /// <summary>
	  /// A token buffer, a token might be build up by multiple
	  /// <seealso cref="#characters(char[], int, int)"/> calls.
	  /// </summary>
	  private readonly StringBuilder tokenBuffer = new StringBuilder();

	  private readonly StringBuilder text = new StringBuilder();

	  private int offset;
	  private readonly Stack<Constituent> stack = new Stack<Constituent>();
	  private readonly IList<Constituent> cons = new LinkedList<Constituent>();

	  internal ConstitDocumentHandler(IList<Parse> parses)
	  {
		this.parses = parses;
	  }

	  private string cat;
	  private string subcat;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void startElement(String uri, String localName, String qName, org.xml.sax.Attributes attributes) throws org.xml.sax.SAXException
	  public override void startElement(string uri, string localName, string qName, saxlib.Entities.Attributes attributes)
	  {

		string type = qName;

		if (SENT_ELEMENT_NAME.Equals(qName))
		{
		  // Clear everything to be ready for the next sentence
		  text.Length = 0;
		  offset = 0;
		  stack.Clear();
		  cons.Clear();

		  type = SENT_TYPE_NAME;

		  insideSentenceElement = true;
		}
		else if (WORD_ELEMENT_NAME.Equals(qName))
		{

		  // Note:
		  // If there are compound words they are represented in a couple
		  // of ways in the training data.
		  // Many of them are marked with the compound attribute, but not 
		  // all of them. Thats why it is not used in the code to detect
		  // a compound word.
		  // Compounds are detected by the fact that a w tag is appearing
		  // inside a w tag.
		  //
		  // The type of a compound word can be encoded either cat of the compound
		  // plus the catint of each word of the compound.
		  // Or all compound words have the cat plus subcat of the compound, in this
		  // case they have an empty cat attribute.
		  //
		  // This implementation hopefully decodes these cases correctly!

		  string newCat = attributes.getValue("cat");
		  if (newCat != null && newCat.Length > 0)
		  {
			cat = newCat;
		  }

		  string newSubcat = attributes.getValue("subcat");
		  if (newSubcat != null && newSubcat.Length > 0)
		  {
			subcat = newSubcat;
		  }

		  if (cat != null)
		  {
			type = cat + (subcat != null ? subcat : "");
		  }
		  else
		  {
			string catint = attributes.getValue("catint");
			if (catint != null)
			{
			  type = cat + (catint != null ? catint : "");
			}
			else
			{
			  type = cat + subcat;
			}
		  }
		}

		stack.Push(new Constituent(type, new Span(offset, offset)));

		tokenBuffer.Length = 0;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void characters(char[] ch, int start, int length) throws org.xml.sax.SAXException
	  public override void characters(char[] ch, int start, int length)
	  {
		tokenBuffer.Append(ch, start, length);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void endElement(String uri, String localName, String qName) throws org.xml.sax.SAXException
	  public override void endElement(string uri, string localName, string qName)
	  {

		bool isCreateConstituent = true;

		if (insideSentenceElement)
		{
		  if (WORD_ELEMENT_NAME.Equals(qName))
		  {
			string token = tokenBuffer.ToString().Trim();

			if (token.Length > 0)
			{
			  cons.Add(new Constituent(AbstractBottomUpParser.TOK_NODE, new Span(offset, offset + token.Length)));

			  text.Append(token).Append(" ");
			  offset += token.Length + 1;
			}
			else
			{
			  isCreateConstituent = false;
			}
		  }

		  Constituent unfinishedCon = stack.Pop();

		  if (isCreateConstituent)
		  {
			int start = unfinishedCon.Span.Start;
			if (start < offset)
			{
			  cons.Add(new Constituent(unfinishedCon.Label, new Span(start, offset - 1)));
			}
		  }

		  if (SENT_ELEMENT_NAME.Equals(qName))
		  {
			// Finished parsing sentence, now put everything together and create
			// a Parse object

			string txt = text.ToString();
			int tokenIndex = -1;
			Parse p = new Parse(txt, new Span(0, txt.Length), AbstractBottomUpParser.TOP_NODE, 1,0);
			for (int ci = 0;ci < cons.Count;ci++)
			{
			  Constituent con = cons[ci];
			  string type = con.Label;
			  if (!type.Equals(AbstractBottomUpParser.TOP_NODE))
			  {
				if (type == AbstractBottomUpParser.TOK_NODE)
				{
				  tokenIndex++;
				}
				Parse c = new Parse(txt, con.Span, type, 1,tokenIndex);
				p.insert(c);
			  }
			}
			parses.Add(p);

			insideSentenceElement = false;
		  }

		  tokenBuffer.Length = 0;
		}
	  }
	}

}