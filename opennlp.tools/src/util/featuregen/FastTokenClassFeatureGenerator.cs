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
using j4n.Lang;

namespace opennlp.tools.util.featuregen
{
    /// <summary>
    /// Generates features for different for the class of the token.
    /// </summary>
    /// @deprecated Use <seealso cref="TokenClassFeatureGenerator"/> instead! 
    [Obsolete("Use <seealso cref=\"TokenClassFeatureGenerator\"/> instead!")]
    public class FastTokenClassFeatureGenerator : FeatureGeneratorAdapter
    {
        private const string TOKEN_CLASS_PREFIX = "wc";
        private const string TOKEN_AND_CLASS_PREFIX = "w&c";

        private static Pattern capPeriod;

        static FastTokenClassFeatureGenerator()
        {
            capPeriod = Pattern.compile("^[A-Z]\\.$");
        }

        private bool generateWordAndClassFeature;


        public FastTokenClassFeatureGenerator() : this(false)
        {
        }

        public FastTokenClassFeatureGenerator(bool genearteWordAndClassFeature)
        {
            this.generateWordAndClassFeature = genearteWordAndClassFeature;
        }


        public static string tokenFeature(string token)
        {
            StringPattern pattern = StringPattern.recognize(token);

            string feat;
            if (pattern.AllLowerCaseLetter)
            {
                feat = "lc";
            }
            else if (pattern.digits() == 2)
            {
                feat = "2d";
            }
            else if (pattern.digits() == 4)
            {
                feat = "4d";
            }
            else if (pattern.containsDigit())
            {
                if (pattern.containsLetters())
                {
                    feat = "an";
                }
                else if (pattern.containsHyphen())
                {
                    feat = "dd";
                }
                else if (pattern.containsSlash())
                {
                    feat = "ds";
                }
                else if (pattern.containsComma())
                {
                    feat = "dc";
                }
                else if (pattern.containsPeriod())
                {
                    feat = "dp";
                }
                else
                {
                    feat = "num";
                }
            }
            else if (pattern.AllCapitalLetter && token.Length == 1)
            {
                feat = "sc";
            }
            else if (pattern.AllCapitalLetter)
            {
                feat = "ac";
            }
            else if (capPeriod.matcher(token).find())
            {
                feat = "cp";
            }
            else if (pattern.InitialCapitalLetter)
            {
                feat = "ic";
            }
            else
            {
                feat = "other";
            }

            return (feat);
        }


        public override void createFeatures(List<string> features, string[] tokens, int index, string[] preds)
        {
            string wordClass = tokenFeature(tokens[index]);
            features.Add(TOKEN_CLASS_PREFIX + "=" + wordClass);

            if (generateWordAndClassFeature)
            {
                features.Add(TOKEN_AND_CLASS_PREFIX + "=" + tokens[index].ToLower() + "," + wordClass);
            }
        }
    }
}