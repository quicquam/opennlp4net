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
using System.IO;
using j4n.IO.File;
using j4n.Serialization;

namespace opennlp.tools.postag
{
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using opennlp.tools.util;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using opennlp.tools.util.eval;
    using Mean = opennlp.tools.util.eval.Mean;
    using ModelType = opennlp.tools.util.model.ModelType;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    public class POSTaggerCrossValidator
    {
        private readonly string languageCode;

        private readonly TrainingParameters @params;

        private int? ngramCutoff;

        private Mean wordAccuracy = new Mean();
        private POSTaggerEvaluationMonitor[] listeners;

        /* this will be used to load the factory after the ngram dictionary was created */
        private string factoryClassName;
        /* user can also send a ready to use factory */
        private POSTaggerFactory factory;

        private int? tagdicCutoff = null;
        private Jfile tagDictionaryFile;

        /// <summary>
        /// Creates a <seealso cref="POSTaggerCrossValidator"/> that builds a ngram dictionary
        /// dynamically. It instantiates a sub-class of <seealso cref="POSTaggerFactory"/> using
        /// the tag and the ngram dictionaries.
        /// </summary>
        public POSTaggerCrossValidator(string languageCode, TrainingParameters trainParam, Jfile tagDictionary,
            int? ngramCutoff, int? tagdicCutoff, string factoryClass, params POSTaggerEvaluationMonitor[] listeners)
        {
            this.languageCode = languageCode;
            this.@params = trainParam;
            this.ngramCutoff = ngramCutoff;
            this.listeners = listeners;
            this.factoryClassName = factoryClass;
            this.tagdicCutoff = tagdicCutoff;
            this.tagDictionaryFile = tagDictionary;
        }

        /// <summary>
        /// Creates a <seealso cref="POSTaggerCrossValidator"/> using the given
        /// <seealso cref="POSTaggerFactory"/>.
        /// </summary>
        public POSTaggerCrossValidator(string languageCode, TrainingParameters trainParam, POSTaggerFactory factory,
            params POSTaggerEvaluationMonitor[] listeners)
        {
            this.languageCode = languageCode;
            this.@params = trainParam;
            this.listeners = listeners;
            this.factory = factory;
            this.ngramCutoff = null;
            this.tagdicCutoff = null;
        }

        /// @deprecated use
        ///             <seealso cref="#POSTaggerCrossValidator(String, TrainingParameters, POSTaggerFactory, POSTaggerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="TrainingParameters"/> object and a
        ///             <seealso cref="POSTaggerFactory"/>. 
        public POSTaggerCrossValidator(string languageCode, ModelType modelType, POSDictionary tagDictionary,
            Dictionary ngramDictionary, int cutoff, int iterations)
            : this(languageCode, create(modelType, cutoff, iterations), create(ngramDictionary, tagDictionary))
        {
        }

        /// @deprecated use
        ///             <seealso cref="#POSTaggerCrossValidator(String, TrainingParameters, POSTaggerFactory, POSTaggerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="TrainingParameters"/> object and a
        ///             <seealso cref="POSTaggerFactory"/>. 
        public POSTaggerCrossValidator(string languageCode, ModelType modelType, POSDictionary tagDictionary,
            Dictionary ngramDictionary)
            : this(languageCode, create(modelType, 5, 100), create(ngramDictionary, tagDictionary))
        {
        }

        /// @deprecated use
        ///             <seealso cref="#POSTaggerCrossValidator(String, TrainingParameters, POSTaggerFactory, POSTaggerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="POSTaggerFactory"/>. 
        public POSTaggerCrossValidator(string languageCode, TrainingParameters trainParam, POSDictionary tagDictionary,
            params POSTaggerEvaluationMonitor[] listeners)
            : this(languageCode, trainParam, create(null, tagDictionary), listeners)
        {
        }

        /// @deprecated use
        ///             <seealso cref="#POSTaggerCrossValidator(String, TrainingParameters, POSDictionary, Integer, String, POSTaggerEvaluationMonitor...)"/>
        ///             instead and pass in the name of <seealso cref="POSTaggerFactory"/>
        ///             sub-class. 
        public POSTaggerCrossValidator(string languageCode, TrainingParameters trainParam, POSDictionary tagDictionary,
            int? ngramCutoff, params POSTaggerEvaluationMonitor[] listeners)
            : this(languageCode, trainParam, create(null, tagDictionary), listeners)
        {
            this.ngramCutoff = ngramCutoff;
        }

        /// @deprecated use
        ///             <seealso cref="#POSTaggerCrossValidator(String, TrainingParameters, POSTaggerFactory, POSTaggerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="POSTaggerFactory"/>. 
        public POSTaggerCrossValidator(string languageCode, TrainingParameters trainParam, POSDictionary tagDictionary,
            Dictionary ngramDictionary, params POSTaggerEvaluationMonitor[] listeners)
            : this(languageCode, trainParam, create(ngramDictionary, tagDictionary), listeners)
        {
        }

        /// <summary>
        /// Starts the evaluation.
        /// </summary>
        /// <param name="samples">
        ///          the data to train and test </param>
        /// <param name="nFolds">
        ///          number of folds
        /// </param>
        /// <exception cref="IOException"> </exception>
        public virtual void evaluate(ObjectStream<POSSample> samples, int nFolds)
        {
            CrossValidationPartitioner<POSSample> partitioner = new CrossValidationPartitioner<POSSample>(samples,
                nFolds);

            while (partitioner.hasNext())
            {
                CrossValidationPartitioner<POSSample>.TrainingSampleStream trainingSampleStream = partitioner.next();

                if (this.factory == null)
                {
                    this.factory = POSTaggerFactory.create(this.factoryClassName, null, null);
                }

                Dictionary ngramDict = this.factory.Dictionary;
                if (ngramDict == null)
                {
                    if (this.ngramCutoff != null)
                    {
                        Console.Error.Write("Building ngram dictionary ... ");
                        ngramDict = POSTaggerME.buildNGramDictionary(trainingSampleStream, this.ngramCutoff.Value);
                        trainingSampleStream.reset();
                        Console.Error.WriteLine("done");
                    }
                    this.factory.Dictionary = ngramDict;
                }

                if (this.tagDictionaryFile != null && this.factory.TagDictionary == null)
                {
                    this.factory.TagDictionary = this.factory.createTagDictionary(tagDictionaryFile);
                }
                if (this.tagdicCutoff != null)
                {
                    TagDictionary dict = this.factory.TagDictionary;
                    if (dict == null)
                    {
                        dict = this.factory.createEmptyTagDictionary();
                        this.factory.TagDictionary = dict;
                    }
                    if (dict is MutableTagDictionary)
                    {
                        POSTaggerME.populatePOSDictionary(trainingSampleStream, (MutableTagDictionary) dict,
                            this.tagdicCutoff.Value);
                    }
                    else
                    {
                        throw new System.ArgumentException(
                            "Can't extend a TagDictionary that does not implement MutableTagDictionary.");
                    }
                    trainingSampleStream.reset();
                }

                POSModel model = POSTaggerME.train(languageCode, trainingSampleStream, @params, this.factory);

                POSEvaluator evaluator = new POSEvaluator(new POSTaggerME(model), listeners);

                evaluator.evaluate(trainingSampleStream.TestSampleStream);

                wordAccuracy.add(evaluator.WordAccuracy, evaluator.WordCount);

                if (this.tagdicCutoff != null)
                {
                    this.factory.TagDictionary = null;
                }
            }
        }

        /// <summary>
        /// Retrieves the accuracy for all iterations.
        /// </summary>
        /// <returns> the word accuracy </returns>
        public virtual double WordAccuracy
        {
            get { return wordAccuracy.mean(); }
        }

        /// <summary>
        /// Retrieves the number of words which where validated
        /// over all iterations. The result is the amount of folds
        /// multiplied by the total number of words.
        /// </summary>
        /// <returns> the word count </returns>
        public virtual long WordCount
        {
            get { return wordAccuracy.count(); }
        }

        private static TrainingParameters create(ModelType type, int cutoff, int iterations)
        {
            TrainingParameters @params = ModelUtil.createTrainingParameters(iterations, cutoff);
            @params.put(TrainingParameters.ALGORITHM_PARAM, type.ToString());
            return @params;
        }

        private static POSTaggerFactory create(Dictionary ngram, TagDictionary pos)
        {
            return new POSTaggerFactory(ngram, pos);
        }
    }
}