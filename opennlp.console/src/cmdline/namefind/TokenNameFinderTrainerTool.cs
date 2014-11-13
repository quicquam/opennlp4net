using System.Collections.Generic;
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
using System.IO;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace opennlp.tools.cmdline.namefind
{


	using opennlp.tools.cmdline;
	using OptionalParameter = opennlp.tools.cmdline.ArgumentParser.OptionalParameter;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using TrainerToolParams = opennlp.tools.cmdline.namefind.TokenNameFinderTrainerTool.TrainerToolParams;
	using TrainingToolParams = opennlp.tools.cmdline.@params.TrainingToolParams;
	using NameSample = opennlp.tools.namefind.NameSample;
	using NameSampleTypeFilter = opennlp.tools.namefind.NameSampleTypeFilter;
	using TokenNameFinderModel = opennlp.tools.namefind.TokenNameFinderModel;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using opennlp.tools.util.model;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	public sealed class TokenNameFinderTrainerTool : AbstractTrainerTool<NameSample, TrainerToolParams>
	{
	    public interface TrainerToolParams : TrainingParams, TrainingToolParams
	  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @OptionalParameter @ParameterDescription(valueName = "types", description = "name types to use for training") String getNameTypes();
		string NameTypes {get;}
	  }

	  public TokenNameFinderTrainerTool() : base(typeof(NameSample), typeof(TrainerToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "trainer for the learnable name finder";
		  }
	  }

	  internal static sbyte[] openFeatureGeneratorBytes(string featureGenDescriptorFile)
	  {
		if (featureGenDescriptorFile != null)
		{
		  return openFeatureGeneratorBytes(new Jfile(featureGenDescriptorFile));
		}
		return null;
	  }

      internal static sbyte[] openFeatureGeneratorBytes(Jfile featureGenDescriptorFile)
	  {
		sbyte[] featureGeneratorBytes = null;
		// load descriptor file into memory
		if (featureGenDescriptorFile != null)
		{
		  InputStream bytesIn = CmdLineUtil.openInFile(featureGenDescriptorFile);

		  try
		  {
			featureGeneratorBytes = ModelUtil.read(bytesIn);
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "IO error while reading training data or indexing data: " + e.Message, e);
		  }
		  finally
		  {
			try
			{
			  bytesIn.close();
			}
			catch (IOException)
			{
			  // sorry that this can fail
			}
		  }
		}
		return featureGeneratorBytes;
	  }

      public static IDictionary<string, object> loadResources(Jfile resourcePath)
	  {
		IDictionary<string, object> resources = new Dictionary<string, object>();

		if (resourcePath != null)
		{

		  IDictionary<string, ArtifactSerializer> artifactSerializers = TokenNameFinderModel.createArtifactSerializers();

		  File[] resourceFiles = resourcePath.listFiles();

		  // TODO: Filter files, also files with start with a dot
		  foreach (File resourceFile in resourceFiles)
		  {

			// TODO: Move extension extracting code to method and
			// write unit test for it

			// extract file ending
			string resourceName = resourceFile.Name;

			int lastDot = resourceName.LastIndexOf('.');

			if (lastDot == -1)
			{
			  continue;
			}

			string ending = resourceName.Substring(lastDot + 1);

			// lookup serializer from map
			ArtifactSerializer serializer = artifactSerializers[ending];

			// TODO: Do different? For now just ignore ....
			if (serializer == null)
			{
			  continue;
			}

			InputStream resoruceIn = CmdLineUtil.openInFile(resourceFile);

			try
			{
			  resources[resourceName] = serializer.create(resoruceIn);
			}
			catch (InvalidFormatException e)
			{
			  // TODO: Fix exception handling
			  Console.WriteLine(e.ToString());
			  Console.Write(e.StackTrace);
			}
			catch (IOException e)
			{
			  // TODO: Fix exception handling
			  Console.WriteLine(e.ToString());
			  Console.Write(e.StackTrace);
			}
			finally
			{
			  try
			  {
				resoruceIn.close();
			  }
			  catch (IOException)
			  {
			  }
			}
		  }
		}
		return resources;
	  }

	  internal static IDictionary<string, object> loadResources(string resourceDirectory)
	  {

		if (resourceDirectory != null)
		{
		  File resourcePath = new File(resourceDirectory);
		  return loadResources(resourcePath);
		}

		return new Dictionary<string, object>();
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		mlParams = CmdLineUtil.loadTrainingParameters(@params.Params, false);
		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(@params.Iterations.Value, @params.Cutoff.Value);
		}

		File modelOutFile = @params.Model;

		sbyte[] featureGeneratorBytes = openFeatureGeneratorBytes(@params.Featuregen);


		// TODO: Support Custom resources:
		//       Must be loaded into memory, or written to tmp file until descriptor 
		//       is loaded which defines parses when model is loaded

		IDictionary<string, object> resources = loadResources(@params.Resources);

		CmdLineUtil.checkOutputFile("name finder model", modelOutFile);

		if (@params.NameTypes != null)
		{
		  string[] nameTypes = @params.NameTypes.Split(",", true);
		  sampleStream = new NameSampleTypeFilter(nameTypes, sampleStream);
		}

		TokenNameFinderModel model;
		try
		{
		  model = opennlp.tools.namefind.NameFinderME.train(@params.Lang, @params.Type, sampleStream, mlParams, featureGeneratorBytes, resources);
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while reading training data or indexing data: " + e.Message, e);
		}
		finally
		{
		  try
		  {
			sampleStream.close();
		  }
		  catch (IOException)
		  {
			// sorry that this can fail
		  }
		}

		CmdLineUtil.writeModel("name finder", modelOutFile, model);
	  }
	}

}