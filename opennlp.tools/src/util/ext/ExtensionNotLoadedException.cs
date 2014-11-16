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

namespace opennlp.tools.util.ext
{

	/// <summary>
	/// Exception indicates that an OpenNLP extension could not be loaded.
	/// </summary>
	public class ExtensionNotLoadedException : Exception
	{

	  private const long serialVersionUID = 1L;

	  private readonly bool isOSGiEnvironment;

	  public ExtensionNotLoadedException(string message) : base(message)
	  {

		isOSGiEnvironment = ExtensionLoader.OSGiAvailable;
	  }

	  public ExtensionNotLoadedException(Exception t) : base(t)
	  {

		isOSGiEnvironment = ExtensionLoader.OSGiAvailable;
	  }

	  /// <summary>
	  /// Indicates if OpenNLP is running in an OSGi environment or not.
	  /// 
	  /// @return
	  /// </summary>
	  public virtual bool OSGiEnvironment
	  {
		  get
		  {
			return isOSGiEnvironment;
		  }
	  }
	}

}