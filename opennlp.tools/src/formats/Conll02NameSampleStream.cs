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
using System.IO;
using j4n.Exceptions;
using j4n.IO.InputStream;

namespace opennlp.tools.formats
{


	using NameSample = opennlp.tools.namefind.NameSample;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
	using Span = opennlp.tools.util.Span;
	using StringUtil = opennlp.tools.util.StringUtil;

	/// <summary>
	/// Parser for the dutch and spanish ner training files of the CONLL 2002 shared task.
	/// <para>
	/// The dutch data has a -DOCSTART- tag to mark article boundaries,
	/// adaptive data in the feature generators will be cleared before every article.<br>
	/// The spanish data does not contain article boundaries,
	/// adaptive data will be cleared for every sentence.
	/// </para>
	/// <para>
	/// The data contains four named entity types: Person, Organization, Location and Misc.<br>
	/// </para>
	/// <para>
	/// Data can be found on this web site:<br>
	/// http://www.cnts.ua.ac.be/conll2002/ner/
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class Conll02NameSampleStream : ObjectStream<NameSample>
	{

	  public enum LANGUAGE
	  {
		NL,
		ES
	  }

	  public const int GENERATE_PERSON_ENTITIES = 0x01;
	  public static readonly int GENERATE_ORGANIZATION_ENTITIES = 0x01 << 1;
	  public static readonly int GENERATE_LOCATION_ENTITIES = 0x01 << 2;
	  public static readonly int GENERATE_MISC_ENTITIES = 0x01 << 3;

	  public const string DOCSTART = "-DOCSTART-";

	  private readonly LANGUAGE lang;
	  private readonly ObjectStream<string> lineStream;

	  private readonly int types;

	  public Conll02NameSampleStream(LANGUAGE lang, ObjectStream<string> lineStream, int types)
	  {
		this.lang = lang;
		this.lineStream = lineStream;
		this.types = types;
	  }

	  /// <param name="lang"> </param>
	  /// <param name="in"> an Input Stream to read data. </param>
	  /// <exception cref="IOException">  </exception>
	  public Conll02NameSampleStream(LANGUAGE lang, InputStream @in, int types)
	  {

		this.lang = lang;
		try
		{
		  this.lineStream = new PlainTextByLineStream(@in, "UTF-8");
		  System.Out = new PrintStream(System.out, true, "UTF-8");
		}
		catch (UnsupportedEncodingException e)
		{
		  // UTF-8 is available on all JVMs, will never happen
		  throw new IllegalStateException(e);
		}
		this.types = types;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static final opennlp.tools.util.Span extract(int begin, int end, String beginTag) throws opennlp.tools.util.InvalidFormatException
	  internal static Span extract(int begin, int end, string beginTag)
	  {

		string type = beginTag.Substring(2);

		if ("PER".Equals(type))
		{
		  type = "person";
		}
		else if ("LOC".Equals(type))
		{
		  type = "location";
		}
		else if ("MISC".Equals(type))
		{
		  type = "misc";
		}
		else if ("ORG".Equals(type))
		{
		  type = "organization";
		}
		else
		{
		  throw new InvalidFormatException("Unknown type: " + type);
		}

		return new Span(begin, end, type);
	  }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.namefind.NameSample read() throws java.io.IOException
	  public virtual NameSample read()
	  {

		IList<string> sentence = new List<string>();
		IList<string> tags = new List<string>();

		bool isClearAdaptiveData = false;

		// Empty line indicates end of sentence

		string line;
		while ((line = lineStream.read()) != null && !StringUtil.isEmpty(line))
		{

		  if (LANGUAGE.NL.Equals(lang) && line.StartsWith(DOCSTART, StringComparison.Ordinal))
		  {
			isClearAdaptiveData = true;
			continue;
		  }

		  string[] fields = line.Split(" ", true);

		  if (fields.Length == 3)
		  {
			sentence.Add(fields[0]);
			tags.Add(fields[2]);
		  }
		  else
		  {
			throw new IOException("Expected three fields per line in training data, got " + fields.Length + " for line '" + line + "'!");
		  }
		}

		// Always clear adaptive data for spanish
		if (LANGUAGE.ES.Equals(lang))
		{
		  isClearAdaptiveData = true;
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

			if (tag.EndsWith("PER", StringComparison.Ordinal) && (types & GENERATE_PERSON_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("ORG", StringComparison.Ordinal) && (types & GENERATE_ORGANIZATION_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("LOC", StringComparison.Ordinal) && (types & GENERATE_LOCATION_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("MISC", StringComparison.Ordinal) && (types & GENERATE_MISC_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.StartsWith("B-", StringComparison.Ordinal))
			{

			  if (beginIndex != -1)
			  {
				names.Add(extract(beginIndex, endIndex, tags[beginIndex]));
				beginIndex = -1;
				endIndex = -1;
			  }

			  beginIndex = i;
			  endIndex = i + 1;
			}
			else if (tag.StartsWith("I-", StringComparison.Ordinal))
			{
			  endIndex++;
			}
			else if (tag.Equals("O"))
			{
			  if (beginIndex != -1)
			  {
				names.Add(extract(beginIndex, endIndex, tags[beginIndex]));
				beginIndex = -1;
				endIndex = -1;
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws java.io.IOException, UnsupportedOperationException
	  public virtual void reset()
	  {
		lineStream.reset();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	  public virtual void close()
	  {
		lineStream.close();
	  }
	}

}