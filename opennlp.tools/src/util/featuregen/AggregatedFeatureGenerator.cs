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


namespace opennlp.tools.util.featuregen
{


	/// <summary>
	/// The <seealso cref="AggregatedFeatureGenerator"/> aggregates a set of
	/// <seealso cref="AdaptiveFeatureGenerator"/>s and calls them to generate the features.
	/// </summary>
	public class AggregatedFeatureGenerator : AdaptiveFeatureGenerator
	{

	  /// <summary>
	  /// Contains all aggregated <seealso cref="AdaptiveFeatureGenerator"/>s.
	  /// </summary>
	  private ICollection<AdaptiveFeatureGenerator> generators;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="generators"> array of generators, null values are not permitted </param>
	  public AggregatedFeatureGenerator(params AdaptiveFeatureGenerator[] generators)
	  {

		foreach (AdaptiveFeatureGenerator generator in generators)
		{
		  if (generator == null)
		  {
			throw new System.ArgumentException("null values in generators are not permitted!");
		  }
		}

		this.generators = new List<AdaptiveFeatureGenerator>(generators.Length);

		Collections.addAll(this.generators, generators);

		this.generators = Collections.unmodifiableCollection(this.generators);
	  }

	  public AggregatedFeatureGenerator(ICollection<AdaptiveFeatureGenerator> generators) : this(generators.toArray(new AdaptiveFeatureGenerator[generators.Count]))
	  {
	  }

	  /// <summary>
	  /// Calls the <seealso cref="AdaptiveFeatureGenerator#clearAdaptiveData()"/> method
	  /// on all aggregated <seealso cref="AdaptiveFeatureGenerator"/>s.
	  /// </summary>
	  public virtual void clearAdaptiveData()
	  {

		foreach (AdaptiveFeatureGenerator generator in generators)
		{
		  generator.clearAdaptiveData();
		}
	  }

	  /// <summary>
	  /// Calls the <seealso cref="AdaptiveFeatureGenerator#createFeatures(List, String[], int, String[])"/>
	  /// method on all aggregated <seealso cref="AdaptiveFeatureGenerator"/>s.
	  /// </summary>
	  public virtual void createFeatures(List<string> features, string[] tokens, int index, string[] previousOutcomes)
	  {

		foreach (AdaptiveFeatureGenerator generator in generators)
		{
		  generator.createFeatures(features, tokens, index, previousOutcomes);
		}
	  }

	  /// <summary>
	  /// Calls the <seealso cref="AdaptiveFeatureGenerator#updateAdaptiveData(String[], String[])"/>
	  /// method on all aggregated <seealso cref="AdaptiveFeatureGenerator"/>s.
	  /// </summary>
	  public virtual void updateAdaptiveData(string[] tokens, string[] outcomes)
	  {

		foreach (AdaptiveFeatureGenerator generator in generators)
		{
		  generator.updateAdaptiveData(tokens, outcomes);
		}
	  }

	  /// <summary>
	  /// Retrieves a <seealso cref="Collections"/> of all aggregated
	  /// <seealso cref="AdaptiveFeatureGenerator"/>s.
	  /// </summary>
	  /// <returns> all aggregated generators </returns>
	  public virtual ICollection<AdaptiveFeatureGenerator> Generators
	  {
		  get
		  {
			return generators;
		  }
	  }
	}

}