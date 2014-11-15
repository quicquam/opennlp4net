using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using j4n.IO.InputStream;
using j4n.IO.Reader;
using j4n.Lang;

namespace opennlp.tools.tokenize.lang.en
{
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Class which produces an Iterator<TokenSample> from a file of space delimited token.
    /// This class uses a number of English-specific heuristics to un-separate tokens which
    /// are typically found together in text.
    /// </summary>
    public class TokenSampleStream : IEnumerator<TokenSample>
    {
        private BufferedReader @in;
        private string line;
        private Pattern alphaNumeric = Pattern.compile("[A-Za-z0-9]");
        private bool evenq = true;

        public TokenSampleStream(InputStream @is)
        {
            this.@in = new BufferedReader(new InputStreamReader(@is));
            line = @in.readLine();
        }

        public virtual bool hasNext()
        {
            return line != null;
        }

        public virtual TokenSample next()
        {
            string[] tokens = line.Split(null);
            if (tokens.Length == 0)
            {
                evenq = true;
            }
            StringBuilder sb = new StringBuilder(line.Length);
            IList<Span> spans = new List<Span>();
            int length = 0;
            for (int ti = 0; ti < tokens.Length; ti++)
            {
                string token = tokens[ti];
                string lastToken = ti - 1 >= 0 ? tokens[ti - 1] : "";
                if (token.Equals("-LRB-"))
                {
                    token = "(";
                }
                else if (token.Equals("-LCB-"))
                {
                    token = "{";
                }
                else if (token.Equals("-RRB-"))
                {
                    token = ")";
                }
                else if (token.Equals("-RCB-"))
                {
                    token = "}";
                }
                if (sb.Length == 0)
                {
                }
                else if (!alphaNumeric.matcher(token).find() || token.StartsWith("'", StringComparison.Ordinal) ||
                         token.Equals("n't", StringComparison.CurrentCultureIgnoreCase))
                {
                    if ((token.Equals("``") || token.Equals("--") || token.Equals("$") || token.Equals("(") ||
                         token.Equals("&") || token.Equals("#") ||
                         (token.Equals("\"") && (evenq && ti != tokens.Length - 1))) &&
                        (!lastToken.Equals("(") || !lastToken.Equals("{")))
                    {
                        //System.out.print(" "+token);
                        length++;
                    }
                    else
                    {
                        //System.out.print(token);
                    }
                }
                else
                {
                    if (lastToken.Equals("``") || (lastToken.Equals("\"") && !evenq) || lastToken.Equals("(") ||
                        lastToken.Equals("{") || lastToken.Equals("$") || lastToken.Equals("#"))
                    {
                        //System.out.print(token);
                    }
                    else
                    {
                        //System.out.print(" "+token);
                        length++;
                    }
                }
                if (token.Equals("\""))
                {
                    if (ti == tokens.Length - 1)
                    {
                        evenq = true;
                    }
                    else
                    {
                        evenq = !evenq;
                    }
                }
                if (sb.Length < length)
                {
                    sb.Append(" ");
                }
                sb.Append(token);
                spans.Add(new Span(length, length + token.Length));
                length += token.Length;
            }
            //System.out.println();
            try
            {
                line = @in.readLine();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                line = null;
            }
            return new TokenSample(sb.ToString(), spans.ToArray());
        }


        public virtual void remove()
        {
            throw new System.NotSupportedException();
        }

        private static void usage()
        {
            Console.Error.WriteLine("TokenSampleStream [-spans] < in");
            Console.Error.WriteLine("Where in is a space delimited list of tokens.");
        }

        public static void Main(string[] args)
        {
            bool showSpans = false;
            int ai = 0;
            while (ai < args.Length)
            {
                if (args[ai].Equals("-spans"))
                {
                    showSpans = true;
                }
                else
                {
                    Console.Error.WriteLine("Unknown option " + args[ai]);
                    usage();
                }
                ai++;
            }
            TokenSampleStream tss = new TokenSampleStream(new InputStream(Console.OpenStandardInput()));
            while (tss.hasNext())
            {
                TokenSample ts = tss.next();
                string text = ts.Text;
                Console.WriteLine(text);
                Span[] tokenSpans = ts.TokenSpans;
                int ti = 0;
                if (showSpans)
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (ti - 1 >= 0 && i == tokenSpans[ti - 1].End - 1)
                        {
                            Console.Write("]");
                        }
                        else if (i == tokenSpans[ti].Start)
                        {
                            ti++;
                            if (ti - 1 >= 0 && i == tokenSpans[ti - 1].End - 1)
                            {
                                Console.Write("|");
                            }
                            else
                            {
                                Console.Write("[");
                            }
                        }
                        else
                        {
                            Console.Write("-");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public TokenSample Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}