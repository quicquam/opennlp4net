using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using j4n.Lang;


namespace opennlp.tools.namefind
{
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Name finder based on a series of regular expressions.
    /// </summary>
    public sealed class RegexNameFinder : TokenNameFinder
    {
        private readonly Pattern[] mPatterns;
        private readonly string sType;

        public RegexNameFinder(Pattern[] patterns, string type)
        {
            if (patterns == null || patterns.Length == 0)
            {
                throw new System.ArgumentException("patterns must not be null or empty!");
            }

            mPatterns = patterns;
            sType = type;
        }

        public RegexNameFinder(Pattern[] patterns)
        {
            if (patterns == null || patterns.Length == 0)
            {
                throw new System.ArgumentException("patterns must not be null or empty!");
            }

            mPatterns = patterns;
            sType = null;
        }

        public Span[] find(string[] tokens)
        {
            IDictionary<int?, int?> sentencePosTokenMap = new Dictionary<int?, int?>();

            StringBuilder sentenceString = new StringBuilder(tokens.Length*10);

            for (int i = 0; i < tokens.Length; i++)
            {
                int startIndex = sentenceString.Length;
                sentencePosTokenMap[startIndex] = i;

                sentenceString.Append(tokens[i]);

                int endIndex = sentenceString.Length;
                sentencePosTokenMap[endIndex] = i + 1;

                if (i < tokens.Length - 1)
                {
                    sentenceString.Append(' ');
                }
            }

            ICollection<Span> annotations = new LinkedList<Span>();

            foreach (Pattern mPattern in mPatterns)
            {
                Matcher matcher = mPattern.matcher(sentenceString);

                while (matcher.find())
                {
                    int? tokenStartIndex = sentencePosTokenMap[matcher.start()];
                    int? tokenEndIndex = sentencePosTokenMap[matcher.end()];

                    if (tokenStartIndex != null && tokenEndIndex != null)
                    {
                        Span annotation = new Span(tokenStartIndex.Value, tokenEndIndex.Value, sType);
                        annotations.Add(annotation);
                    }
                }
            }

            return annotations.ToArray();
        }

        public void clearAdaptiveData()
        {
            // nothing to clear
        }
    }
}