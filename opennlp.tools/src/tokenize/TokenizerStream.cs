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
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// The <seealso cref="TokenizerStream"/> uses a tokenizer to tokenize the
    /// input string and output <seealso cref="TokenSample"/>s.
    /// </summary>
    public class TokenizerStream : ObjectStream<TokenSample>
    {
        private Tokenizer tokenizer;
        private ObjectStream<string> input;

        public TokenizerStream(Tokenizer tokenizer, ObjectStream<string> input)
        {
            this.tokenizer = tokenizer;
            this.input = input;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TokenSample read() throws java.io.IOException
        public virtual TokenSample read()
        {
            string inputString = input.read();

            if (inputString != null)
            {
                Span[] tokens = tokenizer.tokenizePos(inputString);

                return new TokenSample(inputString, tokens);
            }

            return null;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
        public virtual void close()
        {
            input.close();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws java.io.IOException, UnsupportedOperationException
        public virtual void reset()
        {
            input.reset();
        }
    }
}