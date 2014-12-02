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
using opennlp.tools.coref;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    public class CorefSampleStreamFactory : AbstractSampleStreamFactory<CorefSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
	      Jfile Data { get; set; }
	  }

	  protected internal CorefSampleStreamFactory()
	  {
	  }

	  public static void registerFactory()
	  {
          StreamFactoryRegistry<CorefSample>.registerFactory(typeof(CorefSample), StreamFactoryRegistry<CorefSample>.DEFAULT_FORMAT, new CorefSampleStreamFactory());
	  }

	    public Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public ObjectStream<CorefSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse<Parameters>(args);

		CmdLineUtil.checkInputFile("Data", @params.Data);
		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new ParagraphStream(new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding));

		return new CorefSampleDataStream(lineStream);
	  }
	}

}