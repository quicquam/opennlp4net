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
using j4n.Concurrency;
using j4n.Object;
using j4n.Serialization;


namespace opennlp.tools.postag
{
    using AbstractModel = opennlp.model.AbstractModel;
    using EventStream = opennlp.model.EventStream;
    using TrainUtil = opennlp.model.TrainUtil;
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using NGramModel = opennlp.tools.ngram.NGramModel;
    using opennlp.tools.util;
    using opennlp.tools.util;
    using Sequence = opennlp.tools.util.Sequence;
    using opennlp.tools.util;
    using StringList = opennlp.tools.util.StringList;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using StringPattern = opennlp.tools.util.featuregen.StringPattern;
    using ModelType = opennlp.tools.util.model.ModelType;

    /// <summary>
    /// A part-of-speech tagger that uses maximum entropy.  Tries to predict whether
    /// words are nouns, verbs, or any of 70 other POS tags depending on their
    /// surrounding context.
    /// 
    /// </summary>
    public class POSTaggerME : POSTagger
    {
        /// <summary>
        /// The maximum entropy model to use to evaluate contexts.
        /// </summary>
        protected internal AbstractModel posModel;

        /// <summary>
        /// The feature context generator.
        /// </summary>
        protected internal POSContextGenerator contextGen;

        /// <summary>
        /// Tag dictionary used for restricting words to a fixed set of tags.
        /// </summary>
        protected internal TagDictionary tagDictionary;

        protected internal Dictionary ngramDictionary;

        /// <summary>
        /// Says whether a filter should be used to check whether a tag assignment
        /// is to a word outside of a closed class.
        /// </summary>
        protected internal bool useClosedClassTagsFilter = false;

        public const int DEFAULT_BEAM_SIZE = 3;

        /// <summary>
        /// The size of the beam to be used in determining the best sequence of pos tags.
        /// </summary>
        protected internal int size;

        private Sequence bestSequence;

        /// <summary>
        /// The search object used for search multiple sequences of tags.
        /// </summary>
        protected internal BeamSearch<string> beam;

        /// <summary>
        /// Initializes the current instance with the provided
        /// model and provided beam size.
        /// </summary>
        /// <param name="model"> </param>
        /// <param name="beamSize"> </param>
        public POSTaggerME(POSModel model, int beamSize, int cacheSize)
        {
            POSTaggerFactory factory = model.Factory;
            posModel = model.PosModel;
            contextGen = factory.getPOSContextGenerator(beamSize);
            tagDictionary = factory.TagDictionary;
            size = beamSize;
            beam = new BeamSearch<string>(size, contextGen, posModel, factory.SequenceValidator, cacheSize);
        }

        /// <summary>
        /// Initializes the current instance with the provided model
        /// and the default beam size of 3.
        /// </summary>
        /// <param name="model"> </param>
        public POSTaggerME(POSModel model) : this(model, DEFAULT_BEAM_SIZE, 0)
        {
        }

        /// <summary>
        /// Creates a new tagger with the specified model and tag dictionary.
        /// </summary>
        /// <param name="model"> The model used for tagging. </param>
        /// <param name="tagdict"> The tag dictionary used for specifying a set of valid tags. </param>
        [Obsolete]
        public POSTaggerME(AbstractModel model, TagDictionary tagdict)
            : this(model, new DefaultPOSContextGenerator(null), tagdict)
        {
        }

        /// <summary>
        /// Creates a new tagger with the specified model and n-gram dictionary.
        /// </summary>
        /// <param name="model"> The model used for tagging. </param>
        /// <param name="dict"> The n-gram dictionary used for feature generation. </param>
        [Obsolete]
        public POSTaggerME(AbstractModel model, Dictionary dict) : this(model, new DefaultPOSContextGenerator(dict))
        {
        }

        /// <summary>
        /// Creates a new tagger with the specified model, n-gram dictionary, and tag dictionary.
        /// </summary>
        /// <param name="model"> The model used for tagging. </param>
        /// <param name="dict"> The n-gram dictionary used for feature generation. </param>
        /// <param name="tagdict"> The dictionary which specifies the valid set of tags for some words. </param>
        [Obsolete]
        public POSTaggerME(AbstractModel model, Dictionary dict, TagDictionary tagdict)
            : this(DEFAULT_BEAM_SIZE, model, new DefaultPOSContextGenerator(dict), tagdict)
        {
        }

        /// <summary>
        /// Creates a new tagger with the specified model and context generator.
        /// </summary>
        /// <param name="model"> The model used for tagging. </param>
        /// <param name="cg"> The context generator used for feature creation. </param>
        [Obsolete]
        public POSTaggerME(AbstractModel model, POSContextGenerator cg) : this(DEFAULT_BEAM_SIZE, model, cg, null)
        {
        }

        /// <summary>
        /// Creates a new tagger with the specified model, context generator, and tag dictionary.
        /// </summary>
        /// <param name="model"> The model used for tagging. </param>
        /// <param name="cg"> The context generator used for feature creation. </param>
        /// <param name="tagdict"> The dictionary which specifies the valid set of tags for some words. </param>
        [Obsolete]
        public POSTaggerME(AbstractModel model, POSContextGenerator cg, TagDictionary tagdict)
            : this(DEFAULT_BEAM_SIZE, model, cg, tagdict)
        {
        }

        /// <summary>
        /// Creates a new tagger with the specified beam size, model, context generator, and tag dictionary.
        /// </summary>
        /// <param name="beamSize"> The number of alternate tagging considered when tagging. </param>
        /// <param name="model"> The model used for tagging. </param>
        /// <param name="cg"> The context generator used for feature creation. </param>
        /// <param name="tagdict"> The dictionary which specifies the valid set of tags for some words. </param>
        [Obsolete]
        public POSTaggerME(int beamSize, AbstractModel model, POSContextGenerator cg, TagDictionary tagdict)
        {
            size = beamSize;
            posModel = model;
            contextGen = cg;
            beam = new BeamSearch<string>(size, cg, model);
            tagDictionary = tagdict;
        }

        /// <summary>
        /// Returns the number of different tags predicted by this model.
        /// </summary>
        /// <returns> the number of different tags predicted by this model. </returns>
        public virtual int NumTags
        {
            get { return posModel.NumOutcomes; }
        }

        [Obsolete]
        public virtual IList<string> tag(IList<string> sentence)
        {
            bestSequence = beam.bestSequence(sentence.ToArray(), null);
            return bestSequence.Outcomes;
        }

        public virtual string[] tag(string[] sentence)
        {
            return this.tag(sentence, null);
        }

        public virtual string[] tag(string[] sentence, object[] additionaContext)
        {
            bestSequence = beam.bestSequence(sentence, additionaContext);
            IList<string> t = bestSequence.Outcomes;
            return t.ToArray();
        }

        /// <summary>
        /// Returns at most the specified number of taggings for the specified sentence.
        /// </summary>
        /// <param name="numTaggings"> The number of tagging to be returned. </param>
        /// <param name="sentence"> An array of tokens which make up a sentence.
        /// </param>
        /// <returns> At most the specified number of taggings for the specified sentence. </returns>
        public virtual string[][] tag(int numTaggings, string[] sentence)
        {
            Sequence[] bestSequences = beam.bestSequences(numTaggings, sentence, null);
            string[][] tags = new string[bestSequences.Length][];
            for (int si = 0; si < tags.Length; si++)
            {
                IList<string> t = bestSequences[si].Outcomes;
                tags[si] = t.ToArray();
            }
            return tags;
        }

        [Obsolete]
        public virtual Sequence[] topKSequences(IList<string> sentence)
        {
            return beam.bestSequences(size, sentence.ToArray(), null);
        }

        public virtual Sequence[] topKSequences(string[] sentence)
        {
            return this.topKSequences(sentence, null);
        }

        public virtual Sequence[] topKSequences(string[] sentence, object[] additionaContext)
        {
            return beam.bestSequences(size, sentence, additionaContext);
        }

        /// <summary>
        /// Populates the specified array with the probabilities for each tag of the last tagged sentence.
        /// </summary>
        /// <param name="probs"> An array to put the probabilities into. </param>
        public virtual void probs(double[] probs)
        {
            bestSequence.getProbs(probs);
        }

        /// <summary>
        /// Returns an array with the probabilities for each tag of the last tagged sentence.
        /// </summary>
        /// <returns> an array with the probabilities for each tag of the last tagged sentence. </returns>
        public virtual double[] probs()
        {
            return bestSequence.Probs;
        }

        [Obsolete]
        public virtual string tag(string sentence)
        {
            IList<string> toks = new List<string>();
            StringTokenizer st = new StringTokenizer(sentence);
            while (st.hasMoreTokens())
            {
                toks.Add(st.nextToken());
            }
            IList<string> tags = tag(toks);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tags.Count; i++)
            {
                sb.Append(toks[i] + "/" + tags[i] + " ");
            }
            return sb.ToString().Trim();
        }

        public virtual string[] getOrderedTags(IList<string> words, IList<string> tags, int index)
        {
            return getOrderedTags(words, tags, index, null);
        }

        public virtual string[] getOrderedTags(IList<string> words, IList<string> tags, int index, double[] tprobs)
        {
            double[] probs = posModel.eval(contextGen.getContext(index, words.ToArray(), tags.ToArray(), null));

            string[] orderedTags = new string[probs.Length];
            for (int i = 0; i < probs.Length; i++)
            {
                int max = 0;
                for (int ti = 1; ti < probs.Length; ti++)
                {
                    if (probs[ti] > probs[max])
                    {
                        max = ti;
                    }
                }
                orderedTags[i] = posModel.getOutcome(max);
                if (tprobs != null)
                {
                    tprobs[i] = probs[max];
                }
                probs[max] = 0;
            }
            return orderedTags;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static POSModel train(String languageCode, opennlp.tools.util.ObjectStream<POSSample> samples, opennlp.tools.util.TrainingParameters trainParams, POSTaggerFactory posFactory) throws java.io.IOException
        public static POSModel train(string languageCode, ObjectStream<POSSample> samples,
            TrainingParameters trainParams, POSTaggerFactory posFactory)
        {
            POSContextGenerator contextGenerator = posFactory.POSContextGenerator;

            IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

            AbstractModel posModel;

            if (!TrainUtil.isSequenceTraining(trainParams.getSettings()))
            {
                EventStream es = new POSSampleEventStream(samples, contextGenerator);

                posModel = TrainUtil.train(es, trainParams.getSettings(), manifestInfoEntries);
            }
            else
            {
                POSSampleSequenceStream ss = new POSSampleSequenceStream(samples, contextGenerator);

                posModel = TrainUtil.train(ss, trainParams.getSettings(), manifestInfoEntries);
            }

            return new POSModel(languageCode, posModel, manifestInfoEntries, posFactory);
        }

        /// @deprecated use
        ///             <seealso cref="#train(String, ObjectStream, TrainingParameters, POSTaggerFactory)"/>
        ///             instead and pass in a <seealso cref="POSTaggerFactory"/>. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static POSModel train(String languageCode, opennlp.tools.util.ObjectStream<POSSample> samples, opennlp.tools.util.TrainingParameters trainParams, POSDictionary tagDictionary, opennlp.tools.dictionary.Dictionary ngramDictionary) throws java.io.IOException
        public static POSModel train(string languageCode, ObjectStream<POSSample> samples,
            TrainingParameters trainParams, POSDictionary tagDictionary, Dictionary ngramDictionary)
        {
            return train(languageCode, samples, trainParams, new POSTaggerFactory(ngramDictionary, tagDictionary));
        }

        /// @deprecated use
        ///             <seealso cref="#train(String, ObjectStream, TrainingParameters, POSTaggerFactory)"/>
        ///             instead and pass in a <seealso cref="POSTaggerFactory"/> and a
        ///             <seealso cref="TrainingParameters"/>. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("use") public static POSModel train(String languageCode, opennlp.tools.util.ObjectStream<POSSample> samples, opennlp.tools.util.model.ModelType modelType, POSDictionary tagDictionary, opennlp.tools.dictionary.Dictionary ngramDictionary, int cutoff, int iterations) throws java.io.IOException
        [Obsolete("use")]
        public static POSModel train(string languageCode, ObjectStream<POSSample> samples, ModelType modelType,
            POSDictionary tagDictionary, Dictionary ngramDictionary, int cutoff, int iterations)
        {
            TrainingParameters @params = new TrainingParameters();

            @params.put(TrainingParameters.ALGORITHM_PARAM, modelType.ToString());
            @params.put(TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));
            @params.put(TrainingParameters.CUTOFF_PARAM, Convert.ToString(cutoff));

            return train(languageCode, samples, @params, tagDictionary, ngramDictionary);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.tools.dictionary.Dictionary buildNGramDictionary(opennlp.tools.util.ObjectStream<POSSample> samples, int cutoff) throws java.io.IOException
        public static Dictionary buildNGramDictionary(ObjectStream<POSSample> samples, int cutoff)
        {
            NGramModel ngramModel = new NGramModel();

            POSSample sample;
            while ((sample = samples.read()) != null)
            {
                string[] words = sample.Sentence;

                if (words.Length > 0)
                {
                    ngramModel.add(new StringList(words), 1, 1);
                }
            }

            ngramModel.cutoff(cutoff, int.MaxValue);

            return ngramModel.toDictionary(true);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void populatePOSDictionary(opennlp.tools.util.ObjectStream<POSSample> samples, MutableTagDictionary dict, int cutoff) throws java.io.IOException
        public static void populatePOSDictionary(ObjectStream<POSSample> samples, MutableTagDictionary dict, int cutoff)
        {
            Console.WriteLine("Expanding POS Dictionary ...");
            long start = DateTime.Now.Ticks;

            // the data structure will store the word, the tag, and the number of
            // occurrences
            IDictionary<string, IDictionary<string, AtomicInteger>> newEntries =
                new Dictionary<string, IDictionary<string, AtomicInteger>>();
            POSSample sample;
            while ((sample = samples.read()) != null)
            {
                string[] words = sample.Sentence;
                string[] tags = sample.Tags;

                for (int i = 0; i < words.Length; i++)
                {
                    // only store words
                    if (!StringPattern.recognize(words[i]).containsDigit())
                    {
                        string word;
                        if (dict.CaseSensitive)
                        {
                            word = words[i];
                        }
                        else
                        {
                            word = words[i].ToLower();
                        }

                        if (!newEntries.ContainsKey(word))
                        {
                            newEntries[word] = new Dictionary<string, AtomicInteger>();
                        }

                        string[] dictTags = dict.getTags(word);
                        if (dictTags != null)
                        {
                            foreach (string tag in dictTags)
                            {
                                // for this tags we start with the cutoff
                                IDictionary<string, AtomicInteger> value = newEntries[word];
                                if (!value.ContainsKey(tag))
                                {
                                    value[tag] = new AtomicInteger(cutoff);
                                }
                            }
                        }

                        if (!newEntries[word].ContainsKey(tags[i]))
                        {
                            newEntries[word][tags[i]] = new AtomicInteger(1);
                        }
                        else
                        {
                            newEntries[word][tags[i]].incrementAndGet();
                        }
                    }
                }
            }

            // now we check if the word + tag pairs have enough occurrences, if yes we
            // add it to the dictionary 
            foreach (KeyValuePair<string, IDictionary<string, AtomicInteger>> wordEntry in newEntries)
            {
                IList<string> tagsForWord = new List<string>();
                foreach (KeyValuePair<string, AtomicInteger> entry in wordEntry.Value)
                {
                    if (entry.Value.get() >= cutoff)
                    {
                        tagsForWord.Add(entry.Key);
                    }
                }
                if (tagsForWord.Count > 0)
                {
                    dict.put(wordEntry.Key, tagsForWord.ToArray());
                }
            }

            Console.WriteLine("... finished expanding POS Dictionary. [" + (DateTime.Now.Ticks - start)/1000000 + "ms]");
        }
    }
}