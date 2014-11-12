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
using System.IO;
using System.Linq;
using j4n.IO.Reader;


namespace opennlp.tools.tokenize
{
    using Span = opennlp.tools.util.Span;
    using StringUtil = opennlp.tools.util.StringUtil;

    /// <summary>
    /// Performs tokenization using character classes.
    /// </summary>
    public class SimpleTokenizer : AbstractTokenizer
    {
        public static readonly SimpleTokenizer INSTANCE;

        static SimpleTokenizer()
        {
            INSTANCE = new SimpleTokenizer();
        }

        /// @deprecated Use INSTANCE field instead to obtain an instance, constructor
        /// will be made private in the future. 
        [Obsolete("Use INSTANCE field instead to obtain an instance, constructor")]
        public SimpleTokenizer()
        {
        }

        public override Span[] tokenizePos(string s)
        {
            CharacterEnum charType = CharacterEnum.WHITESPACE;
            CharacterEnum state = charType;

            IList<Span> tokens = new List<Span>();
            int sl = s.Length;
            int start = -1;
            char pc = (char) 0;
            for (int ci = 0; ci < sl; ci++)
            {
                char c = s[ci];
                if (StringUtil.isWhitespace(c))
                {
                    charType = CharacterEnum.WHITESPACE;
                }
                else if (char.IsLetter(c))
                {
                    charType = CharacterEnum.ALPHABETIC;
                }
                else if (char.IsDigit(c))
                {
                    charType = CharacterEnum.NUMERIC;
                }
                else
                {
                    charType = CharacterEnum.OTHER;
                }
                if (state == CharacterEnum.WHITESPACE)
                {
                    if (charType != CharacterEnum.WHITESPACE)
                    {
                        start = ci;
                    }
                }
                else
                {
                    if (charType != state || charType == CharacterEnum.OTHER && c != pc)
                    {
                        tokens.Add(new Span(start, ci));
                        start = ci;
                    }
                }
                state = charType;
                pc = c;
            }
            if (charType != CharacterEnum.WHITESPACE)
            {
                tokens.Add(new Span(start, sl));
            }
            return tokens.ToArray();
        }


        /// 
        /// <param name="args">
        /// </param>
        /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static void main(String[] args) throws java.io.IOException
        [Obsolete]
        public static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                Console.Error.WriteLine("Usage:  java opennlp.tools.tokenize.SimpleTokenizer < sentences");
                Environment.Exit(1);
            }
            opennlp.tools.tokenize.Tokenizer tokenizer = new SimpleTokenizer();
            BufferedReader inReader =
                new BufferedReader(new InputStreamReader(Console.OpenStandardInput(), "TODO Encoding"));
            for (string line = inReader.readLine(); line != null; line = inReader.readLine())
            {
                if (line.Equals(""))
                {
                    Console.WriteLine();
                }
                else
                {
                    string[] tokens = tokenizer.tokenize(line);
                    if (tokens.Length > 0)
                    {
                        Console.Write(tokens[0]);
                    }
                    for (int ti = 1, tn = tokens.Length; ti < tn; ti++)
                    {
                        Console.Write(" " + tokens[ti]);
                    }
                    Console.WriteLine();
                }
            }
        }
    }

    internal class CharacterEnum
    {
        internal static readonly CharacterEnum WHITESPACE = new CharacterEnum("whitespace");
        internal static readonly CharacterEnum ALPHABETIC = new CharacterEnum("alphabetic");
        internal static readonly CharacterEnum NUMERIC = new CharacterEnum("numeric");
        internal static readonly CharacterEnum OTHER = new CharacterEnum("other");

        private string name;

        private CharacterEnum(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}