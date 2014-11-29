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


namespace opennlp.tools.sentdetect
{
    using AbstractModel = opennlp.model.AbstractModel;
    using GenericModelReader = opennlp.model.GenericModelReader;
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    /// <summary>
    /// The <seealso cref="SentenceModel"/> is the model used
    /// by a learnable <seealso cref="SentenceDetector"/>.
    /// </summary>
    /// <seealso cref= SentenceDetectorME </seealso>
    public class SentenceModel : BaseModel
    {
        private const string COMPONENT_NAME = "SentenceDetectorME";

        private const string MAXENT_MODEL_ENTRY_NAME = "sent.model";

        public SentenceModel(string languageCode, AbstractModel sentModel,
            IDictionary<string, string> manifestInfoEntries, SentenceDetectorFactory sdFactory)
            : base(COMPONENT_NAME, languageCode, manifestInfoEntries, sdFactory)
        {
            artifactMap[MAXENT_MODEL_ENTRY_NAME] = sentModel;
            checkArtifactMap();
        }

        /// <summary>
        /// TODO: was added in 1.5.3 -> remove </summary>
        /// @deprecated Use
        ///             <seealso cref="#SentenceModel(String, AbstractModel, Map, SentenceDetectorFactory)"/>
        ///             instead and pass in a <seealso cref="SentenceDetectorFactory"/> 
        public SentenceModel(string languageCode, AbstractModel sentModel, bool useTokenEnd, Dictionary abbreviations,
            char[] eosCharacters, IDictionary<string, string> manifestInfoEntries)
            : this(
                languageCode, sentModel, manifestInfoEntries,
                new SentenceDetectorFactory(languageCode, useTokenEnd, abbreviations, eosCharacters))
        {
        }

        /// <summary>
        /// TODO: was added in 1.5.3 -> remove
        /// </summary>
        /// @deprecated Use
        ///             <seealso cref="#SentenceModel(String, AbstractModel, Map, SentenceDetectorFactory)"/>
        ///             instead and pass in a <seealso cref="SentenceDetectorFactory"/> 
        public SentenceModel(string languageCode, AbstractModel sentModel, bool useTokenEnd, Dictionary abbreviations,
            char[] eosCharacters) : this(languageCode, sentModel, useTokenEnd, abbreviations, eosCharacters, null)
        {
        }

        public SentenceModel(string languageCode, AbstractModel sentModel, bool useTokenEnd, Dictionary abbreviations,
            IDictionary<string, string> manifestInfoEntries)
            : this(languageCode, sentModel, useTokenEnd, abbreviations, null, manifestInfoEntries)
        {
        }

        public SentenceModel(string languageCode, AbstractModel sentModel, bool useTokenEnd, Dictionary abbreviations)
            : this(languageCode, sentModel, useTokenEnd, abbreviations, null, null)
        {
        }

        public SentenceModel(InputStream @in, long streamOffset = 0)
            : base(COMPONENT_NAME, @in, streamOffset)
        {
        }

        public SentenceModel(Jfile modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public SentenceModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        {
        }

        protected internal override void validateArtifactMap()
        {
            base.validateArtifactMap();

            if (!(artifactMap[MAXENT_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("Unable to find " + MAXENT_MODEL_ENTRY_NAME + " maxent model!");
            }

            if (!ModelUtil.validateOutcomes(MaxentModel, SentenceDetectorME.SPLIT, SentenceDetectorME.NO_SPLIT))
            {
                throw new InvalidFormatException("The maxent model is not compatible " + "with the sentence detector!");
            }
        }

        public virtual SentenceDetectorFactory Factory
        {
            get { return (SentenceDetectorFactory) this.toolFactory; }
        }

        protected internal override Type DefaultFactory
        {
            get { return typeof (SentenceDetectorFactory); }
        }

        public virtual AbstractModel MaxentModel
        {
            get { return (AbstractModel) artifactMap[MAXENT_MODEL_ENTRY_NAME]; }
        }

        public virtual Dictionary Abbreviations
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

        public virtual bool useTokenEnd()
        {
            if (Factory != null)
            {
                return Factory.UseTokenEnd;
            }
            return true;
        }

        public virtual char[] EosCharacters
        {
            get
            {
                if (Factory != null)
                {
                    return Factory.EOSCharacters;
                }
                return null;
            }
        }

        public string Language { get; set; }

        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine(
                    "SentenceModel [-abbreviationsDictionary] [-useTokenEnd] languageCode packageName modelName");
                Environment.Exit(1);
            }

            int ai = 0;

            Dictionary abbreviations = null;
            if ("-abbreviationsDictionary".Equals(args[ai]))
            {
                ai++;
                abbreviations = new Dictionary(new FileInputStream(args[ai++]));
            }

            bool useTokenEnd = false;
            if ("-useTokenEnd".Equals(args[ai]))
            {
                useTokenEnd = true;
                ai++;
            }

            string languageCode = args[ai++];
            string packageName = args[ai++];
            string modelName = args[ai];

            AbstractModel model = (new GenericModelReader(new Jfile(modelName))).Model;
            SentenceModel packageModel = new SentenceModel(languageCode, model, useTokenEnd, abbreviations,
                (char[]) null);
            packageModel.serialize(new FileOutputStream(packageName));
        }
    }
}