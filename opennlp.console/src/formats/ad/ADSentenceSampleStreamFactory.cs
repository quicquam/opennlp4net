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
using j4n.IO.InputStream;
using j4n.Serialization;
using opennlp.tools.cmdline;

namespace opennlp.tools.formats.ad
{


	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using OptionalParameter = opennlp.tools.cmdline.ArgumentParser.OptionalParameter;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using opennlp.tools.formats;
	using SentenceSample = opennlp.tools.sentdetect.SentenceSample;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
    public class ADSentenceSampleStreamFactory : LanguageSampleStreamFactory<SentenceSample>, ObjectStreamFactory<SentenceSample>
	{

	  internal interface Parameters
	  {
		Charset Encoding {get;}

		Jfile Data {get;}

		string Lang {get;}

		bool? IncludeTitles {get;}
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry<SentenceSample>.registerFactory(typeof(SentenceSample), "ad", new ADSentenceSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ADSentenceSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

      public  ObjectStream<T> create<T>(string[] args)
	  {

		Parameters @params = ArgumentParser.parse<Parameters>(args);

		language = @params.Lang;

		bool includeTitle = @params.IncludeTitles.Value;

		FileInputStream sampleDataIn = CmdLineUtil.openInFile(@params.Data);

		ObjectStream<string> lineStream = new PlainTextByLineStream(sampleDataIn.Channel, @params.Encoding);

		ADSentenceSampleStream sentenceStream = new ADSentenceSampleStream(lineStream, includeTitle);

		//return sentenceStream; 

          throw new NotImplementedException();
	  }

	    public override Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public override ObjectStream<SentenceSample> create(string[] args)
	    {
	        throw new NotImplementedException();
	    }
	}

}