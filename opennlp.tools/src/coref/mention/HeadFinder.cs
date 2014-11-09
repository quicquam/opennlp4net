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

namespace opennlp.tools.coref.mention
{

	/// <summary>
	/// Interface for finding head words in noun phrases and head noun-phrases in parses.
	/// </summary>
	public interface HeadFinder
	{

	  /// <summary>
	  /// Returns the child parse which contains the lexical head of the specified parse.
	  /// </summary>
	  /// <param name="parse"> The parse in which to find the head. </param>
	  /// <returns> The parse containing the lexical head of the specified parse.  If no head is
	  /// available or the constituent has no sub-components that are eligible heads then null is returned. </returns>
	  Parse getHead(Parse parse);

	  /// <summary>
	  /// Returns which index the specified list of token is the head word.
	  /// </summary>
	  /// <param name="parse"> The parse in which to find the head index. </param>
	  /// <returns> The index of the head token. </returns>
	  int getHeadIndex(Parse parse);

	  /// <summary>
	  /// Returns the parse bottom-most head of a <code>Parse</code>. If no
	  /// head is available which is a child of <code>p</code> then <code>p</code> is returned.
	  /// </summary>
	  /// <param name="p"> Parse to find the head of. </param>
	  /// <returns> bottom-most head of p. </returns>
	  Parse getLastHead(Parse p);

	  /// <summary>
	  /// Returns head token for the specified np parse.
	  /// </summary>
	  /// <param name="np"> The noun parse to get head from. </param>
	  /// <returns> head token parse. </returns>
	  Parse getHeadToken(Parse np);
	}

}