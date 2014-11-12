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
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Class used to hold constituents when reading parses.
    /// </summary>
    public class Constituent
    {
        private string label;
        private Span span;

        public Constituent(string label, Span span)
        {
            this.label = label;
            this.span = span;
        }


        /// <summary>
        /// Returns the label of the constituent. </summary>
        /// <returns> the label of the constituent. </returns>
        public virtual string Label
        {
            get { return label; }
            set { this.label = value; }
        }


        /// <summary>
        /// Returns the span of the constituent. </summary>
        /// <returns> the span of the constituent. </returns>
        public virtual Span Span
        {
            get { return span; }
        }
    }
}