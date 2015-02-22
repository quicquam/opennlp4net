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
using opennlp.tools.cmdline;
using opennlp.tools.cmdline.parameters;
using opennlp.tools.namefind;
using opennlp.tools.util;

namespace opennlp.tools.formats
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

        protected internal Conll03NameSampleStreamFactory(Type parameters)
            : base(parameters)
        {
        }

        public override Type getParameters()
        {
            throw new NotImplementedException();
        }

        public override ObjectStream<NameSample> create(string[] args)
        {

            Parameters parameters = ArgumentParser.parse<Parameters>(args);

            // TODO: support the other languages with this CoNLL.
            Conll03NameSampleStream.LANGUAGE lang;
            if ("en".Equals(parameters.Lang))
            {
                lang = Conll03NameSampleStream.LANGUAGE.EN;
                language = parameters.Lang;
            }
            else if ("de".Equals(parameters.Lang))
            {
                lang = Conll03NameSampleStream.LANGUAGE.DE;
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


            return new Conll03NameSampleStream(lang, CmdLineUtil.openInFile(parameters.Data), typesToGenerate);
        }
    }

}