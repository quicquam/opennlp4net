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

using opennlp.tools.cmdline;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.parser;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.tools.formats.convert
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ParseToTokenSampleStreamFactory : DetokenizerSampleStreamFactory<TokenSample>
	{

	  internal interface Parameters : ParseSampleStreamFactory.Parameters, DetokenizerParameter
	  {
	  }

	  private ParseToTokenSampleStreamFactory() : base(typeof(Parameters))
	  {
	  }

	  public  ObjectStream<TokenSample> create(string[] args)
	  {
		Parameters parameters = ArgumentParser.parse<Parameters>(args);

        ObjectStream<Parse> parseSampleStream = StreamFactoryRegistry<Parse>.getFactory(typeof(Parse), StreamFactoryRegistry<TokenSample>.DEFAULT_FORMAT).create(ArgumentParser.filter(args, typeof(WordTagSampleStreamFactory.Parameters)));

		return (new POSToTokenSampleStream(createDetokenizer(parameters), new ParseToPOSSampleStream(parseSampleStream)));
	  }

	  public static void registerFactory()
	  {
          StreamFactoryRegistry<TokenSample>.registerFactory(typeof(TokenSample), "parse", new ParseToTokenSampleStreamFactory());
	  }
	}

}