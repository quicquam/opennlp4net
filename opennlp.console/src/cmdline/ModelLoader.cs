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
using System.IO;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace opennlp.tools.cmdline
{


	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;

	/// <summary>
	/// Loads a model and does all the error handling for the command line tools.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// 
	/// </para>
	/// </summary>
	/// @param <T> </param>
	public abstract class ModelLoader<T>
	{

	  private readonly string modelName;

	  protected internal ModelLoader(string modelName)
	  {

		if (modelName == null)
		{
		  throw new System.ArgumentException("modelName must not be null!");
		}

		this.modelName = modelName;
	  }

	  protected internal abstract T loadModel(InputStream modelIn);

	  public virtual T load(Jfile modelFile)
	  {

		long beginModelLoadingTime = DateTime.Now.Ticks;

		CmdLineUtil.checkInputFile(modelName + " model", modelFile);

		Console.Error.Write("Loading " + modelName + " model ... ");

		InputStream modelIn = new BufferedInputStream(CmdLineUtil.openInFile(modelFile), CmdLineUtil.IO_BUFFER_SIZE);

		T model;

		try
		{
		  model = loadModel(modelIn);
		}
		catch (InvalidFormatException e)
		{
		  Console.Error.WriteLine("failed");
		  throw new TerminateToolException(-1, "Model has invalid format", e);
		}
		catch (IOException e)
		{
		  Console.Error.WriteLine("failed");
		  throw new TerminateToolException(-1, "IO error while loading model file '" + modelFile + "'", e);
		}
		finally
		{
		  // will not be null because openInFile would 
		  // terminate in this case
		  try
		  {
			modelIn.close();
		  }
		  catch (IOException)
		  {
			// sorry that this can fail
		  }
		}

		long modelLoadingDuration = DateTime.Now.Ticks - beginModelLoadingTime;

		Console.Error.WriteLine(string.Format("done (%.3fs)\n", modelLoadingDuration / 1000d));

		return model;
	  }
	}

}