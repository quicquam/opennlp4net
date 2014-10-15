﻿using System.Collections.Generic;

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


namespace opennlp.tools.util.featuregen
{

	/// <summary>
	/// An interface for generating features for name entity identification and for
	/// updating document level contexts.
	/// <para>
	/// Most implementors do not need the adaptive functionality of this
	/// interface, they should extend the <seealso cref="FeatureGeneratorAdapter"/> class instead.
	/// </para>
	/// <para>
	/// <b>Note:</b><br>
	/// Feature generation is not thread safe and a instance of a feature generator
	/// must only be called from one thread. The resources used by a feature
	/// generator are typically shared between man instances of features generators
	/// which are called from many threads and have to be thread safe.
	/// If that is not possible the <seealso cref="FeatureGeneratorFactory"/> must make a copy
	/// of the resource object for each feature generator instance.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= FeatureGeneratorAdapter </seealso>
	/// <seealso cref= FeatureGeneratorFactory </seealso>
	public interface AdaptiveFeatureGenerator
	{

	  /// <summary>
	  /// Adds the appropriate features for the token at the specified index with the
	  /// specified array of previous outcomes to the specified list of features.
	  /// </summary>
	  /// <param name="features"> The list of features to be added to. </param>
	  /// <param name="tokens"> The tokens of the sentence or other text unit being processed. </param>
	  /// <param name="index"> The index of the token which is currently being processed. </param>
	  /// <param name="previousOutcomes"> The outcomes for the tokens prior to the specified index. </param>
	  void createFeatures(List<string> features, string[] tokens, int index, string[] previousOutcomes);

	  /// <summary>
	  /// Informs the feature generator that the specified tokens have been classified with the
	  /// corresponding set of specified outcomes.
	  /// </summary>
	  /// <param name="tokens"> The tokens of the sentence or other text unit which has been processed. </param>
	  /// <param name="outcomes"> The outcomes associated with the specified tokens. </param>
	   void updateAdaptiveData(string[] tokens, string[] outcomes);

	  /// <summary>
	  /// Informs the feature generator that the context of the adaptive data (typically a document)
	  /// is no longer valid.
	  /// </summary>
	   void clearAdaptiveData();
	}

}