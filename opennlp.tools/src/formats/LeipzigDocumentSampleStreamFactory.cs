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
using System.IO;
using j4n.IO.File;
using opennlp.tools.cmdline;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.doccat;
using opennlp.tools.util;

namespace opennlp.tools.formats
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

	  protected internal LeipzigDocumentSampleStreamFactory(Type parameters) : base(parameters)
	  {
	  }

	    public override Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public override ObjectStream<DocumentSample> create(string[] args)
	  {

		Parameters parameters = ArgumentParser.parse<Parameters>(args);
		language = parameters.Lang;

		try
		{
		  return new LeipzigDoccatSampleStream(parameters.Lang, 20, CmdLineUtil.openInFile(parameters.Data));
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while opening sample data: " + e.Message, e);
		}
	  }
	}

}