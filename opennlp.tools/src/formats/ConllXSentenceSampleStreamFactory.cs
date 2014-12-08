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
using opennlp.tools.cmdline;
using opennlp.tools.cmdline.@params;
using opennlp.tools.formats.convert;
using opennlp.tools.postag;
using opennlp.tools.sentdetect;
using opennlp.tools.util;

namespace opennlp.tools.formats
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ConllXSentenceSampleStreamFactory : DetokenizerSampleStreamFactory<SentenceSample>
	{

	  internal interface Parameters : ConllXPOSSampleStreamFactory.Parameters, DetokenizerParameter
	  {
		// TODO: make chunk size configurable
	  }

	  public static void registerFactory()
	  {
          StreamFactoryRegistry<SentenceSample>.registerFactory(typeof(SentenceSample), ConllXPOSSampleStreamFactory.CONLLX_FORMAT, new ConllXSentenceSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ConllXSentenceSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public ObjectStream<SentenceSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse<Parameters>(args);

        ObjectStream<POSSample> posSampleStream = StreamFactoryRegistry<POSSample>.getFactory(typeof(POSSample), ConllXPOSSampleStreamFactory.CONLLX_FORMAT).create(ArgumentParser.filter(args, typeof(ConllXPOSSampleStreamFactory.Parameters)));
		return new POSToSentenceSampleStream(createDetokenizer(@params), posSampleStream, 30);
	  }
	}

}