using System;
using System.Collections.Generic;
/*
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  under the License.
 */
using System.IO;
using System.Linq;
using j4n.Exceptions;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.Serialization;

namespace opennlp.tools.formats
{

	//import static opennlp.tools.formats.Conll02NameSampleStream.extract;


	using NameSample = opennlp.tools.namefind.NameSample;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
	using Span = opennlp.tools.util.Span;
	using StringUtil = opennlp.tools.util.StringUtil;

	/// <summary>
	/// An import stream which can parse the CONLL03 data.
	/// </summary>
	public class Conll03NameSampleStream : ObjectStream<NameSample>
	{

	  public enum LANGUAGE
	  {
		EN,
		DE
	  }

	  private readonly LANGUAGE lang;
	  private readonly ObjectStream<string> lineStream;

	  private readonly int types;

	  /// 
	  /// <param name="lang"> </param>
	  /// <param name="lineStream"> </param>
	  /// <param name="types"> </param>
	  public Conll03NameSampleStream(LANGUAGE lang, ObjectStream<string> lineStream, int types)
	  {
		this.lang = lang;
		this.lineStream = lineStream;
		this.types = types;
	  }

	  /// 
	  /// <param name="lang"> </param>
	  /// <param name="in"> </param>
	  /// <param name="types"> </param>
	  public Conll03NameSampleStream(LANGUAGE lang, InputStream @in, int types)
	  {

		this.lang = lang;
		try
		{
		  this.lineStream = new PlainTextByLineStream(@in, "UTF-8");
          var ps = new PrintStream(Console.OpenStandardOutput(), true, "UTF-8");
        }
		catch (UnsupportedEncodingException e)
		{
		  // UTF-8 is available on all JVMs, will never happen
		  throw new IllegalStateException(e);
		}
		this.types = types;
	  }

	  public virtual NameSample read()
	  {

		IList<string> sentence = new List<string>();
		IList<string> tags = new List<string>();

		bool isClearAdaptiveData = false;

		// Empty line indicates end of sentence

		string line;
		while ((line = lineStream.read()) != null && !StringUtil.isEmpty(line))
		{

		  if (line.StartsWith(Conll02NameSampleStream.DOCSTART, StringComparison.Ordinal))
		  {
			isClearAdaptiveData = true;
			string emptyLine = lineStream.read();

			if (!StringUtil.isEmpty(emptyLine))
			{
			  throw new IOException("Empty line after -DOCSTART- not empty: '" + emptyLine + "'!");
			}

			continue;
		  }

		  string[] fields = line.Split(' ');

		  // For English: WORD  POS-TAG SC-TAG NE-TAG
		  if (LANGUAGE.EN.Equals(lang) && (fields.Length == 4))
		  {
			sentence.Add(fields[0]);
			tags.Add(fields[3]); // 3 is NE-TAG
		  }
		  // For German: WORD  LEMA-TAG POS-TAG SC-TAG NE-TAG
		  else if (LANGUAGE.DE.Equals(lang) && (fields.Length == 5))
		  {
			sentence.Add(fields[0]);
			tags.Add(fields[4]); // 4 is NE-TAG
		  }
		  else
		  {
			throw new IOException("Incorrect number of fields per line for language: '" + line + "'!");
		  }
		}

		if (sentence.Count > 0)
		{

		  // convert name tags into spans
		  IList<Span> names = new List<Span>();

		  int beginIndex = -1;
		  int endIndex = -1;
		  for (int i = 0; i < tags.Count; i++)
		  {

			string tag = tags[i];

			if (tag.EndsWith("PER", StringComparison.Ordinal) && (types & Conll02NameSampleStream.GENERATE_PERSON_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("ORG", StringComparison.Ordinal) && (types & Conll02NameSampleStream.GENERATE_ORGANIZATION_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("LOC", StringComparison.Ordinal) && (types & Conll02NameSampleStream.GENERATE_LOCATION_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("MISC", StringComparison.Ordinal) && (types & Conll02NameSampleStream.GENERATE_MISC_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.Equals("O"))
			{
			  // O means we don't have anything this round.
			  if (beginIndex != -1)
			  {
				names.Add(extract(beginIndex, endIndex, tags[beginIndex]));
				beginIndex = -1;
				endIndex = -1;
			  }
			}
			else if (tag.StartsWith("B-", StringComparison.Ordinal))
			{
			  // B- prefix means we have two same entities next to each other
			  if (beginIndex != -1)
			  {
				names.Add(extract(beginIndex, endIndex, tags[beginIndex]));
			  }
			  beginIndex = i;
			  endIndex = i + 1;
			}
			else if (tag.StartsWith("I-", StringComparison.Ordinal))
			{
			  // I- starts or continues a current name entity
			  if (beginIndex == -1)
			  {
				beginIndex = i;
				endIndex = i + 1;
			  }
			  else if (!tag.EndsWith(tags[beginIndex].Substring(1), StringComparison.Ordinal))
			  {
				// we have a new tag type following a tagged word series
				// also may not have the same I- starting the previous!
				names.Add(extract(beginIndex, endIndex, tags[beginIndex]));
				beginIndex = i;
				endIndex = i + 1;
			  }
			  else
			  {
				endIndex++;
			  }
			}
			else
			{
			  throw new IOException("Invalid tag: " + tag);
			}
		  }

		  // if one span remains, create it here
		  if (beginIndex != -1)
		  {
			names.Add(extract(beginIndex, endIndex, tags[beginIndex]));
		  }

		  return new NameSample(sentence.ToArray(), names.ToArray(), isClearAdaptiveData);
		}
		else if (line != null)
		{
		  // Just filter out empty events, if two lines in a row are empty
		  return read();
		}
		else
		{
		  // source stream is not returning anymore lines
		  return null;
		}
	  }

	  public override void reset()
	  {
		lineStream.reset();
	  }

	  public override void close()
	  {
		lineStream.close();
	  }
      public Span extract(int start, int end, string s)
      {
          throw new NotImplementedException();
      }
	}

}