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
using j4n.IO.Reader;
using j4n.Logging;
using j4n.Serialization;

namespace opennlp.tools.postag
{
    using opennlp.tools.util;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using opennlp.tools.util;
    using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

    /// <summary>
    /// A stream filter which reads a sentence per line which contains
    /// words and tags in word_tag format and outputs a <seealso cref="POSSample"/> objects.
    /// </summary>
    public class WordTagSampleStream : FilterObjectStream<string, POSSample>
    {
        private static Logger logger = Logger.getLogger(typeof (WordTagSampleStream).Name);

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="sentences"> reader with sentences </param>
        /// <exception cref="IOException"> IOException </exception>
        public WordTagSampleStream(Reader sentences) : base(new PlainTextByLineStream(sentences))
        {
        }

        public WordTagSampleStream(ObjectStream<string> sentences) : base(sentences)
        {
        }

        /// <summary>
        /// Parses the next sentence and return the next
        /// <seealso cref="POSSample"/> object.
        /// 
        /// If an error occurs an empty <seealso cref="POSSample"/> object is returned
        /// and an warning message is logged. Usually it does not matter if one
        /// of many sentences is ignored.
        /// 
        /// TODO: An exception in error case should be thrown.
        /// </summary>
        public override POSSample read()
        {
            string sentence = samples.read();

            if (sentence != null)
            {
                POSSample sample;
                try
                {
                    sample = POSSample.parse(sentence);
                }
                catch (InvalidFormatException)
                {
                    if (logger.isLoggable(Logger.Level.WARNING))
                    {
                        logger.warning("Error during parsing, ignoring sentence: " + sentence);
                    }

                    sample = new POSSample(new string[] {}, new string[] {});
                }

                return sample;
            }
            else
            {
                // sentences stream is exhausted
                return null;
            }
        }
    }
}