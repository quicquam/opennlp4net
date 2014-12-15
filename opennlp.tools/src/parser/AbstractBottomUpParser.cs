using System;
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

namespace opennlp.tools.parser
{
    using Chunker = opennlp.tools.chunker.Chunker;
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using NGramModel = opennlp.tools.ngram.NGramModel;
    using ParserEventStream = opennlp.tools.parser.chunking.ParserEventStream;
    using POSTagger = opennlp.tools.postag.POSTagger;
    using opennlp.tools.util;
    using opennlp.tools.util;
    using opennlp.tools.util;
    using Sequence = opennlp.tools.util.Sequence;
    using Span = opennlp.tools.util.Span;
    using StringList = opennlp.tools.util.StringList;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;

    /// <summary>
    /// Abstract class which contains code to tag and chunk parses for bottom up parsing and
    /// leaves implementation of advancing parses and completing parses to extend class.
    /// <para>
    /// <b>Note:</b> <br> The nodes within
    /// the returned parses are shared with other parses and therefore their parent node references will not be consistent
    /// with their child node reference.  <seealso cref="#setParents setParents"/> can be used to make the parents consistent
    /// with a particular parse, but subsequent calls to <code>setParents</code> can invalidate the results of earlier
    /// calls.<br>
    /// </para>
    /// </summary>
    public abstract class AbstractBottomUpParser : Parser
    {
        /// <summary>
        /// The maximum number of parses advanced from all preceding
        /// parses at each derivation step.
        /// </summary>
        protected internal int M;

        /// <summary>
        /// The maximum number of parses to advance from a single preceding parse.
        /// </summary>
        protected internal int K;

        /// <summary>
        /// The minimum total probability mass of advanced outcomes.
        /// </summary>
        protected internal double Q;

        /// <summary>
        /// The default beam size used if no beam size is given.
        /// </summary>
        public const int defaultBeamSize = 20;

        /// <summary>
        /// The default amount of probability mass required of advanced outcomes.
        /// </summary>
        public const double defaultAdvancePercentage = 0.95;

        /// <summary>
        /// Completed parses.
        /// </summary>
        protected internal Heap<Parse> completeParses;

        /// <summary>
        /// Incomplete parses which will be advanced.
        /// </summary>
        protected internal Heap<Parse> odh;

        /// <summary>
        /// Incomplete parses which have been advanced. 
        /// </summary>
        protected internal Heap<Parse> ndh;

        /// <summary>
        /// The head rules for the parser.
        /// </summary>
        protected internal HeadRules headRules;

        /// <summary>
        /// The set strings which are considered punctuation for the parser.
        /// Punctuation is not attached, but floats to the top of the parse as attachment
        /// decisions are made about its non-punctuation sister nodes.
        /// </summary>
        protected internal HashSet<string> punctSet;

        /// <summary>
        /// The label for the top node. 
        /// </summary>
        public const string TOP_NODE = "TOP";

        /// <summary>
        /// The label for the top if an incomplete node.
        /// </summary>
        public const string INC_NODE = "INC";

        /// <summary>
        /// The label for a token node. 
        /// </summary>
        public const string TOK_NODE = "TK";

        /// <summary>
        /// The integer 0.
        /// </summary>
        public const int ZERO = 0;

        /// <summary>
        /// Prefix for outcomes starting a constituent. 
        /// </summary>
        public static string START = "S-";

        /// <summary>
        /// Prefix for outcomes continuing a constituent. 
        /// </summary>
        public static string CONT = "C-";

        /// <summary>
        /// Outcome for token which is not contained in a basal constituent. 
        /// </summary>
        public static string OTHER = "O";

        /// <summary>
        /// Outcome used when a constituent is complete. 
        /// </summary>
        public static string COMPLETE = "c";

        /// <summary>
        /// Outcome used when a constituent is incomplete. 
        /// </summary>
        public static string INCOMPLETE = "i";

        /// <summary>
        /// The pos-tagger that the parser uses. 
        /// </summary>
        protected internal POSTagger tagger;

        /// <summary>
        /// The chunker that the parser uses to chunk non-recursive structures. 
        /// </summary>
        protected internal Chunker chunker;

        /// <summary>
        /// Specifies whether failed parses should be reported to standard error. 
        /// </summary>
        protected internal bool reportFailedParse;

        /// <summary>
        /// Specifies whether a derivation string should be created during parsing. 
        /// This is useful for debugging. 
        /// </summary>
        protected internal bool createDerivationString = false;

        /// <summary>
        /// Turns debug print on or off.
        /// </summary>
        protected internal bool debugOn = false;

        public AbstractBottomUpParser(POSTagger tagger, Chunker chunker, HeadRules headRules, int beamSize,
            double advancePercentage)
        {
            this.tagger = tagger;
            this.chunker = chunker;
            this.M = beamSize;
            this.K = beamSize;
            this.Q = advancePercentage;
            reportFailedParse = true;
            this.headRules = headRules;
            this.punctSet = headRules.PunctuationTags;
            odh = new ListHeap<Parse>(K);
            ndh = new ListHeap<Parse>(K);
            completeParses = new ListHeap<Parse>(K);
        }

        /// <summary>
        /// Specifies whether the parser should report when it was unable to find a parse for
        /// a particular sentence. </summary>
        /// <param name="errorReporting"> If true then un-parsed sentences are reported, false otherwise. </param>
        public virtual bool ErrorReporting
        {
            set { this.reportFailedParse = value; }
        }

        /// <summary>
        /// Assigns parent references for the specified parse so that they
        /// are consistent with the children references. </summary>
        /// <param name="p"> The parse whose parent references need to be assigned. </param>
        public static Parse Parents
        {
            set
            {
                Parse[] children = value.Children;
                for (int ci = 0; ci < children.Length; ci++)
                {
                    children[ci].Parent = value;
                    Parents = children[ci];
                }
            }
        }

        /// <summary>
        /// Removes the punctuation from the specified set of chunks, adds it to the parses
        /// adjacent to the punctuation is specified, and returns a new array of parses with the punctuation
        /// removed. </summary>
        /// <param name="chunks"> A set of parses. </param>
        /// <param name="punctSet"> The set of punctuation which is to be removed. </param>
        /// <returns> An array of parses which is a subset of chunks with punctuation removed. </returns>
        public static Parse[] collapsePunctuation(Parse[] chunks, HashSet<string> punctSet)
        {
            IList<Parse> collapsedParses = new List<Parse>(chunks.Length);
            int lastNonPunct = -1;
            int nextNonPunct = -1;
            for (int ci = 0, cn = chunks.Length; ci < cn; ci++)
            {
                if (punctSet.Contains(chunks[ci].Type))
                {
                    if (lastNonPunct >= 0)
                    {
                        chunks[lastNonPunct].addNextPunctuation(chunks[ci]);
                    }
                    for (nextNonPunct = ci + 1; nextNonPunct < cn; nextNonPunct++)
                    {
                        if (!punctSet.Contains(chunks[nextNonPunct].Type))
                        {
                            break;
                        }
                    }
                    if (nextNonPunct < cn)
                    {
                        chunks[nextNonPunct].addPreviousPunctuation(chunks[ci]);
                    }
                }
                else
                {
                    collapsedParses.Add(chunks[ci]);
                    lastNonPunct = ci;
                }
            }
            if (collapsedParses.Count == chunks.Length)
            {
                return chunks;
            }
            //System.err.println("collapsedPunctuation: collapsedParses"+collapsedParses);
            return collapsedParses.ToArray();
        }


        /// <summary>
        /// Advances the specified parse and returns the an array advanced parses whose probability accounts for
        /// more than the specified amount of probability mass. </summary>
        /// <param name="p"> The parse to advance. </param>
        /// <param name="probMass"> The amount of probability mass that should be accounted for by the advanced parses. </param>
        protected internal abstract Parse[] advanceParses(Parse p, double probMass);

        /// <summary>
        /// Adds the "TOP" node to the specified parse. </summary>
        /// <param name="p"> The complete parse. </param>
        protected internal abstract void advanceTop(Parse p);

        public virtual Parse[] parse(Parse tokens, int numParses)
        {
            if (createDerivationString)
            {
                tokens.Derivation = new StringBuilder(100);
            }
            odh.clear();
            ndh.clear();
            completeParses.clear();
            int derivationStage = 0; //derivation length
            int maxDerivationLength = 2*tokens.ChildCount + 3;
            odh.add(tokens);
            Parse guess = null;
            double minComplete = 2;
            double bestComplete = -100000; //approximating -infinity/0 in ln domain
            while (odh.size() > 0 && (completeParses.size() < M || (odh.first()).Prob < minComplete) &&
                   derivationStage < maxDerivationLength)
            {
                ndh = new ListHeap<Parse>(K);

                int derivationRank = 0;

                for (IEnumerator<Parse> pi = odh.iterator(); pi.MoveNext() && derivationRank < K; derivationRank++)
                    // forearch derivation
                {
                    Parse tp = pi.Current;
                    //TODO: Need to look at this for K-best parsing cases
                    /*
			 if (tp.getProb() < bestComplete) { //this parse and the ones which follow will never win, stop advancing.
			 break;
			 }
			 */
                    if (guess == null && derivationStage == 2)
                    {
                        guess = tp;
                    }
                    if (debugOn)
                    {
                        Console.Write(derivationStage + " " + derivationRank + " " + tp.Prob);
                        tp.show();
                        Console.WriteLine();
                    }
                    Parse[] nd;
                    if (0 == derivationStage)
                    {
                        nd = advanceTags(tp);
                    }
                    else if (1 == derivationStage)
                    {
                        if (ndh.size() < K)
                        {
                            //System.err.println("advancing ts "+j+" "+ndh.size()+" < "+K);
                            nd = advanceChunks(tp, bestComplete);
                        }
                        else
                        {
                            //System.err.println("advancing ts "+j+" prob="+((Parse) ndh.last()).getProb());
                            nd = advanceChunks(tp, (ndh.last()).Prob);
                        }
                    }
                    else // i > 1
                    {
                        nd = advanceParses(tp, Q);
                    }
                    if (nd != null)
                    {
                        for (int k = 0, kl = nd.Length; k < kl; k++)
                        {
                            if (nd[k].complete())
                            {
                                advanceTop(nd[k]);
                                if (nd[k].Prob > bestComplete)
                                {
                                    bestComplete = nd[k].Prob;
                                }
                                if (nd[k].Prob < minComplete)
                                {
                                    minComplete = nd[k].Prob;
                                }
                                completeParses.add(nd[k]);
                            }
                            else
                            {
                                ndh.add(nd[k]);
                            }
                        }
                    }
                    else
                    {
                        if (reportFailedParse)
                        {
                            Console.Error.WriteLine("Couldn't advance parse " + derivationStage + " stage " +
                                                    derivationRank + "!\n");
                        }
                        advanceTop(tp);
                        completeParses.add(tp);
                    }
                }
                derivationStage++;
                odh = ndh;
            }
            if (completeParses.size() == 0)
            {
                if (reportFailedParse)
                {
                    Console.Error.WriteLine("Couldn't find parse for: " + tokens);
                }
                //Parse r = (Parse) odh.first();
                //r.show();
                //System.out.println();
                return new Parse[] {guess};
            }
            else if (numParses == 1)
            {
                return new Parse[] {completeParses.first()};
            }
            else
            {
                IList<Parse> topParses = new List<Parse>(numParses);
                while (!completeParses.Empty && topParses.Count < numParses)
                {
                    Parse tp = completeParses.extract();
                    topParses.Add(tp);
                    //parses.remove(tp);
                }
                return topParses.ToArray();
            }
        }

        public virtual Parse parse(Parse tokens)
        {
            if (tokens.ChildCount > 0)
            {
                Parse p = parse(tokens, 1)[0];
                Parents = p;
                return p;
            }
            else
            {
                return tokens;
            }
        }

        /// <summary>
        /// Returns the top chunk sequences for the specified parse. </summary>
        /// <param name="p"> A pos-tag assigned parse. </param>
        /// <param name="minChunkScore"> A minimum score below which chunks should not be advanced. </param>
        /// <returns> The top chunk assignments to the specified parse. </returns>
        protected internal virtual Parse[] advanceChunks(Parse p, double minChunkScore)
        {
            // chunk
            Parse[] children = p.Children;
            string[] words = new string[children.Length];
            string[] ptags = new string[words.Length];
            double[] probs = new double[words.Length];
            Parse sp = null;
            for (int i = 0, il = children.Length; i < il; i++)
            {
                sp = children[i];
                words[i] = sp.Head.CoveredText;
                ptags[i] = sp.Type;
            }
            //System.err.println("adjusted mcs = "+(minChunkScore-p.getProb()));
            Sequence[] cs = chunker.topKSequences(words, ptags, minChunkScore - p.Prob);
            Parse[] newParses = new Parse[cs.Length];
            for (int si = 0, sl = cs.Length; si < sl; si++)
            {
                newParses[si] = (Parse) p.Clone(); //copies top level
                if (createDerivationString)
                {
                    newParses[si].Derivation.Append(si).Append(".");
                }
                string[] tags = cs[si].Outcomes.ToArray();
                cs[si].getProbs(probs);
                int start = -1;
                int end = 0;
                string type = null;
                //System.err.print("sequence "+si+" ");
                for (int j = 0; j <= tags.Length; j++)
                {
                    //if (j != tags.length) {System.err.println(words[j]+" "+ptags[j]+" "+tags[j]+" "+probs.get(j));}
                    if (j != tags.Length)
                    {
                        newParses[si].addProb(Math.Log(probs[j]));
                    }
                    if (j != tags.Length && tags[j].StartsWith(CONT, StringComparison.Ordinal))
                        // if continue just update end chunking tag don't use contTypeMap
                    {
                        end = j;
                    }
                    else //make previous constituent if it exists
                    {
                        if (type != null)
                        {
                            //System.err.println("inserting tag "+tags[j]);
                            Parse p1 = p.Children[start];
                            Parse p2 = p.Children[end];
                            //System.err.println("Putting "+type+" at "+start+","+end+" for "+j+" "+newParses[si].getProb());
                            Parse[] cons = new Parse[end - start + 1];
                            cons[0] = p1;
                            //cons[0].label="Start-"+type;
                            if (end - start != 0)
                            {
                                cons[end - start] = p2;
                                //cons[end-start].label="Cont-"+type;
                                for (int ci = 1; ci < end - start; ci++)
                                {
                                    cons[ci] = p.Children[ci + start];
                                    //cons[ci].label="Cont-"+type;
                                }
                            }
                            Parse chunk = new Parse(p1.Text, new Span(p1.Span.Start, p2.Span.End), type, 1,
                                headRules.getHead(cons, type));
                            chunk.isChunk(true);
                            newParses[si].insert(chunk);
                        }
                        if (j != tags.Length) //update for new constituent
                        {
                            if (tags[j].StartsWith(START, StringComparison.Ordinal))
                                // don't use startTypeMap these are chunk tags
                            {
                                type = tags[j].Substring(START.Length);
                                start = j;
                                end = j;
                            }
                            else // other
                            {
                                type = null;
                            }
                        }
                    }
                }
                //newParses[si].show();System.out.println();
            }
            return newParses;
        }

        /// <summary>
        /// Advances the parse by assigning it POS tags and returns multiple tag sequences. </summary>
        /// <param name="p"> The parse to be tagged. </param>
        /// <returns> Parses with different POS-tag sequence assignments. </returns>
        protected internal virtual Parse[] advanceTags(Parse p)
        {
            Parse[] children = p.Children;
            string[] words = new string[children.Length];
            double[] probs = new double[words.Length];
            for (int i = 0, il = children.Length; i < il; i++)
            {
                words[i] = children[i].CoveredText;
            }
            Sequence[] ts = tagger.topKSequences(words);
            if (ts.Length == 0)
            {
                Console.Error.WriteLine("no tag sequence");
            }
            Parse[] newParses = new Parse[ts.Length];
            for (int i = 0; i < ts.Length; i++)
            {
                string[] tags = ts[i].Outcomes.ToArray();
                ts[i].getProbs(probs);
                newParses[i] = (Parse) p.Clone(); //copies top level
                if (createDerivationString)
                {
                    newParses[i].Derivation.Append(i).Append(".");
                }
                for (int j = 0; j < words.Length; j++)
                {
                    Parse word = children[j];
                    //System.err.println("inserting tag "+tags[j]);
                    double prob = probs[j];
                    newParses[i].insert(new Parse(word.Text, word.Span, tags[j], prob, j));
                    newParses[i].addProb(Math.Log(prob));
                    //newParses[i].show();
                }
            }
            return newParses;
        }

        /// <summary>
        /// Determines the mapping between the specified index into the specified parses without punctuation to
        /// the corresponding index into the specified parses. </summary>
        /// <param name="index"> An index into the parses without punctuation. </param>
        /// <param name="nonPunctParses"> The parses without punctuation. </param>
        /// <param name="parses"> The parses wit punctuation. </param>
        /// <returns> An index into the specified parses which corresponds to the same node the specified index
        /// into the parses with punctuation. </returns>
        protected internal virtual int mapParseIndex(int index, Parse[] nonPunctParses, Parse[] parses)
        {
            int parseIndex = index;
            while (parses[parseIndex] != nonPunctParses[index])
            {
                parseIndex++;
            }
            return parseIndex;
        }

        private static bool lastChild(Parse child, Parse parent, HashSet<string> punctSet)
        {
            Parse[] kids = collapsePunctuation(parent.Children, punctSet);
            return (kids[kids.Length - 1] == child);
        }

        /// <summary>
        /// Creates a n-gram dictionary from the specified data stream using the specified head rule and specified cut-off.
        /// </summary>
        /// <param name="data"> The data stream of parses. </param>
        /// <param name="rules"> The head rules for the parses. </param>
        /// <param name="params"> can contain a cutoff, the minimum number of entries required for the
        ///        n-gram to be saved as part of the dictionary. </param>
        /// <returns> A dictionary object. </returns>
        public static Dictionary buildDictionary(ObjectStream<Parse> data, HeadRules rules, TrainingParameters @params)
        {
            int cutoff = 5;

            string cutoffString = @params.getSettings("dict")[TrainingParameters.CUTOFF_PARAM];

            if (cutoffString != null)
            {
                // TODO: Maybe throw illegal argument exception if not parse able
                cutoff = Convert.ToInt32(cutoffString);
            }

            NGramModel mdict = new NGramModel();
            Parse p;
            while ((p = data.read()) != null)
            {
                p.updateHeads(rules);
                Parse[] pwords = p.TagNodes;
                string[] words = new string[pwords.Length];
                //add all uni-grams
                for (int wi = 0; wi < words.Length; wi++)
                {
                    words[wi] = pwords[wi].CoveredText;
                }

                mdict.add(new StringList(words), 1, 1);
                //add tri-grams and bi-grams for inital sequence
                Parse[] chunks = collapsePunctuation(ParserEventStream.getInitialChunks(p), rules.PunctuationTags);
                string[] cwords = new string[chunks.Length];
                for (int wi = 0; wi < cwords.Length; wi++)
                {
                    cwords[wi] = chunks[wi].Head.CoveredText;
                }
                mdict.add(new StringList(cwords), 2, 3);

                //emulate reductions to produce additional n-grams
                int ci = 0;
                while (ci < chunks.Length)
                {
                    //System.err.println("chunks["+ci+"]="+chunks[ci].getHead().getCoveredText()+" chunks.length="+chunks.length);
                    if (lastChild(chunks[ci], chunks[ci].Parent, rules.PunctuationTags))
                    {
                        //perform reduce
                        int reduceStart = ci;
                        while (reduceStart >= 0 && chunks[reduceStart].Parent == chunks[ci].Parent)
                        {
                            reduceStart--;
                        }
                        reduceStart++;
                        chunks = ParserEventStream.reduceChunks(chunks, ci, chunks[ci].Parent);
                        ci = reduceStart;
                        if (chunks.Length != 0)
                        {
                            string[] window = new string[5];
                            int wi = 0;
                            if (ci - 2 >= 0)
                            {
                                window[wi++] = chunks[ci - 2].Head.CoveredText;
                            }
                            if (ci - 1 >= 0)
                            {
                                window[wi++] = chunks[ci - 1].Head.CoveredText;
                            }
                            window[wi++] = chunks[ci].Head.CoveredText;
                            if (ci + 1 < chunks.Length)
                            {
                                window[wi++] = chunks[ci + 1].Head.CoveredText;
                            }
                            if (ci + 2 < chunks.Length)
                            {
                                window[wi++] = chunks[ci + 2].Head.CoveredText;
                            }
                            if (wi < 5)
                            {
                                string[] subWindow = new string[wi];
                                for (int swi = 0; swi < wi; swi++)
                                {
                                    subWindow[swi] = window[swi];
                                }
                                window = subWindow;
                            }
                            if (window.Length >= 3)
                            {
                                mdict.add(new StringList(window), 2, 3);
                            }
                            else if (window.Length == 2)
                            {
                                mdict.add(new StringList(window), 2, 2);
                            }
                        }
                        ci = reduceStart - 1; //ci will be incremented at end of loop
                    }
                    ci++;
                }
            }
            //System.err.println("gas,and="+mdict.getCount((new TokenList(new String[] {"gas","and"}))));
            mdict.cutoff(cutoff, int.MaxValue);
            return mdict.toDictionary(true);
        }

        /// <summary>
        /// Creates a n-gram dictionary from the specified data stream using the specified head rule and specified cut-off.
        /// </summary>
        /// <param name="data"> The data stream of parses. </param>
        /// <param name="rules"> The head rules for the parses. </param>
        /// <param name="cutoff"> The minimum number of entries required for the n-gram to be saved as part of the dictionary. </param>
        /// <returns> A dictionary object. </returns>
        public static Dictionary buildDictionary(ObjectStream<Parse> data, HeadRules rules, int cutoff)
        {
            TrainingParameters @params = new TrainingParameters();
            @params.put("dict", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cutoff));

            return buildDictionary(data, rules, @params);
        }
    }
}