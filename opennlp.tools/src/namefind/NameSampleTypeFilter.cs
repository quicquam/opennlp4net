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
using opennlp.nonjava.helperclasses;

namespace opennlp.tools.namefind
{
    using opennlp.tools.util;
    using opennlp.tools.util;
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// A stream which removes Name Samples which do not have a certain type.
    /// </summary>
    public class NameSampleTypeFilter : FilterObjectStream<NameSample, NameSample>
    {
        private readonly HashSet<string> types;

        public NameSampleTypeFilter(string[] types, ObjectStream<NameSample> samples) : base(samples)
        {
            this.types = new HashSet<string>(types);
        }

        public NameSampleTypeFilter(HashSet<string> types, ObjectStream<NameSample> samples) : base(samples)
        {
            this.types = new HashSet<string>(types);
        }

        public override NameSample read()
        {
            NameSample sample = samples.read();

            if (sample != null)
            {
                IList<Span> filteredNames = new List<Span>();

                foreach (Span name in sample.Names)
                {
                    if (types.Contains(name.Type))
                    {
                        filteredNames.Add(name);
                    }
                }

                return new NameSample(sample.Sentence, filteredNames.ToArray(), sample.ClearAdaptiveDataSet);
            }
            else
            {
                return null;
            }
        }
    }
}