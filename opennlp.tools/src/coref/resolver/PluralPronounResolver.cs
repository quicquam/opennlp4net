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
    /// Resolves coreference between plural pronouns and their referents.
    /// </summary>
    public class PluralPronounResolver : MaxentResolver
    {
        internal int NUM_SENTS_BACK_PRONOUNS = 2;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PluralPronounResolver(String projectName, ResolverMode m) throws java.io.IOException
        public PluralPronounResolver(string projectName, ResolverMode m) : base(projectName, "tmodel", m, 30)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PluralPronounResolver(String projectName, ResolverMode m,NonReferentialResolver nrr) throws java.io.IOException
        public PluralPronounResolver(string projectName, ResolverMode m, NonReferentialResolver nrr)
            : base(projectName, "tmodel", m, 30, nrr)
        {
        }

        protected internal override IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
        {
            IList<string> features = new List<string>();
            features.AddRange(base.getFeatures(mention, entity));
            //features.add("eid="+pc.id);
            if (entity != null) //generate pronoun w/ referent features
            {
                features.AddRange(ResolverUtils.getPronounMatchFeatures(mention, entity));
                MentionContext cec = entity.LastExtent;
                features.AddRange(ResolverUtils.getDistanceFeatures(mention, entity));
                features.AddRange(ResolverUtils.getContextFeatures(cec));
                features.Add(ResolverUtils.getMentionCountFeature(entity));
                /*
		  //lexical features
		  Set featureSet = new HashSet();
		  for (Iterator ei = entity.getExtents(); ei.hasNext();) {
		    MentionContext ec = (MentionContext) ei.next();
		    int headIndex = PTBHeadFinder.getInstance().getHeadIndex(ec.tokens);
		    Parse tok = (Parse) ec.tokens.get(headIndex);
		    featureSet.add("hw=" + tok.toString().toLowerCase());
		    if (ec.parse.isCoordinatedNounPhrase()) {
		      featureSet.add("ht=CC");
		    }
		    else {
		      featureSet.add("ht=" + tok.getSyntacticType());
		    }
		    if (ec.neType != null){
		      featureSet.add("ne="+ec.neType);
		    }
		  }
		  Iterator fset = featureSet.iterator();
		  while (fset.hasNext()) {
		    String f = (String) fset.next();
		    features.add(f);
		  }
		  */
            }
            return (features);
        }

        protected internal override bool outOfRange(MentionContext mention, DiscourseEntity entity)
        {
            MentionContext cec = entity.LastExtent;
            //System.err.println("MaxentPluralPronounResolver.outOfRange: ["+ec.toText()+" ("+ec.id+")] ["+cec.toText()+" ("+cec.id+")] ec.sentenceNumber=("+ec.sentenceNumber+")-cec.sentenceNumber=("+cec.sentenceNumber+") > "+NUM_SENTS_BACK_PRONOUNS);
            return (mention.SentenceNumber - cec.SentenceNumber > NUM_SENTS_BACK_PRONOUNS);
        }

        public override bool canResolve(MentionContext mention)
        {
            string tag = mention.HeadTokenTag;
            return (tag != null && tag.StartsWith("PRP", StringComparison.Ordinal) &&
                    ResolverUtils.pluralThirdPersonPronounPattern.matcher(mention.HeadTokenText).matches());
        }
    }
}