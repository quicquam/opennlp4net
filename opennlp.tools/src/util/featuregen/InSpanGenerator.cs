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
    using TokenNameFinder = opennlp.tools.namefind.TokenNameFinder;

    /// <summary>
    /// Generates features if the tokens are recognized by the provided
    /// <seealso cref="TokenNameFinder"/>.
    /// </summary>
    public class InSpanGenerator : FeatureGeneratorAdapter
    {
        private readonly string prefix;

        private readonly TokenNameFinder finder;

        private string[] currentSentence;

        private Span[] currentNames;

        /// <summary>
        /// Initializes the current instance. 
        /// </summary>
        /// <param name="prefix"> the prefix is used to distinguish the generated features
        /// from features generated by other instances of <seealso cref="InSpanGenerator"/>s. </param>
        /// <param name="finder"> the <seealso cref="TokenNameFinder"/> used to detect the names. </param>
        public InSpanGenerator(string prefix, TokenNameFinder finder)
        {
            if (prefix == null)
            {
                throw new System.ArgumentException("prefix must not be null!");
            }

            this.prefix = prefix;

            if (finder == null)
            {
                throw new System.ArgumentException("finder must not be null!");
            }

            this.finder = finder;
        }

        public override void createFeatures(List<string> features, string[] tokens, int index, string[] preds)
        {
            // cache results for sentence
            if (currentSentence != tokens)
            {
                currentSentence = tokens;
                currentNames = finder.find(tokens);
            }

            // iterate over names and check if a span is contained
            foreach (Span currentName in currentNames)
            {
                if (currentName.contains(index))
                {
                    // found a span for the current token
                    features.Add(prefix + ":w=dic");
                    features.Add(prefix + ":w=dic=" + tokens[index]);

                    // TODO: consider generation start and continuation features

                    break;
                }
            }
        }
    }
}