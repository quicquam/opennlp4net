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
using j4n.Serialization;

namespace opennlp.tools.chunker
{
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using opennlp.tools.util;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using opennlp.tools.util.eval;
    using FMeasure = opennlp.tools.util.eval.FMeasure;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    public class ChunkerCrossValidator
    {
        private readonly string languageCode;
        private readonly TrainingParameters @params;

        private FMeasure fmeasure = new FMeasure();
        private ChunkerEvaluationMonitor[] listeners;
        private ChunkerFactory chunkerFactory;

        /// @deprecated Use
        ///             <seealso cref="#ChunkerCrossValidator(String, TrainingParameters, ChunkerFactory, ChunkerEvaluationMonitor...)"/>
        ///             instead. 
        [Obsolete("Use")]
        public ChunkerCrossValidator(string languageCode, int cutoff, int iterations)
        {
            this.languageCode = languageCode;

            @params = ModelUtil.createTrainingParameters(iterations, cutoff);
            listeners = null;
        }

        /// @deprecated Use <seealso cref="#ChunkerCrossValidator(String, TrainingParameters, ChunkerFactory, ChunkerEvaluationMonitor...)"/> instead.  
        public ChunkerCrossValidator(string languageCode, TrainingParameters @params,
            params ChunkerEvaluationMonitor[] listeners)
        {
            this.languageCode = languageCode;
            this.@params = @params;
            this.listeners = listeners;
        }

        public ChunkerCrossValidator(string languageCode, TrainingParameters @params, ChunkerFactory factory,
            params ChunkerEvaluationMonitor[] listeners)
        {
            this.chunkerFactory = factory;
            this.languageCode = languageCode;
            this.@params = @params;
            this.listeners = listeners;
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
        public virtual void evaluate(ObjectStream<ChunkSample> samples, int nFolds)
        {
            CrossValidationPartitioner<ChunkSample> partitioner = new CrossValidationPartitioner<ChunkSample>(samples,
                nFolds);

            while (partitioner.hasNext())
            {
                CrossValidationPartitioner<ChunkSample>.TrainingSampleStream trainingSampleStream = partitioner.next();

                ChunkerModel model = ChunkerME.train(languageCode, trainingSampleStream, @params, chunkerFactory);

                // do testing
                ChunkerEvaluator evaluator = new ChunkerEvaluator(new ChunkerME(model, ChunkerME.DEFAULT_BEAM_SIZE),
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