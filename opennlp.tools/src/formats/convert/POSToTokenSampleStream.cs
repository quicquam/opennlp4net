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

using opennlp.tools.postag;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.tools.formats.convert
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class POSToTokenSampleStream : FilterObjectStream<POSSample, TokenSample>
	{

	  private readonly Detokenizer detokenizer;

	  public POSToTokenSampleStream(Detokenizer detokenizer, ObjectStream<POSSample> samples) : base(samples)
	  {


		if (detokenizer == null)
		{
		  throw new System.ArgumentException("detokenizer must not be null!");
		}

		this.detokenizer = detokenizer;
	  }

	  public override TokenSample read()
	  {

		POSSample posSample = samples.read();

		TokenSample tokenSample = null;

		if (posSample != null)
		{
		  tokenSample = new TokenSample(detokenizer, posSample.Sentence);
		}

		return tokenSample;
	  }
	}

}