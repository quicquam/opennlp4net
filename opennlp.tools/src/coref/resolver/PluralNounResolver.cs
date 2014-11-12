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
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.coref.resolver
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;


    /// <summary>
    /// Resolves coreference between plural nouns.
    /// </summary>
    public class PluralNounResolver : MaxentResolver
    {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PluralNounResolver(String projectName, ResolverMode m) throws java.io.IOException
        public PluralNounResolver(string projectName, ResolverMode m) : base(projectName, "plmodel", m, 80, true)
        {
            showExclusions = false;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PluralNounResolver(String projectName, ResolverMode m, NonReferentialResolver nrr) throws java.io.IOException
        public PluralNounResolver(string projectName, ResolverMode m, NonReferentialResolver nrr)
            : base(projectName, "plmodel", m, 80, true, nrr)
        {
            showExclusions = false;
        }


        protected internal override IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
        {
            IList<string> features = new List<string>();
            features.AddRange(base.getFeatures(mention, entity));
            if (entity != null)
            {
                features.AddRange(ResolverUtils.getContextFeatures(mention));
                features.AddRange(ResolverUtils.getStringMatchFeatures(mention, entity));
            }

            return features;
        }

        public override bool canResolve(MentionContext mention)
        {
            string firstTok = mention.FirstTokenText.ToLower();
            string firstTokTag = mention.FirstToken.SyntacticType;
            bool rv = mention.HeadTokenTag.Equals("NNS") && !ResolverUtils.definiteArticle(firstTok, firstTokTag);
            return rv;
        }

        protected internal override bool excluded(MentionContext mention, DiscourseEntity entity)
        {
            if (base.excluded(mention, entity))
            {
                return true;
            }
            else
            {
                MentionContext cec = entity.LastExtent;
                return (!cec.HeadTokenTag.Equals("NNS") || base.excluded(mention, entity));
            }
        }
    }
}