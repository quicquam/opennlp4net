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

namespace opennlp.tools.parser
{
    using POSSample = opennlp.tools.postag.POSSample;
    using opennlp.tools.util;
    using opennlp.tools.util;

    public class PosSampleStream : FilterObjectStream<Parse, POSSample>
    {
        public PosSampleStream(ObjectStream<Parse> @in) : base(@in)
        {
        }

        public override POSSample read()
        {
            Parse parse = samples.read();

            if (parse != null)
            {
                Parse[] nodes = parse.TagNodes;

                string[] toks = new string[nodes.Length];
                string[] preds = new string[nodes.Length];

                for (int ti = 0; ti < nodes.Length; ti++)
                {
                    Parse tok = nodes[ti];
                    toks[ti] = tok.CoveredText;
                    preds[ti] = tok.Type;
                }

                return new POSSample(toks, preds);
            }
            else
            {
                return null;
            }
        }
    }
}