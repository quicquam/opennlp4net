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
using j4n.Serialization;


namespace opennlp.tools.doccat
{
    using GIS = opennlp.maxent.GIS;
    using AbstractModel = opennlp.model.AbstractModel;
    using MaxentModel = opennlp.model.MaxentModel;
    using TrainUtil = opennlp.model.TrainUtil;
    using TwoPassDataIndexer = opennlp.model.TwoPassDataIndexer;
    using SimpleTokenizer = opennlp.tools.tokenize.SimpleTokenizer;
    using Tokenizer = opennlp.tools.tokenize.Tokenizer;
    using opennlp.tools.util;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    /// <summary>
    /// Maxent implementation of <seealso cref="DocumentCategorizer"/>.
    /// </summary>
    public class DocumentCategorizerME : DocumentCategorizer
    {
        /// <summary>
        /// Shared default thread safe feature generator.
        /// </summary>
        private static FeatureGenerator defaultFeatureGenerator = new BagOfWordsFeatureGenerator();

        private MaxentModel model;
        private DocumentCategorizerContextGenerator mContextGenerator;

        /// <summary>
        /// Initializes a the current instance with a doccat model and custom feature generation.
        /// The feature generation must be identical to the configuration at training time.
        /// </summary>
        /// <param name="model"> </param>
        /// <param name="featureGenerators"> </param>
        public DocumentCategorizerME(DoccatModel model, params FeatureGenerator[] featureGenerators)
        {
            this.model = model.ChunkerModel;
            this.mContextGenerator = new DocumentCategorizerContextGenerator(featureGenerators);
        }

        /// <summary>
        /// Initializes the current instance with a doccat model. Default feature generation is used.
        /// </summary>
        /// <param name="model"> </param>
        public DocumentCategorizerME(DoccatModel model) : this(model, defaultFeatureGenerator)
        {
        }

        /// <summary>
        /// Initializes the current instance with the given <seealso cref="MaxentModel"/>.
        /// </summary>
        /// <param name="model">
        /// </param>
        /// @deprecated Use <seealso cref="DocumentCategorizerME#DocumentCategorizerME(DoccatModel)"/> instead. 
        [Obsolete("Use <seealso cref=\"DocumentCategorizerME#DocumentCategorizerME(DoccatModel)\"/> instead.")]
        public DocumentCategorizerME(MaxentModel model)
            : this(model, new FeatureGenerator[] {new BagOfWordsFeatureGenerator()})
        {
        }

        /// <summary>
        /// Initializes the current instance with a the given <seealso cref="MaxentModel"/>
        /// and <seealso cref="FeatureGenerator"/>s.
        /// </summary>
        /// <param name="model"> </param>
        /// <param name="featureGenerators">
        /// </param>
        /// @deprecated Use <seealso cref="DocumentCategorizerME#DocumentCategorizerME(DoccatModel, FeatureGenerator...)"/> instead. 
        [Obsolete(
            "Use <seealso cref=\"DocumentCategorizerME#DocumentCategorizerME(DoccatModel, FeatureGenerator...)\"/> instead."
            )]
        public DocumentCategorizerME(MaxentModel model, params FeatureGenerator[] featureGenerators)
        {
            this.model = model;
            mContextGenerator = new DocumentCategorizerContextGenerator(featureGenerators);
        }

        /// <summary>
        /// Categorizes the given text.
        /// </summary>
        /// <param name="text"> </param>
        public virtual double[] categorize(string[] text)
        {
            return model.eval(mContextGenerator.getContext(text));
        }

        /// <summary>
        /// Categorizes the given text. The text is tokenized with the SimpleTokenizer before it
        /// is passed to the feature generation.
        /// </summary>
        public virtual double[] categorize(string documentText)
        {
            Tokenizer tokenizer = SimpleTokenizer.INSTANCE;
            return categorize(tokenizer.tokenize(documentText));
        }

        public virtual string getBestCategory(double[] outcome)
        {
            return model.getBestOutcome(outcome);
        }

        public virtual int getIndex(string category)
        {
            return model.getIndex(category);
        }

        public virtual string getCategory(int index)
        {
            return model.getOutcome(index);
        }

        public virtual int NumberOfCategories
        {
            get { return model.NumOutcomes; }
        }

        public virtual string getAllResults(double[] results)
        {
            return model.getAllOutcomes(results);
        }

        /// <summary>
        /// Trains a new model for the <seealso cref="DocumentCategorizerME"/>.
        /// </summary>
        /// <param name="eventStream">
        /// </param>
        /// <returns> the new model </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static opennlp.model.AbstractModel train(DocumentCategorizerEventStream eventStream) throws java.io.IOException
        [Obsolete]
        public static AbstractModel train(DocumentCategorizerEventStream eventStream)
        {
            return GIS.trainModel(100, new TwoPassDataIndexer(eventStream, 5));
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DoccatModel train(String languageCode, opennlp.tools.util.ObjectStream<DocumentSample> samples, opennlp.tools.util.TrainingParameters mlParams, FeatureGenerator... featureGenerators) throws java.io.IOException
        public static DoccatModel train(string languageCode, ObjectStream<DocumentSample> samples,
            TrainingParameters mlParams, params FeatureGenerator[] featureGenerators)
        {
            if (featureGenerators.Length == 0)
            {
                featureGenerators = new FeatureGenerator[] {defaultFeatureGenerator};
            }

            IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

            AbstractModel model = TrainUtil.train(new DocumentCategorizerEventStream(samples, featureGenerators),
                mlParams.getSettings(), manifestInfoEntries);

            return new DoccatModel(languageCode, model, manifestInfoEntries);
        }

        /// <summary>
        /// Trains a document categorizer model with custom feature generation.
        /// </summary>
        /// <param name="languageCode"> </param>
        /// <param name="samples"> </param>
        /// <param name="cutoff"> </param>
        /// <param name="iterations"> </param>
        /// <param name="featureGenerators">
        /// </param>
        /// <returns> the trained doccat model
        /// </returns>
        /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DoccatModel train(String languageCode, opennlp.tools.util.ObjectStream<DocumentSample> samples, int cutoff, int iterations, FeatureGenerator... featureGenerators) throws java.io.IOException
        public static DoccatModel train(string languageCode, ObjectStream<DocumentSample> samples, int cutoff,
            int iterations, params FeatureGenerator[] featureGenerators)
        {
            return train(languageCode, samples, ModelUtil.createTrainingParameters(iterations, cutoff),
                featureGenerators);
        }

        /// <summary>
        /// Trains a doccat model with default feature generation.
        /// </summary>
        /// <param name="languageCode"> </param>
        /// <param name="samples">
        /// </param>
        /// <returns> the trained doccat model
        /// </returns>
        /// <exception cref="IOException"> </exception>
        /// <exception cref="ObjectStreamException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DoccatModel train(String languageCode, opennlp.tools.util.ObjectStream<DocumentSample> samples, int cutoff, int iterations) throws java.io.IOException
        public static DoccatModel train(string languageCode, ObjectStream<DocumentSample> samples, int cutoff,
            int iterations)
        {
            return train(languageCode, samples, cutoff, iterations, defaultFeatureGenerator);
        }

        /// <summary>
        /// Trains a doccat model with default feature generation.
        /// </summary>
        /// <param name="languageCode"> </param>
        /// <param name="samples">
        /// </param>
        /// <returns> the trained doccat model
        /// </returns>
        /// <exception cref="IOException"> </exception>
        /// <exception cref="ObjectStreamException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static DoccatModel train(String languageCode, opennlp.tools.util.ObjectStream<DocumentSample> samples) throws java.io.IOException
        public static DoccatModel train(string languageCode, ObjectStream<DocumentSample> samples)
        {
            return train(languageCode, samples, 5, 100, defaultFeatureGenerator);
        }
    }
}