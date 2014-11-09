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
	/// Specifies the interface that Objects which determine the space of
	/// mentions for coreference should implement.
	/// </summary>
	public interface MentionFinder
	{

	  /// <summary>
	  /// Specifies whether pre-nominal named-entities should be collected as mentions.
	  /// </summary>
	  /// <param name="collectPrenominalNamedEntities"> true if pre-nominal named-entities should be collected; false otherwise. </param>
	  bool PrenominalNamedEntityCollection {set;get;}


	  /// <summary>
	  /// Returns whether this mention finder collects coordinated noun phrases as mentions.
	  /// </summary>
	  /// <returns> true if this mention finder collects coordinated noun phrases as mentions; false otherwise. </returns>
	  bool CoordinatedNounPhraseCollection {get;set;}


	  /// <summary>
	  /// Returns an array of mentions.
	  /// </summary>
	  /// <param name="parse"> A top level parse from which mentions are gathered.
	  /// </param>
	  /// <returns> an array of mentions which implement the <code>Extent</code> interface. </returns>
	  Mention[] getMentions(Parse parse);
	}

}