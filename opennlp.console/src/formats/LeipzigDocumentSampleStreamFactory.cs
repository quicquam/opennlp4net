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
using System.IO;
using j4n.IO.File;
using j4n.Serialization;
using opennlp.console.cmdline;
using opennlp.console.cmdline.@params;
using opennlp.tools.doccat;

namespace opennlp.console.formats
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class LeipzigDocumentSampleStreamFactory : LanguageSampleStreamFactory<DocumentSample>
	{

	  internal interface Parameters : BasicFormatParams, LanguageParams
	  {
	      Jfile Data { get; set; }
	  }

	  public static void registerFactory()
	  {
          StreamFactoryRegistry<DocumentSample>.registerFactory(typeof(DocumentSample), "leipzig", new LeipzigDocumentSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal LeipzigDocumentSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	    public override Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public override ObjectStream<DocumentSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse<Parameters>(args);
		language = @params.Lang;

		try
		{
		  return new LeipzigDoccatSampleStream(@params.Lang, 20, CmdLineUtil.openInFile(@params.Data));
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while opening sample data: " + e.Message, e);
		}
	  }
	}

}