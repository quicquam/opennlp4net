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

using j4n.Serialization;

namespace opennlp.tools.namefind
{
    using DataStream = opennlp.maxent.DataStream;
    using opennlp.tools.util;
    using opennlp.tools.util;

    /// <summary>
    /// The <seealso cref="NameSampleDataStream"/> class converts tagged <seealso cref="String"/>s
    /// provided by a <seealso cref="DataStream"/> to <seealso cref="NameSample"/> objects.
    /// It uses text that is is one-sentence per line and tokenized
    /// with names identified by <code>&lt;START&gt;</code> and <code>&lt;END&gt;</code> tags.
    /// </summary>
    public class NameSampleDataStream : FilterObjectStream<string, NameSample>
    {
        public const string START_TAG_PREFIX = "<START:";
        public const string START_TAG = "<START>";
        public const string END_TAG = "<END>";

        public NameSampleDataStream(ObjectStream<string> @in) : base(@in)
        {
        }

        public override NameSample read()
        {
            string token = samples.read();

            bool isClearAdaptiveData = false;

            // An empty line indicates the begin of a new article
            // for which the adaptive data in the feature generators
            // must be cleared
            while (token != null && token.Trim().Length == 0)
            {
                isClearAdaptiveData = true;
                token = samples.read();
            }

            if (token != null)
            {
                return NameSample.parse(token, isClearAdaptiveData);
            }
            else
            {
                return null;
            }
        }
    }
}