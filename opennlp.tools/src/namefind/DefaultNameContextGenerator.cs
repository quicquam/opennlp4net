using System;
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

namespace opennlp.tools.namefind
{


	using AdaptiveFeatureGenerator = opennlp.tools.util.featuregen.AdaptiveFeatureGenerator;
	using BigramNameFeatureGenerator = opennlp.tools.util.featuregen.BigramNameFeatureGenerator;
	using CachedFeatureGenerator = opennlp.tools.util.featuregen.CachedFeatureGenerator;
	using FeatureGeneratorUtil = opennlp.tools.util.featuregen.FeatureGeneratorUtil;
	using OutcomePriorFeatureGenerator = opennlp.tools.util.featuregen.OutcomePriorFeatureGenerator;
	using PreviousMapFeatureGenerator = opennlp.tools.util.featuregen.PreviousMapFeatureGenerator;
	using TokenClassFeatureGenerator = opennlp.tools.util.featuregen.TokenClassFeatureGenerator;
	using TokenFeatureGenerator = opennlp.tools.util.featuregen.TokenFeatureGenerator;
	using WindowFeatureGenerator = opennlp.tools.util.featuregen.WindowFeatureGenerator;

	/// <summary>
	/// Class for determining contextual features for a tag/chunk style
	/// named-entity recognizer.
	/// </summary>
	public class DefaultNameContextGenerator : NameContextGenerator
	{

	  private AdaptiveFeatureGenerator[] featureGenerators;

	  [Obsolete]
	  private static AdaptiveFeatureGenerator windowFeatures = new CachedFeatureGenerator(new AdaptiveFeatureGenerator[]{ new WindowFeatureGenerator(new TokenFeatureGenerator(), 2, 2), new WindowFeatureGenerator(new TokenClassFeatureGenerator(true), 2, 2), new OutcomePriorFeatureGenerator(), new PreviousMapFeatureGenerator(), new BigramNameFeatureGenerator()
	});

	  /// <summary>
	  /// Creates a name context generator. </summary>
	  /// @deprecated use the other constructor and always provide the feature generators 
	  [Obsolete("use the other constructor and always provide the feature generators")]
	  public DefaultNameContextGenerator()
	  {
		this((AdaptiveFeatureGenerator[]) null);
	  }

	  /// <summary>
	  /// Creates a name context generator with the specified cache size.
	  /// </summary>
	  public DefaultNameContextGenerator(AdaptiveFeatureGenerator featureGenerators)
	  {

		if (featureGenerators != null)
		{
		  this.featureGenerators = new[] {featureGenerators};
		}
		else
		{
		  // use defaults

		  this.featureGenerators = new AdaptiveFeatureGenerator[]{windowFeatures, new PreviousMapFeatureGenerator()};
		}
	  }

	  public void addFeatureGenerator(AdaptiveFeatureGenerator generator)
	  {
		  AdaptiveFeatureGenerator[] generators = featureGenerators;

		  featureGenerators = new AdaptiveFeatureGenerator[featureGenerators.Length + 1];

		  Array.Copy(generators, 0, featureGenerators, 0, generators.Length);

		  featureGenerators[featureGenerators.Length - 1] = generator;
	  }

	  public void updateAdaptiveData(string[] tokens, String[] outcomes)
	  {

		if (tokens != null && outcomes != null && tokens.length != outcomes.length)
		{
			throw new System.ArgumentException("The tokens and outcome arrays MUST have the same size!");
		}

		foreach (AdaptiveFeatureGenerator featureGenerator in featureGenerators)
		{
		  featureGenerator.updateAdaptiveData(tokens, outcomes);
		}
	  }

	  public void clearAdaptiveData()
	  {
		foreach (AdaptiveFeatureGenerator featureGenerator in featureGenerators)
		{
		  featureGenerator.clearAdaptiveData();
		}
	  }

	  /// <summary>
	  /// Return the context for finding names at the specified index. </summary>
	  /// <param name="index"> The index of the token in the specified toks array for which the context should be constructed. </param>
	  /// <param name="tokens"> The tokens of the sentence.  The <code>toString</code> methods of these objects should return the token text. </param>
	  /// <param name="preds"> The previous decisions made in the tagging of this sequence.  Only indices less than i will be examined. </param>
	  /// <param name="additionalContext"> Addition features which may be based on a context outside of the sentence.
	  /// </param>
	  /// <returns> the context for finding names at the specified index. </returns>
	  public string[] getContext(int index, String[] tokens, String[] preds, Object[] additionalContext)
	  {
		IList<string> features = new List<string>();

		foreach (AdaptiveFeatureGenerator featureGenerator in featureGenerators)
		{
		  featureGenerator.createFeatures(features, tokens, index, preds);
		}

		//previous outcome features
		string po = NameFinderME.OTHER;
		string ppo = NameFinderME.OTHER;

		if (index > 1)
		{
		  ppo = preds[index - 2];
		}

		if (index > 0)
		{
		  po = preds[index - 1];
		}
		features.Add("po=" + po);
		features.Add("pow=" + po + "," + tokens[index]);
		features.Add("powf=" + po + "," + FeatureGeneratorUtil.tokenFeature(tokens[index]));
		features.Add("ppo=" + ppo);

		return features.ToArray();
	  }
}
}