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
    /// This feature generator creates sentence begin and end features.
    /// </summary>
    public class SentenceFeatureGenerator : FeatureGeneratorAdapter
    {
        private readonly bool isGenerateFirstWordFeature;
        private readonly bool isGenerateLastWordFeature;

        public SentenceFeatureGenerator(bool isGenerateFirstWordFeature, bool isGenerateLastWordFeature)
        {
            this.isGenerateFirstWordFeature = isGenerateFirstWordFeature;
            this.isGenerateLastWordFeature = isGenerateLastWordFeature;
        }

        public override void createFeatures(List<string> features, string[] tokens, int index, string[] previousOutcomes)
        {
            if (isGenerateFirstWordFeature && index == 0)
            {
                features.Add("S=begin");
            }

            if (isGenerateLastWordFeature && tokens.Length == index + 1)
            {
                features.Add("S=end");
            }
        }
    }
}