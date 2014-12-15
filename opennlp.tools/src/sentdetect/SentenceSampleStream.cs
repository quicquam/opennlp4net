using System.Collections.Generic;
using System.Linq;
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
    using util;
    using Span = util.Span;

    /// <summary>
    /// This class is a stream filter which reads a sentence by line samples from
    /// a <code>Reader</code> and converts them into <seealso cref="SentenceSample"/> objects.
    /// </summary>
    public class SentenceSampleStream : FilterObjectStream<string, SentenceSample>
    {
        public SentenceSampleStream(ObjectStream<string> sentences) : base(new EmptyLinePreprocessorStream(sentences))
        {
        }

        public override SentenceSample read()
        {
            StringBuilder sentencesString = new StringBuilder();
            LinkedList<Span> sentenceSpans = new LinkedList<Span>();

            string sentence;
            while ((sentence = samples.read()) != null && !sentence.Equals(""))
            {
                int begin = sentencesString.Length;
                sentencesString.Append(sentence.Trim());
                int end = sentencesString.Length;
                sentenceSpans.AddLast(new Span(begin, end));
                sentencesString.Append(' ');
            }

            if (sentenceSpans.Count > 0)
            {
                return new SentenceSample(sentencesString.ToString(), sentenceSpans.ToArray());
            }
            else
            {
                return null;
            }
        }
    }
}