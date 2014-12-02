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
using j4n.Serialization;
using opennlp.console.cmdline;
using opennlp.console.cmdline.@params;

namespace opennlp.console.formats
{
    public class BioNLP2004NameSampleStreamFactory<NameSample> : AbstractSampleStreamFactory<NameSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
		string Types {get;}
	      Jfile Data { get; set; }
	  }

	  public static void registerFactory()
	  {
          StreamFactoryRegistry<NameSample>.registerFactory(typeof(NameSample), "bionlp2004", new BioNLP2004NameSampleStreamFactory<NameSample>(typeof(Parameters)));
	  }

	  protected internal BioNLP2004NameSampleStreamFactory(Type @params)
	  {
	  }

        public Type getParameters()
        {
            throw new NotImplementedException();
        }

        public ObjectStream<NameSample> create(string[] args)
        {
            Parameters @params = ArgumentParser.parse<Parameters>(args);

            int typesToGenerate = 0;

            if (@params.Types.Contains("DNA"))
            {
                typesToGenerate = typesToGenerate | BioNLP2004NameSampleStream.GENERATE_DNA_ENTITIES;
            }
            else if (@params.Types.Contains("protein"))
            {
                typesToGenerate = typesToGenerate | BioNLP2004NameSampleStream.GENERATE_PROTEIN_ENTITIES;
            }
            else if (@params.Types.Contains("cell_type"))
            {
                typesToGenerate = typesToGenerate | BioNLP2004NameSampleStream.GENERATE_CELLTYPE_ENTITIES;
            }
            else if (@params.Types.Contains("cell_line"))
            {
                typesToGenerate = typesToGenerate | BioNLP2004NameSampleStream.GENERATE_CELLLINE_ENTITIES;
            }
            else if (@params.Types.Contains("RNA"))
            {
                typesToGenerate = typesToGenerate | BioNLP2004NameSampleStream.GENERATE_RNA_ENTITIES;
            }

            return new BioNLP2004NameSampleStream(CmdLineUtil.openInFile(@params.Data), typesToGenerate) as ObjectStream<NameSample>;
        }
	}

}