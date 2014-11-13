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
using j4n.Serialization;

namespace opennlp.tools.formats
{

	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using NameSample = opennlp.tools.namefind.NameSample;
	using opennlp.tools.util;

	public class BioNLP2004NameSampleStreamFactory : AbstractSampleStreamFactory<NameSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "DNA,protein,cell_type,cell_line,RNA") String getTypes();
		string Types {get;}
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(NameSample), "bionlp2004", new BioNLP2004NameSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal BioNLP2004NameSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override ObjectStream<NameSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

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

		return new BioNLP2004NameSampleStream(CmdLineUtil.openInFile(@params.Data), typesToGenerate);
	  }
	}

}