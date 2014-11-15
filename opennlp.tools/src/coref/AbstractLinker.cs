using System;

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

namespace opennlp.tools.coref
{
    using HeadFinder = opennlp.tools.coref.mention.HeadFinder;
    using Mention = opennlp.tools.coref.mention.Mention;
    using MentionContext = opennlp.tools.coref.mention.MentionContext;
    using MentionFinder = opennlp.tools.coref.mention.MentionFinder;
    using Parse = opennlp.tools.coref.mention.Parse;
    using AbstractResolver = opennlp.tools.coref.resolver.AbstractResolver;
    using Gender = opennlp.tools.coref.sim.Gender;
    using Number = opennlp.tools.coref.sim.Number;

    /// <summary>
    /// Provides a default implementation of many of the methods in <seealso cref="Linker"/> that
    /// most implementations of <seealso cref="Linker"/> will want to extend.
    /// </summary>
    public abstract class AbstractLinker : Linker
    {
        /// <summary>
        /// The mention finder used to find mentions. </summary>
        protected internal MentionFinder mentionFinder;

        /// <summary>
        /// Specifies whether debug print is generated. </summary>
        protected internal bool debug = true;

        /// <summary>
        /// The mode in which this linker is running. </summary>
        protected internal LinkerMode mode;

        /// <summary>
        /// Instance used for for returning the same linker for subsequent getInstance requests. </summary>
        protected internal static Linker linker;

        /// <summary>
        /// The resolvers used by this Linker. </summary>
        protected internal AbstractResolver[] resolvers;

        /// <summary>
        /// The names of the resolvers used by this Linker. </summary>
        protected internal string[] resolverNames;

        /// <summary>
        /// Array used to store the results of each call made to the linker. </summary>
        protected internal DiscourseEntity[] entities;

        /// <summary>
        /// The index of resolver which is used for singular pronouns. </summary>
        protected internal int SINGULAR_PRONOUN;

        /// <summary>
        /// The name of the project where the coreference models are stored. </summary>
        protected internal string corefProject;

        /// <summary>
        /// The head finder used in this linker. </summary>
        protected internal HeadFinder headFinder;

        /// <summary>
        /// Specifies whether coreferent mentions should be combined into a single entity.
        /// Set this to true to combine them, false otherwise.  
        /// </summary>
        protected internal bool useDiscourseModel;

        /// <summary>
        /// Specifies whether mentions for which no resolver can be used should be added to the
        /// discourse model.
        /// </summary>
        protected internal bool removeUnresolvedMentions;

        /// <summary>
        /// Creates a new linker using the models in the specified project directory and using the specified mode. </summary>
        /// <param name="project"> The location of the models or other data needed by this linker. </param>
        /// <param name="mode"> The mode the linker should be run in: testing, training, or evaluation. </param>
        public AbstractLinker(string project, LinkerMode mode) : this(project, mode, true)
        {
        }

        /// <summary>
        /// Creates a new linker using the models in the specified project directory, using the specified mode,
        /// and combining coreferent entities based on the specified value. </summary>
        /// <param name="project"> The location of the models or other data needed by this linker. </param>
        /// <param name="mode"> The mode the linker should be run in: testing, training, or evaluation. </param>
        /// <param name="useDiscourseModel"> Specifies whether coreferent mention should be combined or not. </param>
        public AbstractLinker(string project, LinkerMode mode, bool useDiscourseModel)
        {
            this.corefProject = project;
            this.mode = mode;
            SINGULAR_PRONOUN = -1;
            this.useDiscourseModel = useDiscourseModel;
            removeUnresolvedMentions = true;
        }

        /// <summary>
        /// Resolves the specified mention to an entity in the specified discourse model or creates a new entity for the mention. </summary>
        /// <param name="mention"> The mention to resolve. </param>
        /// <param name="discourseModel"> The discourse model of existing entities. </param>
        protected internal virtual void resolve(MentionContext mention, DiscourseModel discourseModel)
        {
            //System.err.println("AbstractLinker.resolve: "+mode+"("+econtext.id+") "+econtext.toText());
            bool validEntity = true; // true if we should add this entity to the dm
            bool canResolve = false;

            for (int ri = 0; ri < resolvers.Length; ri++)
            {
                if (resolvers[ri].canResolve(mention))
                {
                    if (mode == LinkerMode.TEST)
                    {
                        entities[ri] = resolvers[ri].resolve(mention, discourseModel);
                        canResolve = true;
                    }
                    else if (mode == LinkerMode.TRAIN)
                    {
                        entities[ri] = resolvers[ri].retain(mention, discourseModel);
                        if (ri + 1 != resolvers.Length)
                        {
                            canResolve = true;
                        }
                    }
                    else if (mode == LinkerMode.EVAL)
                    {
                        entities[ri] = resolvers[ri].retain(mention, discourseModel);
                        //DiscourseEntity rde = resolvers[ri].resolve(mention, discourseModel);
                        //eval.update(rde == entities[ri], ri, entities[ri], rde);
                    }
                    else
                    {
                        Console.Error.WriteLine("AbstractLinker.Unknown mode: " + mode);
                    }
                    if (ri == SINGULAR_PRONOUN && entities[ri] == null)
                    {
                        validEntity = false;
                    }
                }
                else
                {
                    entities[ri] = null;
                }
            }
            if (!canResolve && removeUnresolvedMentions)
            {
                //System.err.println("No resolver for: "+econtext.toText()+ " head="+econtext.headTokenText+" "+econtext.headTokenTag);
                validEntity = false;
            }
            DiscourseEntity de = checkForMerges(discourseModel, entities);
            if (validEntity)
            {
                updateExtent(discourseModel, mention, de, useDiscourseModel);
            }
        }

        public virtual HeadFinder HeadFinder
        {
            get { return headFinder; }
        }

        /// <summary>
        /// Updates the specified discourse model with the specified mention as coreferent with the specified entity. </summary>
        /// <param name="dm"> The discourse model </param>
        /// <param name="mention"> The mention to be added to the specified entity. </param>
        /// <param name="entity"> The entity which is mentioned by the specified mention. </param>
        /// <param name="useDiscourseModel"> Whether the mentions should be kept as an entiy or simply co-indexed. </param>
        protected internal virtual void updateExtent(DiscourseModel dm, MentionContext mention, DiscourseEntity entity,
            bool useDiscourseModel)
        {
            if (useDiscourseModel)
            {
                if (entity != null)
                {
                    //System.err.println("AbstractLinker.updateExtent: addingExtent:
                    // "+econtext.toText());
                    if (entity.GenderProbability < mention.GenderProb)
                    {
                        entity.Gender = mention.Gender;
                        entity.GenderProbability = mention.GenderProb;
                    }
                    if (entity.NumberProbability < mention.NumberProb)
                    {
                        entity.Number = mention.Number;
                        entity.NumberProbability = mention.NumberProb;
                    }
                    entity.addMention(mention);
                    dm.mentionEntity(entity);
                }
                else
                {
                    //System.err.println("AbstractLinker.updateExtent: creatingExtent:
                    // "+econtext.toText()+" "+econtext.gender+" "+econtext.number);
                    entity = new DiscourseEntity(mention, mention.Gender, mention.GenderProb, mention.Number,
                        mention.NumberProb);
                    dm.addEntity(entity);
                }
            }
            else
            {
                if (entity != null)
                {
                    DiscourseEntity newEntity = new DiscourseEntity(mention, mention.Gender, mention.GenderProb,
                        mention.Number, mention.NumberProb);
                    dm.addEntity(newEntity);
                    newEntity.Id = entity.Id;
                }
                else
                {
                    DiscourseEntity newEntity = new DiscourseEntity(mention, mention.Gender, mention.GenderProb,
                        mention.Number, mention.NumberProb);
                    dm.addEntity(newEntity);
                }
            }
            //System.err.println(de1);
        }

        protected internal virtual DiscourseEntity checkForMerges(DiscourseModel dm, DiscourseEntity[] des)
        {
            DiscourseEntity de1; //tempory variable
            DiscourseEntity de2; //tempory variable
            de1 = des[0];
            for (int di = 1; di < des.Length; di++)
            {
                de2 = des[di];
                if (de2 != null)
                {
                    if (de1 != null && de1 != de2)
                    {
                        dm.mergeEntities(de1, de2, 1);
                    }
                    else
                    {
                        de1 = de2;
                    }
                }
            }
            return (de1);
        }

        public virtual DiscourseEntity[] getEntities(Mention[] mentions)
        {
            MentionContext[] extentContexts = this.constructMentionContexts(mentions);
            DiscourseModel dm = new DiscourseModel();
            for (int ei = 0; ei < extentContexts.Length; ei++)
            {
                //System.err.println(ei+" "+extentContexts[ei].toText());
                resolve(extentContexts[ei], dm);
            }
            return (dm.Entities);
        }

        public virtual Mention[] Entities
        {
            set { getEntities(value); }
        }

        public virtual void train()
        {
            for (int ri = 0; ri < resolvers.Length; ri++)
            {
                resolvers[ri].train();
            }
        }

        public virtual MentionFinder MentionFinder
        {
            get { return mentionFinder; }
        }

        public virtual MentionContext[] constructMentionContexts(Mention[] mentions)
        {
            int mentionInSentenceIndex = -1;
            int numMentionsInSentence = -1;
            int prevSentenceIndex = -1;
            MentionContext[] contexts = new MentionContext[mentions.Length];
            for (int mi = 0, mn = mentions.Length; mi < mn; mi++)
            {
                Parse mentionParse = mentions[mi].Parse;
                //System.err.println("AbstractLinker.constructMentionContexts: mentionParse="+mentionParse);
                if (mentionParse == null)
                {
                    Console.Error.WriteLine("no parse for " + mentions[mi]);
                }
                int sentenceIndex = mentionParse.SentenceNumber;
                if (sentenceIndex != prevSentenceIndex)
                {
                    mentionInSentenceIndex = 0;
                    prevSentenceIndex = sentenceIndex;
                    numMentionsInSentence = 0;
                    for (int msi = mi; msi < mentions.Length; msi++)
                    {
                        if (sentenceIndex != mentions[msi].Parse.SentenceNumber)
                        {
                            break;
                        }
                        numMentionsInSentence++;
                    }
                }
                contexts[mi] = new MentionContext(mentions[mi], mentionInSentenceIndex, numMentionsInSentence, mi,
                    sentenceIndex, HeadFinder);
                //System.err.println("AbstractLinker.constructMentionContexts: mi="+mi+" sn="+mentionParse.getSentenceNumber()+" extent="+mentions[mi]+" parse="+mentionParse.getSpan()+" mc="+contexts[mi].toText());
                contexts[mi].Id = mentions[mi].Id;
                mentionInSentenceIndex++;
                if (mode != LinkerMode.SIM)
                {
                    Gender g = computeGender(contexts[mi]);
                    contexts[mi].setGender(g.Type, g.Confidence);
                    Number n = computeNumber(contexts[mi]);
                    contexts[mi].setNumber(n.Type, n.Confidence);
                }
            }
            return (contexts);
        }

        protected internal abstract Gender computeGender(MentionContext mention);
        protected internal abstract Number computeNumber(MentionContext mention);
    }
}