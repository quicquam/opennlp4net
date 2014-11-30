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

namespace opennlp.tools.cmdline
{

	/// <summary>
	/// Base class for tools which take additional parameters. For example, trainers or evaluators.
	/// </summary>
	public abstract class AbstractTypedParamTool<T, P> : TypedCmdLineTool<T>
	{

	  /// <summary>
	  /// variable to access the parameters
	  /// </summary>
	  protected internal readonly Type paramsClass;

	  /// <summary>
	  /// Constructor with type parameters.
	  /// </summary>
	  /// <param name="sampleType"> class of the template parameter </param>
	  /// <param name="paramsClass"> interface with parameters </param>
	  protected internal AbstractTypedParamTool(Type sampleType, Type paramsClass) : base(sampleType)
	  {
		this.paramsClass = paramsClass;
	  }

	  public override string getHelp(string format)
	  {
		if ("".Equals(format) || StreamFactoryRegistry<T>.DEFAULT_FORMAT.Equals(format))
		{
		  return getBasicHelp<T>(paramsClass, StreamFactoryRegistry<T>.getFactory(type, StreamFactoryRegistry<T>.DEFAULT_FORMAT).getParameters());
		}
		else
		{
		  ObjectStreamFactory<T> factory = StreamFactoryRegistry<T>.getFactory(type, format);
		  if (null == factory)
		  {
			throw new TerminateToolException(1, "Format " + format + " is not found.\n" + Help);
		  }
		  return "Usage: " + CLI.CMD + " " + Name + "." + format + " " + ArgumentParser.createUsage(paramsClass, factory.getParameters<P>());
		}
	  }
	}

}