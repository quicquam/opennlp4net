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
using opennlp.tools.postag;
using opennlp.tools.util;

namespace opennlp.tools.formats.ad
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ADPOSSampleStreamFactory : LanguageSampleStreamFactory<POSSample>
	{

	  internal interface Parameters
	  {
		Charset Encoding {get;}

		Jfile Data {get;}

		string Lang {get;}

		bool? ExpandME {get;}

		bool? IncludeFeatures {get;}
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry<POSSample>.registerFactory(typeof(POSSample), "ad", new ADPOSSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ADPOSSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	    public override Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public override ObjectStream<POSSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse<Parameters>(args);

		language = @params.Lang;

		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding);

		ADPOSSampleStream sentenceStream = new ADPOSSampleStream(lineStream, @params.ExpandME.Value, @params.IncludeFeatures.Value);

		return sentenceStream;
	  }

	}

}