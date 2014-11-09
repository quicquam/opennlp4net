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
using j4n.IO.InputStream;

namespace opennlp.tools.formats
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using Parse = opennlp.tools.parser.Parse;
	using ParseSampleStream = opennlp.tools.parser.ParseSampleStream;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// Factory producing OpenNLP <seealso cref="ParseSampleStream"/>s.
	/// </summary>
	public class ParseSampleStreamFactory : AbstractSampleStreamFactory<Parse>
	{

	  public interface Parameters : BasicFormatParams
	  {
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(Parse), StreamFactoryRegistry.DEFAULT_FORMAT, new ParseSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ParseSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<Parse> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		CmdLineUtil.checkInputFile("Data", @params.Data);
		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding);

		return new ParseSampleStream(lineStream);
	  }
	}
}