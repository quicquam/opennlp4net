using System;
using System.Collections;
using System.Collections.Generic;
using opennlp.nonjava.helperclasses;
using Event = opennlp.model.Event;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace opennlp.perceptron
{
    using AbstractModel = opennlp.model.AbstractModel;
    using DataIndexer = opennlp.model.DataIndexer;
    using Event = opennlp.model.Event;
    using opennlp.model;
    using MutableContext = opennlp.model.MutableContext;
    using OnePassDataIndexer = opennlp.model.OnePassDataIndexer;
    using SequenceStream = opennlp.model.SequenceStream<Event>;
    using SequenceStreamEventStream = opennlp.model.SequenceStreamEventStream;

    /// <summary>
    /// Trains models for sequences using the perceptron algorithm.  Each outcome is represented as
    /// a binary perceptron classifier.  This supports standard (integer) weighting as well
    /// average weighting.  Sequence information is used in a simplified was to that described in:
    /// Discriminative Training Methods for Hidden Markov Models: Theory and Experiments
    /// with the Perceptron Algorithm. Michael Collins, EMNLP 2002.
    /// Specifically only updates are applied to tokens which were incorrectly tagged by a sequence tagger
    /// rather than to all feature across the sequence which differ from the training sequence.
    /// </summary>
    public class SimplePerceptronSequenceTrainer
    {
        private bool printMessages = true;
        private int iterations;
        private SequenceStream sequenceStream;

        /// <summary>
        /// Number of events in the event set. </summary>
        private int numEvents;

        /// <summary>
        /// Number of predicates. </summary>
        private int numPreds;

        private int numOutcomes;

        /// <summary>
        /// List of outcomes for each event i, in context[i]. </summary>
        private int[] outcomeList;

        private string[] outcomeLabels;

        internal double[] modelDistribution;

        /// <summary>
        /// Stores the average parameter values of each predicate during iteration. </summary>
        private MutableContext[] averageParams;

        /// <summary>
        /// Mapping between context and an integer </summary>
        private IndexHashTable<string> pmap;

        private IDictionary<string, int?> omap;

        /// <summary>
        /// Stores the estimated parameter value of each predicate during iteration. </summary>
        private MutableContext[] @params;

        private bool useAverage;
        private int[][][] updates;
        private int VALUE = 0;
        private int ITER = 1;
        private int EVENT = 2;

        private int[] allOutcomesPattern;
        private string[] predLabels;
        internal int numSequences;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.model.AbstractModel trainModel(int iterations, opennlp.model.SequenceStream sequenceStream, int cutoff, boolean useAverage) throws java.io.IOException
        public virtual AbstractModel trainModel(int iterations, SequenceStream sequenceStream, int cutoff,
            bool useAverage)
        {
            this.iterations = iterations;
            this.sequenceStream = sequenceStream;
            DataIndexer di = new OnePassDataIndexer(new SequenceStreamEventStream(sequenceStream), cutoff, false);
            numSequences = 0;
            foreach (Sequence<Event> s in sequenceStream)
            {
                numSequences++;
            }
            outcomeList = di.OutcomeList;
            predLabels = di.PredLabels;
            pmap = new IndexHashTable<string>(predLabels, 0.7d);

            display("Incorporating indexed data for training...  \n");
            this.useAverage = useAverage;
            numEvents = di.NumEvents;

            this.iterations = iterations;
            outcomeLabels = di.OutcomeLabels;
            omap = new Dictionary<string, int?>();
            for (int oli = 0; oli < outcomeLabels.Length; oli++)
            {
                omap[outcomeLabels[oli]] = oli;
            }
            outcomeList = di.OutcomeList;

            numPreds = predLabels.Length;
            numOutcomes = outcomeLabels.Length;
            if (useAverage)
            {
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: updates = new int[numPreds][numOutcomes][3];
                updates = RectangularArrays.ReturnRectangularIntArray(numPreds, numOutcomes, 3);
            }

            display("done.\n");

            display("\tNumber of Event Tokens: " + numEvents + "\n");
            display("\t    Number of Outcomes: " + numOutcomes + "\n");
            display("\t  Number of Predicates: " + numPreds + "\n");


            @params = new MutableContext[numPreds];
            if (useAverage)
            {
                averageParams = new MutableContext[numPreds];
            }

            allOutcomesPattern = new int[numOutcomes];
            for (int oi = 0; oi < numOutcomes; oi++)
            {
                allOutcomesPattern[oi] = oi;
            }

            for (int pi = 0; pi < numPreds; pi++)
            {
                @params[pi] = new MutableContext(allOutcomesPattern, new double[numOutcomes]);
                if (useAverage)
                {
                    averageParams[pi] = new MutableContext(allOutcomesPattern, new double[numOutcomes]);
                }
                for (int aoi = 0; aoi < numOutcomes; aoi++)
                {
                    @params[pi].setParameter(aoi, 0.0);
                    if (useAverage)
                    {
                        averageParams[pi].setParameter(aoi, 0.0);
                    }
                }
            }
            modelDistribution = new double[numOutcomes];

            display("Computing model parameters...\n");
            findParameters(iterations);
            display("...done.\n");

            /// <summary>
            ///************* Create and return the model ***************** </summary>
            string[] updatedPredLabels = predLabels;
            /*
		String[] updatedPredLabels = new String[pmap.size()];
		for (String pred : pmap.keySet()) {
		  updatedPredLabels[pmap.get(pred)]=pred;
		}
		*/
            if (useAverage)
            {
                return new PerceptronModel(averageParams, updatedPredLabels, outcomeLabels);
            }
            else
            {
                return new PerceptronModel(@params, updatedPredLabels, outcomeLabels);
            }
        }

        private void findParameters(int iterations)
        {
            display("Performing " + iterations + " iterations.\n");
            for (int i = 1; i <= iterations; i++)
            {
                if (i < 10)
                {
                    display("  " + i + ":  ");
                }
                else if (i < 100)
                {
                    display(" " + i + ":  ");
                }
                else
                {
                    display(i + ":  ");
                }
                nextIteration(i);
            }
            if (useAverage)
            {
                trainingStats(averageParams);
            }
            else
            {
                trainingStats(@params);
            }
        }

        private void display(string s)
        {
            if (printMessages)
            {
                Console.Write(s);
            }
        }

        public virtual void nextIteration(int iteration)
        {
            iteration--; //move to 0-based index
            int numCorrect = 0;
            int oei = 0;
            int si = 0;
            IDictionary<string, float?>[] featureCounts = (IDictionary<string, float?>[]) new IDictionary[numOutcomes];
            for (int oi = 0; oi < numOutcomes; oi++)
            {
                featureCounts[oi] = new Dictionary<string, float?>();
            }
            PerceptronModel model = new PerceptronModel(@params, predLabels, pmap, outcomeLabels);
            foreach (Sequence<Event> sequence in sequenceStream)
            {
                Event[] taggerEvents = sequenceStream.updateContext(sequence, model);
                Event[] events = sequence.Events;
                bool update = false;
                for (int ei = 0; ei < events.Length; ei++,oei++)
                {
                    if (!taggerEvents[ei].Outcome.Equals(events[ei].Outcome))
                    {
                        update = true;
                        //break;
                    }
                    else
                    {
                        numCorrect++;
                    }
                }
                if (update)
                {
                    for (int oi = 0; oi < numOutcomes; oi++)
                    {
                        featureCounts[oi].Clear();
                    }
                    //System.err.print("train:");for (int ei=0;ei<events.length;ei++) {System.err.print(" "+events[ei].getOutcome());} System.err.println();
                    //training feature count computation
                    for (int ei = 0; ei < events.Length; ei++,oei++)
                    {
                        string[] contextStrings = events[ei].Context;
                        float[] values = events[ei].Values;
                        int oi = (int) omap[events[ei].Outcome];
                        for (int ci = 0; ci < contextStrings.Length; ci++)
                        {
                            float value = 1;
                            if (values != null)
                            {
                                value = values[ci];
                            }
                            float? c = featureCounts[oi][contextStrings[ci]];
                            if (c == null)
                            {
                                c = value;
                            }
                            else
                            {
                                c += value;
                            }
                            featureCounts[oi][contextStrings[ci]] = c;
                        }
                    }
                    //evaluation feature count computation
                    //System.err.print("test: ");for (int ei=0;ei<taggerEvents.length;ei++) {System.err.print(" "+taggerEvents[ei].getOutcome());} System.err.println();
                    foreach (Event taggerEvent in taggerEvents)
                    {
                        string[] contextStrings = taggerEvent.Context;
                        float[] values = taggerEvent.Values;
                        int oi = (int) omap[taggerEvent.Outcome];
                        for (int ci = 0; ci < contextStrings.Length; ci++)
                        {
                            float value = 1;
                            if (values != null)
                            {
                                value = values[ci];
                            }
                            float? c = featureCounts[oi][contextStrings[ci]];
                            if (c == null)
                            {
                                c = -1*value;
                            }
                            else
                            {
                                c -= value;
                            }
                            if (c == 0f)
                            {
                                featureCounts[oi].Remove(contextStrings[ci]);
                            }
                            else
                            {
                                featureCounts[oi][contextStrings[ci]] = c;
                            }
                        }
                    }
                    for (int oi = 0; oi < numOutcomes; oi++)
                    {
                        foreach (string feature in featureCounts[oi].Keys)
                        {
                            int pi = pmap.get(feature);
                            if (pi != -1)
                            {
                                //System.err.println(si+" "+outcomeLabels[oi]+" "+feature+" "+featureCounts[oi].get(feature));
                                @params[pi].updateParameter(oi, (double) featureCounts[oi][feature]);
                                if (useAverage)
                                {
                                    if (updates[pi][oi][VALUE] != 0)
                                    {
                                        averageParams[pi].updateParameter(oi,
                                            updates[pi][oi][VALUE]*
                                            (numSequences*(iteration - updates[pi][oi][ITER]) +
                                             (si - updates[pi][oi][EVENT])));
                                        //System.err.println("p avp["+pi+"]."+oi+"="+averageParams[pi].getParameters()[oi]);
                                    }
                                    //System.err.println("p updates["+pi+"]["+oi+"]=("+updates[pi][oi][ITER]+","+updates[pi][oi][EVENT]+","+updates[pi][oi][VALUE]+") + ("+iteration+","+oei+","+params[pi].getParameters()[oi]+") -> "+averageParams[pi].getParameters()[oi]);
                                    updates[pi][oi][VALUE] = (int) @params[pi].Parameters[oi];
                                    updates[pi][oi][ITER] = iteration;
                                    updates[pi][oi][EVENT] = si;
                                }
                            }
                        }
                    }
                    model = new PerceptronModel(@params, predLabels, pmap, outcomeLabels);
                }
                si++;
            }
            //finish average computation
            double totIterations = (double) iterations*si;
            if (useAverage && iteration == iterations - 1)
            {
                for (int pi = 0; pi < numPreds; pi++)
                {
                    double[] predParams = averageParams[pi].Parameters;
                    for (int oi = 0; oi < numOutcomes; oi++)
                    {
                        if (updates[pi][oi][VALUE] != 0)
                        {
                            predParams[oi] += updates[pi][oi][VALUE]*
                                              (numSequences*(iterations - updates[pi][oi][ITER]) -
                                               updates[pi][oi][EVENT]);
                        }
                        if (predParams[oi] != 0)
                        {
                            predParams[oi] /= totIterations;
                            averageParams[pi].setParameter(oi, predParams[oi]);
                            //System.err.println("updates["+pi+"]["+oi+"]=("+updates[pi][oi][ITER]+","+updates[pi][oi][EVENT]+","+updates[pi][oi][VALUE]+") + ("+iterations+","+0+","+params[pi].getParameters()[oi]+") -> "+averageParams[pi].getParameters()[oi]);
                        }
                    }
                }
            }
            display(". (" + numCorrect + "/" + numEvents + ") " + ((double) numCorrect/numEvents) + "\n");
        }

        private void trainingStats(MutableContext[] @params)
        {
            int numCorrect = 0;
            int oei = 0;
            foreach (Sequence<Event> sequence in sequenceStream)
            {
                Event[] taggerEvents = sequenceStream.updateContext(sequence,
                    new PerceptronModel(@params, predLabels, pmap, outcomeLabels));
                for (int ei = 0; ei < taggerEvents.Length; ei++,oei++)
                {
                    int max = (int) omap[taggerEvents[ei].Outcome];
                    if (max == outcomeList[oei])
                    {
                        numCorrect++;
                    }
                }
            }
            display(". (" + numCorrect + "/" + numEvents + ") " + ((double) numCorrect/numEvents) + "\n");
        }
    }
}