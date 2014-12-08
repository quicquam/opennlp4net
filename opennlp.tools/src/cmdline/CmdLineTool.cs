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
	/// Base class for all command line tools.
	/// </summary>
	public abstract class CmdLineTool
	{

	  protected internal CmdLineTool()
	  {
	  }

	  /// <summary>
	  /// Retrieves the name of the training data tool. The name (used as command)
	  /// must not contain white spaces.
	  /// </summary>
	  /// <returns> the name of the command line tool </returns>
	  public virtual string Name
	  {
		  get
		  {
			if (this.GetType().Name.EndsWith("Tool", StringComparison.Ordinal))
			{
			  return this.GetType().Name.Substring(0, this.GetType().Name.Length - 4);
			}
			else
			{
			  return this.GetType().Name;
			}
		  }
	  }

	  /// <summary>
	  /// Returns whether the tool has any command line params. </summary>
	  /// <returns> whether the tool has any command line params </returns>
	  public virtual bool hasParams()
	  {
		return true;
	  }

	  protected internal virtual string getBasicHelp<T>(Type argProxyInterface)
	  {
		return getBasicHelp<T>(new Type[]{argProxyInterface});
	  }

	  protected internal virtual string getBasicHelp<T>(params Type[] argProxyInterfaces)
	  {
		return "Usage: " + CLI.CMD + " " + Name + " " + ArgumentParser.createUsage(argProxyInterfaces);
	  }

	  /// <summary>
	  /// Retrieves a description on how to use the tool.
	  /// </summary>
	  /// <returns> a description on how to use the tool </returns>
	  public abstract string Help {get;}

	  protected internal virtual T validateAndParseParams<T>(string[] args, Type argProxyInterface)
	  {
		string errorMessage = ArgumentParser.validateArgumentsLoudly(args, argProxyInterface);
		if (null != errorMessage)
		{
		  throw new TerminateToolException(1, errorMessage + "\n" + Help);
		}
		return ArgumentParser.parse<T>(args, argProxyInterface);
	  }

	  /// <summary>
	  /// Retrieves a short description of what the tool does.
	  /// </summary>
	  /// <returns> a short description of what the tool does </returns>
	  public virtual string ShortDescription
	  {
		  get
		  {
			return "";
		  }
	  }
	}
}