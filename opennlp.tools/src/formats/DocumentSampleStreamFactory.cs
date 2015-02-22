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
using opennlp.tools.cmdline;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.doccat;
using opennlp.tools.util;

namespace opennlp.tools.formats
{
    /// <summary>
	/// Factory producing OpenNLP <seealso cref="DocumentSampleStream"/>s.
	/// </summary>
	public class DocumentSampleStreamFactory : AbstractSampleStreamFactory<DocumentSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
	      Jfile Data { get; set; }
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry<DocumentSample>.registerFactory(typeof(DocumentSample), StreamFactoryRegistry<DocumentSample>.DEFAULT_FORMAT, new DocumentSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal DocumentSampleStreamFactory(Type parameters)
	  {
	  }

	    public Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public ObjectStream<DocumentSample> create(string[] args)
	  {
		Parameters parameters = ArgumentParser.parse<Parameters>(args);

		CmdLineUtil.checkInputFile("Data", parameters.Data);
		FileInputStream sampleDataIn = CmdLineUtil.openInFile(parameters.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, parameters.Encoding);

		return new DocumentSampleStream(lineStream);
	  }
	}

}