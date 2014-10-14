﻿/*
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

namespace opennlp.tools.namefind
{

	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// The interface for name finders which provide name tags for a sequence of tokens.
	/// </summary>
	public interface TokenNameFinder
	{

	  /// <summary>
	  /// Generates name tags for the given sequence, typically a sentence, returning token spans for any identified names. </summary>
	  /// <param name="tokens"> an array of the tokens or words of the sequence, typically a sentence. </param>
	  /// <returns> an array of spans for each of the names identified. </returns>
	  Span[] find(string[] tokens);

	  /// <summary>
	  /// Forgets all adaptive data which was collected during previous
	  /// calls to one of the find methods.
	  /// 
	  /// This method is typical called at the end of a document.
	  /// </summary>
	  void clearAdaptiveData();
	}

}