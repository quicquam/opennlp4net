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
using j4n.IO.File;
using j4n.IO.InputStream;

namespace opennlp.tools.doccat
{
    using AbstractModel = opennlp.model.AbstractModel;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using BaseModel = opennlp.tools.util.model.BaseModel<DoccatModel>;

    public class DoccatModel : BaseModel
    {
        private const string COMPONENT_NAME = "DocumentCategorizerME";
        private const string DOCCAT_MODEL_ENTRY_NAME = "doccat.model";

        protected internal DoccatModel(string languageCode, AbstractModel doccatModel,
            IDictionary<string, string> manifestInfoEntries) : base(COMPONENT_NAME, languageCode, manifestInfoEntries)
        {
            artifactMap[DOCCAT_MODEL_ENTRY_NAME] = doccatModel;
            checkArtifactMap();
        }

        public DoccatModel(string languageCode, AbstractModel doccatModel) : this(languageCode, doccatModel, null)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DoccatModel(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public DoccatModel(InputStream @in) : base(COMPONENT_NAME, @in)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DoccatModel(java.io.File modelFile) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public DoccatModel(Jfile modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DoccatModel(java.net.URL modelURL) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public DoccatModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected void validateArtifactMap() throws opennlp.tools.util.InvalidFormatException
        protected internal override void validateArtifactMap()
        {
            base.validateArtifactMap();

            if (!(artifactMap[DOCCAT_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("Doccat model is incomplete!");
            }
        }

        public virtual AbstractModel ChunkerModel
        {
            get { return (AbstractModel) artifactMap[DOCCAT_MODEL_ENTRY_NAME]; }
        }
    }
}