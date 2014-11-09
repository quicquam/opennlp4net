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

namespace opennlp.tools.formats.muc
{


	using NameSample = opennlp.tools.namefind.NameSample;
	using Tokenizer = opennlp.tools.tokenize.Tokenizer;
	using opennlp.tools.util;
	using opennlp.tools.util;

	public class MucNameSampleStream : FilterObjectStream<string, NameSample>
	{

	  private readonly Tokenizer tokenizer;

	  private IList<NameSample> storedSamples = new List<NameSample>();

	  protected internal MucNameSampleStream(Tokenizer tokenizer, ObjectStream<string> samples) : base(samples)
	  {
		this.tokenizer = tokenizer;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.namefind.NameSample read() throws java.io.IOException
	  public override NameSample read()
	  {
		if (storedSamples.Count == 0)
		{

		  string document = samples.read();

		  if (document != null)
		  {

			// Note: This is a hack to fix invalid formating in
			// some MUC files ...
			document = document.Replace(">>", ">");

			(new SgmlParser()).parse(new StringReader(document), new MucNameContentHandler(tokenizer, storedSamples));
		  }
		}

		if (storedSamples.Count > 0)
		{
		  return storedSamples.Remove(0);
		}
		else
		{
		  return null;
		}
	  }
	}

}