﻿/*
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
using opennlp.tools.namefind;
using opennlp.tools.tokenize;

namespace opennlp.console.formats.convert
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class NameToSentenceSampleStream : AbstractToSentenceSampleStream<NameSample>
	{

	  public NameToSentenceSampleStream(Detokenizer detokenizer, ObjectStream<NameSample> samples, int chunkSize) : base(detokenizer, samples, chunkSize)
	  {
	  }

	  protected internal override string[] toSentence(NameSample sample)
	  {
		return sample.Sentence;
	  }
	}

}