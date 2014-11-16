using System.Collections.Generic;
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

namespace opennlp.tools.sentdetect
{
    using IntegerPool = opennlp.maxent.IntegerPool;

    /// <summary>
    /// Default implementation of the <seealso cref="EndOfSentenceScanner"/>.
    /// It uses an character array with possible end of sentence chars
    /// to identify potential sentence endings.
    /// </summary>
    public class DefaultEndOfSentenceScanner : EndOfSentenceScanner
    {
        protected internal static readonly IntegerPool INT_POOL = new IntegerPool(500);

        private char[] eosCharacters;

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="eosCharacters"> </param>
        public DefaultEndOfSentenceScanner(char[] eosCharacters)
        {
            this.eosCharacters = eosCharacters;
        }

        public virtual IList<int?> getPositions(string s)
        {
            return getPositions(s.ToCharArray());
        }

        public virtual IList<int?> getPositions(StringBuilder buf)
        {
            return getPositions(buf.ToString().ToCharArray());
        }

        public virtual IList<int?> getPositions(char[] cbuf)
        {
            IList<int?> l = new List<int?>();
            char[] eosCharacters = EndOfSentenceCharacters;
            for (int i = 0; i < cbuf.Length; i++)
            {
                foreach (char eosCharacter in eosCharacters)
                {
                    if (cbuf[i] == eosCharacter)
                    {
                        l.Add(INT_POOL.get(i));
                        break;
                    }
                }
            }
            return l;
        }

        public virtual char[] EndOfSentenceCharacters
        {
            get { return eosCharacters; }
        }
    }
}