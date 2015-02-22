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
using opennlp.tools.cmdline;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.namefind;
using opennlp.tools.util;

namespace opennlp.tools.formats
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

	  protected internal Conll02NameSampleStreamFactory(Type parameters) : base(parameters)
	  {
	  }

	    public override Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public override ObjectStream<NameSample> create(string[] args)
	  {

		Parameters parameters = ArgumentParser.parse<Parameters>(args);

		Conll02NameSampleStream.LANGUAGE lang;
		if ("nl".Equals(parameters.Lang))
		{
		  lang = Conll02NameSampleStream.LANGUAGE.NL;
		  language = parameters.Lang;
		}
		else if ("es".Equals(parameters.Lang))
		{
		  lang = Conll02NameSampleStream.LANGUAGE.ES;
		  language = parameters.Lang;
		}
		else
		{
		  throw new TerminateToolException(1, "Unsupported language: " + parameters.Lang);
		}

		int typesToGenerate = 0;

		if (parameters.Types.Contains("per"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_PERSON_ENTITIES;
		}
		if (parameters.Types.Contains("org"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_ORGANIZATION_ENTITIES;
		}
		if (parameters.Types.Contains("loc"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_LOCATION_ENTITIES;
		}
		if (parameters.Types.Contains("misc"))
		{
		  typesToGenerate = typesToGenerate | Conll02NameSampleStream.GENERATE_MISC_ENTITIES;
		}


		return new Conll02NameSampleStream(lang, CmdLineUtil.openInFile(parameters.Data), typesToGenerate);
	  }
	}

}