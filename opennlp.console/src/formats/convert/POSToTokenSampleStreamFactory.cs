using System;
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

namespace opennlp.tools.formats.convert
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using DetokenizerParameter = opennlp.tools.cmdline.@params.DetokenizerParameter;
	using opennlp.tools.formats;
	using POSSample = opennlp.tools.postag.POSSample;
	using TokenSample = opennlp.tools.tokenize.TokenSample;
	using opennlp.tools.util;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class POSToTokenSampleStreamFactory : DetokenizerSampleStreamFactory<TokenSample>
	{

	  internal interface Parameters : WordTagSampleStreamFactory.Parameters, DetokenizerParameter
	  {
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(TokenSample), "pos", new POSToTokenSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal POSToTokenSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<TokenSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		ObjectStream<POSSample> posSampleStream = StreamFactoryRegistry.getFactory(typeof(POSSample), StreamFactoryRegistry.DEFAULT_FORMAT).create(ArgumentParser.filter(args, typeof(WordTagSampleStreamFactory.Parameters)));
		return new POSToTokenSampleStream(createDetokenizer(@params), posSampleStream);
	  }
	}

}