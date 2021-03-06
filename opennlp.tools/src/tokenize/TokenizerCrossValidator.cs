﻿/*
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
using opennlp.tools.util;

namespace opennlp.tools.tokenize
{
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using opennlp.tools.util.eval;
    using FMeasure = opennlp.tools.util.eval.FMeasure;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    public class TokenizerCrossValidator
    {
        private readonly TrainingParameters parameters;

        private FMeasure fmeasure = new FMeasure();
        private TokenizerEvaluationMonitor[] listeners;
        private readonly TokenizerFactory factory;

        public TokenizerCrossValidator(TrainingParameters parameters, TokenizerFactory factory,
            params TokenizerEvaluationMonitor[] listeners)
        {
            this.parameters = parameters;
            this.listeners = listeners;
            this.factory = factory;
        }

        /// @deprecated use
        ///             <seealso cref="#TokenizerCrossValidator(TrainingParameters, TokenizerFactory, TokenizerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="TokenizerFactory"/> 
        public TokenizerCrossValidator(string language, Dictionary abbreviations, bool alphaNumericOptimization,
            TrainingParameters parameters, params TokenizerEvaluationMonitor[] listeners)
            : this(parameters, new TokenizerFactory(language, abbreviations, alphaNumericOptimization, null), listeners)
        {
        }

        /// @deprecated use
        ///             <seealso cref="#TokenizerCrossValidator(TrainingParameters, TokenizerFactory, TokenizerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="TokenizerFactory"/> 
        public TokenizerCrossValidator(string language, bool alphaNumericOptimization, int cutoff, int iterations)
            : this(language, alphaNumericOptimization, ModelUtil.createTrainingParameters(iterations, cutoff))
        {
        }

        /// @deprecated use
        ///             <seealso cref="#TokenizerCrossValidator(TrainingParameters, TokenizerFactory, TokenizerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="TokenizerFactory"/> 
        public TokenizerCrossValidator(string language, bool alphaNumericOptimization)
            : this(language, alphaNumericOptimization, ModelUtil.createTrainingParameters(100, 5))
        {
        }

        /// @deprecated use
        ///             <seealso cref="#TokenizerCrossValidator(TrainingParameters, TokenizerFactory, TokenizerEvaluationMonitor...)"/>
        ///             instead and pass in a <seealso cref="TokenizerFactory"/> 
        public TokenizerCrossValidator(string language, bool alphaNumericOptimization, TrainingParameters parameters,
            params TokenizerEvaluationMonitor[] listeners)
            : this(language, null, alphaNumericOptimization, parameters, listeners)
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
        public virtual void evaluate(ObjectStream<TokenSample> samples, int nFolds)
        {
            CrossValidationPartitioner<TokenSample> partitioner = new CrossValidationPartitioner<TokenSample>(samples,
                nFolds);

            while (partitioner.hasNext())
            {
                CrossValidationPartitioner<TokenSample>.TrainingSampleStream trainingSampleStream = partitioner.next();

                // Maybe throws IOException if temporary file handling fails ...
                TokenizerModel model;

                model = TokenizerME.train(trainingSampleStream, this.factory, parameters);

                TokenizerEvaluator evaluator = new TokenizerEvaluator(new TokenizerME(model), listeners);

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