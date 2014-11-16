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
    /// Resolves pronouns specific to quoted speech such as "you", "me", and "I".
    /// </summary>
    public class SpeechPronounResolver : MaxentResolver
    {
        public SpeechPronounResolver(string projectName, ResolverMode m) : base(projectName, "fmodel", m, 30)
        {
            this.numSentencesBack = 0;
            showExclusions = false;
            preferFirstReferent = true;
        }

        public SpeechPronounResolver(string projectName, ResolverMode m, NonReferentialResolver nrr)
            : base(projectName, "fmodel", m, 30, nrr)
        {
            showExclusions = false;
            preferFirstReferent = true;
        }


        protected internal override IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
        {
            IList<string> features = new List<string>();
            features.AddRange(base.getFeatures(mention, entity));
            if (entity != null)
            {
                features.AddRange(ResolverUtils.getPronounMatchFeatures(mention, entity));
                IList<string> contexts = ResolverUtils.getContextFeatures(mention);
                MentionContext cec = entity.LastExtent;
                if (mention.HeadTokenTag.StartsWith("PRP", StringComparison.Ordinal) &&
                    cec.HeadTokenTag.StartsWith("PRP", StringComparison.Ordinal))
                {
                    features.Add(mention.HeadTokenText + "," + cec.HeadTokenText);
                }
                else if (mention.HeadTokenText.StartsWith("NNP", StringComparison.Ordinal))
                {
                    for (int ci = 0, cl = contexts.Count; ci < cl; ci++)
                    {
                        features.Add(contexts[ci]);
                    }
                    features.Add(mention.NameType + "," + cec.HeadTokenText);
                }
                else
                {
                    IList<string> ccontexts = ResolverUtils.getContextFeatures(cec);
                    for (int ci = 0, cl = ccontexts.Count; ci < cl; ci++)
                    {
                        features.Add(ccontexts[ci]);
                    }
                    features.Add(cec.NameType + "," + mention.HeadTokenText);
                }
            }
            return (features);
        }

        protected internal override bool outOfRange(MentionContext mention, DiscourseEntity entity)
        {
            MentionContext cec = entity.LastExtent;
            return (mention.SentenceNumber - cec.SentenceNumber > numSentencesBack);
        }

        public override bool canResolve(MentionContext mention)
        {
            string tag = mention.HeadTokenTag;
            bool fpp = tag != null && tag.StartsWith("PRP", StringComparison.Ordinal) &&
                       ResolverUtils.speechPronounPattern.matcher(mention.HeadTokenText).matches();
            bool pn = tag != null && tag.StartsWith("NNP", StringComparison.Ordinal);
            return (fpp || pn);
        }

        protected internal override bool excluded(MentionContext mention, DiscourseEntity entity)
        {
            if (base.excluded(mention, entity))
            {
                return true;
            }
            MentionContext cec = entity.LastExtent;
            if (!canResolve(cec))
            {
                return true;
            }
            if (mention.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal)) //mention is a propernoun
            {
                if (cec.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal))
                {
                    return true; // both NNP
                }
                else
                {
                    if (entity.NumMentions > 1)
                    {
                        return true;
                    }
                    return !canResolve(cec);
                }
            }
            else if (mention.HeadTokenTag.StartsWith("PRP", StringComparison.Ordinal)) // mention is a speech pronoun
            {
                // cec can be either a speech pronoun or a propernoun
                if (cec.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal))
                {
                    //exclude antecedents not in the same sentence when they are not pronoun
                    return (mention.SentenceNumber - cec.SentenceNumber != 0);
                }
                else if (cec.HeadTokenTag.StartsWith("PRP", StringComparison.Ordinal))
                {
                    return false;
                }
                else
                {
                    Console.Error.WriteLine("Unexpected candidate exluded: " + cec.toText());
                    return true;
                }
            }
            else
            {
                Console.Error.WriteLine("Unexpected mention exluded: " + mention.toText());
                return true;
            }
        }
    }
}