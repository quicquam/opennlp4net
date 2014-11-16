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

namespace opennlp.tools.sentdetect.lang
{
    using SentenceContextGenerator = opennlp.tools.sentdetect.lang.th.SentenceContextGenerator;

    public class Factory
    {
        public static readonly char[] ptEosCharacters = new char[]
        {'.', '?', '!', ';', ':', '(', ')', '«', '»', '\'', '"'};

        public static readonly char[] defaultEosCharacters = new char[] {'.', '!', '?'};

        public static readonly char[] thEosCharacters = new char[] {' ', '\n'};

        public virtual EndOfSentenceScanner createEndOfSentenceScanner(string languageCode)
        {
            if ("th".Equals(languageCode))
            {
                return new DefaultEndOfSentenceScanner(new char[] {' ', '\n'});
            }
            else if ("pt".Equals(languageCode))
            {
                return new DefaultEndOfSentenceScanner(ptEosCharacters);
            }

            return new DefaultEndOfSentenceScanner(defaultEosCharacters);
        }

        public virtual EndOfSentenceScanner createEndOfSentenceScanner(char[] customEOSCharacters)
        {
            return new DefaultEndOfSentenceScanner(customEOSCharacters);
        }

        public virtual SDContextGenerator createSentenceContextGenerator(string languageCode,
            HashSet<string> abbreviations)
        {
            if ("th".Equals(languageCode))
            {
                return new SentenceContextGenerator();
            }
            else if ("pt".Equals(languageCode))
            {
                return new DefaultSDContextGenerator(abbreviations, ptEosCharacters);
            }

            return new DefaultSDContextGenerator(abbreviations, defaultEosCharacters);
        }

        public virtual SDContextGenerator createSentenceContextGenerator(HashSet<string> abbreviations,
            char[] customEOSCharacters)
        {
            return new DefaultSDContextGenerator(abbreviations, customEOSCharacters);
        }

        public virtual SDContextGenerator createSentenceContextGenerator(string languageCode)
        {
            return createSentenceContextGenerator(languageCode, new HashSet<string>());
        }

        public virtual char[] getEOSCharacters(string languageCode)
        {
            if ("th".Equals(languageCode))
            {
                return thEosCharacters;
            }
            else if ("pt".Equals(languageCode))
            {
                return ptEosCharacters;
            }

            return defaultEosCharacters;
        }
    }
}