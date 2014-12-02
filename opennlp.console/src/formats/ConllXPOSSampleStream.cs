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
using j4n.IO.Reader;
using j4n.Serialization;
using opennlp.tools.postag;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    /// <summary>
	/// Parses the data from the CONLL 06 shared task into POS Samples.
	/// <para>
	/// More information about the data format can be found here:<br>
	/// http://www.cnts.ua.ac.be/conll2006/
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class ConllXPOSSampleStream : FilterObjectStream<string, POSSample>
	{

	  public ConllXPOSSampleStream(ObjectStream<string> lineStream) : base(new ParagraphStream(lineStream))
	  {
	  }

	  internal ConllXPOSSampleStream(Reader @in) : base(new ParagraphStream(new PlainTextByLineStream(@in)))
	  {
		// encoding is handled by the factory...
	  }

	  public override POSSample read()
	  {

		// The CONLL-X data has a word per line and each line is tab separated
		// in the following format:
		// ID, FORM, LEMMA, CPOSTAG, POSTAG, ... (max 10 fields)

		// One paragraph contains a whole sentence and, the token
		// and tag will be read from the FORM and POSTAG field.

	   string paragraph = samples.read();

	   POSSample sample = null;

	   if (paragraph != null)
	   {

		 // paragraph get lines
		 BufferedReader reader = new BufferedReader(new StringReader(paragraph));

		 IList<string> tokens = new List<string>(100);
		 IList<string> tags = new List<string>(100);

		 string line;
		 while ((line = reader.readLine()) != null)
		 {

		   const int minNumberOfFields = 5;

		   string[] parts = line.Split('\t');

		   if (parts.Length >= minNumberOfFields)
		   {
			 tokens.Add(parts[1]);
			 tags.Add(parts[4]);
		   }
		   else
		   {
			 throw new InvalidFormatException("Every non-empty line must have at least " + minNumberOfFields + " fields: '" + line + "'!");
		   }
		 }

		 // just skip empty samples and read next sample
		 if (tokens.Count == 0)
		 {
		   sample = read();
		 }

		 sample = new POSSample(tokens.ToArray(), tags.ToArray());
	   }

	   return sample;
	  }
	}

}