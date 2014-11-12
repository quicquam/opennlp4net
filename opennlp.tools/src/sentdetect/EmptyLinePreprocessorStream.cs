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

namespace opennlp.tools.sentdetect
{
    using opennlp.tools.util;

    /// <summary>
    /// Stream to to clean up empty lines for empty line separated document streams.<br>
    /// 
    /// - Skips empty line at training data start<br>
    /// - Transforms multiple empty lines in a row into one <br>
    /// - Replaces white space lines with empty lines <br>
    /// - TODO: Terminates last document with empty line if it is missing<br>
    /// <br>
    /// This stream should be used by the components that mark empty lines to mark document boundaries.
    /// <para>
    /// <b>Note:</b>
    /// This class is not thread safe. <br>
    /// Do not use this class, internal use only!
    /// </para>
    /// </summary>
    public class EmptyLinePreprocessorStream : FilterObjectStream<string, string>
    {
        private bool lastLineWasEmpty = true;

        public EmptyLinePreprocessorStream(ObjectStream<string> @in) : base(@in)
        {
        }

        private static bool isLineEmpty(string line)
        {
            return line.Trim().Length == 0;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String read() throws java.io.IOException
        public override string read()
        {
            string line = samples.read();

            if (lastLineWasEmpty)
            {
                lastLineWasEmpty = false;

                while (line != null && isLineEmpty(line))
                {
                    line = samples.read();
                }
            }

            if (line != null && isLineEmpty(line))
            {
                lastLineWasEmpty = true;
                line = "";
            }

            return line;
        }
    }
}