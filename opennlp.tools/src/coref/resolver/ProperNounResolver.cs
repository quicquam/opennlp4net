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
using System.Linq;
using j4n.IO.Reader;
using j4n.Object;

namespace opennlp.tools.coref.resolver
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;

    /// <summary>
    /// Resolves coreference between proper nouns.
    /// </summary>
    public class ProperNounResolver : MaxentResolver
    {
        private static IDictionary<string, HashSet<string>> acroMap;
        private static bool acroMapLoaded = false;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ProperNounResolver(String projectName, ResolverMode m) throws java.io.IOException
        public ProperNounResolver(string projectName, ResolverMode m) : base(projectName, "pnmodel", m, 500)
        {
            if (!acroMapLoaded)
            {
                initAcronyms(projectName + "/acronyms");
                acroMapLoaded = true;
            }
            showExclusions = false;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ProperNounResolver(String projectName, ResolverMode m,NonReferentialResolver nonRefResolver) throws java.io.IOException
        public ProperNounResolver(string projectName, ResolverMode m, NonReferentialResolver nonRefResolver)
            : base(projectName, "pnmodel", m, 500, nonRefResolver)
        {
            if (!acroMapLoaded)
            {
                initAcronyms(projectName + "/acronyms");
                acroMapLoaded = true;
            }
            showExclusions = false;
        }

        public override bool canResolve(MentionContext mention)
        {
            return (mention.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal) ||
                    mention.HeadTokenTag.StartsWith("CD", StringComparison.Ordinal));
        }

        private void initAcronyms(string name)
        {
            acroMap = new Dictionary<string, HashSet<string>>(15000);
            try
            {
                BufferedReader str;
                str = new BufferedReader(new FileReader(name));
                //System.err.println("Reading acronyms database: " + file + " ");
                string line;
                while (null != (line = str.readLine()))
                {
                    StringTokenizer st = new StringTokenizer(line, "\t");
                    string acro = st.nextToken();
                    string full = st.nextToken();
                    HashSet<string> exSet = acroMap[acro];
                    if (exSet == null)
                    {
                        exSet = new HashSet<string>();
                        acroMap[acro] = exSet;
                    }
                    exSet.Add(full);
                    exSet = acroMap[full];
                    if (exSet == null)
                    {
                        exSet = new HashSet<string>();
                        acroMap[full] = exSet;
                    }
                    exSet.Add(acro);
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("ProperNounResolver.initAcronyms: Acronym Database not found: " + e);
            }
        }

        private bool isAcronym(string ecStrip, string xecStrip)
        {
            HashSet<string> exSet = acroMap[ecStrip];
            if (exSet != null && exSet.Contains(xecStrip))
            {
                return true;
            }
            return false;
        }

        protected internal virtual IList<string> getAcronymFeatures(MentionContext mention, DiscourseEntity entity)
        {
            MentionContext xec = ResolverUtils.getProperNounExtent(entity);
            string ecStrip = ResolverUtils.stripNp(mention);
            string xecStrip = ResolverUtils.stripNp(xec);
            if (ecStrip != null && xecStrip != null)
            {
                if (isAcronym(ecStrip, xecStrip))
                {
                    IList<string> features = new List<string>(1);
                    features.Add("knownAcronym");
                    return features;
                }
            }
            return new List<string>();
        }

        protected internal override IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
        {
            //System.err.println("ProperNounResolver.getFeatures: "+mention.toText()+" -> "+entity);
            IList<string> features = base.getFeatures(mention, entity).ToList();
            if (entity != null)
            {
                foreach (var feature in ResolverUtils.getStringMatchFeatures(mention, entity))
                {
                    features.Add(feature);
                }
                foreach (var feature in getAcronymFeatures(mention, entity))
                {
                    features.Add(feature);
                }
            }
            return features;
        }

        protected internal override bool excluded(MentionContext mention, DiscourseEntity entity)
        {
            if (base.excluded(mention, entity))
            {
                return true;
            }

            for (IEnumerator<MentionContext> ei = entity.Mentions; ei.MoveNext();)
            {
                MentionContext xec = ei.Current;
                if (xec.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal))
                    // || initialCaps.matcher(xec.headToken.toString()).find()) {
                {
                    //System.err.println("MaxentProperNounResolver.exclude: kept "+xec.toText()+" with "+xec.headTag);
                    return false;
                }
            }

            return true;
        }
    }
}