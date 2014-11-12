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
using j4n.IO.InputStream;
using j4n.Serialization;
using j4n.IO.Reader;

namespace opennlp.tools.parser.chunking
{
    using Event = opennlp.model.Event;
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using opennlp.tools.util;
    using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

    /// <summary>
    /// Wrapper class for one of four parser event streams.  The particular event stream is specified
    /// at construction.
    /// </summary>
    public class ParserEventStream : AbstractParserEventStream
    {
        protected internal BuildContextGenerator bcg;
        protected internal CheckContextGenerator kcg;

        /// <summary>
        /// Create an event stream based on the specified data stream of the specified type using the specified head rules. </summary>
        /// <param name="d"> A 1-parse-per-line Penn Treebank Style parse. </param>
        /// <param name="rules"> The head rules. </param>
        /// <param name="etype"> The type of events desired (tag, chunk, build, or check). </param>
        /// <param name="dict"> A tri-gram dictionary to reduce feature generation. </param>
        public ParserEventStream(ObjectStream<Parse> d, HeadRules rules, ParserEventTypeEnum etype, Dictionary dict)
            : base(d, rules, etype, dict)
        {
        }

        protected internal override void init()
        {
            if (etype == ParserEventTypeEnum.BUILD)
            {
                this.bcg = new BuildContextGenerator(dict);
            }
            else if (etype == ParserEventTypeEnum.CHECK)
            {
                this.kcg = new CheckContextGenerator();
            }
        }


        public ParserEventStream(ObjectStream<Parse> d, HeadRules rules, ParserEventTypeEnum etype)
            : this(d, rules, etype, null)
        {
        }

        /// <summary>
        /// Returns true if the specified child is the first child of the specified parent. </summary>
        /// <param name="child"> The child parse. </param>
        /// <param name="parent"> The parent parse. </param>
        /// <returns> true if the specified child is the first child of the specified parent; false otherwise. </returns>
        protected internal virtual bool firstChild(Parse child, Parse parent)
        {
            return AbstractBottomUpParser.collapsePunctuation(parent.Children, punctSet)[0] == child;
        }

        public static Parse[] reduceChunks(Parse[] chunks, int ci, Parse parent)
        {
            string type = parent.Type;
            //  perform reduce
            int reduceStart = ci;
            int reduceEnd = ci;
            while (reduceStart >= 0 && chunks[reduceStart].Parent == parent)
            {
                reduceStart--;
            }
            reduceStart++;
            Parse[] reducedChunks;
            if (!type.Equals(AbstractBottomUpParser.TOP_NODE))
            {
                reducedChunks = new Parse[chunks.Length - (reduceEnd - reduceStart + 1) + 1];
                    //total - num_removed + 1 (for new node)
                //insert nodes before reduction
                for (int ri = 0, rn = reduceStart; ri < rn; ri++)
                {
                    reducedChunks[ri] = chunks[ri];
                }
                //insert reduced node
                reducedChunks[reduceStart] = parent;
                //propagate punctuation sets
                parent.PrevPunctuation = chunks[reduceStart].PreviousPunctuationSet;
                parent.NextPunctuation = chunks[reduceEnd].NextPunctuationSet;
                //insert nodes after reduction
                int ri2 = reduceStart + 1;
                for (int rci = reduceEnd + 1; rci < chunks.Length; rci++)
                {
                    reducedChunks[ri2] = chunks[rci];
                    ri2++;
                }
                ci = reduceStart - 1; //ci will be incremented at end of loop
            }
            else
            {
                reducedChunks = new Parse[0];
            }
            return reducedChunks;
        }

        /// <summary>
        /// Adds events for parsing (post tagging and chunking to the specified list of events for the specified parse chunks. </summary>
        /// <param name="parseEvents"> The events for the specified chunks. </param>
        /// <param name="chunks"> The incomplete parses to be parsed. </param>
        protected internal override void addParseEvents(IList<Event> parseEvents, Parse[] chunks)
        {
            int ci = 0;
            while (ci < chunks.Length)
            {
                //System.err.println("parserEventStream.addParseEvents: chunks="+Arrays.asList(chunks));
                Parse c = chunks[ci];
                Parse parent = c.Parent;
                if (parent != null)
                {
                    string type = parent.Type;
                    string outcome;
                    if (firstChild(c, parent))
                    {
                        outcome = AbstractBottomUpParser.START + type;
                    }
                    else
                    {
                        outcome = AbstractBottomUpParser.CONT + type;
                    }
                    //System.err.println("parserEventStream.addParseEvents: chunks["+ci+"]="+c+" label="+outcome+" bcg="+bcg);
                    c.Label = outcome;
                    if (etype == ParserEventTypeEnum.BUILD)
                    {
                        parseEvents.Add(new Event(outcome, bcg.getContext(chunks, ci)));
                    }
                    int start = ci - 1;
                    while (start >= 0 && chunks[start].Parent == parent)
                    {
                        start--;
                    }
                    if (lastChild(c, parent))
                    {
                        if (etype == ParserEventTypeEnum.CHECK)
                        {
                            parseEvents.Add(new Event(Parser.COMPLETE, kcg.getContext(chunks, type, start + 1, ci)));
                        }
                        //perform reduce
                        int reduceStart = ci;
                        while (reduceStart >= 0 && chunks[reduceStart].Parent == parent)
                        {
                            reduceStart--;
                        }
                        reduceStart++;
                        chunks = reduceChunks(chunks, ci, parent);
                        ci = reduceStart - 1; //ci will be incremented at end of loop
                    }
                    else
                    {
                        if (etype == ParserEventTypeEnum.CHECK)
                        {
                            parseEvents.Add(new Event(Parser.INCOMPLETE, kcg.getContext(chunks, type, start + 1, ci)));
                        }
                    }
                }
                ci++;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine(
                    "Usage ParserEventStream -[tag|chunk|build|check|fun] head_rules [dictionary] < parses");
                Environment.Exit(1);
            }
            // was = null not valid C# MJJ 09/11/2014
            ParserEventTypeEnum etype = ParserEventTypeEnum.ATTACH;
            bool fun = false;
            int ai = 0;
            while (ai < args.Length && args[ai].StartsWith("-", StringComparison.Ordinal))
            {
                if (args[ai].Equals("-build"))
                {
                    etype = ParserEventTypeEnum.BUILD;
                }
                else if (args[ai].Equals("-check"))
                {
                    etype = ParserEventTypeEnum.CHECK;
                }
                else if (args[ai].Equals("-chunk"))
                {
                    etype = ParserEventTypeEnum.CHUNK;
                }
                else if (args[ai].Equals("-tag"))
                {
                    etype = ParserEventTypeEnum.TAG;
                }
                else if (args[ai].Equals("-fun"))
                {
                    fun = true;
                }
                else
                {
                    Console.Error.WriteLine("Invalid option " + args[ai]);
                    Environment.Exit(1);
                }
                ai++;
            }
            HeadRules rules = new opennlp.tools.parser.lang.en.HeadRules(args[ai++]);
            Dictionary dict = null;
            if (ai < args.Length)
            {
                dict = new Dictionary(new FileInputStream(args[ai++]));
            }
            if (fun)
            {
                Parse.useFunctionTags(true);
            }
            opennlp.model.EventStream es =
                new ParserEventStream(
                    new ParseSampleStream(
                        new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput(), "TODO Encoding"))),
                    rules, etype, dict);
            while (es.hasNext())
            {
                Console.WriteLine(es.next());
            }
        }
    }
}