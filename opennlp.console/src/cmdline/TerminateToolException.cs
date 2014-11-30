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
	/// Exception to terminate the execution of a command line tool.
	/// <para>
	/// The exception should be thrown to indicate that the VM should be terminated with
	/// the specified error code, instead of just calling <seealso cref="System#exit(int)"/>.
	/// </para>
	/// <para>
	/// The return code convention is to return:<br>
	/// 0 in case of graceful termination<br>
	/// -1 in case of runtime errors, such as IOException<br>
	/// 1 in case of invalid parameters.
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class TerminateToolException : Exception
	{

	  private const long serialVersionUID = 1L;

	  private readonly int code;
	  private readonly string message;

	  public TerminateToolException(int code, string message, Exception t) : base(message)
	  {
		this.code = code;
		this.message = message;
	  }

	  public TerminateToolException(int code, string message)
	  {
		this.code = code;
		this.message = message;
	  }

	  public TerminateToolException(int code) : this(code, null)
	  {
	  }

	  public virtual int Code
	  {
		  get
		  {
			return code;
		  }
	  }

	  public override string Message
	  {
		  get
		  {
			return message;
		  }
	  }
	}

}