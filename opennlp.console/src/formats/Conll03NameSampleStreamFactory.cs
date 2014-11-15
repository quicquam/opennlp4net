using System;
using j4n.Serialization;

/*
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  under the License.
 */

namespace opennlp.tools.formats
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using TerminateToolException = opennlp.tools.cmdline.TerminateToolException;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using LANGUAGE = opennlp.tools.formats.Conll03NameSampleStream.LANGUAGE;
	using NameSample = opennlp.tools.namefind.NameSample;
	using opennlp.tools.util;

	public class Conll03NameSampleStreamFactory : LanguageSampleStreamFactory<NameSample>
	{
	    public interface Parameters : BasicFormatParams
	  {
		string Lang {get;}

		string Types {get;}
	      object Data { get; set; }
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(NameSample), "conll03", new Conll03NameSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal Conll03NameSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<NameSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		// TODO: support the other languages with this CoNLL.
		LANGUAGE lang;
		if ("en".Equals(@params.Lang))
		{
		  lang = LANGUAGE.EN;
		  language = @params.Lang;
		}
		else if ("de".Equals(@params.Lang))
		{
		  lang = LANGUAGE.DE;
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


		return new Conll03NameSampleStream(lang, CmdLineUtil.openInFile(@params.Data), typesToGenerate);
	  }
	}

}