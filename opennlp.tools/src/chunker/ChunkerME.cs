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
using System.Linq;
using j4n.Serialization;

namespace opennlp.tools.chunker
{
    using AbstractModel = opennlp.model.AbstractModel;
    using EventStream = opennlp.model.EventStream;
    using MaxentModel = opennlp.model.MaxentModel;
    using TrainUtil = opennlp.model.TrainUtil;
    using opennlp.tools.util;
    using opennlp.tools.util;
    using Sequence = opennlp.tools.util.Sequence;
    using opennlp.tools.util;
    using Span = opennlp.tools.util.Span;
    using TrainingParameters = opennlp.tools.util.TrainingParameters;
    using ModelUtil = opennlp.tools.util.model.ModelUtil;

    /// <summary>
    /// The class represents a maximum-entropy-based chunker.  Such a chunker can be used to
    /// find flat structures based on sequence inputs such as noun phrases or named entities.
    /// </summary>
    public class ChunkerME : Chunker
    {
        public const int DEFAULT_BEAM_SIZE = 10;

        /// <summary>
        /// The beam used to search for sequences of chunk tag assignments.
        /// </summary>
        protected internal BeamSearch<string> beam;

        private Sequence bestSequence;

        /// <summary>
        /// The model used to assign chunk tags to a sequence of tokens.
        /// </summary>
        protected internal MaxentModel model;

        /// <summary>
        /// Initializes the current instance with the specified model and
        /// the specified beam size.
        /// </summary>
        /// <param name="model"> The model for this chunker. </param>
        /// <param name="beamSize"> The size of the beam that should be used when decoding sequences. </param>
        /// <param name="sequenceValidator">  The <seealso cref="SequenceValidator"/> to determines whether the outcome 
        ///        is valid for the preceding sequence. This can be used to implement constraints 
        ///        on what sequences are valid. </param>
        /// @deprecated Use <seealso cref="#ChunkerME(ChunkerModel, int)"/> instead 
        ///    and use the <seealso cref="ChunkerFactory"/> to configure the <seealso cref="SequenceValidator"/> and <seealso cref="ChunkerContextGenerator"/>. 
        public ChunkerME(ChunkerModel model, int beamSize, SequenceValidator<string> sequenceValidator,
            ChunkerContextGenerator contextGenerator)
        {
            this.model = model.getChunkerModel();
            beam = new BeamSearch<string>(beamSize, contextGenerator, this.model, sequenceValidator, 0);
        }

        /// <summary>
        /// Initializes the current instance with the specified model and
        /// the specified beam size.
        /// </summary>
        /// <param name="model"> The model for this chunker. </param>
        /// <param name="beamSize"> The size of the beam that should be used when decoding sequences. </param>
        /// <param name="sequenceValidator">  The <seealso cref="SequenceValidator"/> to determines whether the outcome 
        ///        is valid for the preceding sequence. This can be used to implement constraints 
        ///        on what sequences are valid. </param>
        /// @deprecated Use <seealso cref="#ChunkerME(ChunkerModel, int)"/> instead 
        ///    and use the <seealso cref="ChunkerFactory"/> to configure the <seealso cref="SequenceValidator"/>. 
        public ChunkerME(ChunkerModel model, int beamSize, SequenceValidator<string> sequenceValidator)
            : this(model, beamSize, sequenceValidator, new DefaultChunkerContextGenerator())
        {
        }

        /// <summary>
        /// Initializes the current instance with the specified model and
        /// the specified beam size.
        /// </summary>
        /// <param name="model"> The model for this chunker. </param>
        /// <param name="beamSize"> The size of the beam that should be used when decoding sequences. </param>
        public ChunkerME(ChunkerModel model, int beamSize)
        {
            this.model = model.getChunkerModel();
            ChunkerContextGenerator contextGenerator = model.Factory.ContextGenerator;
            SequenceValidator<string> sequenceValidator = model.Factory.SequenceValidator;
            beam = new BeamSearch<string>(beamSize, contextGenerator, this.model, sequenceValidator, 0);
        }

        /// <summary>
        /// Initializes the current instance with the specified model.
        /// The default beam size is used.
        /// </summary>
        /// <param name="model"> </param>
        public ChunkerME(ChunkerModel model) : this(model, DEFAULT_BEAM_SIZE)
        {
        }

        /// <summary>
        /// Creates a chunker using the specified model.
        /// </summary>
        /// <param name="mod"> The maximum entropy model for this chunker. </param>
        [Obsolete]
        public ChunkerME(MaxentModel mod) : this(mod, new DefaultChunkerContextGenerator(), DEFAULT_BEAM_SIZE)
        {
        }

        /// <summary>
        /// Creates a chunker using the specified model and context generator.
        /// </summary>
        /// <param name="mod"> The maximum entropy model for this chunker. </param>
        /// <param name="cg"> The context generator to be used by the specified model. </param>
        [Obsolete]
        public ChunkerME(MaxentModel mod, ChunkerContextGenerator cg) : this(mod, cg, DEFAULT_BEAM_SIZE)
        {
        }

        /// <summary>
        /// Creates a chunker using the specified model and context generator and decodes the
        /// model using a beam search of the specified size.
        /// </summary>
        /// <param name="mod"> The maximum entropy model for this chunker. </param>
        /// <param name="cg"> The context generator to be used by the specified model. </param>
        /// <param name="beamSize"> The size of the beam that should be used when decoding sequences. </param>
        [Obsolete]
        public ChunkerME(MaxentModel mod, ChunkerContextGenerator cg, int beamSize)
        {
            beam = new BeamSearch<string>(beamSize, cg, mod);
            this.model = mod;
        }

        [Obsolete]
        public virtual IList<string> chunk(IList<string> toks, IList<string> tags)
        {
            bestSequence = beam.bestSequence(toks.ToArray(), new object[] {tags.ToArray()});
            return bestSequence.Outcomes;
        }

        public virtual string[] chunk(string[] toks, string[] tags)
        {
            bestSequence = beam.bestSequence(toks, new object[] {tags});
            IList<string> c = bestSequence.Outcomes;
            return c.ToArray();
        }

        public virtual Span[] chunkAsSpans(string[] toks, string[] tags)
        {
            string[] preds = chunk(toks, tags);
            return ChunkSample.phrasesAsSpanList(toks, tags, preds);
        }

        [Obsolete]
        public virtual Sequence[] topKSequences(IList<string> sentence, IList<string> tags)
        {
            return topKSequences(sentence.ToArray(), tags.ToArray());
        }

        public virtual Sequence[] topKSequences(string[] sentence, string[] tags)
        {
            return beam.bestSequences(DEFAULT_BEAM_SIZE, sentence, new object[] {tags});
        }

        public virtual Sequence[] topKSequences(string[] sentence, string[] tags, double minSequenceScore)
        {
            return beam.bestSequences(DEFAULT_BEAM_SIZE, sentence, new object[] {tags}, minSequenceScore);
        }

        /// <summary>
        /// Populates the specified array with the probabilities of the last decoded sequence.  The
        /// sequence was determined based on the previous call to <code>chunk</code>.  The
        /// specified array should be at least as large as the numbe of tokens in the previous call to <code>chunk</code>.
        /// </summary>
        /// <param name="probs"> An array used to hold the probabilities of the last decoded sequence. </param>
        public virtual void probs(double[] probs)
        {
            bestSequence.getProbs(probs);
        }

        /// <summary>
        /// Returns an array with the probabilities of the last decoded sequence.  The
        /// sequence was determined based on the previous call to <code>chunk</code>. </summary>
        /// <returns> An array with the same number of probabilities as tokens were sent to <code>chunk</code>
        /// when it was last called. </returns>
        public virtual double[] probs()
        {
            return bestSequence.Probs;
        }

        public static ChunkerModel train(string lang, ObjectStream<ChunkSample> @in, TrainingParameters mlParams,
            ChunkerFactory factory)
        {
            IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

            EventStream es = new ChunkerEventStream(@in, factory.ContextGenerator);

            AbstractModel maxentModel = TrainUtil.train(es, mlParams.getSettings(), manifestInfoEntries);

            return new ChunkerModel(lang, maxentModel, manifestInfoEntries, factory);
        }

        /// @deprecated Use
        ///             <seealso cref="#train(String, ObjectStream, ChunkerContextGenerator, TrainingParameters, ChunkerFactory)"/>
        ///             instead. 
        public static ChunkerModel train(string lang, ObjectStream<ChunkSample> @in,
            ChunkerContextGenerator contextGenerator, TrainingParameters mlParams)
        {
            IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

            EventStream es = new ChunkerEventStream(@in, contextGenerator);

            AbstractModel maxentModel = TrainUtil.train(es, mlParams.getSettings(), manifestInfoEntries);

            return new ChunkerModel(lang, maxentModel, manifestInfoEntries);
        }

        /// @deprecated use <seealso cref="#train(String, ObjectStream, ChunkerContextGenerator, TrainingParameters)"/>
        /// instead and pass in a TrainingParameters object. 
        public static ChunkerModel train(string lang, ObjectStream<ChunkSample> @in, int cutoff, int iterations,
            ChunkerContextGenerator contextGenerator)
        {
            return train(lang, @in, contextGenerator, ModelUtil.createTrainingParameters(iterations, cutoff));
        }

        /// <summary>
        /// Trains a new model for the <seealso cref="ChunkerME"/>.
        /// </summary>
        /// <param name="in"> </param>
        /// <param name="cutoff"> </param>
        /// <param name="iterations">
        /// </param>
        /// <returns> the new model
        /// </returns>
        /// <exception cref="IOException">
        /// </exception>
        /// @deprecated use <seealso cref="#train(String, ObjectStream, ChunkerContextGenerator, TrainingParameters)"/>
        /// instead and pass in a TrainingParameters object. 
        [Obsolete(
            "use <seealso cref=\"#train(String, opennlp.tools.util.ObjectStream, ChunkerContextGenerator, opennlp.tools.util.TrainingParameters)\"/>"
            )]
        public static ChunkerModel train(string lang, ObjectStream<ChunkSample> @in, int cutoff, int iterations)
        {
            return train(lang, @in, cutoff, iterations, new DefaultChunkerContextGenerator());
        }
    }
}