using System;
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
using j4n.IO.Reader;
using j4n.IO.Writer;
using j4n.Lang;
using opennlp.tools.nonjava.extensions;


namespace opennlp.tools.parser
{
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Data structure for holding parse constituents.
    /// </summary>
    public class Parse : ICloneable, IComparable<Parse>
    {
        public const string BRACKET_LRB = "(";
        public const string BRACKET_RRB = ")";
        public const string BRACKET_LCB = "{";
        public const string BRACKET_RCB = "}";

        /// <summary>
        /// The text string on which this parse is based.
        /// This object is shared among all parses for the same sentence.
        /// </summary>
        private string text;

        /// <summary>
        /// The character offsets into the text for this constituent.
        /// </summary>
        private Span span;

        /// <summary>
        /// The syntactic type of this parse.
        /// </summary>
        private string type;

        /// <summary>
        /// The sub-constituents of this parse.
        /// </summary>
        private IList<Parse> parts;

        /// <summary>
        /// The head parse of this parse. A parse can be its own head.
        /// </summary>
        private Parse head;

        /// <summary>
        /// A string used during parse construction to specify which
        /// stage of parsing has been performed on this node.
        /// </summary>
        private string label;

        /// <summary>
        /// Index in the sentence of the head of this constituent.
        /// </summary>
        private int headIndex;

        /// <summary>
        /// The parent parse of this parse.
        /// </summary>
        private Parse parent;

        /// <summary>
        /// The probability associated with the syntactic type
        /// assigned to this parse.
        /// </summary>
        private double prob;

        /// <summary>
        /// The string buffer used to track the derivation of this parse.
        /// </summary>
        private StringBuilder derivation;

        /// <summary>
        /// Specifies whether this constituent was built during the chunking phase.
        /// </summary>
        private bool isChunk_Renamed;

        /// <summary>
        /// The pattern used to find the base constituent label of a
        /// Penn Treebank labeled constituent.
        /// </summary>
        private static Pattern typePattern = Pattern.compile("^([^ =-]+)");

        /// <summary>
        /// The pattern used to find the function tags.
        /// </summary>
        private static Pattern funTypePattern = Pattern.compile("^[^ =-]+-([^ =-]+)");

        /// <summary>
        /// The patter used to identify tokens in Penn Treebank labeled constituents.
        /// </summary>
        private static Pattern tokenPattern = Pattern.compile("^[^ ()]+ ([^ ()]+)\\s*\\)");

        /// <summary>
        /// The set of punctuation parses which are between this parse and the previous parse.
        /// </summary>
        private ICollection<Parse> prevPunctSet;

        /// <summary>
        /// The set of punctuation parses which are between this parse and
        /// the subsequent parse.
        /// </summary>
        private ICollection<Parse> nextPunctSet;

        /// <summary>
        /// Specifies whether constituent labels should include parts specified
        /// after minus character.
        /// </summary>
        private static bool useFunctionTags_Renamed;

        /// <summary>
        /// Creates a new parse node for this specified text and span of the specified type with the specified probability
        /// and the specified head index.
        /// </summary>
        /// <param name="text"> The text of the sentence for which this node is a part of. </param>
        /// <param name="span"> The character offsets for this node within the specified text. </param>
        /// <param name="type"> The constituent label of this node. </param>
        /// <param name="p"> The probability of this parse. </param>
        /// <param name="index"> The token index of the head of this parse. </param>
        public Parse(string text, Span span, string type, double p, int index)
        {
            this.text = text;
            this.span = span;
            this.type = type;
            this.prob = p;
            this.head = this;
            this.headIndex = index;
            this.parts = new List<Parse>();
            this.label = null;
            this.parent = null;
        }

        /// <summary>
        /// Creates a new parse node for this specified text and span of the specified type with the specified probability
        /// and the specified head and head index.
        /// </summary>
        /// <param name="text"> The text of the sentence for which this node is a part of. </param>
        /// <param name="span"> The character offsets for this node within the specified text. </param>
        /// <param name="type"> The constituent label of this node. </param>
        /// <param name="p"> The probability of this parse. </param>
        /// <param name="h"> The head token of this parse. </param>
        public Parse(string text, Span span, string type, double p, Parse h) : this(text, span, type, p, 0)
        {
            if (h != null)
            {
                this.head = h;
                this.headIndex = h.headIndex;
            }
        }

        /// <summary>
        /// Clones the right frontier of parse up to the specified node.
        /// </summary>
        /// <param name="node"> The last node in the right frontier of the parse tree which should be cloned. </param>
        /// <returns> A clone of this parse and its right frontier up to and including the specified node. </returns>
        public virtual Parse clone(Parse node)
        {
            if (this.Equals(node))
            {
                return (Parse) this.Clone();
            }
            else
            {
                Parse c = (Parse) this.Clone();
                Parse lc = c.parts[parts.Count - 1];
                c.parts[parts.Count - 1] = lc.Clone() as Parse;
                return c;
            }
        }

        /// <summary>
        /// Clones the right frontier of this root parse up to and including the specified node.
        /// </summary>
        /// <param name="node"> The last node in the right frontier of the parse tree which should be cloned. </param>
        /// <param name="parseIndex"> The child index of the parse for this root node. </param>
        /// <returns> A clone of this root parse and its right frontier up to and including the specified node. </returns>
        public virtual Parse cloneRoot(Parse node, int parseIndex)
        {
            Parse c = (Parse) this.clone(node);
            Parse fc = c.parts[parseIndex];
            c.parts[parseIndex] = fc.clone(node);
            return c;
        }

        /// <summary>
        /// Specifies whether function tags should be included as part of the constituent type.
        /// </summary>
        /// <param name="uft"> true is they should be included; false otherwise. </param>
        public static void useFunctionTags(bool uft)
        {
            useFunctionTags_Renamed = uft;
        }


        /// <summary>
        /// Set the type of this constituent to the specified type.
        /// </summary>
        /// <param name="type"> The type of this constituent. </param>
        public virtual string Type
        {
            set { this.type = value; }
            get { return type; }
        }


        /// <summary>
        /// Returns the set of punctuation parses that occur immediately before this parse.
        /// </summary>
        /// <returns> the set of punctuation parses that occur immediately before this parse. </returns>
        public virtual ICollection<Parse> PreviousPunctuationSet
        {
            get { return prevPunctSet; }
        }

        /// <summary>
        /// Designates that the specified punctuation should is prior to this parse.
        /// </summary>
        /// <param name="punct"> The punctuation. </param>
        public virtual void addPreviousPunctuation(Parse punct)
        {
            if (this.prevPunctSet == null)
            {
                this.prevPunctSet = new SortedSet<Parse>();
            }
            prevPunctSet.Add(punct);
        }

        /// <summary>
        /// Returns the set of punctuation parses that occur immediately after this parse.
        /// </summary>
        /// <returns> the set of punctuation parses that occur immediately after this parse. </returns>
        public virtual ICollection<Parse> NextPunctuationSet
        {
            get { return nextPunctSet; }
        }

        /// <summary>
        /// Designates that the specified punctuation follows this parse.
        /// </summary>
        /// <param name="punct"> The punctuation set. </param>
        public virtual void addNextPunctuation(Parse punct)
        {
            if (this.nextPunctSet == null)
            {
                this.nextPunctSet = new SortedSet<Parse>();
            }
            nextPunctSet.Add(punct);
        }

        /// <summary>
        /// Sets the set of punctuation tags which follow this parse.
        /// </summary>
        /// <param name="punctSet"> The set of punctuation tags which follow this parse. </param>
        public virtual ICollection<Parse> NextPunctuation
        {
            set { this.nextPunctSet = value; }
        }

        /// <summary>
        /// Sets the set of punctuation tags which preceed this parse.
        /// </summary>
        /// <param name="punctSet"> The set of punctuation tags which preceed this parse. </param>
        public virtual ICollection<Parse> PrevPunctuation
        {
            set { this.prevPunctSet = value; }
        }

        /// <summary>
        /// Inserts the specified constituent into this parse based on its text span.This
        /// method assumes that the specified constituent can be inserted into this parse.
        /// </summary>
        /// <param name="constituent"> The constituent to be inserted. </param>
        public virtual void insert(Parse constituent)
        {
            Span ic = constituent.span;
            if (span.contains(ic))
            {
                //double oprob=c.prob;
                int pi = 0;
                int pn = parts.Count;
                for (; pi < pn; pi++)
                {
                    Parse subPart = parts[pi];
                    //System.err.println("Parse.insert:con="+constituent+" sp["+pi+"] "+subPart+" "+subPart.getType());
                    Span sp = subPart.span;
                    if (sp.Start >= ic.End)
                    {
                        break;
                    }
                        // constituent contains subPart
                    else if (ic.contains(sp))
                    {
                        //System.err.println("Parse.insert:con contains subPart");
                        parts.RemoveAt(pi);
                        pi--;
                        constituent.parts.Add(subPart);
                        subPart.Parent = constituent;
                        //System.err.println("Parse.insert: "+subPart.hashCode()+" -> "+subPart.getParent().hashCode());
                        pn = parts.Count;
                    }
                    else if (sp.contains(ic))
                    {
                        //System.err.println("Parse.insert:subPart contains con");
                        subPart.insert(constituent);
                        return;
                    }
                }
                //System.err.println("Parse.insert:adding con="+constituent+" to "+this);
                parts.Insert(pi, constituent);
                constituent.Parent = this;
                //System.err.println("Parse.insert: "+constituent.hashCode()+" -> "+constituent.getParent().hashCode());
            }
            else
            {
                throw new System.ArgumentException("Inserting constituent not contained in the sentence!");
            }
        }

        /// <summary>
        /// Appends the specified string buffer with a string representation of this parse.
        /// </summary>
        /// <param name="sb"> A string buffer into which the parse string can be appended. </param>
        public virtual void show(StringBuilder sb)
        {
            int start;
            start = span.Start;
            if (!type.Equals(AbstractBottomUpParser.TOK_NODE))
            {
                sb.Append("(");
                sb.Append(type).Append(" ");
                //System.out.print(label+" ");
                //System.out.print(head+" ");
                //System.out.print(df.format(prob)+" ");
            }
            for (IEnumerator<Parse> i = parts.GetEnumerator(); i.MoveNext();)
            {
                Parse c = i.Current;
                Span s = c.span;
                if (start < s.Start)
                {
                    //System.out.println("pre "+start+" "+s.getStart());
                    sb.Append(encodeToken(text.Substring(start, s.Start - start)));
                }
                c.show(sb);
                start = s.End;
            }
            if (start < span.End)
            {
                sb.Append(encodeToken(text.Substring(start, span.End - start)));
            }
            if (!type.Equals(AbstractBottomUpParser.TOK_NODE))
            {
                sb.Append(")");
            }
        }

        /// <summary>
        /// Displays this parse using Penn Treebank-style formatting.
        /// </summary>
        public virtual void show()
        {
            StringBuilder sb = new StringBuilder(text.Length*4);
            show(sb);
            Console.WriteLine(sb);
        }

        public virtual void show(OutputStreamWriter writer)
        {
            StringBuilder sb = new StringBuilder(text.Length * 4);
            show(sb);
            writer.writeLine(sb.ToString());
        }

        /// <summary>
        /// Returns the probability associated with the pos-tag sequence assigned to this parse.
        /// </summary>
        /// <returns> The probability associated with the pos-tag sequence assigned to this parse. </returns>
        public virtual double TagSequenceProb
        {
            get
            {
                //System.err.println("Parse.getTagSequenceProb: "+type+" "+this);
                if (parts.Count == 1 && (parts[0]).type.Equals(AbstractBottomUpParser.TOK_NODE))
                {
                    //System.err.println(this+" "+prob);
                    return (Math.Log(prob));
                }
                else if (parts.Count == 0)
                {
                    Console.Error.WriteLine("Parse.getTagSequenceProb: Wrong base case!");
                    return (0.0);
                }
                else
                {
                    double sum = 0.0;
                    for (IEnumerator<Parse> pi = parts.GetEnumerator(); pi.MoveNext();)
                    {
                        sum += pi.Current.TagSequenceProb;
                    }
                    return sum;
                }
            }
        }

        /// <summary>
        /// Returns whether this parse is complete.
        /// </summary>
        /// <returns> Returns true if the parse contains a single top-most node. </returns>
        public virtual bool complete()
        {
            return (parts.Count == 1);
        }

        public virtual string CoveredText
        {
            get { return text.SubstringSpecial(span.Start, span.End); }
        }

        /// <summary>
        /// Represents this parse in a human readable way.  
        /// </summary>
        public override string ToString()
        {
            // TODO: Use the commented code in next bigger release,
            // change probably breaks backward compatibility in some
            // applications
            //StringBuffer buffer = new StringBuffer();
            //show(buffer);
            //return buffer.toString();
            return CoveredText;
        }

        /// <summary>
        /// Returns the text of the sentence over which this parse was formed.
        /// </summary>
        /// <returns> The text of the sentence over which this parse was formed. </returns>
        public virtual string Text
        {
            get { return text; }
        }

        /// <summary>
        /// Returns the character offsets for this constituent.
        /// </summary>
        /// <returns> The character offsets for this constituent. </returns>
        public virtual Span Span
        {
            get { return span; }
        }

        /// <summary>
        /// Returns the log of the product of the probability associated with all the decisions which formed this constituent.
        /// </summary>
        /// <returns> The log of the product of the probability associated with all the decisions which formed this constituent. </returns>
        public virtual double Prob
        {
            get { return prob; }
        }

        /// <summary>
        /// Adds the specified probability log to this current log for this parse.
        /// </summary>
        /// <param name="logProb"> The probability of an action performed on this parse. </param>
        public virtual void addProb(double logProb)
        {
            this.prob += logProb;
        }

        /// <summary>
        /// Returns the child constituents of this constituent
        /// . </summary>
        /// <returns> The child constituents of this constituent. </returns>
        public virtual Parse[] Children
        {
            get { return parts.ToArray(); }
        }

        /// <summary>
        /// Replaces the child at the specified index with a new child with the specified label.
        /// </summary>
        /// <param name="index"> The index of the child to be replaced. </param>
        /// <param name="label"> The label to be assigned to the new child. </param>
        public virtual void setChild(int index, string label)
        {
            Parse newChild = (Parse) (parts[index]).Clone();
            newChild.Label = label;
            parts[index] = newChild;
        }

        public virtual void add(Parse daughter, HeadRules rules)
        {
            if (daughter.prevPunctSet != null)
            {
                foreach (var parse in daughter.prevPunctSet)
                {
                    parts.Add(parse);
                }
            }
            parts.Add(daughter);
            this.span = new Span(span.Start, daughter.Span.End);
            this.head = rules.getHead(Children, type);
            if (head == null)
            {
                Console.Error.WriteLine(parts);
            }
            this.headIndex = head.headIndex;
        }

        public virtual void remove(int index)
        {
            parts.RemoveAt(index);
            if (index == 0 || index == parts.Count) //size is orig last element
            {
                span = new Span((parts[0]).span.Start, (parts[parts.Count - 1]).span.End);
            }
        }

        public virtual Parse adjoinRoot(Parse node, HeadRules rules, int parseIndex)
        {
            Parse lastChild = parts[parseIndex];
            Parse adjNode = new Parse(this.text, new Span(lastChild.Span.Start, node.Span.End), lastChild.Type, 1,
                rules.getHead(new Parse[] {lastChild, node}, lastChild.Type));
            adjNode.parts.Add(lastChild);
            if (node.prevPunctSet != null)
            {
                foreach (var parse in node.prevPunctSet)
                {
                    adjNode.parts.Add(parse);
                }
            }
            adjNode.parts.Add(node);
            parts[parseIndex] = adjNode;
            return adjNode;
        }

        /// <summary>
        /// Sister adjoins this node's last child and the specified sister node and returns their
        /// new parent node.  The new parent node replace this nodes last child.
        /// </summary>
        /// <param name="sister"> The node to be adjoined. </param>
        /// <param name="rules"> The head rules for the parser. </param>
        /// <returns> The new parent node of this node and the specified sister node. </returns>
        public virtual Parse adjoin(Parse sister, HeadRules rules)
        {
            Parse lastChild = parts[parts.Count - 1];
            Parse adjNode = new Parse(this.text, new Span(lastChild.Span.Start, sister.Span.End), lastChild.Type, 1,
                rules.getHead(new Parse[] {lastChild, sister}, lastChild.Type));
            adjNode.parts.Add(lastChild);
            if (sister.prevPunctSet != null)
            {
                foreach (var parse in sister.prevPunctSet)
                {
                    adjNode.parts.Add(parse);
                }
            }
            adjNode.parts.Add(sister);
            parts[parts.Count - 1] = adjNode;
            this.span = new Span(span.Start, sister.Span.End);
            this.head = rules.getHead(Children, type);
            this.headIndex = head.headIndex;
            return adjNode;
        }

        public virtual void expandTopNode(Parse root)
        {
            bool beforeRoot = true;
            //System.err.println("expandTopNode: parts="+parts);
            for (int pi = 0, ai = 0; pi < parts.Count; pi++,ai++)
            {
                Parse node = parts[pi];
                if (node == root)
                {
                    beforeRoot = false;
                }
                else if (beforeRoot)
                {
                    root.parts.Insert(ai, node);
                    parts.RemoveAt(pi);
                    pi--;
                }
                else
                {
                    root.parts.Add(node);
                    parts.RemoveAt(pi);
                    pi--;
                }
            }
            root.updateSpan();
        }

        /// <summary>
        /// Returns the number of children for this parse node.
        /// </summary>
        /// <returns> the number of children for this parse node. </returns>
        public virtual int ChildCount
        {
            get { return parts.Count; }
        }

        /// <summary>
        /// Returns the index of this specified child.
        /// </summary>
        /// <param name="child"> A child of this parse.
        /// </param>
        /// <returns> the index of this specified child or -1 if the specified child is not a child of this parse. </returns>
        public virtual int IndexOf(Parse child)
        {
            return parts.IndexOf(child);
        }

        /// <summary>
        /// Returns the head constituent associated with this constituent.
        /// </summary>
        /// <returns> The head constituent associated with this constituent. </returns>
        public virtual Parse Head
        {
            get { return head; }
        }

        /// <summary>
        /// Returns the index within a sentence of the head token for this parse.
        /// </summary>
        /// <returns> The index within a sentence of the head token for this parse. </returns>
        public virtual int HeadIndex
        {
            get { return headIndex; }
        }

        /// <summary>
        /// Returns the label assigned to this parse node during parsing
        /// which specifies how this node will be formed into a constituent.
        /// </summary>
        /// <returns> The outcome label assigned to this node during parsing. </returns>
        public virtual string Label
        {
            get { return label; }
            set { this.label = value; }
        }


        private static string getType(string rest)
        {
            if (rest.StartsWith("-LCB-", StringComparison.Ordinal))
            {
                return "-LCB-";
            }
            else if (rest.StartsWith("-RCB-", StringComparison.Ordinal))
            {
                return "-RCB-";
            }
            else if (rest.StartsWith("-LRB-", StringComparison.Ordinal))
            {
                return "-LRB-";
            }
            else if (rest.StartsWith("-RRB-", StringComparison.Ordinal))
            {
                return "-RRB-";
            }
            else if (rest.StartsWith("-NONE-", StringComparison.Ordinal))
            {
                return "-NONE-";
            }
            else
            {
                Matcher typeMatcher = typePattern.matcher(rest);
                if (typeMatcher.find())
                {
                    string type = typeMatcher.group(1);
                    if (useFunctionTags_Renamed)
                    {
                        Matcher funMatcher = funTypePattern.matcher(rest);
                        if (funMatcher.find())
                        {
                            string ftag = funMatcher.group(1);
                            type = type + "-" + ftag;
                        }
                    }
                    return type;
                }
            }
            return null;
        }

        private static string encodeToken(string token)
        {
            if (BRACKET_LRB.Equals(token))
            {
                return "-LRB-";
            }
            else if (BRACKET_RRB.Equals(token))
            {
                return "-RRB-";
            }
            else if (BRACKET_LCB.Equals(token))
            {
                return "-LCB-";
            }
            else if (BRACKET_RCB.Equals(token))
            {
                return "-RCB-";
            }

            return token;
        }

        private static string decodeToken(string token)
        {
            if ("-LRB-".Equals(token))
            {
                return BRACKET_LRB;
            }
            else if ("-RRB-".Equals(token))
            {
                return BRACKET_RRB;
            }
            else if ("-LCB-".Equals(token))
            {
                return BRACKET_LCB;
            }
            else if ("-RCB-".Equals(token))
            {
                return BRACKET_RCB;
            }

            return token;
        }

        /// <summary>
        /// Returns the string containing the token for the specified portion of the parse string or
        /// null if the portion of the parse string does not represent a token.
        /// </summary>
        /// <param name="rest"> The portion of the parse string remaining to be processed.
        /// </param>
        /// <returns> The string containing the token for the specified portion of the parse string or
        /// null if the portion of the parse string does not represent a token. </returns>
        private static string getToken(string rest)
        {
            Matcher tokenMatcher = tokenPattern.matcher(rest);
            if (tokenMatcher.find())
            {
                return decodeToken(tokenMatcher.group(1));
            }
            return null;
        }

        /// <summary>
        /// Computes the head parses for this parse and its sub-parses and stores this information
        /// in the parse data structure.
        /// </summary>
        /// <param name="rules"> The head rules which determine how the head of the parse is computed. </param>
        public virtual void updateHeads(HeadRules rules)
        {
            if (parts != null && parts.Count != 0)
            {
                for (int pi = 0, pn = parts.Count; pi < pn; pi++)
                {
                    Parse c = parts[pi];
                    c.updateHeads(rules);
                }
                this.head = rules.getHead(parts.ToArray(), type);
                if (head == null)
                {
                    head = this;
                }
                else
                {
                    this.headIndex = head.headIndex;
                }
            }
            else
            {
                this.head = this;
            }
        }

        public virtual void updateSpan()
        {
            span = new Span((parts[0]).span.Start, (parts[parts.Count - 1]).span.End);
        }

        /// <summary>
        /// Prune the specified sentence parse of vacuous productions.
        /// </summary>
        /// <param name="parse"> </param>
        public static void pruneParse(Parse parse)
        {
            IList<Parse> nodes = new List<Parse>();
            nodes.Add(parse);
            while (nodes.Count != 0)
            {
                Parse node = nodes.ElementAt(0);
                Parse[] children = node.Children;
                if (children.Length == 1 && node.Type.Equals(children[0].Type))
                {
                    int index = node.Parent.parts.IndexOf(node);
                    children[0].Parent = node.Parent;
                    node.Parent.parts[index] = children[0];
                    node.parent = null;
                    node.parts = null;
                }
                foreach (var child in children)
                {
                    nodes.Add(child);
                }
                nodes.RemoveAt(0);
            }
        }

        public static void fixPossesives(Parse parse)
        {
            Parse[] tags = parse.TagNodes;
            for (int ti = 0; ti < tags.Length; ti++)
            {
                if (tags[ti].Type.Equals("POS"))
                {
                    if (ti + 1 < tags.Length && tags[ti + 1].Parent == tags[ti].Parent.Parent)
                    {
                        int start = tags[ti + 1].Span.Start;
                        int end = tags[ti + 1].Span.End;
                        for (int npi = ti + 2; npi < tags.Length; npi++)
                        {
                            if (tags[npi].Parent == tags[npi - 1].Parent)
                            {
                                end = tags[npi].Span.End;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Parse npPos = new Parse(parse.Text, new Span(start, end), "NP", 1, tags[ti + 1]);
                        parse.insert(npPos);
                    }
                }
            }
        }


        /// <summary>
        /// Parses the specified tree-bank style parse string and return a Parse structure for that string.
        /// </summary>
        /// <param name="parse"> A tree-bank style parse string.
        /// </param>
        /// <param name="rules"></param>
        /// <returns> a Parse structure for the specified tree-bank style parse string. </returns>
        public static Parse parseParse(string parse, HeadRules rules)
        {
            return parseParse(parse, (HeadRules) null);
        }

        /// <summary>
        /// Parses the specified tree-bank style parse string and return a Parse structure
        /// for that string.
        /// </summary>
        /// <param name="parse"> A tree-bank style parse string. </param>
        /// <param name="gl"> The gap labeler.
        /// </param>
        /// <returns> a Parse structure for the specified tree-bank style parse string. </returns>
        public static Parse parseParse(string parse, GapLabeler gl)
        {
            StringBuilder text = new StringBuilder();
            int offset = 0;
            Stack<Constituent> stack = new Stack<Constituent>();
            IList<Constituent> cons = new List<Constituent>();
            for (int ci = 0, cl = parse.Length; ci < cl; ci++)
            {
                char c = parse[ci];
                if (c == '(')
                {
                    string rest = parse.Substring(ci + 1);
                    string type = getType(rest);
                    if (type == null)
                    {
                        Console.Error.WriteLine("null type for: " + rest);
                    }
                    string token = getToken(rest);
                    stack.Push(new Constituent(type, new Span(offset, offset)));
                    if (token != null)
                    {
                        if (type.Equals("-NONE-") && gl != null)
                        {
                            //System.err.println("stack.size="+stack.size());
                            gl.labelGaps(stack);
                        }
                        else
                        {
                            cons.Add(new Constituent(AbstractBottomUpParser.TOK_NODE,
                                new Span(offset, offset + token.Length)));
                            text.Append(token).Append(" ");
                            offset += token.Length + 1;
                        }
                    }
                }
                else if (c == ')')
                {
                    Constituent con = stack.Pop();
                    int start = con.Span.Start;
                    if (start < offset)
                    {
                        cons.Add(new Constituent(con.Label, new Span(start, offset - 1)));
                    }
                }
            }
            string txt = text.ToString();
            int tokenIndex = -1;
            Parse p = new Parse(txt, new Span(0, txt.Length), AbstractBottomUpParser.TOP_NODE, 1, 0);
            for (int ci = 0; ci < cons.Count; ci++)
            {
                Constituent con = cons[ci];
                string type = con.Label;
                if (!type.Equals(AbstractBottomUpParser.TOP_NODE))
                {
                    if (type == AbstractBottomUpParser.TOK_NODE)
                    {
                        tokenIndex++;
                    }
                    Parse c = new Parse(txt, con.Span, type, 1, tokenIndex);
                    //System.err.println("insert["+ci+"] "+type+" "+c.toString()+" "+c.hashCode());
                    p.insert(c);
                    //codeTree(p);
                }
            }
            return p;
        }

        /// <summary>
        /// Returns the parent parse node of this constituent.
        /// </summary>
        /// <returns> The parent parse node of this constituent. </returns>
        public virtual Parse Parent
        {
            get { return parent; }
            set { this.parent = value; }
        }


        /// <summary>
        /// Indicates whether this parse node is a pos-tag.
        /// </summary>
        /// <returns> true if this node is a pos-tag, false otherwise. </returns>
        public virtual bool PosTag
        {
            get { return (parts.Count == 1 && (parts[0]).Type.Equals(AbstractBottomUpParser.TOK_NODE)); }
        }

        /// <summary>
        /// Returns true if this constituent contains no sub-constituents.
        /// </summary>
        /// <returns> true if this constituent contains no sub-constituents; false otherwise. </returns>
        public virtual bool Flat
        {
            get
            {
                bool flat = true;
                for (int ci = 0; ci < parts.Count; ci++)
                {
                    flat &= (parts[ci]).PosTag;
                }
                return flat;
            }
        }

        public virtual void isChunk(bool ic)
        {
            this.isChunk_Renamed = ic;
        }

        public virtual bool Chunk
        {
            get { return isChunk_Renamed; }
        }

        /// <summary>
        /// Returns the parse nodes which are children of this node and which are pos tags.
        /// </summary>
        /// <returns> the parse nodes which are children of this node and which are pos tags. </returns>
        public virtual Parse[] TagNodes
        {
            get
            {
                IList<Parse> tags = new List<Parse>();
                IList<Parse> nodes = parts.ToList();
                while (nodes.Count != 0)
                {
                    Parse p = nodes.ElementAt(0);
                    if (p.PosTag)
                    {
                        tags.Add(p);
                    }
                    else
                    {
                        foreach (var part in p.parts)
                        {
                            nodes.Add(part);
                        }
                    }
                    nodes.RemoveAt(0);
                }
                return tags.ToArray();
            }
        }

        /// <summary>
        /// Returns the deepest shared parent of this node and the specified node.
        /// If the nodes are identical then their parent is returned.
        /// If one node is the parent of the other then the parent node is returned.
        /// </summary>
        /// <param name="node"> The node from which parents are compared to this node's parents.
        /// </param>
        /// <returns> the deepest shared parent of this node and the specified node. </returns>
        public virtual Parse getCommonParent(Parse node)
        {
            if (this == node)
            {
                return parent;
            }
            HashSet<Parse> parents = new HashSet<Parse>();
            Parse cparent = this;
            while (cparent != null)
            {
                parents.Add(cparent);
                cparent = cparent.Parent;
            }
            while (node != null)
            {
                if (parents.Contains(node))
                {
                    return node;
                }
                node = node.Parent;
            }
            return null;
        }

        public override bool Equals(object o)
        {
            if (o is Parse)
            {
                Parse p = (Parse) o;
                if (this.label == null)
                {
                    if (p.label != null)
                    {
                        return false;
                    }
                }
                else if (!this.label.Equals(p.label))
                {
                    return false;
                }
                if (!this.span.Equals(p.span))
                {
                    return false;
                }
                if (!this.text.Equals(p.text))
                {
                    return false;
                }
                if (this.parts.Count != p.parts.Count)
                {
                    return false;
                }
                for (int ci = 0; ci < parts.Count; ci++)
                {
                    if (!parts[ci].Equals(p.parts[ci]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = 37*result + span.GetHashCode();

            // TODO: This might lead to a performance regression
            //    result = 37*result + label.hashCode();
            result = 37*result + text.GetHashCode();
            return result;
        }

        public object Clone()
        {
            Parse p = new Parse(this.text, this.span, this.type, this.prob, this.head);
            p.parts = new List<Parse>();
            foreach (var part in parts)
            {
                p.parts.Add(part);
            }

            if (derivation != null)
            {
                p.derivation = new StringBuilder(100);
                p.derivation.Append(this.derivation.ToString());
            }
            return p;
        }

        public virtual int CompareTo(Parse p)
        {
            if (this.Prob > p.Prob)
            {
                return -1;
            }
            else if (this.Prob < p.Prob)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Returns the derivation string for this parse if one has been created.
        /// </summary>
        /// <returns> the derivation string for this parse or null if no derivation
        /// string has been created. </returns>
        public virtual StringBuilder Derivation
        {
            get { return derivation; }
            set { this.derivation = value; }
        }


        private void codeTree(Parse p, int[] levels)
        {
            Parse[] kids = p.Children;
            StringBuilder levelsBuff = new StringBuilder();
            levelsBuff.Append("[");
            int[] nlevels = new int[levels.Length + 1];
            for (int li = 0; li < levels.Length; li++)
            {
                nlevels[li] = levels[li];
                levelsBuff.Append(levels[li]).Append(".");
            }
            for (int ki = 0; ki < kids.Length; ki++)
            {
                nlevels[levels.Length] = ki;
                Console.WriteLine(levelsBuff.ToString() + ki + "] " + kids[ki].Type + " " + kids[ki].GetHashCode() +
                                  " -> " + kids[ki].Parent.GetHashCode() + " " + kids[ki].Parent.Type + " " +
                                  kids[ki].CoveredText);
                codeTree(kids[ki], nlevels);
            }
        }

        /// <summary>
        /// Prints to standard out a representation of the specified parse which
        /// contains hash codes so that parent/child relationships can be explicitly seen.
        /// </summary>
        public virtual void showCodeTree()
        {
            codeTree(this, new int[0]);
        }

        /// <summary>
        /// Utility method to inserts named entities.
        /// </summary>
        /// <param name="tag"> </param>
        /// <param name="names"> </param>
        /// <param name="tokens"> </param>
        public static void addNames(string tag, Span[] names, Parse[] tokens)
        {
            for (int ni = 0, nn = names.Length; ni < nn; ni++)
            {
                Span nameTokenSpan = names[ni];
                Parse startToken = tokens[nameTokenSpan.Start];
                Parse endToken = tokens[nameTokenSpan.End - 1];
                Parse commonParent = startToken.getCommonParent(endToken);
                //System.err.println("addNames: "+startToken+" .. "+endToken+" commonParent = "+commonParent);
                if (commonParent != null)
                {
                    Span nameSpan = new Span(startToken.Span.Start, endToken.Span.End);
                    if (nameSpan.Equals(commonParent.Span))
                    {
                        commonParent.insert(new Parse(commonParent.Text, nameSpan, tag, 1.0, endToken.HeadIndex));
                    }
                    else
                    {
                        Parse[] kids = commonParent.Children;
                        bool crossingKids = false;
                        for (int ki = 0, kn = kids.Length; ki < kn; ki++)
                        {
                            if (nameSpan.crosses(kids[ki].Span))
                            {
                                crossingKids = true;
                            }
                        }
                        if (!crossingKids)
                        {
                            commonParent.insert(new Parse(commonParent.Text, nameSpan, tag, 1.0, endToken.HeadIndex));
                        }
                        else
                        {
                            if (commonParent.Type.Equals("NP"))
                            {
                                Parse[] grandKids = kids[0].Children;
                                if (grandKids.Length > 1 && nameSpan.contains(grandKids[grandKids.Length - 1].Span))
                                {
                                    commonParent.insert(new Parse(commonParent.Text, commonParent.Span, tag, 1.0,
                                        commonParent.HeadIndex));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads training parses (one-sentence-per-line) and displays parse structure.
        /// </summary>
        /// <param name="args"> The head rules files.
        /// </param>
        /// <exception cref="IOException"> If the head rules file can not be opened and read. </exception>
        [Obsolete]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: Parse -fun -pos head_rules < train_parses");
                Console.Error.WriteLine("Reads training parses (one-sentence-per-line) and displays parse structure.");
                Environment.Exit(1);
            }
            int ai = 0;
            bool fixPossesives = false;
            while (args[ai].StartsWith("-", StringComparison.Ordinal) && ai < args.Length)
            {
                if (args[ai].Equals("-fun"))
                {
                    Parse.useFunctionTags(true);
                    ai++;
                }
                else if (args[ai].Equals("-pos"))
                {
                    fixPossesives = true;
                    ai++;
                }
            }

            HeadRules rules = new opennlp.tools.parser.lang.en.HeadRules(args[ai]);
            BufferedReader @in = new BufferedReader(new InputStreamReader(Console.OpenStandardInput(), "TODO Encoding"));

            for (string line = @in.readLine(); line != null; line = @in.readLine())
            {
                Parse p = Parse.parseParse(line, rules);
                Parse.pruneParse(p);
                if (fixPossesives)
                {
                    Parse.fixPossesives(p);
                }
                p.updateHeads(rules);
                p.show();
                //p.showCodeTree();
            }
        }
    }
}