using System;
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

namespace opennlp.tools.coref.resolver
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;
    using Parse = opennlp.tools.coref.mention.Parse;
    using opennlp.tools.util;

    /// <summary>
    /// Default implementation of some methods in the <seealso cref="Resolver"/> interface.
    /// </summary>
    public abstract class AbstractResolver : Resolver
    {
        public abstract DiscourseEntity resolve(MentionContext ec, DiscourseModel dm);
        public abstract bool canResolve(MentionContext mention);

        /// <summary>
        /// The number of previous entities that resolver should consider.
        /// </summary>
        protected internal int numEntitiesBack;

        /// <summary>
        /// Debugging variable which specifies whether error output is generated
        /// if a class excludes as possibly coreferent mentions which are in-fact
        /// coreferent.
        /// </summary>
        protected internal bool showExclusions;

        /// <summary>
        /// Debugging variable which holds statistics about mention distances
        /// during training.
        /// </summary>
        protected internal CountedSet<int?> distances;

        /// <summary>
        /// The number of sentences back this resolver should look for a referent.
        /// </summary>
        protected internal int numSentencesBack;

        public AbstractResolver(int neb)
        {
            numEntitiesBack = neb;
            showExclusions = true;
            distances = new CountedSet<int?>();
        }

        /// <summary>
        /// Returns the number of previous entities that resolver should consider.
        /// </summary>
        /// <returns> the number of previous entities that resolver should consider. </returns>
        protected internal virtual int NumEntities
        {
            get { return numEntitiesBack; }
        }

        /// <summary>
        /// Specifies the number of sentences back this resolver should look for a referent.
        /// </summary>
        /// <param name="nsb"> the number of sentences back this resolver should look for a referent. </param>
        public virtual int NumberSentencesBack
        {
            set { numSentencesBack = value; }
        }

        /// <summary>
        /// The number of entities that should be considered for resolution with the specified discourse model.
        /// </summary>
        /// <param name="dm"> The discourse model.
        /// </param>
        /// <returns> number of entities that should be considered for resolution. </returns>
        protected internal virtual int getNumEntities(DiscourseModel dm)
        {
            return Math.Min(dm.NumEntities, numEntitiesBack);
        }

        /// <summary>
        /// Returns the head parse for the specified mention.
        /// </summary>
        /// <param name="mention"> The mention.
        /// </param>
        /// <returns> the head parse for the specified mention. </returns>
        protected internal virtual Parse getHead(MentionContext mention)
        {
            return mention.HeadTokenParse;
        }

        /// <summary>
        /// Returns the index for the head word for the specified mention.
        /// </summary>
        /// <param name="mention"> The mention.
        /// </param>
        /// <returns> the index for the head word for the specified mention. </returns>
        protected internal virtual int getHeadIndex(MentionContext mention)
        {
            Parse[] mtokens = mention.TokenParses;
            for (int ti = mtokens.Length - 1; ti >= 0; ti--)
            {
                Parse tok = mtokens[ti];
                if (!tok.SyntacticType.Equals("POS") && !tok.SyntacticType.Equals(",") && !tok.SyntacticType.Equals("."))
                {
                    return ti;
                }
            }
            return mtokens.Length - 1;
        }

        /// <summary>
        /// Returns the text of the head word for the specified mention.
        /// </summary>
        /// <param name="mention"> The mention.
        /// </param>
        /// <returns> The text of the head word for the specified mention. </returns>
        protected internal virtual string getHeadString(MentionContext mention)
        {
            return mention.HeadTokenText.ToLower();
        }

        /// <summary>
        /// Determines if the specified entity is too far from the specified mention to be resolved to it.
        /// Once an entity has been determined to be out of range subsequent entities are not considered. </summary>
        /// To skip intermediate entities <seealso cref= excluded.
        /// </seealso>
        /// <param name="mention"> The mention which is being considered. </param>
        /// <param name="entity"> The entity to which the mention is to be resolved.
        /// </param>
        /// <returns> true is the entity is in range of the mention, false otherwise. </returns>
        protected internal virtual bool outOfRange(MentionContext mention, DiscourseEntity entity)
        {
            return false;
        }

        /// <summary>
        /// Excludes entities which you are not compatible with the entity under consideration.  The default
        /// implementation excludes entities whose last extent contains the extent under consideration.
        /// This prevents possessive pronouns from referring to the noun phrases they modify and other
        /// undesirable things.
        /// </summary>
        /// <param name="mention"> The mention which is being considered as referential. </param>
        /// <param name="entity"> The entity to which the mention is to be resolved.
        /// </param>
        /// <returns> true if the entity should be excluded, false otherwise. </returns>
        protected internal virtual bool excluded(MentionContext mention, DiscourseEntity entity)
        {
            MentionContext cec = entity.LastExtent;
            return mention.SentenceNumber == cec.SentenceNumber && mention.IndexSpan.End <= cec.IndexSpan.End;
        }

        public virtual DiscourseEntity retain(MentionContext mention, DiscourseModel dm)
        {
            int ei = 0;
            if (mention.Id == -1)
            {
                return null;
            }
            for (; ei < dm.NumEntities; ei++)
            {
                DiscourseEntity cde = dm.getEntity(ei);
                MentionContext cec = cde.LastExtent; // candidate extent context
                if (cec.Id == mention.Id)
                {
                    distances.add(ei);
                    return cde;
                }
            }
            //System.err.println("AbstractResolver.retain: non-refering entity with id: "+ec.toText()+" id="+ec.id);
            return null;
        }

        /// <summary>
        /// Returns the string of "_" delimited tokens for the specified mention.
        /// </summary>
        /// <param name="mention"> The mention.
        /// </param>
        /// <returns> the string of "_" delimited tokens for the specified mention. </returns>
        protected internal virtual string featureString(MentionContext mention)
        {
            StringBuilder fs = new StringBuilder();
            object[] mtokens = mention.Tokens;
            fs.Append(mtokens[0].ToString());
            for (int ti = 1, tl = mtokens.Length; ti < tl; ti++)
            {
                fs.Append("_").Append(mtokens[ti].ToString());
            }
            return fs.ToString();
        }

        public virtual void train()
        {
        }
    }
}