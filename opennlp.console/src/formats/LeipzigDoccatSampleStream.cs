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
using System.IO;
using System.Text;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using opennlp.tools.doccat;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    /// <summary>
	/// Stream filter to produce document samples out of a Leipzig sentences.txt file.
	/// In the Leipzig corpus the encoding of the various sentences.txt file is defined by
	/// the language. The language must be specified to produce the category tags and is used
	/// to determine the correct input encoding.
	/// <para>
	/// The input text is tokenized with the <seealso cref="SimpleTokenizer"/>. The input text classified
	/// by the language model must also be tokenized by the <seealso cref="SimpleTokenizer"/> to produce
	/// exactly the same tokenization during testing and training.
	/// </para>
	/// </summary>
	public class LeipzigDoccatSampleStream : FilterObjectStream<string, DocumentSample>
	{

	  private readonly string language;
	  private readonly int sentencesPerDocument;

	  /// <summary>
	  /// Creates a new LeipzigDoccatSampleStream with the specified parameters.
	  /// </summary>
	  /// <param name="language"> the Leipzig input sentences.txt file </param>
	  /// <param name="sentencesPerDocument"> the number of sentences which should be grouped into once <seealso cref="DocumentSample"/> </param>
	  /// <param name="in"> the InputStream pointing to the contents of the sentences.txt input file </param>
	  /// <exception cref="IOException"> IOException </exception>
	  internal LeipzigDoccatSampleStream(string language, int sentencesPerDocument, InputStream @in) : base(new PlainTextByLineStream(@in, "UTF-8"))
	  {
        var ps = new PrintStream(Console.OpenStandardOutput(), true, "UTF-8");
		this.language = language;
		this.sentencesPerDocument = sentencesPerDocument;
	  }

	  public override DocumentSample read()
	  {

		int count = 0;

		StringBuilder sampleText = new StringBuilder();

		string line;
		while (count < sentencesPerDocument && (line = samples.read()) != null)
		{

		  string[] tokens = SimpleTokenizer.INSTANCE.tokenize(line);

		  if (tokens.Length == 0)
		  {
			throw new IOException("Empty lines are not allowed!");
		  }

		  // Always skip first token, that is the sentence number!
		  for (int i = 1; i < tokens.Length; i++)
		  {
			sampleText.Append(tokens[i]);
			sampleText.Append(' ');
		  }

		  count++;
		}


		if (sampleText.Length > 0)
		{
		  return new DocumentSample(language, sampleText.ToString());
		}

		return null;
	  }
	}

}