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
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.Serialization;
using opennlp.console.cmdline;
using opennlp.console.cmdline.@params;
using opennlp.tools.chunker;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    /// <summary>
	/// Factory producing OpenNLP <seealso cref="ChunkSampleStream"/>s.
	/// </summary>
	public class ChunkerSampleStreamFactory : AbstractSampleStreamFactory<ChunkSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
	      Jfile Data { get; set; }
	  }

	  public static void registerFactory()
	  {
          StreamFactoryRegistry<ChunkSample>.registerFactory(typeof(ChunkSample), StreamFactoryRegistry<ChunkSample>.DEFAULT_FORMAT, new ChunkerSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ChunkerSampleStreamFactory(Type @params)
	  {
	  }

	    public Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public ObjectStream<ChunkSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse<Parameters>(args);

		CmdLineUtil.checkInputFile("Data", @params.Data);
		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding);

		return new ChunkSampleStream(lineStream);
	  }
	}
}