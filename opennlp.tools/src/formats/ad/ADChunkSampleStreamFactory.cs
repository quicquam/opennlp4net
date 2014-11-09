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
using j4n.IO.File;

namespace opennlp.tools.formats.ad
{


	using ChunkSample = opennlp.tools.chunker.ChunkSample;
	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using OptionalParameter = opennlp.tools.cmdline.ArgumentParser.OptionalParameter;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using opennlp.tools.formats;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// A Factory to create a Arvores Deitadas ChunkStream from the command line
	/// utility.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class ADChunkSampleStreamFactory : LanguageSampleStreamFactory<ChunkSample>
	{

	  internal interface Parameters
	  {
		//all have to be repeated, because encoding is not optional,
		//according to the check if (encoding == null) { below (now removed)
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "charsetName", description = "encoding for reading and writing text, if absent the system default is used.") java.nio.charset.Charset getEncoding();
		Charset Encoding {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "sampleData", description = "data to be used, usually a file name.") java.io.File getData();
		Jfile Data {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "language", description = "language which is being processed.") String getLang();
		string Lang {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "start", description = "index of first sentence") @OptionalParameter Integer getStart();
		int? Start {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "end", description = "index of last sentence") @OptionalParameter Integer getEnd();
		int? End {get;}
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(ChunkSample), "ad", new ADChunkSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ADChunkSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<ChunkSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		language = @params.Lang;

		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding);

		ADChunkSampleStream sampleStream = new ADChunkSampleStream(lineStream);

		if (@params.Start != null && @params.Start > -1)
		{
		  sampleStream.Start = @params.Start.Value;
		}

		if (@params.End != null && @params.End > -1)
		{
		  sampleStream.End = @params.End.Value;
		}

		return sampleStream;
	  }
	}

}