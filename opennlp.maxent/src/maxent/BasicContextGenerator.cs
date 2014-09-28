﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using opennlp.nonjava.helperclasses;

namespace opennlp.maxent
{


	/// <summary>
	/// Generate contexts for maxent decisions, assuming that the input
	/// given to the getContext() method is a String containing contextual
	/// predicates separated by spaces. 
	/// e.g:
	/// <para>
	/// cp_1 cp_2 ... cp_n
	/// </para>
	/// </summary>
	public class BasicContextGenerator : ContextGenerator
	{

	  private string separator = " ";

	  public BasicContextGenerator()
	  {
	  }

	  public BasicContextGenerator(string sep)
	  {
		separator = sep;
	  }

	  /// <summary>
	  /// Builds up the list of contextual predicates given a String.
	  /// </summary>
	  public virtual string[] getContext(object o)
	  {
		string s = (string) o;
		return (string[]) s.Split(separator, true);
	  }

	}


}