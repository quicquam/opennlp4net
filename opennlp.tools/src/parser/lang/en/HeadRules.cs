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
using System.IO;
using j4n.IO.Reader;
using j4n.IO.Writer;
using j4n.Object;
using opennlp.nonjava.helperclasses;
using opennlp.tools.nonjava.extensions;


namespace opennlp.tools.parser.lang.en
{
    using Parser = opennlp.tools.parser.chunking.Parser;

    /// <summary>
    /// Class for storing the English head rules associated with parsing.
    /// </summary>
    public class HeadRules : opennlp.tools.parser.HeadRules, GapLabeler
    {
        private class HeadRule
        {
            public bool leftToRight;
            public string[] tags;

            public HeadRule(bool l2r, string[] tags)
            {
                leftToRight = l2r;

                foreach (string tag in tags)
                {
                    if (tag == null)
                    {
                        throw new System.ArgumentException("tags must not contain null values!");
                    }
                }

                this.tags = tags;
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }
                else if (obj is HeadRule)
                {
                    HeadRule rule = (HeadRule) obj;

                    return (rule.leftToRight == leftToRight) && Equals(rule.tags, tags);
                }
                else
                {
                    return false;
                }
            }
        }

        private IDictionary<string, HeadRule> headRules;
        private HashSet<string> punctSet;

        /// <summary>
        /// Creates a new set of head rules based on the specified head rules file.
        /// </summary>
        /// <param name="ruleFile"> the head rules file.
        /// </param>
        /// <exception cref="IOException"> if the head rules file can not be read. </exception>
        [Obsolete]
        public HeadRules(string ruleFile) : this(new BufferedReader(new FileReader(ruleFile)))
        {
        }

        /// <summary>
        /// Creates a new set of head rules based on the specified reader.
        /// </summary>
        /// <param name="rulesReader"> the head rules reader.
        /// </param>
        /// <exception cref="IOException"> if the head rules reader can not be read. </exception>
        public HeadRules(Reader rulesReader)
        {
            BufferedReader @in = new BufferedReader(rulesReader);
            readHeadRules(@in);

            punctSet = new HashSet<string>();
            punctSet.Add(".");
            punctSet.Add(",");
            punctSet.Add("``");
            punctSet.Add("''");
            //punctSet.add(":");
        }

        public override HashSet<string> PunctuationTags
        {
            get { return punctSet; }
        }

        public override Parse getHead(Parse[] constituents, string type)
        {
            if (constituents[0].Type == Parser.TOK_NODE)
            {
                return null;
            }
            HeadRule hr;
            if (type.Equals("NP") || type.Equals("NX"))
            {
                string[] tags1 = new string[] {"NN", "NNP", "NNPS", "NNS", "NX", "JJR", "POS"};
                for (int ci = constituents.Length - 1; ci >= 0; ci--)
                {
                    for (int ti = tags1.Length - 1; ti >= 0; ti--)
                    {
                        if (constituents[ci].Type.Equals(tags1[ti]))
                        {
                            return constituents[ci].Head;
                        }
                    }
                }
                for (int ci = 0; ci < constituents.Length; ci++)
                {
                    if (constituents[ci].Type.Equals("NP"))
                    {
                        return constituents[ci].Head;
                    }
                }
                string[] tags2 = new string[] {"$", "ADJP", "PRN"};
                for (int ci = constituents.Length - 1; ci >= 0; ci--)
                {
                    for (int ti = tags2.Length - 1; ti >= 0; ti--)
                    {
                        if (constituents[ci].Type.Equals(tags2[ti]))
                        {
                            return constituents[ci].Head;
                        }
                    }
                }
                string[] tags3 = new string[] {"JJ", "JJS", "RB", "QP"};
                for (int ci = constituents.Length - 1; ci >= 0; ci--)
                {
                    for (int ti = tags3.Length - 1; ti >= 0; ti--)
                    {
                        if (constituents[ci].Type.Equals(tags3[ti]))
                        {
                            return constituents[ci].Head;
                        }
                    }
                }
                return constituents[constituents.Length - 1].Head;
            }
            else if ((hr = headRules[type]) != null)
            {
                string[] tags = hr.tags;
                int cl = constituents.Length;
                int tl = tags.Length;
                if (hr.leftToRight)
                {
                    for (int ti = 0; ti < tl; ti++)
                    {
                        for (int ci = 0; ci < cl; ci++)
                        {
                            if (constituents[ci].Type.Equals(tags[ti]))
                            {
                                return constituents[ci].Head;
                            }
                        }
                    }
                    return constituents[0].Head;
                }
                else
                {
                    for (int ti = 0; ti < tl; ti++)
                    {
                        for (int ci = cl - 1; ci >= 0; ci--)
                        {
                            if (constituents[ci].Type.Equals(tags[ti]))
                            {
                                return constituents[ci].Head;
                            }
                        }
                    }
                    return constituents[cl - 1].Head;
                }
            }
            return constituents[constituents.Length - 1].Head;
        }

        private void readHeadRules(BufferedReader str)
        {
            string line;
            headRules = new Dictionary<string, HeadRule>(30);
            while ((line = str.readLine()) != null)
            {
                StringTokenizer st = new StringTokenizer(line);
                string num = st.nextToken();
                string type = st.nextToken();
                string dir = st.nextToken();
                string[] tags = new string[Convert.ToInt32(num) - 2];
                int ti = 0;
                while (st.hasMoreTokens())
                {
                    tags[ti] = st.nextToken();
                    ti++;
                }
                headRules[type] = new HeadRule(dir.Equals("1"), tags);
            }
        }

        public virtual void labelGaps(Stack<Constituent> stack)
        {
            if (stack.Count > 4)
            {
                //Constituent con0 = (Constituent) stack.get(stack.size()-1);
                Constituent con1 = stack.get(stack.Count - 2);
                Constituent con2 = stack.get(stack.Count - 3);
                Constituent con3 = stack.get(stack.Count - 4);
                Constituent con4 = stack.get(stack.Count - 5);
                //System.err.println("con0="+con0.label+" con1="+con1.label+" con2="+con2.label+" con3="+con3.label+" con4="+con4.label);
                //subject extraction
                if (con1.Label.Equals("NP") && con2.Label.Equals("S") && con3.Label.Equals("SBAR"))
                {
                    con1.Label = con1.Label + "-G";
                    con2.Label = con2.Label + "-G";
                    con3.Label = con3.Label + "-G";
                }
                    //object extraction
                else if (con1.Label.Equals("NP") && con2.Label.Equals("VP") && con3.Label.Equals("S") &&
                         con4.Label.Equals("SBAR"))
                {
                    con1.Label = con1.Label + "-G";
                    con2.Label = con2.Label + "-G";
                    con3.Label = con3.Label + "-G";
                    con4.Label = con4.Label + "-G";
                }
            }
        }

        /// <summary>
        /// Writes the head rules to the writer in a format suitable for loading
        /// the head rules again with the constructor. The encoding must be
        /// taken into account while working with the writer and reader.
        /// <para> 
        /// After the entries have been written, the writer is flushed.
        /// The writer remains open after this method returns.
        /// 
        /// </para>
        /// </summary>
        /// <param name="writer"> </param>
        /// <exception cref="IOException"> </exception>
        public virtual void serialize(Writer writer)
        {
            foreach (string type in headRules.Keys)
            {
                HeadRule headRule = headRules[type];

                // write num of tags
                writer.write(Convert.ToString(headRule.tags.Length + 2));
                writer.write(' ');

                // write type
                writer.write(type);
                writer.write(' ');

                // write l2r true == 1
                if (headRule.leftToRight)
                {
                    writer.write("1");
                }
                else
                {
                    writer.write("0");
                }

                // write tags
                foreach (string tag in headRule.tags)
                {
                    writer.write(' ');
                    writer.write(tag);
                }

                writer.write('\n');
            }

            writer.flush();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            else if (obj is HeadRules)
            {
                HeadRules rules = (HeadRules) obj;

                return rules.headRules.Equals(headRules) && rules.punctSet.Equals(punctSet);
            }
            else
            {
                return false;
            }
        }
    }
}