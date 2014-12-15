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
using System.IO;
using System.Text;
using j4n.IO.Reader;
using j4n.Lang;
using opennlp.tools.util;

namespace opennlp.tools.formats.muc
{
    /// <summary>
	/// SAX style SGML parser. 
	/// <para>
	/// Note:<br>
	/// The implementation is very limited, but good enough to
	/// parse the MUC corpora. Its must very likely be extended/improved/fixed to parse
	/// a different SGML corpora.
	/// </para>
	/// </summary>
	public class SgmlParser
	{

	  public abstract class ContentHandler
	  {

		public virtual void startElement(string name, IDictionary<string, string> attributes)
		{
		}

		public virtual void characters(CharSequence chars)
		{
		}

		public virtual void endElement(string name)
		{
		}
	  }

	  private static string extractTagName(CharSequence tagChars)
	  {

		int fromOffset = 1;

		if (tagChars.length() > 1 && tagChars.charAt(1) == '/')
		{
		  fromOffset = 2;
		}

		for (int ci = 1; ci < tagChars.length(); ci++)
		{

		  if (tagChars.charAt(ci) == '>' || StringUtil.isWhitespace(tagChars.charAt(ci)))
		  {
			return tagChars.subSequence(fromOffset, ci).ToString();
		  }
		}

		throw new InvalidFormatException("Failed to extract tag name!");
	  }

	  private static IDictionary<string, string> getAttributes(CharSequence tagChars)
	  {

		// format:
		// space
		// key
		// = 
		// " <- begin
		// value chars
		// " <- end

		IDictionary<string, string> attributes = new Dictionary<string, string>();

		StringBuilder key = new StringBuilder();
		StringBuilder value = new StringBuilder();

		bool extractKey = false;
		bool extractValue = false;

		for (int i = 0; i < tagChars.length(); i++)
		{

		  // White space indicates begin of new key name
		  if (StringUtil.isWhitespace(tagChars.charAt(i)) && !extractValue)
		  {
			extractKey = true;
		  }
		  // Equals sign indicated end of key name
		  else if (extractKey && ('=' == tagChars.charAt(i) || StringUtil.isWhitespace(tagChars.charAt(i))))
		  {
			extractKey = false;
		  }
		  // Inside key name, extract all chars
		  else if (extractKey)
		  {
			key.Append(tagChars.charAt(i));
		  }
		  // " Indicates begin or end of value chars
		  else if ('"' == tagChars.charAt(i))
		  {

			if (extractValue)
			{
			  attributes[key.ToString()] = value.ToString();

			  // clear key and value buffers
			  key.Length = 0;
			  value.Length = 0;
			}

			extractValue = !extractValue;
		  }
		  // Inside value, extract all chars
		  else if (extractValue)
		  {
			value.Append(tagChars.charAt(i));
		  }
		}

		return attributes;
	  }

	  public virtual void parse(StringReader @in, ContentHandler handler)
	  {

        CharSequence buffer = new CharSequence("");

		bool isInsideTag = false;
		bool isStartTag = true;

		int lastChar = -1;
		int c;
		while ((c = @in.Read()) != -1)
		{

		  if ('<' == c)
		  {
			if (isInsideTag)
			{
			  throw new InvalidFormatException("Did not expect < char!");
			}

			if (buffer.ToString().Trim().Length > 0)
			{
			  handler.characters(new CharSequence(buffer.ToString().Trim()));
			}

			buffer.Length = 0;

			isInsideTag = true;
			isStartTag = true;
		  }

		  buffer.appendCodePoint(c);

		  if ('/' == c && lastChar == '<')
		  {
			isStartTag = false;
		  }

		  if ('>' == c)
		  {

			if (!isInsideTag)
			{
			  throw new InvalidFormatException("Did not expect > char!");
			}

			if (isStartTag)
			{
			  handler.startElement(extractTagName(buffer), getAttributes(buffer));
			}
			else
			{
			  handler.endElement(extractTagName(buffer));
			}

			buffer.Length = 0;

			isInsideTag = false;
		  }

		  lastChar = c;
		}

		if (isInsideTag)
		{
		  throw new InvalidFormatException("Did not find matching > char!");
		}
	  }
	}

}