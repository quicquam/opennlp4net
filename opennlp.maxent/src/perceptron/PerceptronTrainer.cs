using System;

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
    using EvalParameters = opennlp.model.EvalParameters;
    using MutableContext = opennlp.model.MutableContext;

    /// <summary>
    /// Trains models using the perceptron algorithm.  Each outcome is represented as
    /// a binary perceptron classifier.  This supports standard (integer) weighting as well
    /// average weighting as described in:
    /// Discriminative Training Methods for Hidden Markov Models: Theory and Experiments
    /// with the Perceptron Algorithm. Michael Collins, EMNLP 2002.
    /// 
    /// </summary>
    public class PerceptronTrainer
    {
        public const double TOLERANCE_DEFAULT = .00001;

        /// <summary>
        /// Number of unique events which occurred in the event set. </summary>
        private int numUniqueEvents;

        /// <summary>
        /// Number of events in the event set. </summary>
        private int numEvents;

        /// <summary>
        /// Number of predicates. </summary>
        private int numPreds;

        /// <summary>
        /// Number of outcomes. </summary>
        private int numOutcomes;

        /// <summary>
        /// Records the array of predicates seen in each event. </summary>
        private int[][] contexts;

        /// <summary>
        /// The value associates with each context. If null then context values are assumes to be 1. </summary>
        private float[][] values;

        /// <summary>
        /// List of outcomes for each event i, in context[i]. </summary>
        private int[] outcomeList;

        /// <summary>
        /// Records the num of times an event has been seen for each event i, in context[i]. </summary>
        private int[] numTimesEventsSeen;

        /// <summary>
        /// Stores the String names of the outcomes.  The GIS only tracks outcomes
        /// as ints, and so this array is needed to save the model to disk and
        /// thereby allow users to know what the outcome was in human
        /// understandable terms. 
        /// </summary>
        private string[] outcomeLabels;

        /// <summary>
        /// Stores the String names of the predicates. The GIS only tracks
        /// predicates as ints, and so this array is needed to save the model to
        /// disk and thereby allow users to know what the outcome was in human
        /// understandable terms. 
        /// </summary>
        private string[] predLabels;

        private bool printMessages = true;

        private double tolerance = TOLERANCE_DEFAULT;

        private double? stepSizeDecrease;

        private bool useSkippedlAveraging;

        /// <summary>
        /// Specifies the tolerance. If the change in training set accuracy
        /// is less than this, stop iterating.
        /// </summary>
        /// <param name="tolerance"> </param>
        public virtual double Tolerance
        {
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentException("tolerance must be a positive number but is " + value + "!");
                }

                this.tolerance = value;
            }
        }

        /// <summary>
        /// Enables and sets step size decrease. The step size is
        /// decreased every iteration by the specified value.
        /// </summary>
        /// <param name="decrease"> - step size decrease in percent </param>
        public virtual double StepSizeDecrease
        {
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new System.ArgumentException("decrease must be between 0 and 100 but is " + value + "!");
                }

                stepSizeDecrease = value;
            }
        }

        /// <summary>
        /// Enables skipped averaging, this flag changes the standard
        /// averaging to special averaging instead.
        /// <para>
        /// If we are doing averaging, and the current iteration is one
        /// of the first 20 or it is a perfect square, then updated the
        /// summed parameters. 
        /// </para>
        /// <para>
        /// The reason we don't take all of them is that the parameters change
        /// less toward the end of training, so they drown out the contributions
        /// of the more volatile early iterations. The use of perfect
        /// squares allows us to sample from successively farther apart iterations.
        ///  
        /// </para>
        /// </summary>
        /// <param name="averaging"> averaging flag </param>
        public virtual bool SkippedAveraging
        {
            set { useSkippedlAveraging = value; }
        }

        public virtual AbstractModel trainModel(int iterations, DataIndexer di, int cutoff)
        {
            return trainModel(iterations, di, cutoff, true);
        }

        public virtual AbstractModel trainModel(int iterations, DataIndexer di, int cutoff, bool useAverage)
        {
            display("Incorporating indexed data for training...  \n");
            contexts = di.Contexts;
            values = di.Values;
            numTimesEventsSeen = di.NumTimesEventsSeen;
            numEvents = di.NumEvents;
            numUniqueEvents = contexts.Length;

            outcomeLabels = di.OutcomeLabels;
            outcomeList = di.OutcomeList;

            predLabels = di.PredLabels;
            numPreds = predLabels.Length;
            numOutcomes = outcomeLabels.Length;

            display("done.\n");

            display("\tNumber of Event Tokens: " + numUniqueEvents + "\n");
            display("\t    Number of Outcomes: " + numOutcomes + "\n");
            display("\t  Number of Predicates: " + numPreds + "\n");

            display("Computing model parameters...\n");

            MutableContext[] finalParameters = findParameters(iterations, useAverage);

            display("...done.\n");

            /// <summary>
            ///************* Create and return the model ***************** </summary>
            return new PerceptronModel(finalParameters, predLabels, outcomeLabels);
        }

        private MutableContext[] findParameters(int iterations, bool useAverage)
        {
            display("Performing " + iterations + " iterations.\n");

            int[] allOutcomesPattern = new int[numOutcomes];
            for (int oi = 0; oi < numOutcomes; oi++)
            {
                allOutcomesPattern[oi] = oi;
            }

            /// <summary>
            /// Stores the estimated parameter value of each predicate during iteration. </summary>
            MutableContext[] parameters = new MutableContext[numPreds];
            for (int pi = 0; pi < numPreds; pi++)
            {
                parameters[pi] = new MutableContext(allOutcomesPattern, new double[numOutcomes]);
                for (int aoi = 0; aoi < numOutcomes; aoi++)
                {
                    parameters[pi].setParameter(aoi, 0.0);
                }
            }

            EvalParameters evalParams = new EvalParameters(parameters, numOutcomes);

            /// <summary>
            /// Stores the sum of parameter values of each predicate over many iterations. </summary>
            MutableContext[] summedParams = new MutableContext[numPreds];
            if (useAverage)
            {
                for (int pi = 0; pi < numPreds; pi++)
                {
                    summedParams[pi] = new MutableContext(allOutcomesPattern, new double[numOutcomes]);
                    for (int aoi = 0; aoi < numOutcomes; aoi++)
                    {
                        summedParams[pi].setParameter(aoi, 0.0);
                    }
                }
            }

            // Keep track of the previous three accuracies. The difference of
            // the mean of these and the current training set accuracy is used
            // with tolerance to decide whether to stop.
            double prevAccuracy1 = 0.0;
            double prevAccuracy2 = 0.0;
            double prevAccuracy3 = 0.0;

            // A counter for the denominator for averaging.
            int numTimesSummed = 0;

            double stepsize = 1;
            for (int i = 1; i <= iterations; i++)
            {
                // Decrease the stepsize by a small amount.
                if (stepSizeDecrease != null)
                {
                    stepsize *= 1 - stepSizeDecrease.GetValueOrDefault();
                }

                displayIteration(i);

                int numCorrect = 0;

                for (int ei = 0; ei < numUniqueEvents; ei++)
                {
                    int targetOutcome = outcomeList[ei];

                    for (int ni = 0; ni < this.numTimesEventsSeen[ei]; ni++)
                    {
                        // Compute the model's prediction according to the current parameters.
                        double[] modelDistribution = new double[numOutcomes];
                        if (values != null)
                        {
                            PerceptronModel.eval(contexts[ei], values[ei], modelDistribution, evalParams, false);
                        }
                        else
                        {
                            PerceptronModel.eval(contexts[ei], null, modelDistribution, evalParams, false);
                        }

                        int maxOutcome = maxIndex(modelDistribution);

                        // If the predicted outcome is different from the target
                        // outcome, do the standard update: boost the parameters
                        // associated with the target and reduce those associated
                        // with the incorrect predicted outcome.
                        if (maxOutcome != targetOutcome)
                        {
                            for (int ci = 0; ci < contexts[ei].Length; ci++)
                            {
                                int pi = contexts[ei][ci];
                                if (values == null)
                                {
                                    parameters[pi].updateParameter(targetOutcome, stepsize);
                                    parameters[pi].updateParameter(maxOutcome, -stepsize);
                                }
                                else
                                {
                                    parameters[pi].updateParameter(targetOutcome, stepsize*values[ei][ci]);
                                    parameters[pi].updateParameter(maxOutcome, -stepsize*values[ei][ci]);
                                }
                            }
                        }

                        // Update the counts for accuracy.
                        if (maxOutcome == targetOutcome)
                        {
                            numCorrect++;
                        }
                    }
                }

                // Calculate the training accuracy and display.
                double trainingAccuracy = (double) numCorrect/numEvents;
                if (i < 10 || (i%10) == 0)
                {
                    display(". (" + numCorrect + "/" + numEvents + ") " + trainingAccuracy + "\n");
                }

                // TODO: Make averaging configurable !!!

                bool doAveraging;

                if (useAverage && useSkippedlAveraging && (i < 20 || isPerfectSquare(i)))
                {
                    doAveraging = true;
                }
                else if (useAverage)
                {
                    doAveraging = true;
                }
                else
                {
                    doAveraging = false;
                }

                if (doAveraging)
                {
                    numTimesSummed++;
                    for (int pi = 0; pi < numPreds; pi++)
                    {
                        for (int aoi = 0; aoi < numOutcomes; aoi++)
                        {
                            summedParams[pi].updateParameter(aoi, parameters[pi].Parameters[aoi]);
                        }
                    }
                }

                // If the tolerance is greater than the difference between the
                // current training accuracy and all of the previous three
                // training accuracies, stop training.
                if (Math.Abs(prevAccuracy1 - trainingAccuracy) < tolerance &&
                    Math.Abs(prevAccuracy2 - trainingAccuracy) < tolerance &&
                    Math.Abs(prevAccuracy3 - trainingAccuracy) < tolerance)
                {
                    display("Stopping: change in training set accuracy less than " + tolerance + "\n");
                    break;
                }

                // Update the previous training accuracies.
                prevAccuracy1 = prevAccuracy2;
                prevAccuracy2 = prevAccuracy3;
                prevAccuracy3 = trainingAccuracy;
            }

            // Output the final training stats.
            trainingStats(evalParams);

            // Create averaged parameters
            if (useAverage)
            {
                for (int pi = 0; pi < numPreds; pi++)
                {
                    for (int aoi = 0; aoi < numOutcomes; aoi++)
                    {
                        summedParams[pi].setParameter(aoi, summedParams[pi].Parameters[aoi]/numTimesSummed);
                    }
                }

                return summedParams;
            }
            else
            {
                return parameters;
            }
        }

        private double trainingStats(EvalParameters evalParams)
        {
            int numCorrect = 0;

            for (int ei = 0; ei < numUniqueEvents; ei++)
            {
                for (int ni = 0; ni < this.numTimesEventsSeen[ei]; ni++)
                {
                    double[] modelDistribution = new double[numOutcomes];

                    if (values != null)
                    {
                        PerceptronModel.eval(contexts[ei], values[ei], modelDistribution, evalParams, false);
                    }
                    else
                    {
                        PerceptronModel.eval(contexts[ei], null, modelDistribution, evalParams, false);
                    }

                    int max = maxIndex(modelDistribution);
                    if (max == outcomeList[ei])
                    {
                        numCorrect++;
                    }
                }
            }
            double trainingAccuracy = (double) numCorrect/numEvents;
            display("Stats: (" + numCorrect + "/" + numEvents + ") " + trainingAccuracy + "\n");
            return trainingAccuracy;
        }


        private int maxIndex(double[] values)
        {
            int max = 0;
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > values[max])
                {
                    max = i;
                }
            }
            return max;
        }

        private void display(string s)
        {
            if (printMessages)
            {
                Console.Write(s);
            }
        }

        private void displayIteration(int i)
        {
            if (i > 10 && (i%10) != 0)
            {
                return;
            }

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
        }

        // See whether a number is a perfect square. Inefficient, but fine
        // for our purposes.
        private static bool isPerfectSquare(int n)
        {
            int root = (int) Math.Sqrt(n);
            return root*root == n;
        }
    }
}