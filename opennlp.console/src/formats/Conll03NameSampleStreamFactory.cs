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
using System;
using j4n.IO.File;
using opennlp.console.cmdline;
using opennlp.console.cmdline.@params;
using opennlp.tools.namefind;
using opennlp.tools.util;

namespace opennlp.console.formats
{
    public class Conll03NameSampleStreamFactory : LanguageSampleStreamFactory<NameSample>
    {
        public class Parameters : BasicFormatParams
        {
            public string Lang { get; private set; }

            public string Types { get; set; }
            public Jfile Data { get; set; }
            public Charset Encoding { get; private set; }
        }

        public static void registerFactory()
        {
            StreamFactoryRegistry<NameSample>.registerFactory(typeof(NameSample), "conll03", new Conll03NameSampleStreamFactory(typeof(Parameters)));
        }

        protected internal Conll03NameSampleStreamFactory(Type @params)
            : base(@params)
        {
        }

        public override Type getParameters()
        {
            throw new NotImplementedException();
        }

        public override ObjectStream<NameSample> create(string[] args)
        {

            Parameters @params = ArgumentParser.parse<Parameters>(args);

            // TODO: support the other languages with this CoNLL.
            Conll03NameSampleStream.LANGUAGE lang;
            if ("en".Equals(@params.Lang))
            {
                lang = Conll03NameSampleStream.LANGUAGE.EN;
                language = @params.Lang;
            }
            else if ("de".Equals(@params.Lang))
            {
                lang = Conll03NameSampleStream.LANGUAGE.DE;
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