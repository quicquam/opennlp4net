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

namespace opennlp.tools.doccat
{
    using StringPattern = opennlp.tools.util.featuregen.StringPattern;

    /// <summary>
    /// Generates a feature for each word in a document.
    /// </summary>
    public class BagOfWordsFeatureGenerator : FeatureGenerator
    {
        private bool useOnlyAllLetterTokens = false;

        public BagOfWordsFeatureGenerator()
        {
        }

        internal BagOfWordsFeatureGenerator(bool useOnlyAllLetterTokens)
        {
            this.useOnlyAllLetterTokens = useOnlyAllLetterTokens;
        }

        public virtual ICollection<string> extractFeatures(string[] text)
        {
            ICollection<string> bagOfWords = new List<string>(text.Length);

            foreach (string word in text)
            {
                if (useOnlyAllLetterTokens)
                {
                    StringPattern pattern = StringPattern.recognize(word);

                    if (pattern.AllLetter)
                    {
                        bagOfWords.Add("bow=" + word);
                    }
                }
                else
                {
                    bagOfWords.Add("bow=" + word);
                }
            }

            return bagOfWords;
        }
    }
}