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

namespace opennlp.tools.formats.ad
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using DetokenizerParameter = opennlp.tools.cmdline.@params.DetokenizerParameter;
	using opennlp.tools.formats;
	using NameToTokenSampleStream = opennlp.tools.formats.convert.NameToTokenSampleStream;
	using NameSample = opennlp.tools.namefind.NameSample;
	using TokenSample = opennlp.tools.tokenize.TokenSample;
	using opennlp.tools.util;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ADTokenSampleStreamFactory : DetokenizerSampleStreamFactory<TokenSample>
	{

	  internal interface Parameters : ADNameSampleStreamFactory.Parameters, DetokenizerParameter
	  {
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(TokenSample), "ad", new ADTokenSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ADTokenSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<TokenSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		ObjectStream<NameSample> samples = StreamFactoryRegistry.getFactory(typeof(NameSample), "ad").create(ArgumentParser.filter(args, typeof(ADNameSampleStreamFactory.Parameters)));
		return new NameToTokenSampleStream(createDetokenizer(@params), samples);
	  }
	}

}