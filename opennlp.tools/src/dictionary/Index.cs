using System.Collections.Generic;

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

namespace opennlp.tools.dictionary
{


	using StringList = opennlp.tools.util.StringList;

	/// <summary>
	/// This classes indexes <seealso cref="StringList"/>s. This makes it possible
	/// to check if a certain token is contained in at least one of the
	/// <seealso cref="StringList"/>s.
	/// </summary>
	public class Index
	{

	  private HashSet<string> tokens = new HashSet<string>();

	  /// <summary>
	  /// Initializes the current instance with the given
	  /// <seealso cref="StringList"/> <seealso cref="Iterator"/>.
	  /// </summary>
	  /// <param name="tokenLists"> </param>
	  public Index(IEnumerator<StringList> tokenLists)
	  {

		while (tokenLists.MoveNext())
		{

		  StringList tokens = tokenLists.Current;

		  for (int i = 0; i < tokens.size(); i++)
		  {
			this.tokens.Add(tokens.getToken(i));
		  }
		}
	  }

	  /// <summary>
	  /// Checks if at leat one <seealso cref="StringList"/> contains the
	  /// given token.
	  /// </summary>
	  /// <param name="token">
	  /// </param>
	  /// <returns> true if the token is contained otherwise false. </returns>
	  public virtual bool contains(string token)
	  {
		return tokens.Contains(token);
	  }
	}

}