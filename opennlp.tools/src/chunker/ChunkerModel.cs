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
using j4n.IO.OutputStream;
using opennlp.tools.util.model;


namespace opennlp.tools.chunker
{
    using AbstractModel = opennlp.model.AbstractModel;
    using BinaryFileDataReader = opennlp.model.BinaryFileDataReader;
    using GenericModelReader = opennlp.model.GenericModelReader;
    using BaseToolFactory = opennlp.tools.util.BaseToolFactory;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;

    /// <summary>
    /// The <seealso cref="ChunkerModel"/> is the model used
    /// by a learnable <seealso cref="Chunker"/>.
    /// </summary>
    /// <seealso cref= ChunkerME </seealso>
    public class ChunkerModel : BaseModel
    {
        private const string COMPONENT_NAME = "ChunkerME";
        private const string CHUNKER_MODEL_ENTRY_NAME = "chunker.model";

        /// @deprecated Use
        ///             <seealso cref="#ChunkerModel(String, AbstractModel, Map, ChunkerFactory)"/>
        ///             instead. 
        public ChunkerModel(string languageCode, AbstractModel chunkerModel,
            IDictionary<string, string> manifestInfoEntries)
            : this(languageCode, chunkerModel, manifestInfoEntries, new ChunkerFactory())
        {
        }

        public ChunkerModel(string languageCode, AbstractModel chunkerModel,
            IDictionary<string, string> manifestInfoEntries, ChunkerFactory factory)
            : base(COMPONENT_NAME, languageCode, manifestInfoEntries, factory)
        {
            artifactMap[CHUNKER_MODEL_ENTRY_NAME] = chunkerModel;
            checkArtifactMap();
        }

        /// @deprecated Use
        ///             {@link #ChunkerModel(String, AbstractModel, ChunkerFactory)
        ///             instead.} 
        public ChunkerModel(string languageCode, AbstractModel chunkerModel)
            : this(languageCode, chunkerModel, null, new ChunkerFactory())
        {
        }

        public ChunkerModel(string languageCode, AbstractModel chunkerModel, ChunkerFactory factory)
            : this(languageCode, chunkerModel, null, factory)
        {
        }

        public ChunkerModel(InputStream @in, long streamOffset = 0)
            : base(COMPONENT_NAME, @in, streamOffset)
        {
        }

        public ChunkerModel(Jfile modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public ChunkerModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        {
        }

        protected internal override void validateArtifactMap()
        {
            base.validateArtifactMap();

            if (!(artifactMap[CHUNKER_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("Chunker model is incomplete!");
            }
        }

        public virtual AbstractModel getChunkerModel()
        {
            return (AbstractModel) artifactMap[CHUNKER_MODEL_ENTRY_NAME];
        }

        protected internal override Type DefaultFactory
        {
            get { return typeof (ChunkerFactory); }
        }


        public virtual ChunkerFactory Factory
        {
            get { return (ChunkerFactory) this.toolFactory; }
        }

        public static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.Error.WriteLine("ChunkerModel -lang code packageName modelName");
                Environment.Exit(1);
            }

            string lang = args[1];
            string packageName = args[2];
            string modelName = args[3];

            AbstractModel chunkerModel =
                (new GenericModelReader(new BinaryFileDataReader(new FileInputStream(modelName)))).Model;

            ChunkerModel packageModel = new ChunkerModel(lang, chunkerModel);
            packageModel.serialize(new FileOutputStream(packageName));
        }
    }
}