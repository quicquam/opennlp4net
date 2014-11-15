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
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.coref.resolver
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;

    /// <summary>
    /// Resolves coreference between definite noun-phrases.
    /// </summary>
    public class DefiniteNounResolver : MaxentResolver
    {
        public DefiniteNounResolver(string projectName, ResolverMode m) : base(projectName, "defmodel", m, 80)
        {
            //preferFirstReferent = true;
        }

        public DefiniteNounResolver(string projectName, ResolverMode m, NonReferentialResolver nrr)
            : base(projectName, "defmodel", m, 80, nrr)
        {
            //preferFirstReferent = true;
        }


        public override bool canResolve(MentionContext mention)
        {
            object[] mtokens = mention.Tokens;

            string firstTok = mention.FirstTokenText.ToLower();
            bool rv = mtokens.Length > 1 && !mention.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal) &&
                      ResolverUtils.definiteArticle(firstTok, mention.FirstTokenTag);
            //if (rv) {
            //  System.err.println("defNp "+ec);
            //}
            return (rv);
        }

        protected internal override IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
        {
            IList<string> features = new List<string>();
            features.AddRange(base.getFeatures(mention, entity));
            if (entity != null)
            {
                features.AddRange(ResolverUtils.getContextFeatures(mention));
                features.AddRange(ResolverUtils.getStringMatchFeatures(mention, entity));
                features.AddRange(ResolverUtils.getDistanceFeatures(mention, entity));
            }
            return (features);
        }
    }
}