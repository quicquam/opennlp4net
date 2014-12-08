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
using opennlp.tools.parser;
using opennlp.tools.postag;
using opennlp.tools.util;

namespace opennlp.tools.formats.convert
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ParseToPOSSampleStream : FilterObjectStream<Parse, POSSample>
	{

	  protected internal ParseToPOSSampleStream(ObjectStream<Parse> samples) : base(samples)
	  {
	  }

	  public override POSSample read()
	  {

		Parse parse = samples.read();

		if (parse != null)
		{

		  IList<string> sentence = new List<string>();
		  IList<string> tags = new List<string>();

		  foreach (Parse tagNode in parse.TagNodes)
		  {
			sentence.Add(tagNode.CoveredText);
			tags.Add(tagNode.Type);
		  }

		  return new POSSample(sentence, tags);
		}
		else
		{
		  return null;
		}
	  }
	}

}