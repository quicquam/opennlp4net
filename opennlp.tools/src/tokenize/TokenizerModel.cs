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
using j4n.Interfaces;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using opennlp.tools.util.model;


namespace opennlp.tools.tokenize
{
    using BinaryGISModelReader = opennlp.maxent.io.BinaryGISModelReader;
    using AbstractModel = opennlp.model.AbstractModel;
    using MaxentModel = opennlp.model.MaxentModel;
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    /// <summary>
    /// The <seealso cref="TokenizerModel"/> is the model used
    /// by a learnable <seealso cref="Tokenizer"/>.
    /// </summary>
    /// <seealso cref= TokenizerME </seealso>
    public sealed class TokenizerModel : BaseModel
    {
        private const string COMPONENT_NAME = "TokenizerME";

        private const string TOKENIZER_MODEL_ENTRY = "token.model";

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="tokenizerModel"> the model </param>
        /// <param name="manifestInfoEntries"> the manifest </param>
        /// <param name="tokenizerFactory"> the factory </param>
        public TokenizerModel(AbstractModel tokenizerModel, IDictionary<string, string> manifestInfoEntries,
            TokenizerFactory tokenizerFactory)
            : base(COMPONENT_NAME, tokenizerFactory.LanguageCode, manifestInfoEntries, tokenizerFactory)
        {
            artifactMap[TOKENIZER_MODEL_ENTRY] = tokenizerModel;
            checkArtifactMap();
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="tokenizerMaxentModel"> </param>
        /// <param name="useAlphaNumericOptimization">
        /// </param>
        /// @deprecated Use
        ///             <seealso cref="TokenizerModel#TokenizerModel(String, AbstractModel, Map, TokenizerFactory)"/>
        ///             instead and pass in a <seealso cref="TokenizerFactory"/>. 
        public TokenizerModel(string language, AbstractModel tokenizerMaxentModel, Dictionary abbreviations,
            bool useAlphaNumericOptimization, IDictionary<string, string> manifestInfoEntries)
            : this(
                tokenizerMaxentModel, manifestInfoEntries,
                new TokenizerFactory(language, abbreviations, useAlphaNumericOptimization, null))
        {
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="language"> </param>
        /// <param name="tokenizerMaxentModel"> </param>
        /// <param name="useAlphaNumericOptimization"> </param>
        /// <param name="manifestInfoEntries">
        /// </param>
        /// @deprecated Use
        ///             <seealso cref="TokenizerModel#TokenizerModel(String, AbstractModel, Map, TokenizerFactory)"/>
        ///             instead and pass in a <seealso cref="TokenizerFactory"/>. 
        public TokenizerModel(string language, AbstractModel tokenizerMaxentModel, bool useAlphaNumericOptimization,
            IDictionary<string, string> manifestInfoEntries)
            : this(language, tokenizerMaxentModel, null, useAlphaNumericOptimization, manifestInfoEntries)
        {
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="language"> </param>
        /// <param name="tokenizerMaxentModel"> </param>
        /// <param name="useAlphaNumericOptimization">
        /// </param>
        /// @deprecated Use
        ///             <seealso cref="TokenizerModel#TokenizerModel(String, AbstractModel, Map, TokenizerFactory)"/>
        ///             instead and pass in a <seealso cref="TokenizerFactory"/>. 
        public TokenizerModel(string language, AbstractModel tokenizerMaxentModel, bool useAlphaNumericOptimization)
            : this(language, tokenizerMaxentModel, useAlphaNumericOptimization, null)
        {
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="in">
        /// </param>
        /// <exception cref="IOException"> </exception>
        /// <exception cref="InvalidFormatException"> </exception>
        public TokenizerModel(InputStream @in) : base(COMPONENT_NAME, @in)
        {
        }

        public TokenizerModel(Jfile modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public TokenizerModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        {
        }

        /// <summary>
        /// Checks if the tokenizer model has the right outcomes.
        /// </summary>
        /// <param name="model">
        /// @return </param>
        private static bool isModelCompatible(MaxentModel model)
        {
            return ModelUtil.validateOutcomes(model, TokenizerME.SPLIT, TokenizerME.NO_SPLIT);
        }

        protected internal override void validateArtifactMap()
        {
            base.validateArtifactMap();

            if (!(artifactMap[TOKENIZER_MODEL_ENTRY] is AbstractModel))
            {
                throw new InvalidFormatException("Token model is incomplete!");
            }

            if (!isModelCompatible(MaxentModel))
            {
                throw new InvalidFormatException("The maxent model is not compatible with the tokenizer!");
            }
        }

        public TokenizerFactory Factory
        {
            get { return (TokenizerFactory) this.toolFactory; }
        }

        protected internal override Type DefaultFactory
        {
            get { return typeof (TokenizerFactory); }
        }

        public AbstractModel MaxentModel
        {
            get { return (AbstractModel) artifactMap[TOKENIZER_MODEL_ENTRY]; }
        }

        public Dictionary Abbreviations
        {
            get
            {
                if (Factory != null)
                {
                    return Factory.AbbreviationDictionary;
                }
                return null;
            }
        }

        public bool useAlphaNumericOptimization()
        {
            if (Factory != null)
            {
                return Factory.UseAlphaNumericOptmization;
            }
            return false;
        }

        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("TokenizerModel [-alphaNumericOptimization] languageCode packageName modelName");
                Environment.Exit(1);
            }

            int ai = 0;

            bool alphaNumericOptimization = false;

            if ("-alphaNumericOptimization".Equals(args[ai]))
            {
                alphaNumericOptimization = true;
                ai++;
            }

            string languageCode = args[ai++];
            string packageName = args[ai++];
            string modelName = args[ai];

            AbstractModel model = (new BinaryGISModelReader(new DataInputStream(new FileInputStream(modelName)))).Model;

            TokenizerModel packageModel = new TokenizerModel(languageCode, model, alphaNumericOptimization);

            OutputStream @out = null;
            try
            {
                @out = new FileOutputStream(packageName);
                packageModel.serialize(@out);
            }
            finally
            {
                if (@out != null)
                {
                    @out.close();
                }
            }
        }

        private void serialize(OutputStream @out)
        {
            throw new NotImplementedException();
        }
    }
}