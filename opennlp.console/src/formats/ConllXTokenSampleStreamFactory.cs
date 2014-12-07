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
using opennlp.console.cmdline;
using opennlp.console.cmdline.@params;
using opennlp.console.formats.convert;
using opennlp.tools.postag;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ConllXTokenSampleStreamFactory : DetokenizerSampleStreamFactory<TokenSample>
	{

	  internal interface Parameters : ConllXPOSSampleStreamFactory.Parameters, DetokenizerParameter
	  {
	  }

	  public static void registerFactory()
	  {
          StreamFactoryRegistry<TokenSample>.registerFactory(typeof(TokenSample), ConllXPOSSampleStreamFactory.CONLLX_FORMAT, new ConllXTokenSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ConllXTokenSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public ObjectStream<TokenSample> create(string[] args)
	  {
          Parameters @params = ArgumentParser.parse<Parameters>(args);

		ObjectStream<POSSample> samples = StreamFactoryRegistry<POSSample>.getFactory(typeof(POSSample), ConllXPOSSampleStreamFactory.CONLLX_FORMAT).create(ArgumentParser.filter(args, typeof(ConllXPOSSampleStreamFactory.Parameters)));
		return new POSToTokenSampleStream(createDetokenizer(@params), samples);
	  }
	}

}