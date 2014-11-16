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

namespace opennlp.tools.parser
{
    using ChunkerContextGenerator = opennlp.tools.chunker.ChunkerContextGenerator;
    using Cache = opennlp.tools.util.Cache;

    /// <summary>
    /// Creates predivtive context for the pre-chunking phases of parsing.
    /// </summary>
    public class ChunkContextGenerator : ChunkerContextGenerator
    {
        private const string EOS = "eos";
        private Cache contextsCache;
        private object wordsKey;


        public ChunkContextGenerator() : this(0)
        {
        }

        public ChunkContextGenerator(int cacheSize) : base()
        {
            if (cacheSize > 0)
            {
                contextsCache = new Cache(cacheSize);
            }
        }

        public virtual string[] getContext(object o)
        {
            object[] data = (object[]) o;
            return getContext((int) ((int?) data[0]), (string[]) data[1], (string[]) data[2], (string[]) data[3]);
        }

        public virtual string[] getContext(int i, string[] words, string[] prevDecisions, object[] ac)
        {
            return getContext(i, words, (string[]) ac[0], prevDecisions);
        }

        public virtual string[] getContext(int i, string[] words, string[] tags, string[] preds)
        {
            IList<string> features = new List<string>(19);
            int x0 = i;
            int x_2 = x0 - 2;
            int x_1 = x0 - 1;
            int x2 = x0 + 2;
            int x1 = x0 + 1;

            string w_2, w_1, w0, w1, w2;
            string t_2, t_1, t0, t1, t2;
            string p_2, p_1;

            // chunkandpostag(-2)
            if (x_2 >= 0)
            {
                t_2 = tags[x_2];
                p_2 = preds[x_2];
                w_2 = words[x_2];
            }
            else
            {
                t_2 = EOS;
                p_2 = EOS;
                w_2 = EOS;
            }

            // chunkandpostag(-1)
            if (x_1 >= 0)
            {
                t_1 = tags[x_1];
                p_1 = preds[x_1];
                w_1 = words[x_1];
            }
            else
            {
                t_1 = EOS;
                p_1 = EOS;
                w_1 = EOS;
            }

            // chunkandpostag(0)
            t0 = tags[x0];
            w0 = words[x0];

            // chunkandpostag(1)
            if (x1 < tags.Length)
            {
                t1 = tags[x1];
                w1 = words[x1];
            }
            else
            {
                t1 = EOS;
                w1 = EOS;
            }

            // chunkandpostag(2)
            if (x2 < tags.Length)
            {
                t2 = tags[x2];
                w2 = words[x2];
            }
            else
            {
                t2 = EOS;
                w2 = EOS;
            }

            string cacheKey = x0 + t_2 + t1 + t0 + t1 + t2 + p_2 + p_1;
            if (contextsCache != null)
            {
                if (wordsKey == words)
                {
                    string[] cContexts = (string[]) contextsCache[cacheKey];
                    if (cContexts != null)
                    {
                        return cContexts;
                    }
                }
                else
                {
                    contextsCache.Clear();
                    wordsKey = words;
                }
            }

            string ct_2 = chunkandpostag(-2, w_2, t_2, p_2);
            string ctbo_2 = chunkandpostagbo(-2, t_2, p_2);
            string ct_1 = chunkandpostag(-1, w_1, t_1, p_1);
            string ctbo_1 = chunkandpostagbo(-1, t_1, p_1);
            string ct0 = chunkandpostag(0, w0, t0, null);
            string ctbo0 = chunkandpostagbo(0, t0, null);
            string ct1 = chunkandpostag(1, w1, t1, null);
            string ctbo1 = chunkandpostagbo(1, t1, null);
            string ct2 = chunkandpostag(2, w2, t2, null);
            string ctbo2 = chunkandpostagbo(2, t2, null);

            features.Add("default");
            features.Add(ct_2);
            features.Add(ctbo_2);
            features.Add(ct_1);
            features.Add(ctbo_1);
            features.Add(ct0);
            features.Add(ctbo0);
            features.Add(ct1);
            features.Add(ctbo1);
            features.Add(ct2);
            features.Add(ctbo2);

            //chunkandpostag(-1,0)
            features.Add(ct_1 + "," + ct0);
            features.Add(ctbo_1 + "," + ct0);
            features.Add(ct_1 + "," + ctbo0);
            features.Add(ctbo_1 + "," + ctbo0);

            //chunkandpostag(0,1)
            features.Add(ct0 + "," + ct1);
            features.Add(ctbo0 + "," + ct1);
            features.Add(ct0 + "," + ctbo1);
            features.Add(ctbo0 + "," + ctbo1);
            string[] contexts = features.ToArray();
            if (contextsCache != null)
            {
                contextsCache[cacheKey] = contexts;
            }
            return (contexts);
        }

        private string chunkandpostag(int i, string tok, string tag, string chunk)
        {
            StringBuilder feat = new StringBuilder(20);
            feat.Append(i).Append("=").Append(tok).Append("|").Append(tag);
            if (i < 0)
            {
                feat.Append("|").Append(chunk);
            }
            return (feat.ToString());
        }

        private string chunkandpostagbo(int i, string tag, string chunk)
        {
            StringBuilder feat = new StringBuilder(20);
            feat.Append(i).Append("*=").Append(tag);
            if (i < 0)
            {
                feat.Append("|").Append(chunk);
            }
            return (feat.ToString());
        }
    }
}