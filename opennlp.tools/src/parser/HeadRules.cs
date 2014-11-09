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


namespace opennlp.tools.parser
{

	/// <summary>
	/// Interface for encoding the head rules associated with parsing.
	/// </summary>
	public interface HeadRules
	{

	  /// <summary>
	  /// Returns the head constituent for the specified constituents of the specified type.
	  /// </summary>
	  /// <param name="constituents"> The constituents which make up a constituent of the specified type. </param>
	  /// <param name="type"> The type of a constituent which is made up of the specified constituents. </param>
	  /// <returns> The constituent which is the head. </returns>
	  Parse getHead(Parse[] constituents, string type);

	  /// <summary>
	  /// Returns the set of punctuation tags.  Attachment decisions for these tags will not be modeled.
	  /// </summary>
	  /// <returns> the set of punctuation tags. </returns>
	  HashSet<string> PunctuationTags {get;}
	}
}