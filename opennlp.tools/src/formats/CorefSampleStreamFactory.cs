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

namespace opennlp.tools.formats
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using CorefSample = opennlp.tools.coref.CorefSample;
	using CorefSampleDataStream = opennlp.tools.coref.CorefSampleDataStream;
	using opennlp.tools.util;
	using ParagraphStream = opennlp.tools.util.ParagraphStream;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	public class CorefSampleStreamFactory : AbstractSampleStreamFactory<CorefSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
	  }

	  protected internal CorefSampleStreamFactory() : base(typeof(Parameters))
	  {
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(CorefSample), StreamFactoryRegistry.DEFAULT_FORMAT, new CorefSampleStreamFactory());
	  }

	  public override ObjectStream<CorefSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		CmdLineUtil.checkInputFile("Data", @params.Data);
		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new ParagraphStream(new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding));

		return new CorefSampleDataStream(lineStream);
	  }
	}

}