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

using System;
using j4n.IO.File;
using j4n.IO.InputStream;
using opennlp.tools.chunker;
using opennlp.tools.cmdline;
using opennlp.tools.util;

namespace opennlp.tools.formats.ad
{
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
		Charset Encoding {get;}

		Jfile Data {get;}

		string Lang {get;}

		int? Start {get;}

		int? End {get;}
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry<ChunkSample>.registerFactory(typeof(ChunkSample), "ad", new ADChunkSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ADChunkSampleStreamFactory(Type parameters) : base(parameters)
	  {
	  }

	    public override Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public override ObjectStream<ChunkSample> create(string[] args)
	  {

		Parameters parameters = ArgumentParser.parse<Parameters>(args);

		language = parameters.Lang;

		FileInputStream sampleDataIn = CmdLineUtil.openInFile(parameters.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, parameters.Encoding);

		ADChunkSampleStream sampleStream = new ADChunkSampleStream(lineStream);

		if (parameters.Start != null && parameters.Start > -1)
		{
		  sampleStream.Start = parameters.Start.Value;
		}

		if (parameters.End != null && parameters.End > -1)
		{
		  sampleStream.End = parameters.End.Value;
		}

		return sampleStream;
	  }
	}

}