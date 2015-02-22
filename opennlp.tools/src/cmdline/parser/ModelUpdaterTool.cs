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
using opennlp.tools.cmdline.parameters;
using opennlp.tools.parser;
using opennlp.tools.util;

namespace opennlp.tools.cmdline.parser
{
    /// <summary>
	/// Abstract base class for tools which update the parser model.
	/// </summary>
	public abstract class ModelUpdaterTool : AbstractTypedParamTool<Parse, ModelUpdaterTool.ModelUpdaterParams>
	{
	    public interface ModelUpdaterParams : TrainingToolParams
	  {
	  }

	  protected internal ModelUpdaterTool() : base(typeof(Parse), typeof(ModelUpdaterParams))
	  {
	  }

	  protected internal abstract ParserModel trainAndUpdate(ParserModel originalModel, ObjectStream<Parse> parseSamples, ModelUpdaterParams parameters);

	  public sealed override void run(string format, string[] args)
	  {
          ModelUpdaterParams parameters = validateAndParseParams<ModelUpdaterParams>(ArgumentParser.filter(args, typeof(ModelUpdaterParams)), typeof(ModelUpdaterParams));

		// Load model to be updated
		Jfile modelFile = parameters.Model;
		ParserModel originalParserModel = (new ParserModelLoader()).load(modelFile);

		ObjectStreamFactory<Parse> factory = getStreamFactory(format);
		string[] fargs = ArgumentParser.filter(args, factory.getParameters());
		validateFactoryArgs(factory, fargs);
		ObjectStream<Parse> sampleStream = factory.create(fargs);

		ParserModel updatedParserModel;
		try
		{
		  updatedParserModel = trainAndUpdate(originalParserModel, sampleStream, parameters);
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

		CmdLineUtil.writeModel("parser", modelFile, updatedParserModel);
	  }
	}

}