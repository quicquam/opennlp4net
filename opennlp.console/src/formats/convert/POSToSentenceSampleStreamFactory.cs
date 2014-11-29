﻿using System;
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
using opennlp.tools.cmdline;

namespace opennlp.tools.formats.convert
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using DetokenizerParameter = opennlp.tools.cmdline.@params.DetokenizerParameter;
	using opennlp.tools.formats;
	using POSSample = opennlp.tools.postag.POSSample;
	using SentenceSample = opennlp.tools.sentdetect.SentenceSample;
	using opennlp.tools.util;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class POSToSentenceSampleStreamFactory : DetokenizerSampleStreamFactory<SentenceSample>
	{

	  internal interface Parameters : WordTagSampleStreamFactory.Parameters, DetokenizerParameter
	  {
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry<SentenceSample>.registerFactory(typeof(SentenceSample), "pos", new POSToSentenceSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal POSToSentenceSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public ObjectStream<SentenceSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse<Parameters>(args);

        ObjectStream<POSSample> posSampleStream = StreamFactoryRegistry<POSSample>.getFactory(typeof(POSSample), StreamFactoryRegistry<POSSample>.DEFAULT_FORMAT).create(ArgumentParser.filter(args, typeof(WordTagSampleStreamFactory.Parameters)));
		return new POSToSentenceSampleStream(createDetokenizer(@params), posSampleStream, 30);
	  }
	}

}