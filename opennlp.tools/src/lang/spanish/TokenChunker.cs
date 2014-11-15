using System;
using j4n.IO.File;
using j4n.IO.OutputStream;
using j4n.IO.Reader;

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

namespace opennlp.tools.lang.spanish
{
    using SuffixSensitiveGISModelReader = opennlp.maxent.io.SuffixSensitiveGISModelReader;
    using NameFinderEventStream = opennlp.tools.namefind.NameFinderEventStream;
    using NameFinderME = opennlp.tools.namefind.NameFinderME;
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Class which identifies multi-token chunk which are treated as a single token in for POS-tagging.
    /// </summary>
    public class TokenChunker
    {
        private NameFinderME nameFinder;

        public TokenChunker(string modelName)
        {
            nameFinder = new NameFinderME((new SuffixSensitiveGISModelReader(new Jfile(modelName))).Model);
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: java opennlp.tools.spanish.TokenChunker model < tokenized_sentences");
                Environment.Exit(1);
            }
            TokenChunker chunker = new TokenChunker(args[0]);
            BufferedReader inReader =
                new BufferedReader(new InputStreamReader(Console.OpenStandardInput(), "ISO-8859-1"));
            PrintStream @out = new PrintStream(Console.OpenStandardOutput(), true, "ISO-8859-1");
            for (string line = inReader.readLine(); line != null; line = inReader.readLine())
            {
                if (line.Equals(""))
                {
                    @out.println();
                }
                else
                {
                    string[] tokens = line.Split(' ');
                    Span[] spans = chunker.nameFinder.find(tokens);
                    string[] outcomes = NameFinderEventStream.generateOutcomes(spans, null, tokens.Length);
                    //System.err.println(java.util.Arrays.asList(chunks));
                    for (int ci = 0, cn = outcomes.Length; ci < cn; ci++)
                    {
                        if (ci == 0)
                        {
                            @out.print(tokens[ci]);
                        }
                        else if (outcomes[ci].Equals(NameFinderME.CONTINUE))
                        {
                            @out.print("_" + tokens[ci]);
                        }
                        else
                        {
                            @out.print(" " + tokens[ci]);
                        }
                    }
                    @out.println();
                }
            }
        }
    }
}