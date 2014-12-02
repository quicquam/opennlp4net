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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using j4n.Exceptions;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.Serialization;
using opennlp.tools.namefind;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    /// <summary>
	/// Parser for the training files of the BioNLP/NLPBA 2004 shared task.
	/// <para>
	/// The data contains five named entity types: DNA, RNA, protein, cell_type and cell_line.<br>
	/// </para>
	/// <para>
	/// Data can be found on this web site:<br>
	/// http://www-tsujii.is.s.u-tokyo.ac.jp/GENIA/ERtask/report.html
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class BioNLP2004NameSampleStream : ObjectStream<NameSample>
	{

	  public const int GENERATE_DNA_ENTITIES = 0x01;
	  public static readonly int GENERATE_PROTEIN_ENTITIES = 0x01 << 1;
	  public static readonly int GENERATE_CELLTYPE_ENTITIES = 0x01 << 2;
	  public static readonly int GENERATE_CELLLINE_ENTITIES = 0x01 << 3;
	  public static readonly int GENERATE_RNA_ENTITIES = 0x01 << 4;

	  private readonly int types;

	  private readonly ObjectStream<string> lineStream;

	  public BioNLP2004NameSampleStream(InputStream @in, int types)
	  {
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
		while ((line = lineStream.read()) != null && !StringUtil.isEmpty(line.Trim()))
		{

		  if (line.StartsWith("###MEDLINE:", StringComparison.Ordinal))
		  {
			isClearAdaptiveData = true;
			lineStream.read();
			continue;
		  }

		  if (line.Contains("ABSTRACT TRUNCATED"))
		  {
			continue;
		  }

          var regex = new Regex("\t");
          string[] fields = regex.Split(line);

		  if (fields.Length == 2)
		  {
			sentence.Add(fields[0]);
			tags.Add(fields[1]);
		  }
		  else
		  {
			throw new IOException("Expected two fields per line in training data, got " + fields.Length + " for line '" + line + "'!");
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

			if (tag.EndsWith("DNA", StringComparison.Ordinal) && (types & GENERATE_DNA_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("protein", StringComparison.Ordinal) && (types & GENERATE_PROTEIN_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("cell_type", StringComparison.Ordinal) && (types & GENERATE_CELLTYPE_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.EndsWith("cell_line", StringComparison.Ordinal) && (types & GENERATE_CELLTYPE_ENTITIES) == 0)
			{
			  tag = "O";
			}
			if (tag.EndsWith("RNA", StringComparison.Ordinal) && (types & GENERATE_RNA_ENTITIES) == 0)
			{
			  tag = "O";
			}

			if (tag.StartsWith("B-", StringComparison.Ordinal))
			{

			  if (beginIndex != -1)
			  {
				names.Add(new Span(beginIndex, endIndex, tags[beginIndex].Substring(2)));
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
				names.Add(new Span(beginIndex, endIndex, tags[beginIndex].Substring(2)));
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
			names.Add(new Span(beginIndex, endIndex, tags[beginIndex].Substring(2)));
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

	  public virtual void reset()
	  {
		lineStream.reset();
	  }

	  public virtual void close()
	  {
		lineStream.close();
	  }
	}

}