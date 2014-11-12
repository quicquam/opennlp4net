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

namespace opennlp.tools.tokenize
{
    using Span = opennlp.tools.util.Span;
    using StringUtil = opennlp.tools.util.StringUtil;

    /// <summary>
    /// This tokenizer uses white spaces to tokenize the input text.
    /// 
    /// To obtain an instance of this tokenizer use the static final 
    /// <code>INSTANCE</code> field.
    /// </summary>
    public class WhitespaceTokenizer : AbstractTokenizer
    {
        /// <summary>
        /// Use this static reference to retrieve an instance of the
        /// <seealso cref="WhitespaceTokenizer"/>.
        /// </summary>
        public static readonly WhitespaceTokenizer INSTANCE = new WhitespaceTokenizer();

        /// <summary>
        /// Use the <seealso cref="WhitespaceTokenizer#INSTANCE"/> field to retrieve an instance.
        /// </summary>
        private WhitespaceTokenizer()
        {
        }

        public override Span[] tokenizePos(string d)
        {
            int tokStart = -1;
            var tokens = new List<Span>();
            bool inTok = false;

            //gather up potential tokens
            int end = d.Length;
            for (int i = 0; i < end; i++)
            {
                if (StringUtil.isWhitespace(d[i]))
                {
                    if (inTok)
                    {
                        tokens.Add(new Span(tokStart, i));
                        inTok = false;
                        tokStart = -1;
                    }
                }
                else
                {
                    if (!inTok)
                    {
                        tokStart = i;
                        inTok = true;
                    }
                }
            }

            if (inTok)
            {
                tokens.Add(new Span(tokStart, end));
            }

            return tokens.ToArray();
        }
    }
}