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
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.Reader;
using j4n.Serialization;


namespace opennlp.tools.parser.treeinsert
{
    using SuffixSensitiveGISModelReader = opennlp.maxent.io.SuffixSensitiveGISModelReader;
    using AbstractModel = opennlp.model.AbstractModel;
    using Event = opennlp.model.Event;
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using opennlp.tools.util;
    using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
    using Span = opennlp.tools.util.Span;

    public class ParserEventStream : AbstractParserEventStream
    {
        protected internal AttachContextGenerator attachContextGenerator;
        protected internal BuildContextGenerator buildContextGenerator;
        protected internal CheckContextGenerator checkContextGenerator;

        private const bool debug = false;

        public ParserEventStream(ObjectStream<Parse> d, HeadRules rules, ParserEventTypeEnum etype, Dictionary dict)
            : base(d, rules, etype, dict)
        {
        }

        protected internal override void init()
        {
            buildContextGenerator = new BuildContextGenerator();
            attachContextGenerator = new AttachContextGenerator(punctSet);
            checkContextGenerator = new CheckContextGenerator(punctSet);
        }

        public ParserEventStream(ObjectStream<Parse> d, HeadRules rules, ParserEventTypeEnum etype)
            : base(d, rules, etype)
        {
        }

        /// <summary>
        /// Returns a set of parent nodes which consist of the immediate
        /// parent of the specified node and any of its parent which
        /// share the same syntactic type. </summary>
        /// <param name="node"> The node whose parents are to be returned. </param>
        /// <returns> a set of parent nodes. </returns>
        private IDictionary<Parse, int?> getNonAdjoinedParent(Parse node)
        {
            IDictionary<Parse, int?> parents = new Dictionary<Parse, int?>();
            Parse parent = node.Parent;
            int index = IndexOf(node, parent);
            parents[parent] = index;
            while (parent.Type.Equals(node.Type))
            {
                node = parent;
                parent = parent.Parent;
                index = IndexOf(node, parent);
                parents[parent] = index;
            }
            return parents;
        }

        private int IndexOf(Parse child, Parse parent)
        {
            Parse[] kids = Parser.collapsePunctuation(parent.Children, punctSet);
            for (int ki = 0; ki < kids.Length; ki++)
            {
                if (child == kids[ki])
                {
                    return ki;
                }
            }
            return -1;
        }

        private int nonPunctChildCount(Parse node)
        {
            return Parser.collapsePunctuation(node.Children, punctSet).Length;
        }

        /*
	  private Set getNonAdjoinedParent(Parse node) {
	    Set parents = new HashSet();
	    Parse parent = node.getParent();
	    do {
	      parents.add(parent);
	      parent = parent.getParent();
	    }
	    while(parent.getType().equals(node.getType()));
	    return parents;
	  }
	  */

        protected internal override bool lastChild(Parse child, Parse parent)
        {
            bool lc = base.lastChild(child, parent);
            while (!lc)
            {
                Parse cp = child.Parent;
                if (cp != parent && cp.Type.Equals(child.Type))
                {
                    lc = base.lastChild(cp, parent);
                    child = cp;
                }
                else
                {
                    break;
                }
            }
            return lc;
        }

        protected internal override void addParseEvents(IList<Event> parseEvents, Parse[] chunks)
        {
            /// <summary>
            /// Frontier nodes built from node in a completed parse.  Specifically,
            /// they have all their children regardless of the stage of parsing.
            /// </summary>
            IList<Parse> rightFrontier = new List<Parse>();
            IList<Parse> builtNodes = new List<Parse>();
            /// <summary>
            /// Nodes which characterize what the parse looks like to the parser as its being built.
            /// Specifically, these nodes don't have all their children attached like the parents of
            /// the chunk nodes do.
            /// </summary>
            Parse[] currentChunks = new Parse[chunks.Length];
            for (int ci = 0; ci < chunks.Length; ci++)
            {
                currentChunks[ci] = (Parse) chunks[ci].Clone();
                currentChunks[ci].PrevPunctuation = chunks[ci].PreviousPunctuationSet;
                currentChunks[ci].NextPunctuation = chunks[ci].NextPunctuationSet;
                currentChunks[ci].Label = Parser.COMPLETE;
                chunks[ci].Label = Parser.COMPLETE;
            }
            for (int ci = 0; ci < chunks.Length; ci++)
            {
                //System.err.println("parserEventStream.addParseEvents: chunks="+Arrays.asList(chunks));
                Parse parent = chunks[ci].Parent;
                Parse prevParent = chunks[ci];
                int off = 0;
                //build un-built parents
                if (!chunks[ci].PosTag)
                {
                    builtNodes.Insert(off++, chunks[ci]);
                }
                //perform build stages
                while (!parent.Type.Equals(AbstractBottomUpParser.TOP_NODE) && parent.Label == null)
                {
                    if (parent.Label == null && !prevParent.Type.Equals(parent.Type))
                    {
                        //build level
                        if (debug)
                        {
                            Console.Error.WriteLine("Build: " + parent.Type + " for: " + currentChunks[ci]);
                        }
                        if (etype == ParserEventTypeEnum.BUILD)
                        {
                            parseEvents.Add(new Event(parent.Type, buildContextGenerator.getContext(currentChunks, ci)));
                        }
                        builtNodes.Insert(off++, parent);
                        Parse newParent = new Parse(currentChunks[ci].Text, currentChunks[ci].Span, parent.Type, 1, 0);
                        newParent.add(currentChunks[ci], rules);
                        newParent.PrevPunctuation = currentChunks[ci].PreviousPunctuationSet;
                        newParent.NextPunctuation = currentChunks[ci].NextPunctuationSet;
                        currentChunks[ci].Parent = newParent;
                        currentChunks[ci] = newParent;
                        newParent.Label = Parser.BUILT;
                        //see if chunk is complete
                        if (lastChild(chunks[ci], parent))
                        {
                            if (etype == ParserEventTypeEnum.CHECK)
                            {
                                parseEvents.Add(new Event(Parser.COMPLETE,
                                    checkContextGenerator.getContext(currentChunks[ci], currentChunks, ci, false)));
                            }
                            currentChunks[ci].Label = Parser.COMPLETE;
                            parent.Label = Parser.COMPLETE;
                        }
                        else
                        {
                            if (etype == ParserEventTypeEnum.CHECK)
                            {
                                parseEvents.Add(new Event(Parser.INCOMPLETE,
                                    checkContextGenerator.getContext(currentChunks[ci], currentChunks, ci, false)));
                            }
                            currentChunks[ci].Label = Parser.INCOMPLETE;
                            parent.Label = Parser.COMPLETE;
                        }

                        chunks[ci] = parent;
                        //System.err.println("build: "+newParent+" for "+parent);
                    }
                    //TODO: Consider whether we need to set this label or train parses at all.
                    parent.Label = Parser.BUILT;
                    prevParent = parent;
                    parent = parent.Parent;
                }
                //decide to attach
                if (etype == ParserEventTypeEnum.BUILD)
                {
                    parseEvents.Add(new Event(Parser.DONE, buildContextGenerator.getContext(currentChunks, ci)));
                }
                //attach node
                string attachType = null;
                /// <summary>
                /// Node selected for attachment. </summary>
                Parse attachNode = null;
                int attachNodeIndex = -1;
                if (ci == 0)
                {
                    Parse top = new Parse(currentChunks[ci].Text, new Span(0, currentChunks[ci].Text.Length),
                        AbstractBottomUpParser.TOP_NODE, 1, 0);
                    top.insert(currentChunks[ci]);
                }
                else
                {
                    /// <summary>
                    /// Right frontier consisting of partially-built nodes based on current state of the parse. </summary>
                    IList<Parse> currentRightFrontier = Parser.getRightFrontier(currentChunks[0], punctSet);
                    if (currentRightFrontier.Count != rightFrontier.Count)
                    {
                        Console.Error.WriteLine("fontiers mis-aligned: " + currentRightFrontier.Count + " != " +
                                                rightFrontier.Count + " " + currentRightFrontier + " " + rightFrontier);
                        Environment.Exit(1);
                    }
                    IDictionary<Parse, int?> parents = getNonAdjoinedParent(chunks[ci]);
                    //try daughters first.
                    for (int cfi = 0; cfi < currentRightFrontier.Count; cfi++)
                    {
                        Parse frontierNode = rightFrontier[cfi];
                        Parse cfn = currentRightFrontier[cfi];
                        if (!Parser.checkComplete || !Parser.COMPLETE.Equals(cfn.Label))
                        {
                            int? i = parents[frontierNode];
                            if (debug)
                            {
                                Console.Error.WriteLine("Looking at attachment site (" + cfi + "): " + cfn.Type + " ci=" +
                                                        i + " cs=" + nonPunctChildCount(cfn) + ", " + cfn + " :for " +
                                                        currentChunks[ci].Type + " " + currentChunks[ci] + " -> " +
                                                        parents);
                            }
                            if (attachNode == null && i != null && i == nonPunctChildCount(cfn))
                            {
                                attachType = Parser.ATTACH_DAUGHTER;
                                attachNodeIndex = cfi;
                                attachNode = cfn;
                                if (etype == ParserEventTypeEnum.ATTACH)
                                {
                                    parseEvents.Add(new Event(attachType,
                                        attachContextGenerator.getContext(currentChunks, ci, currentRightFrontier,
                                            attachNodeIndex)));
                                }
                                //System.err.println("daughter attach "+attachNode+" at "+fi);
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                Console.Error.WriteLine("Skipping (" + cfi + "): " + cfn.Type + "," +
                                                        cfn.PreviousPunctuationSet + " " + cfn + " :for " +
                                                        currentChunks[ci].Type + " " + currentChunks[ci] + " -> " +
                                                        parents);
                            }
                        }
                        // Can't attach past first incomplete node.
                        if (Parser.checkComplete && cfn.Label.Equals(Parser.INCOMPLETE))
                        {
                            if (debug)
                            {
                                Console.Error.WriteLine("breaking on incomplete:" + cfn.Type + " " + cfn);
                            }
                            break;
                        }
                    }
                    //try sisters, and generate non-attach events.
                    for (int cfi = 0; cfi < currentRightFrontier.Count; cfi++)
                    {
                        Parse frontierNode = rightFrontier[cfi];
                        Parse cfn = currentRightFrontier[cfi];
                        if (attachNode == null && parents.ContainsKey(frontierNode.Parent) &&
                            frontierNode.Type.Equals(frontierNode.Parent.Type))
                            //&& frontierNode.getParent().getLabel() == null) {
                        {
                            attachType = Parser.ATTACH_SISTER;
                            attachNode = cfn;
                            attachNodeIndex = cfi;
                            if (etype == ParserEventTypeEnum.ATTACH)
                            {
                                parseEvents.Add(new Event(Parser.ATTACH_SISTER,
                                    attachContextGenerator.getContext(currentChunks, ci, currentRightFrontier, cfi)));
                            }
                            chunks[ci].Parent.Label = Parser.BUILT;
                            //System.err.println("in search sister attach "+attachNode+" at "+cfi);
                        }
                        else if (cfi == attachNodeIndex)
                        {
                            //skip over previously attached daughter.
                        }
                        else
                        {
                            if (etype == ParserEventTypeEnum.ATTACH)
                            {
                                parseEvents.Add(new Event(Parser.NON_ATTACH,
                                    attachContextGenerator.getContext(currentChunks, ci, currentRightFrontier, cfi)));
                            }
                        }
                        //Can't attach past first incomplete node.
                        if (Parser.checkComplete && cfn.Label.Equals(Parser.INCOMPLETE))
                        {
                            if (debug)
                            {
                                Console.Error.WriteLine("breaking on incomplete:" + cfn.Type + " " + cfn);
                            }
                            break;
                        }
                    }
                    //attach Node
                    if (attachNode != null)
                    {
                        if (attachType == Parser.ATTACH_DAUGHTER)
                        {
                            Parse daughter = currentChunks[ci];
                            if (debug)
                            {
                                Console.Error.WriteLine("daughter attach a=" + attachNode.Type + ":" + attachNode +
                                                        " d=" + daughter + " com=" +
                                                        lastChild(chunks[ci], rightFrontier[attachNodeIndex]));
                            }
                            attachNode.add(daughter, rules);
                            daughter.Parent = attachNode;
                            if (lastChild(chunks[ci], rightFrontier[attachNodeIndex]))
                            {
                                if (etype == ParserEventTypeEnum.CHECK)
                                {
                                    parseEvents.Add(new Event(Parser.COMPLETE,
                                        checkContextGenerator.getContext(attachNode, currentChunks, ci, true)));
                                }
                                attachNode.Label = Parser.COMPLETE;
                            }
                            else
                            {
                                if (etype == ParserEventTypeEnum.CHECK)
                                {
                                    parseEvents.Add(new Event(Parser.INCOMPLETE,
                                        checkContextGenerator.getContext(attachNode, currentChunks, ci, true)));
                                }
                            }
                        }
                        else if (attachType == Parser.ATTACH_SISTER)
                        {
                            Parse frontierNode = rightFrontier[attachNodeIndex];
                            rightFrontier[attachNodeIndex] = frontierNode.Parent;
                            Parse sister = currentChunks[ci];
                            if (debug)
                            {
                                Console.Error.WriteLine("sister attach a=" + attachNode.Type + ":" + attachNode + " s=" +
                                                        sister + " ap=" + attachNode.Parent + " com=" +
                                                        lastChild(chunks[ci], rightFrontier[attachNodeIndex]));
                            }
                            Parse newParent = attachNode.Parent.adjoin(sister, rules);

                            newParent.Parent = attachNode.Parent;
                            attachNode.Parent = newParent;
                            sister.Parent = newParent;
                            if (attachNode == currentChunks[0])
                            {
                                currentChunks[0] = newParent;
                            }
                            if (lastChild(chunks[ci], rightFrontier[attachNodeIndex]))
                            {
                                if (etype == ParserEventTypeEnum.CHECK)
                                {
                                    parseEvents.Add(new Event(Parser.COMPLETE,
                                        checkContextGenerator.getContext(newParent, currentChunks, ci, true)));
                                }
                                newParent.Label = Parser.COMPLETE;
                            }
                            else
                            {
                                if (etype == ParserEventTypeEnum.CHECK)
                                {
                                    parseEvents.Add(new Event(Parser.INCOMPLETE,
                                        checkContextGenerator.getContext(newParent, currentChunks, ci, true)));
                                }
                                newParent.Label = Parser.INCOMPLETE;
                            }
                        }
                        //update right frontier
                        for (int ni = 0; ni < attachNodeIndex; ni++)
                        {
                            //System.err.println("removing: "+rightFrontier.get(0));
                            rightFrontier.RemoveAt(0);
                        }
                    }
                    else
                    {
                        //System.err.println("No attachment!");
                        throw new Exception("No Attachment: " + chunks[ci]);
                    }
                }
                foreach (var builtNode in builtNodes)
                {
                    rightFrontier.Add(builtNode);
                }
                builtNodes.Clear();
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine(
                    "Usage ParserEventStream -[tag|chunk|build|attach] [-fun] [-dict dictionary] [-model model] head_rules < parses");
                Environment.Exit(1);
            }
            // was = null changes MJJ 09/11/2014
            ParserEventTypeEnum etype = ParserEventTypeEnum.ATTACH;
            bool fun = false;
            int ai = 0;
            Dictionary dict = null;
            AbstractModel model = null;

            while (ai < args.Length && args[ai].StartsWith("-", StringComparison.Ordinal))
            {
                if (args[ai].Equals("-build"))
                {
                    etype = ParserEventTypeEnum.BUILD;
                }
                else if (args[ai].Equals("-attach"))
                {
                    etype = ParserEventTypeEnum.ATTACH;
                }
                else if (args[ai].Equals("-chunk"))
                {
                    etype = ParserEventTypeEnum.CHUNK;
                }
                else if (args[ai].Equals("-check"))
                {
                    etype = ParserEventTypeEnum.CHECK;
                }
                else if (args[ai].Equals("-tag"))
                {
                    etype = ParserEventTypeEnum.TAG;
                }
                else if (args[ai].Equals("-fun"))
                {
                    fun = true;
                }
                else if (args[ai].Equals("-dict"))
                {
                    ai++;
                    dict = new Dictionary(new FileInputStream(args[ai]));
                }
                else if (args[ai].Equals("-model"))
                {
                    ai++;
                    model = (new SuffixSensitiveGISModelReader(new Jfile(args[ai]))).Model;
                }
                else
                {
                    Console.Error.WriteLine("Invalid option " + args[ai]);
                    Environment.Exit(1);
                }
                ai++;
            }
            HeadRules rules = new opennlp.tools.parser.lang.en.HeadRules(args[ai++]);
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
                Event e = es.next();
                if (model != null)
                {
                    Console.Write(model.eval(e.Context)[model.getIndex(e.Outcome)] + " ");
                }
                Console.WriteLine(e);
            }
        }
    }
}