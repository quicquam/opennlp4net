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

	using NGramModel = opennlp.tools.ngram.NGramModel;

	/// <summary>
	/// The <seealso cref="CharacterNgramFeatureGenerator"/> uses character ngrams to
	/// generate features about each token.
	/// The minimum and maximum length can be specified.
	/// </summary>
	public class CharacterNgramFeatureGenerator : FeatureGeneratorAdapter
	{

	  private readonly int minLength;
	  private readonly int maxLength;

	  public CharacterNgramFeatureGenerator(int minLength, int maxLength)
	  {
		this.minLength = minLength;
		this.maxLength = maxLength;
	  }

	  /// <summary>
	  /// Initializes the current instance with min 2 length and max 5 length of ngrams.
	  /// </summary>
	  public CharacterNgramFeatureGenerator() : this(2, 5)
	  {
	  }

	  public override void createFeatures(List<string> features, string[] tokens, int index, string[] preds)
	  {

		NGramModel model = new NGramModel();
		model.add(tokens[index], minLength, maxLength);

		foreach (StringList tokenList in model)
		{

		  if (tokenList.size() > 0)
		  {
			features.Add("ng=" + tokenList.getToken(0).ToLower());
		  }
		}
	  }
	}

}