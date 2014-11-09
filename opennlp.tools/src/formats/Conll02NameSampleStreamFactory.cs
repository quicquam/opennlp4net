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

namespace opennlp.tools.formats
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using TerminateToolException = opennlp.tools.cmdline.TerminateToolException;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using LANGUAGE = opennlp.tools.formats.Conll02NameSampleStream.LANGUAGE;
	using NameSample = opennlp.tools.namefind.NameSample;
	using opennlp.tools.util;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class Conll02NameSampleStreamFactory : LanguageSampleStreamFactory<NameSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "es|nl") String getLang();
		string Lang {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "per,loc,org,misc") String getTypes();
		string Types {get;}
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(NameSample), "conll02", new Conll02NameSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal Conll02NameSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<NameSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		LANGUAGE lang;
		if ("nl".Equals(@params.Lang))
		{
		  lang = LANGUAGE.NL;
		  language = @params.Lang;
		}
		else if ("es".Equals(@params.Lang))
		{
		  lang = LANGUAGE.ES;
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