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
using System.Linq;
using j4n.Serialization;

namespace opennlp.tools.chunker
{
    using opennlp.tools.util;
    using opennlp.tools.util;

    /// <summary>
    /// Parses the conll 2000 shared task shallow parser training data.
    /// <para>
    /// Data format is specified on the conll page:<br>
    /// <a hraf="http://www.cnts.ua.ac.be/conll2000/chunking/">
    /// http://www.cnts.ua.ac.be/conll2000/chunking/</a>
    /// </para>
    /// </summary>
    public class ChunkSampleStream : FilterObjectStream<string, ChunkSample>
    {
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="samples"> a plain text line stream </param>
        public ChunkSampleStream(ObjectStream<string> samples) : base(samples)
        {
        }

        public override ChunkSample read()
        {
            IList<string> toks = new List<string>();
            IList<string> tags = new List<string>();
            IList<string> preds = new List<string>();

            for (string line = samples.read(); line != null && !line.Equals(""); line = samples.read())
            {
                string[] parts = line.Split(' ');
                if (parts.Length != 3)
                {
                    Console.Error.WriteLine("Skipping corrupt line: " + line);
                }
                else
                {
                    toks.Add(parts[0]);
                    tags.Add(parts[1]);
                    preds.Add(parts[2]);
                }
            }

            if (toks.Count > 0)
            {
                return new ChunkSample(toks.ToArray(), tags.ToArray(), preds.ToArray());
            }
            else
            {
                return null;
            }
        }        
    }
}