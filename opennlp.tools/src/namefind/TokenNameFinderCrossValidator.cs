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
using System.Linq;
using j4n.Serialization;
using opennlp.nonjava.helperclasses;

namespace opennlp.tools.namefind
{
    using opennlp.tools.util;
    using opennlp.tools.util;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using opennlp.tools.util.eval;
    using FMeasure = opennlp.tools.util.eval.FMeasure;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    public class TokenNameFinderCrossValidator
    {
        private class DocumentSample
        {
            private readonly TokenNameFinderCrossValidator outerInstance;


            internal NameSample[] samples;

            internal DocumentSample(TokenNameFinderCrossValidator outerInstance, NameSample[] samples)
            {
                this.outerInstance = outerInstance;
                this.samples = samples;
            }

            internal virtual NameSample[] Samples
            {
                get { return samples; }
            }
        }

        /// <summary>
        /// Reads Name Samples to group them as a document based on the clear adaptive data flag.
        /// </summary>
        private class NameToDocumentSampleStream : FilterObjectStream<NameSample, DocumentSample>
        {
            private readonly TokenNameFinderCrossValidator outerInstance;


            internal NameSample beginSample;

            protected internal NameToDocumentSampleStream(TokenNameFinderCrossValidator outerInstance,
                ObjectStream<NameSample> samples) : base(samples)
            {
                this.outerInstance = outerInstance;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DocumentSample read() throws java.io.IOException
            public override DocumentSample read()
            {
                IList<NameSample> document = new List<NameSample>();

                if (beginSample == null)
                {
                    // Assume that the clear flag is set
                    beginSample = samples.read();
                }

                // Underlying stream is exhausted! 
                if (beginSample == null)
                {
                    return null;
                }

                document.Add(beginSample);

                NameSample sample;
                while ((sample = samples.read()) != null)
                {
                    if (sample.ClearAdaptiveDataSet)
                    {
                        beginSample = sample;
                        break;
                    }

                    document.Add(sample);
                }

                // Underlying stream is exhausted,
                // next call must return null
                if (sample == null)
                {
                    beginSample = null;
                }

                return new DocumentSample(outerInstance, document.ToArray());
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void reset() throws java.io.IOException, UnsupportedOperationException
            public override void reset()
            {
                base.reset();

                beginSample = null;
            }
        }

        /// <summary>
        /// Splits DocumentSample into NameSamples. 
        /// </summary>
        private class DocumentToNameSampleStream : FilterObjectStream<DocumentSample, NameSample>
        {
            private readonly TokenNameFinderCrossValidator outerInstance;


            protected internal DocumentToNameSampleStream(TokenNameFinderCrossValidator outerInstance,
                ObjectStream<DocumentSample> samples) : base(samples)
            {
                this.outerInstance = outerInstance;
            }

            internal IEnumerable<NameSample> documentSamples = new List<NameSample>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NameSample read() throws java.io.IOException
            public override NameSample read()
            {
                // Note: Empty document samples should be skipped

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                if (documentSamples.GetEnumerator().Current != null)
                {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                    documentSamples.GetEnumerator().MoveNext();
                    return documentSamples.GetEnumerator().Current;
                }
                else
                {
                    DocumentSample docSample = samples.read();

                    if (docSample != null)
                    {
                        documentSamples = docSample.Samples.ToArray();

                        return read();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private readonly string languageCode;
        private readonly TrainingParameters @params;
        private readonly string type;
        private readonly sbyte[] featureGeneratorBytes;
        private readonly IDictionary<string, object> resources;
        private TokenNameFinderEvaluationMonitor[] listeners;


        private FMeasure fmeasure = new FMeasure();

        /// <summary>
        /// Name finder cross validator
        /// </summary>
        /// <param name="languageCode"> 
        ///          the language of the training data </param>
        /// <param name="cutoff"> </param>
        /// <param name="iterations">
        /// </param>
        /// @deprecated use <seealso cref="#TokenNameFinderCrossValidator(String, String, TrainingParameters, byte[], Map, TokenNameFinderEvaluationMonitor...)"/>
        /// instead and pass in a TrainingParameters object. 
        [Obsolete(
            "use <seealso cref=\"#TokenNameFinderCrossValidator(String, String, opennlp.tools.util.TrainingParameters, byte[] , java.util.Map, TokenNameFinderEvaluationMonitor...)\"/>"
            )]
        public TokenNameFinderCrossValidator(string languageCode, int cutoff, int iterations)
            : this(languageCode, null, cutoff, iterations)
        {
        }

        /// <summary>
        /// Name finder cross validator
        /// </summary>
        /// <param name="languageCode">
        ///          the language of the training data </param>
        /// <param name="type">
        ///          null or an override type for all types in the training data </param>
        /// <param name="cutoff">
        ///          specifies the min number of times a feature must be seen </param>
        /// <param name="iterations">
        ///          the number of iterations
        /// </param>
        /// @deprecated use <seealso cref="#TokenNameFinderCrossValidator(String, String, TrainingParameters, byte[], Map, TokenNameFinderEvaluationMonitor...)"/>
        /// instead and pass in a TrainingParameters object. 
        [Obsolete(
            "use <seealso cref=\"#TokenNameFinderCrossValidator(String, String, opennlp.tools.util.TrainingParameters, byte[] , java.util.Map, TokenNameFinderEvaluationMonitor...)\"/>"
            )]
        public TokenNameFinderCrossValidator(string languageCode, string type, int cutoff, int iterations)
        {
            this.languageCode = languageCode;
            this.type = type;

            this.@params = ModelUtil.createTrainingParameters(iterations, cutoff);
            this.featureGeneratorBytes = null;
            this.resources = new Dictionary<string, object>();
        }

        /// <summary>
        /// Name finder cross validator
        /// </summary>
        /// <param name="languageCode">
        ///          the language of the training data </param>
        /// <param name="type">
        ///          null or an override type for all types in the training data </param>
        /// <param name="featureGeneratorBytes">
        ///          descriptor to configure the feature generation or null </param>
        /// <param name="resources">
        ///          the resources for the name finder or null if none </param>
        /// <param name="cutoff">
        ///          specifies the min number of times a feature must be seen </param>
        /// <param name="iterations">
        ///          the number of iterations
        /// </param>
        /// @deprecated use <seealso cref="#TokenNameFinderCrossValidator(String, String, TrainingParameters, byte[], Map, TokenNameFinderEvaluationMonitor...)"/>
        /// instead and pass in a TrainingParameters object. 
        [Obsolete(
            "use <seealso cref=\"#TokenNameFinderCrossValidator(String, String, opennlp.tools.util.TrainingParameters, byte[] , java.util.Map, TokenNameFinderEvaluationMonitor...)\"/>"
            )]
        public TokenNameFinderCrossValidator(string languageCode, string type, sbyte[] featureGeneratorBytes,
            IDictionary<string, object> resources, int iterations, int cutoff)
        {
            this.languageCode = languageCode;
            this.type = type;
            this.featureGeneratorBytes = featureGeneratorBytes;
            this.resources = resources;

            this.@params = ModelUtil.createTrainingParameters(iterations, cutoff);
        }

        /// <summary>
        /// Name finder cross validator
        /// </summary>
        /// <param name="languageCode">
        ///          the language of the training data </param>
        /// <param name="type">
        ///          null or an override type for all types in the training data </param>
        /// <param name="trainParams">
        ///          machine learning train parameters </param>
        /// <param name="featureGeneratorBytes">
        ///          descriptor to configure the feature generation or null </param>
        /// <param name="listeners">
        ///          a list of listeners </param>
        /// <param name="resources">
        ///          the resources for the name finder or null if none </param>
        public TokenNameFinderCrossValidator(string languageCode, string type, TrainingParameters trainParams,
            sbyte[] featureGeneratorBytes, IDictionary<string, object> resources,
            params TokenNameFinderEvaluationMonitor[] listeners)
        {
            this.languageCode = languageCode;
            this.type = type;
            this.featureGeneratorBytes = featureGeneratorBytes;
            this.resources = resources;

            this.@params = trainParams;

            this.listeners = listeners;
        }

        /// <summary>
        /// Starts the evaluation.
        /// </summary>
        /// <param name="samples">
        ///          the data to train and test </param>
        /// <param name="nFolds">
        ///          number of folds </param>
        /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void evaluate(opennlp.tools.util.ObjectStream<NameSample> samples, int nFolds) throws java.io.IOException
        public virtual void evaluate(ObjectStream<NameSample> samples, int nFolds)
        {
            // Note: The name samples need to be grouped on a document basis.

            CrossValidationPartitioner<DocumentSample> partitioner =
                new CrossValidationPartitioner<DocumentSample>(new NameToDocumentSampleStream(this, samples), nFolds);

            while (partitioner.hasNext())
            {
                CrossValidationPartitioner<DocumentSample>.TrainingSampleStream trainingSampleStream =
                    partitioner.next();

                TokenNameFinderModel model = opennlp.tools.namefind.NameFinderME.train(languageCode, type,
                    new DocumentToNameSampleStream(this, trainingSampleStream), @params, featureGeneratorBytes,
                    resources);

                // do testing
                TokenNameFinderEvaluator evaluator = new TokenNameFinderEvaluator(new NameFinderME(model), listeners);

                evaluator.evaluate(new DocumentToNameSampleStream(this, trainingSampleStream.TestSampleStream));

                fmeasure.mergeInto(evaluator.FMeasure);
            }
        }

        public virtual FMeasure FMeasure
        {
            get { return fmeasure; }
        }
    }
}