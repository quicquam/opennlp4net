using System.Collections.Generic;
using System.Linq;
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
using opennlp.nonjava.helperclasses;

namespace opennlp.tools.sentdetect
{


	using Detokenizer = opennlp.tools.tokenize.Detokenizer;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// A <seealso cref="SentenceSample"/> contains a document with
	/// begin indexes of the individual sentences.
	/// </summary>
	public class SentenceSample
	{

	  private readonly string document;

	  private readonly IList<Span> sentences;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="document"> </param>
	  /// <param name="sentences"> </param>
	  public SentenceSample(string document, params Span[] sentences)
	  {
		this.document = document;
		this.sentences = new List<Span>(sentences.AsEnumerable());
	  }

	  public SentenceSample(Detokenizer detokenizer, string[][] sentences)
	  {

		IList<Span> spans = new List<Span>(sentences.Length);

		StringBuilder documentBuilder = new StringBuilder();

		foreach (var sentenceTokens in sentences)
		{

		  string sampleSentence = detokenizer.detokenize(sentenceTokens, null);

		  int beginIndex = documentBuilder.Length;
		  documentBuilder.Append(sampleSentence);

		  spans.Add(new Span(beginIndex, documentBuilder.Length));
		}

		document = documentBuilder.ToString();
		this.sentences = spans;
	  }

	  /// <summary>
	  /// Retrieves the document.
	  /// </summary>
	  /// <returns> the document </returns>
	  public virtual string Document
	  {
		  get
		  {
			return document;
		  }
	  }

	  /// <summary>
	  /// Retrieves the sentences.
	  /// </summary>
	  /// <returns> the begin indexes of the sentences
	  /// in the document. </returns>
	  public virtual Span[] Sentences
	  {
		  get
		  {
			return sentences.ToArray();
		  }
	  }

	  public override string ToString()
	  {

		StringBuilder documentBuilder = new StringBuilder();

		foreach (Span sentSpan in sentences)
		{
		  documentBuilder.Append(sentSpan.getCoveredText(document));
		  documentBuilder.Append("\n");
		}

		return documentBuilder.ToString();
	  }

	  public override bool Equals(object obj)
	  {
		if (this == obj)
		{
		  return true;
		}
		else if (obj is SentenceSample)
		{
		  SentenceSample a = (SentenceSample) obj;

		  return Document.Equals(a.Document) && Arrays.Equals(Sentences, a.Sentences);
		}
		else
		{
		  return false;
		}
	  }
	}

}