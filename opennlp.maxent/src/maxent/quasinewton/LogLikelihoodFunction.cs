using System;
using System.Collections.Generic;
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
using opennlp.nonjava.extensions;
using opennlp.nonjava.helperclasses;

namespace opennlp.maxent.quasinewton
{
    using DataIndexer = opennlp.model.DataIndexer;
    using OnePassRealValueDataIndexer = opennlp.model.OnePassRealValueDataIndexer;

    /// <summary>
    /// Evaluate log likelihood and its gradient from DataIndexer.
    /// </summary>
    public class LogLikelihoodFunction : DifferentiableFunction
    {
        private int domainDimension;
        private double value;
        private double[] gradient;
        private double[] lastX;
        private double[] empiricalCount;
        private int numOutcomes;
        private int numFeatures;
        private int numContexts;
        private double[][] probModel;

        private string[] outcomeLabels;
        private string[] predLabels;

        private int[][] outcomePatterns;

        // infos from data index;
        private readonly float[][] values;
        private readonly int[][] contexts;
        private readonly int[] outcomeList;
        private readonly int[] numTimesEventsSeen;

        public LogLikelihoodFunction(DataIndexer indexer)
        {
            // get data from indexer.
            if (indexer is OnePassRealValueDataIndexer)
            {
                this.values = indexer.Values;
            }
            else
            {
                this.values = null;
            }

            this.contexts = indexer.Contexts;
            this.outcomeList = indexer.OutcomeList;
            this.numTimesEventsSeen = indexer.NumTimesEventsSeen;

            this.outcomeLabels = indexer.OutcomeLabels;
            this.predLabels = indexer.PredLabels;

            this.numOutcomes = indexer.OutcomeLabels.Length;
            this.numFeatures = indexer.PredLabels.Length;
            this.numContexts = this.contexts.Length;
            this.domainDimension = numOutcomes*numFeatures;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: this.probModel = new double[numContexts][numOutcomes];
            this.probModel = RectangularArrays.ReturnRectangularDoubleArray(numContexts, numOutcomes);
            this.gradient = null;
        }

        public virtual double valueAt(double[] x)
        {
            if (!checkLastX(x))
            {
                calculate(x);
            }
            return value;
        }

        public virtual double[] gradientAt(double[] x)
        {
            if (!checkLastX(x))
            {
                calculate(x);
            }
            return gradient;
        }

        public virtual int DomainDimension
        {
            get { return this.domainDimension; }
        }

        public virtual double[] InitialPoint
        {
            get { return new double[domainDimension]; }
        }

        public virtual string[] PredLabels
        {
            get { return this.predLabels; }
        }

        public virtual string[] OutcomeLabels
        {
            get { return this.outcomeLabels; }
        }

        public virtual int[][] OutcomePatterns
        {
            get { return this.outcomePatterns; }
        }

        private void calculate(double[] x)
        {
            if (x.Length != this.domainDimension)
            {
                throw new ArgumentException("x is invalid, its dimension is not equal to the function.");
            }

            initProbModel();
            if (this.empiricalCount == null)
            {
                initEmpCount();
            }

            // sum up log likelihood and empirical feature count for gradient calculation.
            double logLikelihood = 0.0;

            for (int ci = 0; ci < numContexts; ci++)
            {
                double voteSum = 0.0;

                for (int af = 0; af < this.contexts[ci].Length; af++)
                {
                    int vectorIndex = indexOf(contexts, outcomeList[ci], contexts[ci][af]);
                    double predValue = 1.0;
                    if (values != null)
                    {
                        predValue = this.values[ci][af];
                    }
                    if (predValue == 0.0)
                    {
                        continue;
                    }

                    voteSum += predValue*x[vectorIndex];
                }
                probModel[ci][this.outcomeList[ci]] = Math.Exp(voteSum);

                double totalVote = 0.0;
                for (int i = 0; i < numOutcomes; i++)
                {
                    totalVote += probModel[ci][i];
                }
                for (int i = 0; i < numOutcomes; i++)
                {
                    probModel[ci][i] /= totalVote;
                }
                for (int i = 0; i < numTimesEventsSeen[ci]; i++)
                {
                    logLikelihood += Math.Log(probModel[ci][this.outcomeList[ci]]);
                }
            }
            this.value = logLikelihood;

            // calculate gradient.
            double[] expectedCount = new double[numOutcomes*numFeatures];
            for (int ci = 0; ci < numContexts; ci++)
            {
                for (int oi = 0; oi < numOutcomes; oi++)
                {
                    for (int af = 0; af < contexts[ci].Length; af++)
                    {
                        int vectorIndex = indexOf(oi, contexts[ci][af]);
                        double predValue = 1.0;
                        if (values != null)
                        {
                            predValue = this.values[ci][af];
                        }
                        if (predValue == 0.0)
                        {
                            continue;
                        }

                        expectedCount[vectorIndex] += predValue*probModel[ci][oi]*this.numTimesEventsSeen[ci];
                    }
                }
            }

            double[] gradient = new double[domainDimension];
            for (int i = 0; i < numOutcomes*numFeatures; i++)
            {
                gradient[i] = expectedCount[i] - this.empiricalCount[i];
            }
            this.gradient = gradient;

            // update last evaluated x.
            this.lastX = x.Clone() as double[];
        }

        /// <param name="x"> vector that represents point to evaluate at. </param>
        /// <returns> check x is whether last evaluated point or not. </returns>
        private bool checkLastX(double[] x)
        {
            if (this.lastX == null)
            {
                return false;
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (lastX[i] != x[i])
                {
                    return false;
                }
            }
            return true;
        }

        private int IndexOf(int outcomeId, int featureId)
        {
            return outcomeId*numFeatures + featureId;
        }

        private void initProbModel()
        {
            for (int i = 0; i < this.probModel.Length; i++)
            {
                Arrays.fill(this.probModel[i], 1.0);
            }
        }

        private void initEmpCount()
        {
            this.empiricalCount = new double[numOutcomes*numFeatures];
            this.outcomePatterns = new int[predLabels.Length][];

            for (int ci = 0; ci < numContexts; ci++)
            {
                for (int af = 0; af < contexts[ci].Length; af++)
                {
                    int vectorIndex = indexOf(outcomeList[ci], contexts[ci][af]);
                    if (values != null)
                    {
                        empiricalCount[vectorIndex] += values[ci][af]*numTimesEventsSeen[ci];
                    }
                    else
                    {
                        empiricalCount[vectorIndex] += 1.0*numTimesEventsSeen[ci];
                    }
                }
            }

            for (int fi = 0; fi < this.outcomePatterns.Length; fi++)
            {
                List<int?> pattern = new List<int?>();
                for (int oi = 0; oi < outcomeLabels.Length; oi++)
                {
                    int countIndex = fi + (this.predLabels.Length*oi);
                    if (this.empiricalCount[countIndex] > 0)
                    {
                        pattern.Add(oi);
                    }
                }
                outcomePatterns[fi] = new int[pattern.Count];
                for (int i = 0; i < pattern.Count; i++)
                {
                    outcomePatterns[fi][i] = pattern[i].GetValueOrDefault();
                }
            }
        }

        private int indexOf(int outcome, int i)
        {
            throw new NotImplementedException();
        }

        public int indexOf(int[][] array, int index, int otherIndex)
        {
            throw new NotImplementedException();
        }
    }
}