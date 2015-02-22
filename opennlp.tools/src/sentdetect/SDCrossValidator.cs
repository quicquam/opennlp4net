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

namespace opennlp.tools.sentdetect
{
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using opennlp.tools.util;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using opennlp.tools.util.eval;
    using FMeasure = opennlp.tools.util.eval.FMeasure;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    /// 
    public class SDCrossValidator
    {
        private readonly string languageCode;

        private readonly TrainingParameters parameters;

        private FMeasure fmeasure = new FMeasure();

        private SentenceDetectorEvaluationMonitor[] listeners;

        private SentenceDetectorFactory sdFactory;

        public SDCrossValidator(string languageCode, TrainingParameters parameters, SentenceDetectorFactory sdFactory,
            params SentenceDetectorEvaluationMonitor[] listeners)
        {
            this.languageCode = languageCode;
            this.parameters = parameters;
            this.listeners = listeners;
            this.sdFactory = sdFactory;
        }

        /// @deprecated Use
        ///             <seealso cref="#SDCrossValidator(String, TrainingParameters, SentenceDetectorFactory, SentenceDetectorEvaluationMonitor...)"/>
        ///             and pass in a <seealso cref="SentenceDetectorFactory"/>. 
        public SDCrossValidator(string languageCode, int cutoff, int iterations)
            : this(languageCode, ModelUtil.createTrainingParameters(cutoff, iterations))
        {
        }

        /// @deprecated Use
        ///             <seealso cref="#SDCrossValidator(String, TrainingParameters, SentenceDetectorFactory, SentenceDetectorEvaluationMonitor...)"/>
        ///             and pass in a <seealso cref="SentenceDetectorFactory"/>. 
        public SDCrossValidator(string languageCode, TrainingParameters parameters)
            : this(languageCode, parameters, new SentenceDetectorFactory(languageCode, true, null, null))
        {
        }

        /// @deprecated use <seealso cref="#SDCrossValidator(String, TrainingParameters, Dictionary, SentenceDetectorEvaluationMonitor...)"/>
        /// instead and pass in a TrainingParameters object. 
        [Obsolete(
            "use <seealso cref=\"#SDCrossValidator(String, opennlp.tools.util.TrainingParameters, opennlp.tools.dictionary.Dictionary, SentenceDetectorEvaluationMonitor...)\"/>"
            )]
        public SDCrossValidator(string languageCode, int cutoff, int iterations, Dictionary abbreviations)
            : this(
                languageCode, ModelUtil.createTrainingParameters(cutoff, iterations),
                new SentenceDetectorFactory(languageCode, true, abbreviations, null))
        {
        }

        /// @deprecated use
        ///             <seealso cref="#SDCrossValidator(String, TrainingParameters, Dictionary, SentenceDetectorEvaluationMonitor...)"/>
        ///             instead and pass in a TrainingParameters object. 
        public SDCrossValidator(string languageCode, TrainingParameters parameters,
            params SentenceDetectorEvaluationMonitor[] listeners)
            : this(languageCode, parameters, new SentenceDetectorFactory(languageCode, true, null, null), listeners)
        {
        }

        /// @deprecated use <seealso cref="#SDCrossValidator(String, TrainingParameters, Dictionary, SentenceDetectorEvaluationMonitor...)"/>
        /// instead and pass in a TrainingParameters object. 
        public SDCrossValidator(string languageCode) : this(languageCode, 5, 100)
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
        public virtual void evaluate(ObjectStream<SentenceSample> samples, int nFolds)
        {
            CrossValidationPartitioner<SentenceSample> partitioner =
                new CrossValidationPartitioner<SentenceSample>(samples, nFolds);

            while (partitioner.hasNext())
            {
                CrossValidationPartitioner<SentenceSample>.TrainingSampleStream trainingSampleStream =
                    partitioner.next();

                SentenceModel model;

                model = SentenceDetectorME.train(languageCode, trainingSampleStream, sdFactory, parameters);

                // do testing
                SentenceDetectorEvaluator evaluator = new SentenceDetectorEvaluator(new SentenceDetectorME(model),
                    listeners);

                evaluator.evaluate(trainingSampleStream.TestSampleStream);

                fmeasure.mergeInto(evaluator.FMeasure);
            }
        }

        public virtual FMeasure FMeasure
        {
            get { return fmeasure; }
        }
    }
}