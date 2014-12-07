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
using opennlp.console.cmdline;
using opennlp.console.cmdline.@params;
using opennlp.tools.namefind;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    /// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class Conll02NameSampleStreamFactory : LanguageSampleStreamFactory<NameSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
		string Lang {get;}

		string Types {get;}
	      Jfile Data { get; set; }
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry<NameSample>.registerFactory(typeof(NameSample), "conll02", new Conll02NameSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal Conll02NameSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	    public override Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public override ObjectStream<NameSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse<Parameters>(args);

		Conll02NameSampleStream.LANGUAGE lang;
		if ("nl".Equals(@params.Lang))
		{
		  lang = Conll02NameSampleStream.LANGUAGE.NL;
		  language = @params.Lang;
		}
		else if ("es".Equals(@params.Lang))
		{
		  lang = Conll02NameSampleStream.LANGUAGE.ES;
		  language = @params.Lang;
		}
		else
		{
		  throw new TerminateToolException(1, "Unsupported language: " + @params.Lang);
		}

		int typesToGenerate = 0;

		if (@params.Types.Contains("per"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_PERSON_ENTITIES;
		}
		if (@params.Types.Contains("org"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_ORGANIZATION_ENTITIES;
		}
		if (@params.Types.Contains("loc"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_LOCATION_ENTITIES;
		}
		if (@params.Types.Contains("misc"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_MISC_ENTITIES;
		}


		return new Conll02NameSampleStream(lang, CmdLineUtil.openInFile(@params.Data), typesToGenerate);
	  }
	}

}