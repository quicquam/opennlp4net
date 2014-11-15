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
using j4n.Serialization;

namespace opennlp.tools.formats.muc
{


	using Tokenizer = opennlp.tools.tokenize.Tokenizer;
	using opennlp.tools.util;
	using opennlp.tools.util;

	public class MucCorefSampleStream : FilterObjectStream<string, RawCorefSample>
	{

	  private readonly Tokenizer tokenizer;

	  private IList<RawCorefSample> documents = new List<RawCorefSample>();

	  public MucCorefSampleStream(Tokenizer tokenizer, ObjectStream<string> documents) : base(new DocumentSplitterStream(documents))
	  {
		this.tokenizer = tokenizer;
	  }

	  public override RawCorefSample read()
	  {

		if (documents.Count == 0)
		{

		  string document = samples.read();

		  if (document != null)
		  {
			(new SgmlParser()).parse(new StringReader(document), new MucCorefContentHandler(tokenizer, documents));
		  }
		}

		if (documents.Count > 0)
		{
		  return documents.Remove(0);
		}
		else
		{
		  return null;
		}
	  }
	}

}