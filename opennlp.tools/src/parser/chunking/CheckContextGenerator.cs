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

namespace opennlp.tools.parser.chunking
{
    /// <summary>
    /// Class for generating predictive context for deciding when a constituent is complete.
    /// </summary>
    public class CheckContextGenerator : AbstractContextGenerator
    {
        /// <summary>
        /// Creates a new context generator for generating predictive context for deciding when a constituent is complete.
        /// </summary>
        public CheckContextGenerator() : base()
        {
        }

        public virtual string[] getContext(object o)
        {
            object[] parameters = (object[]) o;
            return getContext((Parse[]) parameters[0], (string) parameters[1], (int) parameters[2], (int) parameters[3]);
        }

        /// <summary>
        /// Returns predictive context for deciding whether the specified constituents between the specified start and end index
        /// can be combined to form a new constituent of the specified type. </summary>
        /// <param name="constituents"> The constituents which have yet to be combined into new constituents. </param>
        /// <param name="type"> The type of the new constituent proposed. </param>
        /// <param name="start"> The first constituent of the proposed constituent. </param>
        /// <param name="end"> The last constituent of the proposed constituent. </param>
        /// <returns> The predictive context for deciding whether a new constituent should be created. </returns>
        public virtual string[] getContext(Parse[] constituents, string type, int start, int end)
        {
            int ps = constituents.Length;
            IList<string> features = new List<string>(100);

            //default
            features.Add("default");
            //first constituent label
            features.Add("fl=" + constituents[0].Label);
            Parse pstart = constituents[start];
            Parse pend = constituents[end];
            checkcons(pstart, "begin", type, features);
            checkcons(pend, "last", type, features);
            StringBuilder production = new StringBuilder(20);
            StringBuilder punctProduction = new StringBuilder(20);
            production.Append("p=").Append(type).Append("->");
            punctProduction.Append("pp=").Append(type).Append("->");
            for (int pi = start; pi < end; pi++)
            {
                Parse p = constituents[pi];
                checkcons(p, pend, type, features);
                production.Append(p.Type).Append(",");
                punctProduction.Append(p.Type).Append(",");
                ICollection<Parse> nextPunct = p.NextPunctuationSet;
                if (nextPunct != null)
                {
                    for (IEnumerator<Parse> pit = nextPunct.GetEnumerator(); pit.MoveNext();)
                    {
                        Parse punct = pit.Current;
                        punctProduction.Append(punct.Type).Append(",");
                    }
                }
            }
            production.Append(pend.Type);
            punctProduction.Append(pend.Type);
            features.Add(production.ToString());
            features.Add(punctProduction.ToString());
            Parse p_2 = null;
            Parse p_1 = null;
            Parse p1 = null;
            Parse p2 = null;
            ICollection<Parse> p1s = constituents[end].NextPunctuationSet;
            ICollection<Parse> p2s = null;
            ICollection<Parse> p_1s = constituents[start].PreviousPunctuationSet;
            ICollection<Parse> p_2s = null;
            if (start - 2 >= 0)
            {
                p_2 = constituents[start - 2];
            }
            if (start - 1 >= 0)
            {
                p_1 = constituents[start - 1];
                p_2s = p_1.PreviousPunctuationSet;
            }
            if (end + 1 < ps)
            {
                p1 = constituents[end + 1];
                p2s = p1.NextPunctuationSet;
            }
            if (end + 2 < ps)
            {
                p2 = constituents[end + 2];
            }
            surround(p_1, -1, type, p_1s, features);
            surround(p_2, -2, type, p_2s, features);
            surround(p1, 1, type, p1s, features);
            surround(p2, 2, type, p2s, features);

            return features.ToArray();
        }
    }
}