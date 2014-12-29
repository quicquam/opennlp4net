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
using System.IO;
using System.Linq;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;

namespace opennlp.tools.cmdline
{

	/// <summary>
	/// A simple tool which can be executed from the command line.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public abstract class BasicCmdLineTool : CmdLineTool
	{

	  /// <summary>
	  /// Executes the tool with the given parameters.
	  /// </summary>
	  /// <param name="args"> arguments </param>
	  public abstract void run(string[] args);

      protected InputStream GetInputStream(string[] args)
      {
          return args.Count() < 2 ? new InputStream(Console.OpenStandardInput()) : new InputStream(args[1]);
      }

	    protected OutputStream GetOutputStream(string[] args)
	    {
            return args.Count() < 3 ? new OutputStream(Console.OpenStandardOutput()) : new OutputStream(new FileOutputStream(args[2]));
        }
	}
}