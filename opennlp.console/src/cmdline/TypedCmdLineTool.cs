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

namespace opennlp.console.cmdline
{

	/// <summary>
	/// Base class for tools which support processing of samples of some type T
	/// coming from a stream of a certain format.
	/// </summary>
	public abstract class TypedCmdLineTool<T> : CmdLineTool
	{

	  /// <summary>
	  /// variable to access the type of the generic parameter.
	  /// </summary>
	  protected internal readonly Type type;

	  /// <summary>
	  /// Constructor with type parameters.
	  /// </summary>
	  /// <param name="sampleType"> class of the template parameter </param>
	  protected internal TypedCmdLineTool(Type sampleType)
	  {
		this.type = sampleType;
	  }

	  /// <summary>
	  /// Returns stream factory for the type of this tool for the </code>format</code>.
	  /// </summary>
	  /// <param name="format"> data format name </param>
	  /// <returns> stream factory for the type of this tool for the format </returns>
	  protected internal virtual ObjectStreamFactory<T> getStreamFactory(string format)
	  {
		ObjectStreamFactory<T> factory = StreamFactoryRegistry<T>.getFactory(type, format);
		if (null != factory)
		{
		  return factory;
		}
		else
		{
		  throw new TerminateToolException(1, "Format " + format + " is not found.\n" + Help);
		}
	  }

	  /// <summary>
	  /// Validates arguments using parameters from <code>argProxyInterface</code> and the parameters of the
	  /// <code>format</code>.
	  /// </summary>
	  /// <param name="args"> arguments </param>
	  /// <param name="argProxyInterface"> interface with parameter descriptions </param>
	  /// <param name="format"> data format name </param>
	  /// @param <A> A </param>
	  protected internal virtual void validateAllArgs<A>(string[] args, Type argProxyInterface, string format)
	  {
		ObjectStreamFactory<T> factory = getStreamFactory(format);
		string errMessage = ArgumentParser.validateArgumentsLoudly(args, argProxyInterface, factory.getParameters());
		if (null != errMessage)
		{
		  throw new TerminateToolException(1, errMessage + "\n" + getHelp(format));
		}
	  }

	  /// <summary>
	  /// Validates arguments for a format processed by the <code>factory</code>. </summary>
	  /// <param name="factory"> a stream factory </param>
	  /// <param name="args"> arguments </param>
	  protected internal virtual void validateFactoryArgs(ObjectStreamFactory<T> factory, string[] args)
	  {
		string errMessage = ArgumentParser.validateArgumentsLoudly(args, factory.getParameters());
		if (null != errMessage)
		{
		  throw new TerminateToolException(1, "Format parameters are invalid: " + errMessage + "\n" + "Usage: " + ArgumentParser.createUsage(factory.getParameters()));
		}
	  }

	  protected internal override string getBasicHelp<A>(params Type[] argProxyInterfaces)
	  {
		IDictionary<string, ObjectStreamFactory<T>> factories = StreamFactoryRegistry<T>.getFactories(type);

		string formatsHelp = " ";
		if (1 < factories.Count)
		{
		  StringBuilder formats = new StringBuilder();
		  foreach (string format in factories.Keys)
		  {
			if (!StreamFactoryRegistry<T>.DEFAULT_FORMAT.Equals(format))
			{
			  formats.Append(".").Append(format).Append("|");
			}
		  }
		  formatsHelp = "[" + formats.ToString().Substring(0, formats.Length - 1) + "] ";
		}

		return "Usage: " + CLI.CMD + " " + Name + formatsHelp + ArgumentParser.createUsage(argProxyInterfaces);
	  }

	  public override string Help
	  {
		  get
		  {
			return getHelp("");
		  }
	  }

	  /// <summary>
	  /// Executes the tool with the given parameters.
	  /// </summary>
	  /// <param name="format"> format to work with </param>
	  /// <param name="args"> command line arguments </param>
	  public abstract void run(string format, string[] args);

	  /// <summary>
	  /// Retrieves a description on how to use the tool.
	  /// </summary>
	  /// <param name="format"> data format </param>
	  /// <returns> a description on how to use the tool </returns>
	  public abstract string getHelp(string format);
	}
}