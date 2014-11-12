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

using System;

namespace opennlp.tools.util.featuregen
{
    /// <summary>
    /// Recognizes predefined patterns in strings.
    /// </summary>
    public class StringPattern
    {
        private const int INITAL_CAPITAL_LETTER = 0x1;
        private static readonly int ALL_CAPITAL_LETTER = 0x1 << 1;
        private static readonly int ALL_LOWERCASE_LETTER = 0x1 << 2;
        private static readonly int ALL_LETTERS = 0x1 << 3;
        private static readonly int ALL_DIGIT = 0x1 << 4;
        private static readonly int CONTAINS_PERIOD = 0x1 << 5;
        private static readonly int CONTAINS_COMMA = 0x1 << 6;
        private static readonly int CONTAINS_SLASH = 0x1 << 7;
        private static readonly int CONTAINS_DIGIT = 0x1 << 8;
        private static readonly int CONTAINS_HYPHEN = 0x1 << 9;
        private static readonly int CONTAINS_LETTERS = 0x1 << 10;
        private static readonly int CONTAINS_UPPERCASE = 0x1 << 11;

        private readonly int pattern;

        private readonly int digits_Renamed;

        private StringPattern(int pattern, int digits)
        {
            this.pattern = pattern;
            this.digits_Renamed = digits;
        }

        /// <returns> true if all characters are letters. </returns>
        public virtual bool AllLetter
        {
            get { return (pattern & ALL_LETTERS) > 0; }
        }

        /// <returns> true if first letter is capital. </returns>
        public virtual bool InitialCapitalLetter
        {
            get { return (pattern & INITAL_CAPITAL_LETTER) > 0; }
        }

        /// <returns> true if all letters are capital. </returns>
        public virtual bool AllCapitalLetter
        {
            get { return (pattern & ALL_CAPITAL_LETTER) > 0; }
        }

        /// <returns> true if all letters are lower case. </returns>
        public virtual bool AllLowerCaseLetter
        {
            get { return (pattern & ALL_LOWERCASE_LETTER) > 0; }
        }

        /// <returns> true if all chars are digits. </returns>
        public virtual bool AllDigit
        {
            get { return (pattern & ALL_DIGIT) > 0; }
        }

        /// <summary>
        /// Retrieves the number of digits.
        /// </summary>
        public virtual int digits()
        {
            return digits_Renamed;
        }

        public virtual bool containsPeriod()
        {
            return (pattern & CONTAINS_PERIOD) > 0;
        }

        public virtual bool containsComma()
        {
            return (pattern & CONTAINS_COMMA) > 0;
        }

        public virtual bool containsSlash()
        {
            return (pattern & CONTAINS_SLASH) > 0;
        }

        public virtual bool containsDigit()
        {
            return (pattern & CONTAINS_DIGIT) > 0;
        }

        public virtual bool containsHyphen()
        {
            return (pattern & CONTAINS_HYPHEN) > 0;
        }

        public virtual bool containsLetters()
        {
            return (pattern & CONTAINS_LETTERS) > 0;
        }

        public static StringPattern recognize(string token)
        {
            int pattern = ALL_CAPITAL_LETTER | ALL_LOWERCASE_LETTER | ALL_DIGIT | ALL_LETTERS;

            int digits = 0;

            for (int i = 0; i < token.Length; i++)
            {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final char ch = token.charAt(i);
                Char ch = token[i];

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int letterType = Character.getType(ch);

                bool isLetter = Char.IsUpper(ch) || Char.IsLower(ch);
                    // || letterType == char.TITLECASE_LETTER || letterType == char.MODIFIER_LETTER || letterType == char.OTHER_LETTER;

                if (isLetter)
                {
                    pattern |= CONTAINS_LETTERS;
                    pattern &= ~ALL_DIGIT;

                    if (Char.IsUpper(ch))
                    {
                        if (i == 0)
                        {
                            pattern |= INITAL_CAPITAL_LETTER;
                        }

                        pattern |= CONTAINS_UPPERCASE;

                        pattern &= ~ALL_LOWERCASE_LETTER;
                    }
                    else
                    {
                        pattern &= ~ALL_CAPITAL_LETTER;
                    }
                }
                else
                {
                    // contains chars other than letter, this means
                    // it can not be one of these:
                    pattern &= ~ALL_LETTERS;
                    pattern &= ~ALL_CAPITAL_LETTER;
                    pattern &= ~ALL_LOWERCASE_LETTER;

                    if (Char.IsDigit(ch))
                    {
                        pattern |= CONTAINS_DIGIT;
                        digits++;
                    }
                    else
                    {
                        pattern &= ~ALL_DIGIT;
                    }

                    switch (ch)
                    {
                        case ',':
                            pattern |= CONTAINS_COMMA;
                            break;

                        case '.':
                            pattern |= CONTAINS_PERIOD;
                            break;

                        case '/':
                            pattern |= CONTAINS_SLASH;
                            break;

                        case '-':
                            pattern |= CONTAINS_HYPHEN;
                            break;

                        default:
                            break;
                    }
                }
            }

            return new StringPattern(pattern, digits);
        }
    }
}