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
using j4n.Serialization;

namespace opennlp.tools.tokenize
{
    using opennlp.tools.util;
    using opennlp.tools.util;
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// This stream formats a <seealso cref="TokenSample"/>s into whitespace
    /// separated token strings.
    /// </summary>
    public class WhitespaceTokenStream : FilterObjectStream<TokenSample, string>
    {
        public WhitespaceTokenStream(ObjectStream<TokenSample> tokens) : base(tokens)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String read() throws java.io.IOException
        public override string read()
        {
            TokenSample tokenSample = samples.read();

            if (tokenSample != null)
            {
                StringBuilder whitespaceSeparatedTokenString = new StringBuilder();

                foreach (Span token in tokenSample.TokenSpans)
                {
                    whitespaceSeparatedTokenString.Append(token.getCoveredText(tokenSample.Text));
                    whitespaceSeparatedTokenString.Append(' ');
                }

                // Shorten string by one to get rid of last space
                if (whitespaceSeparatedTokenString.Length > 0)
                {
                    whitespaceSeparatedTokenString.Length = whitespaceSeparatedTokenString.Length - 1;
                }

                return whitespaceSeparatedTokenString.ToString();
            }

            return null;
        }
    }
}