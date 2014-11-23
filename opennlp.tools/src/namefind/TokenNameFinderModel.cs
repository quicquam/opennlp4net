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
using j4n.Exceptions;
using j4n.Interfaces;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using opennlp.tools.util.model;


namespace opennlp.tools.namefind
{
    using AbstractModel = opennlp.model.AbstractModel;
    using MaxentModel = opennlp.model.MaxentModel;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using AdaptiveFeatureGenerator = opennlp.tools.util.featuregen.AdaptiveFeatureGenerator;
    using AggregatedFeatureGenerator = opennlp.tools.util.featuregen.AggregatedFeatureGenerator;
    using FeatureGeneratorResourceProvider = opennlp.tools.util.featuregen.FeatureGeneratorResourceProvider;
    using GeneratorFactory = opennlp.tools.util.featuregen.GeneratorFactory;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    /// <summary>
    /// The <seealso cref="TokenNameFinderModel"/> is the model used
    /// by a learnable <seealso cref="TokenNameFinder"/>.
    /// </summary>
    /// <seealso cref= NameFinderME </seealso>
    public class TokenNameFinderModel : BaseModel
    {
        public static string SEQUENCE_CODEC_CLASS_NAME_PARAMETER;

        public class FeatureGeneratorCreationError : Exception
        {
            internal FeatureGeneratorCreationError(Exception t) : base(t.Message)
            {
            }
        }

        private class ByteArraySerializer : ArtifactSerializer<byte[]>
        {
            public virtual sbyte[] create(InputStream @in)
            {
                return ModelUtil.read(@in);
            }

            public void serialize(byte[] artifact, OutputStream @out)
            {
                throw new NotImplementedException();
            }

            public virtual void serialize(sbyte[] artifact, OutputStream @out)
            {
                @out.write(artifact);
            }

            byte[] ArtifactSerializer<byte[]>.create(InputStream @in)
            {
                throw new NotImplementedException();
            }
        }

        private const string COMPONENT_NAME = "NameFinderME";
        private const string MAXENT_MODEL_ENTRY_NAME = "nameFinder.model";

        public const string GENERATOR_DESCRIPTOR_ENTRY_NAME = "generator.featuregen";

        public TokenNameFinderModel(string languageCode, AbstractModel nameFinderModel, sbyte[] generatorDescriptor,
            IDictionary<string, object> resources, IDictionary<string, string> manifestInfoEntries)
            : base(COMPONENT_NAME, languageCode, manifestInfoEntries)
        {
            if (!isModelValid(nameFinderModel))
            {
                throw new System.ArgumentException("Model not compatible with name finder!");
            }

            artifactMap[MAXENT_MODEL_ENTRY_NAME] = nameFinderModel;

            if (generatorDescriptor != null && generatorDescriptor.Length > 0)
            {
                artifactMap[GENERATOR_DESCRIPTOR_ENTRY_NAME] = generatorDescriptor;
            }

            if (resources != null)
            {
                // The resource map must not contain key which are already taken
                // like the name finder maxent model name
                if (resources.ContainsKey(MAXENT_MODEL_ENTRY_NAME) ||
                    resources.ContainsKey(GENERATOR_DESCRIPTOR_ENTRY_NAME))
                {
                    throw new System.ArgumentException();
                }

                // TODO: Add checks to not put resources where no serializer exists,
                // make that case fail here, should be done in the BaseModel
                foreach (var resource in resources)
                {
                    artifactMap.Add(resource);
                }
            }
            checkArtifactMap();
        }

        public TokenNameFinderModel(string languageCode, AbstractModel nameFinderModel,
            IDictionary<string, object> resources, IDictionary<string, string> manifestInfoEntries)
            : this(languageCode, nameFinderModel, null, resources, manifestInfoEntries)
        {
        }

        public TokenNameFinderModel(InputStream @in, long streamOffset = 0)
            : base(COMPONENT_NAME, @in, streamOffset)
        {
        }

        public TokenNameFinderModel(Jfile modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public TokenNameFinderModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        {
        }


        /// <summary>
        /// Retrieves the <seealso cref="TokenNameFinder"/> model.
        /// </summary>
        /// <returns> the classification model </returns>
        public virtual AbstractModel NameFinderModel
        {
            get { return (AbstractModel) artifactMap[MAXENT_MODEL_ENTRY_NAME]; }
        }

        /// <summary>
        /// Creates the <seealso cref="AdaptiveFeatureGenerator"/>. Usually this
        /// is a set of generators contained in the <seealso cref="AggregatedFeatureGenerator"/>.
        /// 
        /// Note:
        /// The generators are created on every call to this method.
        /// </summary>
        /// <returns> the feature generator or null if there is no descriptor in the model </returns>
        public virtual AdaptiveFeatureGenerator createFeatureGenerators()
        {
            sbyte[] descriptorBytes = null;

            if (artifactMap.ContainsKey(GENERATOR_DESCRIPTOR_ENTRY_NAME))
            {
                descriptorBytes = (sbyte[]) artifactMap[GENERATOR_DESCRIPTOR_ENTRY_NAME];
            }

            if (descriptorBytes != null)
            {
                InputStream descriptorIn = new ByteArrayInputStream(descriptorBytes);

                AdaptiveFeatureGenerator generator = null;
                try
                {
                    generator = GeneratorFactory.create(descriptorIn,
                        new FeatureGeneratorResourceProviderAnonymousInnerClassHelper(this));
                }
                catch (InvalidFormatException e)
                {
                    // It is assumed that the creation of the feature generation does not
                    // fail after it succeeded once during model loading.

                    // But it might still be possible that such an exception is thrown,
                    // in this case the caller should not be forced to handle the exception
                    // and a Runtime Exception is thrown instead.

                    // If the re-creation of the feature generation fails it is assumed
                    // that this can only be caused by a programming mistake and therefore
                    // throwing a Runtime Exception is reasonable

                    throw new FeatureGeneratorCreationError(e);
                }
                catch (IOException e)
                {
                    throw new IllegalStateException("Reading from mem cannot result in an I/O error", e);
                }

                return generator;
            }
            else
            {
                return null;
            }
        }

        private class FeatureGeneratorResourceProviderAnonymousInnerClassHelper : FeatureGeneratorResourceProvider
        {
            private readonly TokenNameFinderModel outerInstance;

            public FeatureGeneratorResourceProviderAnonymousInnerClassHelper(TokenNameFinderModel outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public virtual object getResource(string key)
            {
                return outerInstance.artifactMap[key];
            }
        }

        public virtual TokenNameFinderModel updateFeatureGenerator(sbyte[] descriptor)
        {
            TokenNameFinderModel model = new TokenNameFinderModel(Language, NameFinderModel, descriptor,
                new Dictionary<string, object>(), new Dictionary<string, string>());

            model.artifactMap.Clear();
            model.artifactMap[GENERATOR_DESCRIPTOR_ENTRY_NAME] = descriptor;

            return model;
        }

        public override void createArtifactSerializers()
        {
            base.createArtifactSerializers();
            if (!artifactSerializers.Contains("featuregen"))
                artifactSerializers.Add("featuregen", new ByteArraySerializer());
        }

        // TODO: Write test for this method
        public static bool isModelValid(MaxentModel model)
        {
            // We should have *optionally* one outcome named "other", some named xyz-start and sometimes 
            // they have a pair xyz-cont. We should not have any other outcome
            // To validate the model we check if we have one outcome named "other", at least
            // one outcome with suffix start. After that we check if all outcomes that ends with
            // "cont" have a pair that ends with "start".
            IList<string> start = new List<string>();
            IList<string> cont = new List<string>();

            for (int i = 0; i < model.NumOutcomes; i++)
            {
                string outcome = model.getOutcome(i);
                if (outcome.EndsWith(NameFinderME.START, StringComparison.Ordinal))
                {
                    start.Add(outcome.Substring(0, outcome.Length - NameFinderME.START.Length));
                }
                else if (outcome.EndsWith(NameFinderME.CONTINUE, StringComparison.Ordinal))
                {
                    cont.Add(outcome.Substring(0, outcome.Length - NameFinderME.CONTINUE.Length));
                }
                else if (outcome.Equals(NameFinderME.OTHER))
                {
                    // don't fail anymore if couldn't find outcome named OTHER
                }
                else
                {
                    // got unexpected outcome
                    return false;
                }
            }

            if (start.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (string contPreffix in cont)
                {
                    if (!start.Contains(contPreffix))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected internal override void validateArtifactMap()
        {
            base.validateArtifactMap();

            if (artifactMap[MAXENT_MODEL_ENTRY_NAME] is AbstractModel)
            {
                AbstractModel model = (AbstractModel) artifactMap[MAXENT_MODEL_ENTRY_NAME];
                isModelValid(model);
            }
            else
            {
                throw new InvalidFormatException("Token Name Finder model is incomplete!");
            }
        }

        protected internal override Type DefaultFactory
        {
            get { return typeof (TokenNameFinderFactory); }
        }
    }
}