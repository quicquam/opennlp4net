using System;
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
using j4n.IO.InputStream;
using j4n.IO.Reader;
using j4n.Serialization;
using opennlp.tools.util.cmdline;


namespace opennlp.tools.namefind
{
    using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
    using Span = opennlp.tools.util.Span;
    using opennlp.tools.util.eval;
    using FMeasure = opennlp.tools.util.eval.FMeasure;

    /// <summary>
    /// The <seealso cref="TokenNameFinderEvaluator"/> measures the performance
    /// of the given <seealso cref="TokenNameFinder"/> with the provided
    /// reference <seealso cref="NameSample"/>s.
    /// </summary>
    /// <seealso cref= Evaluator </seealso>
    /// <seealso cref= TokenNameFinder </seealso>
    /// <seealso cref= NameSample </seealso>
    public class TokenNameFinderEvaluator : Evaluator<NameSample>
    {
        private FMeasure fmeasure = new FMeasure();

        /// <summary>
        /// The <seealso cref="TokenNameFinder"/> used to create the predicted
        /// <seealso cref="NameSample"/> objects.
        /// </summary>
        private TokenNameFinder nameFinder;

        /// <summary>
        /// Initializes the current instance with the given
        /// <seealso cref="TokenNameFinder"/>.
        /// </summary>
        /// <param name="nameFinder"> the <seealso cref="TokenNameFinder"/> to evaluate. </param>
        /// <param name="listeners"> evaluation sample listeners  </param>
        public TokenNameFinderEvaluator(TokenNameFinder nameFinder, params TokenNameFinderEvaluationMonitor[] listeners)
            : base(listeners)
        {
            this.nameFinder = nameFinder;
        }

        /// <summary>
        /// Evaluates the given reference <seealso cref="NameSample"/> object.
        /// 
        /// This is done by finding the names with the
        /// <seealso cref="TokenNameFinder"/> in the sentence from the reference
        /// <seealso cref="NameSample"/>. The found names are then used to
        /// calculate and update the scores.
        /// </summary>
        /// <param name="reference"> the reference <seealso cref="NameSample"/>.
        /// </param>
        /// <returns> the predicted <seealso cref="NameSample"/>. </returns>
        protected internal override NameSample processSample(NameSample reference)
        {
            if (reference.ClearAdaptiveDataSet)
            {
                nameFinder.clearAdaptiveData();
            }

            Span[] predictedNames = nameFinder.find(reference.Sentence);
            Span[] references = reference.Names;

            // OPENNLP-396 When evaluating with a file in the old format
            // the type of the span is null, but must be set to default to match
            // the output of the name finder.
            for (int i = 0; i < references.Length; i++)
            {
                if (references[i].Type == null)
                {
                    references[i] = new Span(references[i].Start, references[i].End, "default");
                }
            }

            fmeasure.updateScores(references, predictedNames);

            return new NameSample(reference.Sentence, predictedNames, reference.ClearAdaptiveDataSet);
        }

        public virtual FMeasure FMeasure
        {
            get { return fmeasure; }
        }

        [Obsolete]
        public static void Main(string[] args)
        {
            if (args.Length == 4)
            {
                Console.WriteLine("Loading name finder model ...");
                InputStream modelIn = new FileInputStream(args[3]);

                TokenNameFinderModel model = new TokenNameFinderModel(modelIn);

                TokenNameFinder nameFinder = new NameFinderME(model);

                Console.WriteLine("Performing evaluation ...");
                TokenNameFinderEvaluator evaluator = new TokenNameFinderEvaluator(nameFinder);

                NameSampleDataStream sampleStream =
                    new NameSampleDataStream(
                        new PlainTextByLineStream(new InputStreamReader(new FileInputStream(args[2]), args[1])));

                PerformanceMonitor monitor = new PerformanceMonitor("sent");

                monitor.startAndPrintThroughput();

                ObjectStream<NameSample> iterator = new ObjectStreamAnonymousInnerClassHelper(sampleStream, monitor);

                evaluator.evaluate(iterator);

                monitor.stopAndPrintFinalResult();

                Console.WriteLine();
                Console.WriteLine("F-Measure: " + evaluator.FMeasure.getFMeasure());
                Console.WriteLine("Recall: " + evaluator.FMeasure.RecallScore);
                Console.WriteLine("Precision: " + evaluator.FMeasure.PrecisionScore);
            }
            else
            {
                // usage: -encoding code test.file model.file
            }
        }

        private class ObjectStreamAnonymousInnerClassHelper : ObjectStream<NameSample>
        {
            private opennlp.tools.namefind.NameSampleDataStream sampleStream;
            private PerformanceMonitor monitor;

            public ObjectStreamAnonymousInnerClassHelper(opennlp.tools.namefind.NameSampleDataStream sampleStream,
                PerformanceMonitor monitor)
            {
                this.sampleStream = sampleStream;
                this.monitor = monitor;
            }

            public virtual NameSample read()
            {
                monitor.incrementCounter();
                return sampleStream.read();
            }

            public virtual void reset()
            {
                sampleStream.reset();
            }
        }
    }
}