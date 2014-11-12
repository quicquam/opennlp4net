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

namespace opennlp.tools.coref.sim
{
    using Dictionary = opennlp.tools.coref.mention.Dictionary;
    using DictionaryFactory = opennlp.tools.coref.mention.DictionaryFactory;
    using HeadFinder = opennlp.tools.coref.mention.HeadFinder;
    using Mention = opennlp.tools.coref.mention.Mention;
    using Parse = opennlp.tools.coref.mention.Parse;
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Specifies the context of a mention for computing gender, number, and semantic compatibility.
    /// </summary>
    public class Context : Mention
    {
        protected internal string headTokenText;
        protected internal string headTokenTag;
        protected internal HashSet<string> synsets;
        protected internal object[] tokens;

        /// <summary>
        /// The token index in of the head word of this mention. </summary>
        protected internal int headTokenIndex;

        public Context(Span span, Span headSpan, int entityId, Parse parse, string extentType, string nameType,
            HeadFinder headFinder) : base(span, headSpan, entityId, parse, extentType, nameType)
        {
            init(headFinder);
        }

        public Context(object[] tokens, string headToken, string headTag, string neType)
            : base(null, null, 1, null, null, neType)
        {
            this.tokens = tokens;
            this.headTokenIndex = tokens.Length - 1;
            this.headTokenText = headToken;
            this.headTokenTag = headTag;
            this.synsets = getSynsetSet(this);
        }

        public Context(Mention mention, HeadFinder headFinder) : base(mention)
        {
            init(headFinder);
        }

        private void init(HeadFinder headFinder)
        {
            Parse head = headFinder.getLastHead(parse);
            IList<Parse> tokenList = head.Tokens;
            headTokenIndex = headFinder.getHeadIndex(head);
            Parse headToken = headFinder.getHeadToken(head);
            tokens = tokenList.ToArray();
            this.headTokenTag = headToken.SyntacticType;
            this.headTokenText = headToken.ToString();
            if (headTokenTag.StartsWith("NN", StringComparison.Ordinal) &&
                !headTokenTag.StartsWith("NNP", StringComparison.Ordinal))
            {
                this.synsets = getSynsetSet(this);
            }
            else
            {
                this.synsets = new HashSet<string>();
            }
        }


        public static Context[] constructContexts(Mention[] mentions, HeadFinder headFinder)
        {
            Context[] contexts = new Context[mentions.Length];
            for (int mi = 0; mi < mentions.Length; mi++)
            {
                contexts[mi] = new Context(mentions[mi], headFinder);
            }
            return contexts;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int ti = 0, tl = tokens.Length; ti < tl; ti++)
            {
                sb.Append(tokens[ti]).Append(" ");
            }
            return sb.ToString();
        }

        public virtual object[] Tokens
        {
            get { return tokens; }
        }

        public virtual string HeadTokenText
        {
            get { return headTokenText; }
        }

        public virtual string HeadTokenTag
        {
            get { return headTokenTag; }
        }

        public virtual HashSet<string> Synsets
        {
            get { return synsets; }
        }

        public static Context parseContext(string word)
        {
            string[] parts = word.Split('/'); // was true
            if (parts.Length == 2)
            {
                string[] tokens = parts[0].Split(' '); //was true
                return new Context(tokens, tokens[tokens.Length - 1], parts[1], null);
            }
            else if (parts.Length == 3)
            {
                string[] tokens = parts[0].Split(' '); // was true
                return new Context(tokens, tokens[tokens.Length - 1], parts[1], parts[2]);
            }
            return null;
        }

        private static HashSet<string> getSynsetSet(Context c)
        {
            HashSet<string> synsetSet = new HashSet<string>();
            string[] lemmas = getLemmas(c);
            Dictionary dict = DictionaryFactory.Dictionary;
            //System.err.println(lemmas.length+" lemmas for "+c.headToken);
            for (int li = 0; li < lemmas.Length; li++)
            {
                string senseKey = dict.getSenseKey(lemmas[li], "NN", 0);
                if (senseKey != null)
                {
                    synsetSet.Add(senseKey);
                    string[] synsets = dict.getParentSenseKeys(lemmas[li], "NN", 0);
                    for (int si = 0, sn = synsets.Length; si < sn; si++)
                    {
                        synsetSet.Add(synsets[si]);
                    }
                }
            }
            return synsetSet;
        }

        private static string[] getLemmas(Context c)
        {
            string word = c.headTokenText.ToLower();
            return DictionaryFactory.Dictionary.getLemmas(word, "NN");
        }

        /// <summary>
        /// Returns the token index into the mention for the head word. </summary>
        /// <returns> the token index into the mention for the head word. </returns>
        public virtual int HeadTokenIndex
        {
            get { return headTokenIndex; }
        }
    }
}