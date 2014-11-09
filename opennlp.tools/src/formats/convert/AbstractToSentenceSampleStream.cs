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

namespace opennlp.tools.formats.convert
{


	using SentenceSample = opennlp.tools.sentdetect.SentenceSample;
	using Detokenizer = opennlp.tools.tokenize.Detokenizer;
	using opennlp.tools.util;
	using opennlp.tools.util;

	public abstract class AbstractToSentenceSampleStream<T> : FilterObjectStream<T, SentenceSample>
	{

	  private readonly Detokenizer detokenizer;

	  private readonly int chunkSize;

	  internal AbstractToSentenceSampleStream(Detokenizer detokenizer, ObjectStream<T> samples, int chunkSize) : base(samples)
	  {

		if (detokenizer == null)
		{
		  throw new System.ArgumentException("detokenizer must not be null!");
		}

		this.detokenizer = detokenizer;

		if (chunkSize < 0)
		{
		  throw new System.ArgumentException("chunkSize must be zero or larger but was " + chunkSize + "!");
		}

		if (chunkSize > 0)
		{
		  this.chunkSize = chunkSize;
		}
		else
		{
		  this.chunkSize = int.MaxValue;
		}
	  }

	  protected internal abstract string[] toSentence(T sample);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.sentdetect.SentenceSample read() throws java.io.IOException
	  public override SentenceSample read()
	  {
		IList<string[]> sentences = new List<string[]>();

		T posSample;
		int chunks = 0;
		while ((posSample = samples.read()) != null && chunks < chunkSize)
		{
		  sentences.Add(toSentence(posSample));
		  chunks++;
		}

		if (sentences.Count > 0)
		{
		  return new SentenceSample(detokenizer, sentences.ToArray());
		}
		else if (posSample != null)
		{
		  return read(); // filter out empty line
		}
		else
		{
		  return null; // last sample was read
		}
	  }
	}

}