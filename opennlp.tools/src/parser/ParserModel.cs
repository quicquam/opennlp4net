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
using j4n.Interfaces;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.IO.Reader;
using j4n.IO.Writer;


namespace opennlp.tools.parser
{
    using AbstractModel = opennlp.model.AbstractModel;
    using ChunkerModel = opennlp.tools.chunker.ChunkerModel;
    using POSModel = opennlp.tools.postag.POSModel;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using opennlp.tools.util.model;
    using UncloseableInputStream = opennlp.tools.util.model.UncloseableInputStream;

    /// <summary>
    /// This is an abstract base class for <seealso cref="ParserModel"/> implementations.
    /// </summary>
    // TODO: Model should validate the artifact map
    public class ParserModel : BaseModel
    {
        public class POSModelSerializer : ArtifactSerializer<POSModel>
        {
            public virtual POSModel create(InputStream @in)
            {
                return new POSModel(@in);
            }

            public virtual void serialize(POSModel artifact, OutputStream @out)
            {
                artifact.serialize(@out as FileOutputStream);
            }
        }

        public class ChunkerModelSerializer : ArtifactSerializer<ChunkerModel>
        {

            public virtual ChunkerModel create(InputStream @in)
            {
                return new ChunkerModel(@in);
            }

            public virtual void serialize(ChunkerModel artifact, OutputStream @out)
            {
                artifact.serialize(@out as FileOutputStream);
            }
        }

        public class HeadRulesSerializer : ArtifactSerializer<opennlp.tools.parser.lang.en.HeadRules>
        {

            public virtual opennlp.tools.parser.lang.en.HeadRules create(InputStream @in)
            {
                return
                    new opennlp.tools.parser.lang.en.HeadRules(new BufferedReader(new InputStreamReader(@in, "UTF-8")));
            }

            public virtual void serialize(opennlp.tools.parser.lang.en.HeadRules artifact, OutputStream @out)
            {
                artifact.serialize(new OutputStreamWriter((FileOutputStream) @out, "UTF-8"));
            }
        }

        private const string COMPONENT_NAME = "Parser";

        private const string BUILD_MODEL_ENTRY_NAME = "build.model";

        private const string CHECK_MODEL_ENTRY_NAME = "check.model";

        private const string ATTACH_MODEL_ENTRY_NAME = "attach.model";

        private const string PARSER_TAGGER_MODEL_ENTRY_NAME = "parsertager.postagger";

        private const string CHUNKER_TAGGER_MODEL_ENTRY_NAME = "parserchunker.chunker";

        private const string HEAD_RULES_MODEL_ENTRY_NAME = "head-rules.headrules";

        private const string PARSER_TYPE = "parser-type";

        public ParserModel(string languageCode, AbstractModel buildModel, AbstractModel checkModel,
            AbstractModel attachModel, POSModel parserTagger, ChunkerModel chunkerTagger,
            opennlp.tools.parser.lang.en.HeadRules headRules, ParserType modelType,
            IDictionary<string, string> manifestInfoEntries) : base(COMPONENT_NAME, languageCode, manifestInfoEntries)
        {
            setManifestProperty(PARSER_TYPE, modelType.name);

            artifactMap[BUILD_MODEL_ENTRY_NAME] = buildModel;

            artifactMap[CHECK_MODEL_ENTRY_NAME] = checkModel;

            if (ParserType.CHUNKING.Equals(modelType))
            {
                if (attachModel != null)
                {
                    throw new System.ArgumentException("attachModel must be null for chunking parser!");
                }
            }
            else if (ParserType.TREEINSERT.Equals(modelType))
            {
                if (attachModel == null)
                {
                    throw new System.ArgumentException("attachModel must not be null!");
                }

                artifactMap[ATTACH_MODEL_ENTRY_NAME] = attachModel;
            }
            else
            {
                throw new IllegalStateException("Unknown ParserType '" + modelType + "'!");
            }

            artifactMap[PARSER_TAGGER_MODEL_ENTRY_NAME] = parserTagger;

            artifactMap[CHUNKER_TAGGER_MODEL_ENTRY_NAME] = chunkerTagger;

            artifactMap[HEAD_RULES_MODEL_ENTRY_NAME] = headRules;
            checkArtifactMap();
        }

        public ParserModel(string languageCode, AbstractModel buildModel, AbstractModel checkModel,
            AbstractModel attachModel, POSModel parserTagger, ChunkerModel chunkerTagger,
            opennlp.tools.parser.lang.en.HeadRules headRules, ParserType modelType)
            : this(
                languageCode, buildModel, checkModel, attachModel, parserTagger, chunkerTagger, headRules, modelType,
                null)
        {
        }

        public ParserModel(string languageCode, AbstractModel buildModel, AbstractModel checkModel,
            POSModel parserTagger, ChunkerModel chunkerTagger, opennlp.tools.parser.lang.en.HeadRules headRules,
            ParserType type, IDictionary<string, string> manifestInfoEntries)
            : this(
                languageCode, buildModel, checkModel, null, parserTagger, chunkerTagger, headRules, type,
                manifestInfoEntries)
        {
        }

        public ParserModel(InputStream @in) : base(COMPONENT_NAME, @in)
        {
        }

        public ParserModel(Jfile modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public ParserModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        {
        }

        public override void createArtifactSerializers()
        {
            base.createArtifactSerializers();

            if (!artifactSerializers.Contains("postagger"))
                artifactSerializers.Add("postagger", new POSModelSerializer());
            if (!artifactSerializers.Contains("chunker"))
                artifactSerializers.Add("chunker", new ChunkerModelSerializer());
            if (!artifactSerializers.Contains("headrules"))
                artifactSerializers.Add("headrules", new HeadRulesSerializer());
        }

        protected internal void createArtifactSerializers(IDictionary<string, ArtifactSerializer<Object>> serializers)
        {
        }

        public virtual ParserType ParserType
        {
            get { return ParserType.parse(getManifestProperty(PARSER_TYPE)); }
        }

        public virtual AbstractModel BuildModel
        {
            get { return (AbstractModel) artifactMap[BUILD_MODEL_ENTRY_NAME]; }
        }

        public virtual AbstractModel CheckModel
        {
            get { return (AbstractModel) artifactMap[CHECK_MODEL_ENTRY_NAME]; }
        }

        public virtual AbstractModel AttachModel
        {
            get { return (AbstractModel) artifactMap[ATTACH_MODEL_ENTRY_NAME]; }
        }

        public virtual POSModel ParserTaggerModel
        {
            get { return (POSModel) artifactMap[PARSER_TAGGER_MODEL_ENTRY_NAME]; }
        }

        public virtual ChunkerModel ParserChunkerModel
        {
            get { return (ChunkerModel) artifactMap[CHUNKER_TAGGER_MODEL_ENTRY_NAME]; }
        }

        public virtual opennlp.tools.parser.lang.en.HeadRules HeadRules
        {
            get { return (opennlp.tools.parser.lang.en.HeadRules) artifactMap[HEAD_RULES_MODEL_ENTRY_NAME]; }
        }

        // TODO: Update model methods should make sure properties are copied correctly ...
        public virtual ParserModel updateBuildModel(AbstractModel buildModel)
        {
            return new ParserModel(Language, buildModel, CheckModel, AttachModel, ParserTaggerModel, ParserChunkerModel,
                HeadRules, ParserType);
        }

        public virtual ParserModel updateCheckModel(AbstractModel checkModel)
        {
            return new ParserModel(Language, BuildModel, checkModel, AttachModel, ParserTaggerModel, ParserChunkerModel,
                HeadRules, ParserType);
        }

        public virtual ParserModel updateTaggerModel(POSModel taggerModel)
        {
            return new ParserModel(Language, BuildModel, CheckModel, AttachModel, taggerModel, ParserChunkerModel,
                HeadRules, ParserType);
        }

        public virtual ParserModel updateChunkerModel(ChunkerModel chunkModel)
        {
            return new ParserModel(Language, BuildModel, CheckModel, AttachModel, ParserTaggerModel, chunkModel,
                HeadRules, ParserType);
        }

        protected internal override void validateArtifactMap()
        {
            base.validateArtifactMap();

            if (!(artifactMap[BUILD_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("Missing the build model!");
            }

            ParserType modelType = ParserType;

            if (modelType != null)
            {
                if (ParserType.CHUNKING.Equals(modelType))
                {
                    if (artifactMap[ATTACH_MODEL_ENTRY_NAME] != null)
                    {
                        throw new InvalidFormatException("attachModel must be null for chunking parser!");
                    }
                }
                else if (ParserType.TREEINSERT.Equals(modelType))
                {
                    if (!(artifactMap[ATTACH_MODEL_ENTRY_NAME] is AbstractModel))
                    {
                        throw new InvalidFormatException("attachModel must not be null!");
                    }
                }
                else
                {
                    throw new InvalidFormatException("Unknown ParserType '" + modelType + "'!");
                }
            }
            else
            {
                throw new InvalidFormatException("Missing the parser type property!");
            }

            if (!(artifactMap[CHECK_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("Missing the check model!");
            }

            if (!(artifactMap[PARSER_TAGGER_MODEL_ENTRY_NAME] is POSModel))
            {
                throw new InvalidFormatException("Missing the tagger model!");
            }

            if (!(artifactMap[CHUNKER_TAGGER_MODEL_ENTRY_NAME] is ChunkerModel))
            {
                throw new InvalidFormatException("Missing the chunker model!");
            }

            if (!(artifactMap[HEAD_RULES_MODEL_ENTRY_NAME] is HeadRules))
            {
                throw new InvalidFormatException("Missing the head rules!");
            }
        }
    }
}