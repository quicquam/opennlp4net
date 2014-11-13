using System;
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
using j4n.Exceptions;
using j4n.IO.InputStream;
using j4n.Lang;
using j4n.Serialization;
using opennlp.nonjava.helperclasses;

namespace opennlp.tools.formats.ad
{


	using Sentence = opennlp.tools.formats.ad.ADSentenceStream.Sentence;
	using SentenceSample = opennlp.tools.sentdetect.SentenceSample;
	using Factory = opennlp.tools.sentdetect.lang.Factory;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ADSentenceSampleStream : ObjectStream<SentenceSample>
	{

	  private readonly ObjectStream<ADSentenceStream.Sentence> adSentenceStream;

	  private int text = -1;
	  private int para = -1;
	  private bool isSameText;
	  private bool isSamePara;
	  private Sentence sent;
	  private bool isIncludeTitles = true;
	  private bool isTitle;

	  private readonly char[] ptEosCharacters;

	  /// <summary>
	  /// Creates a new <seealso cref="SentenceSample"/> stream from a line stream, i.e.
	  /// <seealso cref="ObjectStream"/>< <seealso cref="String"/>>, that could be a
	  /// <seealso cref="PlainTextByLineStream"/> object.
	  /// </summary>
	  /// <param name="lineStream">
	  ///          a stream of lines as <seealso cref="String"/> </param>
	  /// <param name="includeHeadlines">
	  ///          if true will output the sentences marked as news headlines </param>
	  public ADSentenceSampleStream(ObjectStream<string> lineStream, bool includeHeadlines)
	  {
		this.adSentenceStream = new ADSentenceStream(lineStream);
		ptEosCharacters = Factory.ptEosCharacters;
		Arrays.sort(ptEosCharacters);
		this.isIncludeTitles = includeHeadlines;
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="SentenceSample"/> stream from a <seealso cref="FileInputStream"/>
	  /// </summary>
	  /// <param name="in">
	  ///          input stream from the corpus </param>
	  /// <param name="charsetName">
	  ///          the charset to use while reading the corpus </param>
	  /// <param name="includeHeadlines">
	  ///          if true will output the sentences marked as news headlines </param>
	  public ADSentenceSampleStream(FileInputStream @in, string charsetName, bool includeHeadlines)
	  {
		try
		{
		  this.adSentenceStream = new ADSentenceStream(new PlainTextByLineStream(@in, charsetName));
		}
		catch (UnsupportedEncodingException e)
		{
		  // UTF-8 is available on all JVMs, will never happen
		  throw new IllegalStateException(e);
		}
		ptEosCharacters = Factory.ptEosCharacters;
		Arrays.sort(ptEosCharacters);
		this.isIncludeTitles = includeHeadlines;
	  }

	  // The Arvores Deitadas Corpus has information about texts and paragraphs.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.sentdetect.SentenceSample read() throws java.io.IOException
	  public virtual SentenceSample read()
	  {

		if (sent == null)
		{
		  sent = this.adSentenceStream.read();
		  updateMeta();
		  if (sent == null)
		  {
			return null;
		  }
		}

		StringBuilder document = new StringBuilder();
		IList<Span> sentences = new List<Span>();
		do
		{
		  do
		  {
			if (!isTitle || (isTitle && isIncludeTitles))
			{
			  if (hasPunctuation(sent.Text))
			  {
				int start = document.Length;
				document.Append(sent.Text);
				sentences.Add(new Span(start, document.Length));
				document.Append(" ");
			  }

			}
			sent = this.adSentenceStream.read();
			updateMeta();
		  } while (isSamePara);
		  // break; // got one paragraph!
		} while (isSameText);

		string doc;
		if (document.Length > 0)
		{
		  doc = document.Substring(0, document.Length - 1);
		}
		else
		{
		  doc = document.ToString();
		}

		return new SentenceSample(doc, sentences.ToArray());
	  }

	  private bool hasPunctuation(string text)
	  {
		text = text.Trim();
		if (text.Length > 0)
		{
		  char lastChar = text[text.Length - 1];
		  if (Arrays.binarySearch(ptEosCharacters, lastChar) >= 0)
		  {
			return true;
		  }
		}
		return false;
	  }

	  // there are some different types of metadata depending on the corpus.
	  // todo: merge this patterns
	  private Pattern meta1 = Pattern.compile("^(?:[a-zA-Z\\-]*(\\d+)).*?p=(\\d+).*");

	  private void updateMeta()
	  {
		if (this.sent != null)
		{
		  string meta = this.sent.Metadata;
		  Matcher m = meta1.matcher(meta);
		  int currentText;
		  int currentPara;
		  if (m.matches())
		  {
			currentText = Convert.ToInt32(m.group(1));
			currentPara = Convert.ToInt32(m.group(2));
		  }
		  else
		  {
			throw new Exception("Invalid metadata: " + meta);
		  }
		  isSamePara = isSameText = false;
		  if (currentText == text)
		  {
			isSameText = true;
		  }

		  if (isSameText && currentPara == para)
		  {
			isSamePara = true;
		  }

		  isTitle = meta.Contains("title");

		  text = currentText;
		  para = currentPara;

		}
		else
		{
		  this.isSamePara = this.isSameText = false;
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws java.io.IOException, UnsupportedOperationException
	  public virtual void reset()
	  {
		adSentenceStream.reset();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	  public virtual void close()
	  {
		adSentenceStream.close();
	  }
	}

}