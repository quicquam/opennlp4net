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
using System.Collections.Generic;
using System.IO;
using j4n.Exceptions;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using opennlp.model;
using opennlp.tools.nonjava.extensions;
using opennlp.tools.util;
using opennlp.tools.util.model;
using FileNotFoundException = j4n.Exceptions.FileNotFoundException;

namespace opennlp.tools.cmdline
{
    /// <summary>
	/// Util class for the command line interface.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public sealed class CmdLineUtil
	{

	 internal const int IO_BUFFER_SIZE = 1024 * 1024;

	  private CmdLineUtil()
	  {
		// not intended to be instantiated
	  }

	  /// <summary>
	  /// Check that the given input file is valid.
	  /// <para>
	  /// To pass the test it must:<br>
	  /// - exist<br>
	  /// - not be a directory<br>
	  /// - accessibly<br>
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="name"> the name which is used to refer to the file in an error message, it
	  /// should start with a capital letter.
	  /// </param>
	  /// <param name="inFile"> the particular file to check to qualify an input file
	  /// </param>
	  /// <exception cref="TerminateToolException">  if test does not pass this exception is
	  /// thrown and an error message is printed to the console. </exception>
	  public static void checkInputFile(string name, Jfile inFile)
	  {

		string isFailure = null;

		if (inFile.IsDirectory)
		{
		  isFailure = "The " + name + " file is a directory!";
		}
		else if (!inFile.exists())
		{
		  isFailure = "The " + name + " file does not exist!";
		}
		else if (!inFile.canRead())
		{
		  isFailure = "No permissions to read the " + name + " file!";
		}

		if (null != isFailure)
		{
		  throw new TerminateToolException(-1, isFailure + " Path: " + inFile.AbsolutePath);
		}
	  }

	  /// <summary>
	  /// Tries to ensure that it is possible to write to an output file. 
	  /// <para>
	  /// The method does nothing if it is possible to write otherwise
	  /// it prints an appropriate error message and a <seealso cref="TerminateToolException"/> is thrown.
	  /// </para>
	  /// <para>
	  /// Computing the contents of an output file (e.g. ME model) can be very time consuming.
	  /// Prior to this computation it should be checked once that writing this output file is
	  /// possible to be able to fail fast if not. If this validation is only done after a time
	  /// consuming computation it could frustrate the user.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="name"> human-friendly file name. for example perceptron model </param>
	  /// <param name="outFile"> file </param>
	  public static void checkOutputFile(string name, Jfile outFile)
	  {

		string isFailure = null;

		if (outFile.exists())
		{

		  // The file already exists, ensure that it is a normal file and that it is
		  // possible to write into it

		  if (outFile.IsDirectory)
		  {
			isFailure = "The " + name + " file is a directory!";
		  }
		  else if (outFile.IsFile)
		  {
			if (!outFile.canWrite())
			{
			  isFailure = "No permissions to write the " + name + " file!";
			}
		  }
		  else
		  {
			isFailure = "The " + name + " file is not a normal file!";
		  }
		}
		else
		{

		  // The file does not exist ensure its parent
		  // directory exists and has write permissions to create
		  // a new file in it

		  Jfile parentDir = outFile.AbsoluteFile.ParentFile;

		  if (parentDir != null && parentDir.exists())
		  {

			if (!parentDir.canWrite())
			{
			  isFailure = "No permissions to create the " + name + " file!";
			}
		  }
		  else
		  {
			isFailure = "The parent directory of the " + name + " file does not exist, " + "please create it first!";
		  }

		}

		if (null != isFailure)
		{
		  throw new TerminateToolException(-1, isFailure + " Path: " + outFile.AbsolutePath);
		}
	  }

	  public static FileInputStream openInFile(Jfile file)
	  {
		try
		{
		  return new FileInputStream(file);
		}
		catch (FileNotFoundException e)
		{
		  throw new TerminateToolException(-1, "File '" + file + "' cannot be found", e);
		}
	  }

	  /// <summary>
	  /// Writes a <seealso cref="BaseModel"/> to disk. Occurring errors are printed to the console
	  /// to inform the user.
	  /// </summary>
	  /// <param name="modelName"> type of the model, name is used in error messages. </param>
	  /// <param name="modelFile"> output file of the model </param>
	  /// <param name="model"> the model itself which should be written to disk </param>
	  public static void writeModel(string modelName, Jfile modelFile, BaseModel model)
	  {

		CmdLineUtil.checkOutputFile(modelName + " model", modelFile);

		Console.Error.Write("Writing " + modelName + " model ... ");

	      long beginModelWritingTime = DateTime.Now.Ticks;

          FileOutputStream modelOut = null;
		try
		{
		  modelOut = new FileOutputStream(modelFile);
		  model.serialize(modelOut);
		}
		catch (IOException e)
		{
		  Console.Error.WriteLine("failed");
		  throw new TerminateToolException(-1, "Error during writing model file '" + modelFile + "'", e);
		}
		finally
		{
		  if (modelOut != null)
		  {
			try
			{
			  modelOut.close();
			}
			catch (IOException e)
			{
			  Console.Error.WriteLine("Failed to properly close model file '" + modelFile + "': " + e.Message);
			}
		  }
		}

		long modelWritingDuration = DateTime.Now.Ticks - beginModelWritingTime;

		//Console.Erroror.printf("done (%.3fs)\n", modelWritingDuration / 1000d);

		Console.Error.WriteLine();

		Console.Error.WriteLine("Wrote " + modelName + " model to");
		Console.Error.WriteLine("path: " + modelFile.AbsolutePath);

		Console.Error.WriteLine();
	  }

	  /// <summary>
	  /// Returns the index of the parameter in the arguments, or -1 if the parameter is not found.
	  /// </summary>
	  /// <param name="param"> parameter name </param>
	  /// <param name="args"> arguments </param>
	  /// <returns> the index of the parameter in the arguments, or -1 if the parameter is not found </returns>
	  public static int getParameterIndex(string param, string[] args)
	  {
		for (int i = 0; i < args.Length; i++)
		{
		  if (args[i].StartsWith("-", StringComparison.Ordinal) && args[i].Equals(param))
		  {
			return i;
		  }
		}

		return -1;
	  }

	  /// <summary>
	  /// Retrieves the specified parameter from the given arguments.
	  /// </summary>
	  /// <param name="param"> parameter name </param>
	  /// <param name="args"> arguments </param>
	  /// <returns> parameter value </returns>
	  public static string getParameter(string param, string[] args)
	  {
		int i = getParameterIndex(param, args);
		  if (-1 < i)
		  {
			i++;
			if (i < args.Length)
			{
			  return args[i];
			}
		  }

		return null;
	  }

	  /// <summary>
	  /// Retrieves the specified parameter from the specified arguments.
	  /// </summary>
	  /// <param name="param"> parameter name </param>
	  /// <param name="args"> arguments </param>
	  /// <returns> parameter value </returns>
	  public static int? getIntParameter(string param, string[] args)
	  {
		string value = getParameter(param, args);

		try
		{
		  if (value != null)
		  {
			  return Convert.ToInt32(value);
		  }
		}
		catch (NumberFormatException)
		{
		}

		return null;
	  }

	  /// <summary>
	  /// Retrieves the specified parameter from the specified arguments.
	  /// </summary>
	  /// <param name="param"> parameter name </param>
	  /// <param name="args"> arguments </param>
	  /// <returns> parameter value </returns>
	  public static double? getDoubleParameter(string param, string[] args)
	  {
		string value = getParameter(param, args);

		try
		{
		  if (value != null)
		  {
			  return Convert.ToDouble(value);
		  }
		}
		catch (NumberFormatException)
		{
		}

		return null;
	  }

	  public static void checkLanguageCode(string code)
	  {
		IList<string> languageCodes = new List<string>();
		languageCodes.AddRange(Locale.ISOLanguages);
		languageCodes.Add("x-unspecified");

		if (!languageCodes.Contains(code))
		{
		  throw new TerminateToolException(1, "Unknown language code " + code + ", " + "must be an ISO 639 code!");
		}
	  }

	  public static bool containsParam(string param, string[] args)
	  {
		foreach (string arg in args)
		{
		  if (arg.Equals(param))
		  {
			return true;
		  }
		}

		return false;
	  }

	  public static void handleStdinIoError(IOException e)
	  {
		throw new TerminateToolException(-1, "IO Error while reading from stdin: " + e.Message, e);
	  }

	  // its optional, passing null is allowed
	  public static TrainingParameters loadTrainingParameters(string paramFile, bool supportSequenceTraining)
	  {

		TrainingParameters @params = null;

		if (paramFile != null)
		{

		  checkInputFile("Training Parameter", new Jfile(paramFile));

		  InputStream paramsIn = null;
		  try
		  {
			paramsIn = new FileInputStream(new Jfile(paramFile));

			@params = new TrainingParameters(paramsIn);
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "Error during parameters loading: " + e.Message, e);
		  }
		  finally
		  {
			try
			{
			  if (paramsIn != null)
			  {
				paramsIn.close();
			  }
			}
			catch (IOException)
			{
			  //sorry that this can fail
			}
		  }

          if (!TrainUtil.isValid(@params.getSettings()))
		  {
			throw new TerminateToolException(1, "Training parameters file '" + paramFile + "' is invalid!");
		  }

		  if (!supportSequenceTraining && TrainUtil.isSequenceTraining(@params.getSettings()))
		  {
			throw new TerminateToolException(1, "Sequence training is not supported!");
		  }
		}

		return @params;
	  }
	}

}