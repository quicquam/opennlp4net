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

using j4n.Lang;

namespace opennlp.tools.util
{
    public class StringUtil
    {
        /// <summary>
        /// Determines if the specified character is a whitespace.
        /// 
        /// A character is considered a whitespace when one
        /// of the following conditions is meet:
        /// 
        /// <ul>
        /// <li>Its a <seealso cref="Character#isWhitespace(int)"/> whitespace.</li>
        /// <li>Its a part of the Unicode Zs category (<seealso cref="Character#SPACE_SEPARATOR"/>).</li>
        /// </ul>
        /// 
        /// <code>Character.isWhitespace(int)</code> does not include no-break spaces.
        /// In OpenNLP no-break spaces are also considered as white spaces.
        /// </summary>
        /// <param name="charCode"> </param>
        /// <returns> true if white space otherwise false </returns>
        public static bool isWhitespace(char charCode)
        {
            return char.IsWhiteSpace(charCode);
        }

        /// <summary>
        /// Determines if the specified character is a whitespace.
        /// 
        /// A character is considered a whitespace when one
        /// of the following conditions is meet:
        /// 
        /// <ul>
        /// <li>Its a <seealso cref="Character#isWhitespace(int)"/> whitespace.</li>
        /// <li>Its a part of the Unicode Zs category (<seealso cref="Character#SPACE_SEPARATOR"/>).</li>
        /// </ul>
        /// 
        /// <code>Character.isWhitespace(int)</code> does not include no-break spaces.
        /// In OpenNLP no-break spaces are also considered as white spaces.
        /// </summary>
        /// <param name="charCode"> </param>
        /// <returns> true if white space otherwise false </returns>
        public static bool isWhitespace(int charCode)
        {
            return char.IsWhiteSpace((char) charCode);
        }


        /// <summary>
        /// Converts to lower case independent of the current locale via 
        /// <seealso cref="Character#toLowerCase(char)"/> which uses mapping information
        /// from the UnicodeData file.
        /// </summary>
        /// <param name="string"> </param>
        /// <returns> lower cased String </returns>
        public static string ToLower(CharSequence @string)
        {
            char[] lowerCaseChars = new char[@string.length()];

            for (int i = 0; i < @string.length(); i++)
            {
                lowerCaseChars[i] = char.ToLower(@string.charAt(i));
            }

            return new string(lowerCaseChars);
        }

        /// <summary>
        /// Converts to upper case independent of the current locale via 
        /// <seealso cref="Character#toUpperCase(char)"/> which uses mapping information
        /// from the UnicodeData file.
        /// </summary>
        /// <param name="string"> </param>
        /// <returns> upper cased String </returns>
        public static string ToUpper(CharSequence @string)
        {
            char[] upperCaseChars = new char[@string.length()];

            for (int i = 0; i < @string.length(); i++)
            {
                upperCaseChars[i] = char.ToUpper(@string.charAt(i));
            }

            return new string(upperCaseChars);
        }

        /// <summary>
        /// Returns <tt>true</tt> if <seealso cref="CharSequence#length()"/> is
        /// <tt>0</tt> or <tt>null</tt>.
        /// </summary>
        /// <returns> <tt>true</tt> if <seealso cref="CharSequence#length()"/> is <tt>0</tt>, otherwise
        ///         <tt>false</tt>
        /// 
        /// @since 1.5.1 </returns>
        public static bool isEmpty(CharSequence theString)
        {
            return theString.length() == 0;
        }

        public static bool isEmpty(string theString)
        {
            return string.IsNullOrEmpty(theString);
        }

        public static string ToLower(string s)
        {
            return s.ToLower();
        }
    }
}