using System;
using System.Collections.Generic;
using System.Text;

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

namespace opennlp.tools.cmdline
{


	using opennlp.tools.util;

	/// <summary>
	/// Base class for format conversion tools.
	/// </summary>
	/// @param <T> class of data sample the tool converts, for example {@link opennlp.tools.postag
	/// .POSSample} </param>
	public abstract class AbstractConverterTool<T> : TypedCmdLineTool<T>
	{

	  /// <summary>
	  /// Constructor with type parameter.
	  /// </summary>
	  /// <param name="sampleType"> class of the template parameter </param>
	  protected internal AbstractConverterTool(Type sampleType) : base(sampleType)
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			IDictionary<string, ObjectStreamFactory<T>> factories = StreamFactoryRegistry<T>.getFactories(type);
			StringBuilder help = new StringBuilder();
			if (2 == factories.Keys.Count) //opennlp + foreign
			{
			  foreach (string format in factories.Keys)
			  {
				if (!StreamFactoryRegistry<T>.DEFAULT_FORMAT.Equals(format))
				{
				  help.Append(format);
				}
			  }
			  return "converts " + help.ToString() + " data format to native OpenNLP format";
			}
			else if (2 < factories.Keys.Count)
			{
			  foreach (string format in factories.Keys)
			  {
				if (!StreamFactoryRegistry<T>.DEFAULT_FORMAT.Equals(format))
				{
				  help.Append(format).Append(",");
				}
			  }
			  return "converts foreign data formats (" + help.ToString().Substring(0, help.Length - 1) + ") to native OpenNLP format";
			}
			else
			{
			  throw new AssertionError("There should be more than 1 factory registered for converter " + "tool");
			}
		  }
	  }

	  private string createHelpString(string format, string usage)
	  {
		return "Usage: " + CLI.CMD + " " + Name + " " + format + " " + usage;
	  }

	  public override string Help
	  {
		  get
		  {
			IDictionary<string, ObjectStreamFactory> factories = StreamFactoryRegistry.getFactories(type);
			StringBuilder help = new StringBuilder("help|");
			foreach (string formatName in factories.Keys)
			{
			  if (!StreamFactoryRegistry.DEFAULT_FORMAT.Equals(formatName))
			  {
				help.Append(formatName).Append("|");
			  }
			}
			return createHelpString(help.Substring(0, help.Length - 1), "[help|options...]");
		  }
	  }

	  public override string getHelp(string format)
	  {
		return Help;
	  }

	  public override void run(string format, string[] args)
	  {
		if (0 == args.Length)
		{
		  Console.WriteLine(Help);
		}
		else
		{
		  format = args[0];
		  ObjectStreamFactory streamFactory = getStreamFactory(format);

		  string[] formatArgs = new string[args.Length - 1];
		  Array.Copy(args, 1, formatArgs, 0, formatArgs.Length);

		  string helpString = createHelpString(format, ArgumentParser.createUsage(streamFactory.Parameters));
		  if (0 == formatArgs.Length || (1 == formatArgs.Length && "help".Equals(formatArgs[0])))
		  {
			Console.WriteLine(helpString);
			Environment.Exit(0);
		  }

		  string errorMessage = ArgumentParser.validateArgumentsLoudly(formatArgs, streamFactory.Parameters);
		  if (null != errorMessage)
		  {
			throw new TerminateToolException(1, errorMessage + "\n" + helpString);
		  }

		  ObjectStream<T> sampleStream = streamFactory.create(formatArgs);

		  try
		  {
			object sample;
			while ((sample = sampleStream.read()) != null)
			{
			  Console.WriteLine(sample.ToString());
			}
		  }
		  catch (IOException e)
		  {
			throw new TerminateToolException(-1, "IO error while converting data : " + e.Message, e);
		  }
		  finally
		  {
			if (sampleStream != null)
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
		  }
		}
	  }
	}

}