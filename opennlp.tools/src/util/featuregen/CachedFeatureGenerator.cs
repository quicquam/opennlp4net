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
    /// Caches features of the aggregated <seealso cref="AdaptiveFeatureGenerator"/>s.
    /// </summary>
    public class CachedFeatureGenerator : AdaptiveFeatureGenerator
    {
        private readonly AdaptiveFeatureGenerator generator;

        private string[] prevTokens;

        private Cache contextsCache;

        private long numberOfCacheHits;
        private long numberOfCacheMisses;

        public CachedFeatureGenerator(params AdaptiveFeatureGenerator[] generators)
        {
            this.generator = new AggregatedFeatureGenerator(generators);
            contextsCache = new Cache(100);
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public void createFeatures(java.util.List<String> features, String[] tokens, int index, String[] previousOutcomes)
        public virtual void createFeatures(List<string> features, string[] tokens, int index, string[] previousOutcomes)
        {
            List<string> cacheFeatures;

            if (tokens == prevTokens)
            {
                cacheFeatures = (List<string>) contextsCache.get(index);

                if (cacheFeatures != null)
                {
                    numberOfCacheHits++;
                    features.AddRange(cacheFeatures);
                    return;
                }
            }
            else
            {
                contextsCache.Clear();
                prevTokens = tokens;
            }

            cacheFeatures = new List<string>();

            numberOfCacheMisses++;

            generator.createFeatures(cacheFeatures, tokens, index, previousOutcomes);

            contextsCache.put(index, cacheFeatures);
            features.AddRange(cacheFeatures);
        }

        public virtual void updateAdaptiveData(string[] tokens, string[] outcomes)
        {
            generator.updateAdaptiveData(tokens, outcomes);
        }

        public virtual void clearAdaptiveData()
        {
            generator.clearAdaptiveData();
        }

        /// <summary>
        /// Retrieves the number of times a cache hit occurred.
        /// </summary>
        /// <returns> number of cache hits </returns>
        public virtual long NumberOfCacheHits
        {
            get { return numberOfCacheHits; }
        }

        /// <summary>
        /// Retrieves the number of times a cache miss occurred.
        /// </summary>
        /// <returns> number of cache misses </returns>
        public virtual long NumberOfCacheMisses
        {
            get { return numberOfCacheMisses; }
        }

        public override string ToString()
        {
            return base.ToString() + ": hits=" + numberOfCacheHits + " misses=" + numberOfCacheMisses + " hit%" +
                   (numberOfCacheHits > 0 ? (double) numberOfCacheHits/(numberOfCacheMisses + numberOfCacheHits) : 0);
        }
    }
}