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
using j4n.Exceptions;
using j4n.IO.File;
using j4n.IO.InputStream;


namespace opennlp.tools.postag
{
    using AbstractModel = opennlp.model.AbstractModel;
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using BaseToolFactory = opennlp.tools.util.BaseToolFactory;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using opennlp.tools.util.model;
    using BaseModel = opennlp.tools.util.model.BaseModel<POSModel>;

    /// <summary>
    /// The <seealso cref="POSModel"/> is the model used
    /// by a learnable <seealso cref="POSTagger"/>.
    /// </summary>
    /// <seealso cref= POSTaggerME </seealso>
    public sealed class POSModel : BaseModel
    {
        private const string COMPONENT_NAME = "POSTaggerME";

        public const string POS_MODEL_ENTRY_NAME = "pos.model";

        /// @deprecated Use
        ///             <seealso cref="#POSModel(String, AbstractModel, Map, POSTaggerFactory)"/>
        ///             instead. 
        public POSModel(string languageCode, AbstractModel posModel, POSDictionary tagDictionary, Dictionary ngramDict,
            IDictionary<string, string> manifestInfoEntries)
            : this(languageCode, posModel, manifestInfoEntries, new POSTaggerFactory(ngramDict, tagDictionary))
        {
        }

        /// @deprecated Use
        ///             <seealso cref="#POSModel(String, AbstractModel, Map, POSTaggerFactory)"/>
        ///             instead. 
        public POSModel(string languageCode, AbstractModel posModel, POSDictionary tagDictionary, Dictionary ngramDict)
            : this(languageCode, posModel, null, new POSTaggerFactory(ngramDict, tagDictionary))
        {
        }

        public POSModel(string languageCode, AbstractModel posModel, IDictionary<string, string> manifestInfoEntries,
            POSTaggerFactory posFactory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, posFactory)
        {
            if (posModel == null)
            {
                throw new System.ArgumentException("The maxentPosModel param must not be null!");
            }

            artifactMap[POS_MODEL_ENTRY_NAME] = posModel;
            checkArtifactMap();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public POSModel(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public POSModel(InputStream @in) : base(COMPONENT_NAME, @in)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public POSModel(java.io.File modelFile) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public POSModel(Jfile modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public POSModel(java.net.URL modelURL) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public POSModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        {
        }

        protected internal override Type DefaultFactory
        {
            get { return typeof (POSTaggerFactory); }
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("rawtypes") protected void createArtifactSerializers(java.util.Map<String, opennlp.tools.util.model.ArtifactSerializer> serializers)
        protected internal void createArtifactSerializers(IDictionary<string, ArtifactSerializer<Object>> serializers)
        {
            base.createArtifactSerializers();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected void validateArtifactMap() throws opennlp.tools.util.InvalidFormatException
        protected internal override void validateArtifactMap()
        {
            base.validateArtifactMap();

            if (!(artifactMap[POS_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("POS model is incomplete!");
            }
        }

        public AbstractModel PosModel
        {
            get { return (AbstractModel) artifactMap[POS_MODEL_ENTRY_NAME]; }
        }

        /// <summary>
        /// Retrieves the tag dictionary.
        /// </summary>
        /// <returns> tag dictionary or null if not used
        /// </returns>
        /// @deprecated Use <seealso cref="POSModel#getFactory()"/> to get a
        ///             <seealso cref="POSTaggerFactory"/> and
        ///             <seealso cref="POSTaggerFactory#getTagDictionary()"/> to get a
        ///             <seealso cref="TagDictionary"/>.
        /// 
        /// <exception cref="IllegalStateException">
        ///           if the TagDictionary is not an instance of POSDictionary </exception>
        public POSDictionary TagDictionary
        {
            get
            {
                if (Factory != null)
                {
                    TagDictionary dict = Factory.TagDictionary;
                    if (dict != null)
                    {
                        if (dict is POSDictionary)
                        {
                            return (POSDictionary) dict;
                        }
                        //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
                        string clazz = dict.GetType().FullName;
                        throw new IllegalStateException("Can not get a dictionary of type " + clazz +
                                                        " using the deprecated method POSModel.getTagDictionary() " +
                                                        "because it can only return dictionaries of type POSDictionary. " +
                                                        "Use POSModel.getFactory().getTagDictionary() instead.");
                    }
                }
                return null;
            }
        }

        public POSTaggerFactory Factory
        {
            get { return (POSTaggerFactory) this.toolFactory; }
        }

        /// <summary>
        /// Retrieves the ngram dictionary.
        /// </summary>
        /// <returns> ngram dictionary or null if not used </returns>
        public Dictionary NgramDictionary
        {
            get
            {
                if (Factory != null)
                {
                    return Factory.Dictionary;
                }
                return null;
            }
        }
    }
}