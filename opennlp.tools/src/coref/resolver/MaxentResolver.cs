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
using j4n.IO.File;
using j4n.IO.Writer;
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.coref.resolver
{
    using GIS = opennlp.maxent.GIS;
    using SuffixSensitiveGISModelReader = opennlp.maxent.io.SuffixSensitiveGISModelReader;
    using SuffixSensitiveGISModelWriter = opennlp.maxent.io.SuffixSensitiveGISModelWriter;
    using Event = opennlp.model.Event;
    using MaxentModel = opennlp.model.MaxentModel;
    using MentionContext = opennlp.tools.coref.mention.MentionContext;
    using TestSimilarityModel = opennlp.tools.coref.sim.TestSimilarityModel;
    using CollectionEventStream = opennlp.tools.util.CollectionEventStream;

    /// <summary>
    ///  Provides common functionality used by classes which implement the <seealso cref="Resolver"/> class and use maximum entropy models to make resolution decisions.
    /// </summary>
    public abstract class MaxentResolver : AbstractResolver
    {
        /// <summary>
        /// Outcomes when two mentions are coreferent. </summary>
        public const string SAME = "same";

        /// <summary>
        /// Outcome when two mentions are not coreferent. </summary>
        public const string DIFF = "diff";

        /// <summary>
        /// Default feature value. </summary>
        public const string DEFAULT = "default";


        private static bool debugOn = false;

        private string modelName;
        private MaxentModel model;
        private double[] candProbs;
        private int sameIndex;
        private ResolverMode mode;
        private IList<Event> events;

        /// <summary>
        /// When true, this designates that the resolver should use the first referent encountered which it
        /// more preferable than non-reference.  When false all non-excluded referents within this resolvers range
        /// are considered.
        /// </summary>
        protected internal bool preferFirstReferent;

        /// <summary>
        /// When true, this designates that training should consist of a single positive and a single negative example
        /// (when possible) for each mention. 
        /// </summary>
        protected internal bool pairedSampleSelection;

        /// <summary>
        /// When true, this designates that the same maximum entropy model should be used non-reference
        /// events (the pairing of a mention and the "null" reference) as is used for potentially
        /// referential pairs.  When false a separate model is created for these events.
        /// </summary>
        protected internal bool useSameModelForNonRef;

        private static TestSimilarityModel simModel = null;

        /// <summary>
        /// The model for computing non-referential probabilities. </summary>
        protected internal NonReferentialResolver nonReferentialResolver;

        private const string modelExtension = ".bin.gz";

        /// <summary>
        /// Creates a maximum-entropy-based resolver which will look the specified number of entities back for a referent.
        /// This constructor is only used for unit testing. </summary>
        /// <param name="numberOfEntitiesBack"> </param>
        /// <param name="preferFirstReferent"> </param>
        protected internal MaxentResolver(int numberOfEntitiesBack, bool preferFirstReferent)
            : base(numberOfEntitiesBack)
        {
            this.preferFirstReferent = preferFirstReferent;
        }


        /// <summary>
        /// Creates a maximum-entropy-based resolver with the specified model name, using the
        /// specified mode, which will look the specified number of entities back for a referent and
        /// prefer the first referent if specified. </summary>
        /// <param name="modelDirectory"> The name of the directory where the resolver models are stored. </param>
        /// <param name="name"> The name of the file where this model will be read or written. </param>
        /// <param name="mode"> The mode this resolver is being using in (training, testing). </param>
        /// <param name="numberOfEntitiesBack"> The number of entities back in the text that this resolver will look
        /// for a referent. </param>
        /// <param name="preferFirstReferent"> Set to true if the resolver should prefer the first referent which is more
        /// likely than non-reference.  This only affects testing. </param>
        /// <param name="nonReferentialResolver"> Determines how likely it is that this entity is non-referential. </param>
        /// <exception cref="IOException"> If the model file is not found or can not be written to. </exception>
        public MaxentResolver(string modelDirectory, string name, ResolverMode mode, int numberOfEntitiesBack,
            bool preferFirstReferent, NonReferentialResolver nonReferentialResolver) : base(numberOfEntitiesBack)
        {
            this.preferFirstReferent = preferFirstReferent;
            this.nonReferentialResolver = nonReferentialResolver;
            this.mode = mode;
            this.modelName = modelDirectory + "/" + name;
            if (ResolverMode.TEST == this.mode)
            {
                model = (new SuffixSensitiveGISModelReader(new Jfile(modelName + modelExtension))).Model;
                sameIndex = model.getIndex(SAME);
            }
            else if (ResolverMode.TRAIN == this.mode)
            {
                events = new List<Event>();
            }
            else
            {
                Console.Error.WriteLine("Unknown mode: " + this.mode);
            }
            //add one for non-referent possibility
            candProbs = new double[NumEntities + 1];
        }

        /// <summary>
        /// Creates a maximum-entropy-based resolver with the specified model name, using the
        /// specified mode, which will look the specified number of entities back for a referent. </summary>
        /// <param name="modelDirectory"> The name of the directory where the resover models are stored. </param>
        /// <param name="modelName"> The name of the file where this model will be read or written. </param>
        /// <param name="mode"> The mode this resolver is being using in (training, testing). </param>
        /// <param name="numberEntitiesBack"> The number of entities back in the text that this resolver will look
        /// for a referent. </param>
        /// <exception cref="IOException"> If the model file is not found or can not be written to. </exception>
        public MaxentResolver(string modelDirectory, string modelName, ResolverMode mode, int numberEntitiesBack)
            : this(modelDirectory, modelName, mode, numberEntitiesBack, false)
        {
        }

        public MaxentResolver(string modelDirectory, string modelName, ResolverMode mode, int numberEntitiesBack,
            NonReferentialResolver nonReferentialResolver)
            : this(modelDirectory, modelName, mode, numberEntitiesBack, false, nonReferentialResolver)
        {
        }

        public MaxentResolver(string modelDirectory, string modelName, ResolverMode mode, int numberEntitiesBack,
            bool preferFirstReferent)
            : this(
                modelDirectory, modelName, mode, numberEntitiesBack, preferFirstReferent,
                new DefaultNonReferentialResolver(modelDirectory, modelName, mode))
        {
            //this(projectName, modelName, mode, numberEntitiesBack, preferFirstReferent, SingletonNonReferentialResolver.getInstance(projectName,mode));
        }

        public MaxentResolver(string modelDirectory, string modelName, ResolverMode mode, int numberEntitiesBack,
            bool preferFirstReferent, double nonReferentialProbability)
            : this(
                modelDirectory, modelName, mode, numberEntitiesBack, preferFirstReferent,
                new FixedNonReferentialResolver(nonReferentialProbability))
        {
            //this(projectName, modelName, mode, numberEntitiesBack, preferFirstReferent, SingletonNonReferentialResolver.getInstance(projectName,mode));
        }

        public override DiscourseEntity resolve(MentionContext ec, DiscourseModel dm)
        {
            DiscourseEntity de;
            int ei = 0;
            double nonReferentialProbability = nonReferentialResolver.getNonReferentialProbability(ec);
            if (debugOn)
            {
                Console.Error.WriteLine(this + ".resolve: " + ec.toText() + " -> " + "null " + nonReferentialProbability);
            }
            for (; ei < getNumEntities(dm); ei++)
            {
                de = dm.getEntity(ei);
                if (outOfRange(ec, de))
                {
                    break;
                }
                if (excluded(ec, de))
                {
                    candProbs[ei] = 0;
                    if (debugOn)
                    {
                        Console.Error.WriteLine("excluded " + this + ".resolve: " + ec.toText() + " -> " + de + " " +
                                                candProbs[ei]);
                    }
                }
                else
                {
                    IList<string> lfeatures = getFeatures(ec, de);
                    string[] features = lfeatures.ToArray();
                    try
                    {
                        candProbs[ei] = model.eval(features)[sameIndex];
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        candProbs[ei] = 0;
                    }
                    if (debugOn)
                    {
                        Console.Error.WriteLine(this + ".resolve: " + ec.toText() + " -> " + de + " (" + ec.Gender + "," +
                                                de.Gender + ") " + candProbs[ei] + " " + lfeatures);
                    }
                }
                if (preferFirstReferent && candProbs[ei] > nonReferentialProbability)
                {
                    ei++; //update for nonRef assignment
                    break;
                }
            }
            candProbs[ei] = nonReferentialProbability;

            // find max
            int maxCandIndex = 0;
            for (int k = 1; k <= ei; k++)
            {
                if (candProbs[k] > candProbs[maxCandIndex])
                {
                    maxCandIndex = k;
                }
            }
            if (maxCandIndex == ei) // no referent
            {
                return (null);
            }
            else
            {
                de = dm.getEntity(maxCandIndex);
                return (de);
            }
        }


        /// <summary>
        /// Returns whether the specified entity satisfies the criteria for being a default referent.
        /// This criteria is used to perform sample selection on the training data and to select a single
        /// non-referent entity. Typically the criteria is a heuristic for a likely referent. </summary>
        /// <param name="de"> The discourse entity being considered for non-reference. </param>
        /// <returns> True if the entity should be used as a default referent, false otherwise. </returns>
        protected internal virtual bool defaultReferent(DiscourseEntity de)
        {
            MentionContext ec = de.LastExtent;
            if (ec.NounPhraseSentenceIndex == 0)
            {
                return (true);
            }
            return (false);
        }

        public override DiscourseEntity retain(MentionContext mention, DiscourseModel dm)
        {
            //System.err.println(this+".retain("+ec+") "+mode);
            if (ResolverMode.TRAIN == mode)
            {
                DiscourseEntity de = null;
                bool referentFound = false;
                bool hasReferentialCandidate = false;
                bool nonReferentFound = false;
                for (int ei = 0; ei < getNumEntities(dm); ei++)
                {
                    DiscourseEntity cde = dm.getEntity(ei);
                    MentionContext entityMention = cde.LastExtent;
                    if (outOfRange(mention, cde))
                    {
                        if (mention.Id != -1 && !referentFound)
                        {
                            //System.err.println("retain: Referent out of range: "+ec.toText()+" "+ec.parse.getSpan());
                        }
                        break;
                    }
                    if (excluded(mention, cde))
                    {
                        if (showExclusions)
                        {
                            if (mention.Id != -1 && entityMention.Id == mention.Id)
                            {
                                Console.Error.WriteLine(this + ".retain: Referent excluded: (" + mention.Id + ") " +
                                                        mention.toText() + " " + mention.IndexSpan + " -> (" +
                                                        entityMention.Id + ") " + entityMention.toText() + " " +
                                                        entityMention.Span + " " + this);
                            }
                        }
                    }
                    else
                    {
                        hasReferentialCandidate = true;
                        bool useAsDifferentExample = defaultReferent(cde);
                        //if (!sampleSelection || (mention.getId() != -1 && entityMention.getId() == mention.getId()) || (!nonReferentFound && useAsDifferentExample)) {
                        IList<string> features = getFeatures(mention, cde);

                        //add Event to Model
                        if (debugOn)
                        {
                            Console.Error.WriteLine(this + ".retain: " + mention.Id + " " + mention.toText() + " -> " +
                                                    entityMention.Id + " " + cde);
                        }
                        if (mention.Id != -1 && entityMention.Id == mention.Id)
                        {
                            referentFound = true;
                            events.Add(new Event(SAME, features.ToArray()));
                            de = cde;
                            //System.err.println("MaxentResolver.retain: resolved at "+ei);
                            distances.add(ei);
                        }
                        else if (!pairedSampleSelection || (!nonReferentFound && useAsDifferentExample))
                        {
                            nonReferentFound = true;
                            events.Add(new Event(DIFF, features.ToArray()));
                        }
                        //}
                    }
                    if (pairedSampleSelection && referentFound && nonReferentFound)
                    {
                        break;
                    }
                    if (preferFirstReferent && referentFound)
                    {
                        break;
                    }
                }
                // doesn't refer to anything
                if (hasReferentialCandidate)
                {
                    nonReferentialResolver.addEvent(mention);
                }
                return (de);
            }
            else
            {
                return (base.retain(mention, dm));
            }
        }

        /// <summary>
        /// Returns a list of features for deciding whether the specified mention refers to the specified discourse entity. </summary>
        /// <param name="mention"> the mention being considers as possibly referential. </param>
        /// <param name="entity"> The discourse entity with which the mention is being considered referential. </param>
        /// <returns> a list of features used to predict reference between the specified mention and entity. </returns>
        protected internal virtual IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
        {
            IList<string> features = new List<string>();
            features.Add(DEFAULT);
            features.AddRange(ResolverUtils.getCompatibilityFeatures(mention, entity, simModel));
            return features;
        }

        public override void train()
        {
            if (ResolverMode.TRAIN == mode)
            {
                if (debugOn)
                {
                    Console.Error.WriteLine(this + " referential");
                    FileWriter writer = new FileWriter(modelName + ".events");
                    for (IEnumerator<Event> ei = events.GetEnumerator(); ei.MoveNext();)
                    {
                        Event e = ei.Current;
                        writer.write(e.ToString() + "\n");
                    }
                    writer.close();
                }
                (new SuffixSensitiveGISModelWriter(GIS.trainModel(new CollectionEventStream(events), 100, 10),
                    new Jfile(modelName + modelExtension))).persist();
                nonReferentialResolver.train();
            }
        }

        public static TestSimilarityModel SimilarityModel
        {
            set { simModel = value; }
        }

        protected internal override bool excluded(MentionContext ec, DiscourseEntity de)
        {
            if (base.excluded(ec, de))
            {
                return true;
            }
            return false;
            /*
		else {
		  if (GEN_INCOMPATIBLE == getGenderCompatibilityFeature(ec,de)) {
		    return true;
		  }
		  else if (NUM_INCOMPATIBLE == getNumberCompatibilityFeature(ec,de)) {
		    return true;
		  }
		  else if (SIM_INCOMPATIBLE == getSemanticCompatibilityFeature(ec,de)) {
		    return true;
		  }
		  return false;
		}
		*/
        }
    }
}