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
using j4n.IO.InputStream;
using j4n.Serialization;

namespace opennlp.tools.formats
{

	using ChunkSample = opennlp.tools.chunker.ChunkSample;
	using ChunkSampleStream = opennlp.tools.chunker.ChunkSampleStream;
	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// Factory producing OpenNLP <seealso cref="ChunkSampleStream"/>s.
	/// </summary>
	public class ChunkerSampleStreamFactory : AbstractSampleStreamFactory<ChunkSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(ChunkSample), StreamFactoryRegistry.DEFAULT_FORMAT, new ChunkerSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ChunkerSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<ChunkSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		CmdLineUtil.checkInputFile("Data", @params.Data);
		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding);

		return new ChunkSampleStream(lineStream);
	  }
	}
}