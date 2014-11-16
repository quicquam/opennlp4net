using System;
using System.Collections.Generic;
using j4n.Interfaces;
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
using opennlp.nonjava.helperclasses;

namespace opennlp.maxent
{
    using DataIndexer = opennlp.model.DataIndexer;
    using EvalParameters = opennlp.model.EvalParameters;
    using EventStream = opennlp.model.EventStream;
    using MutableContext = opennlp.model.MutableContext;
    using OnePassDataIndexer = opennlp.model.OnePassDataIndexer;
    using Prior = opennlp.model.Prior;
    using UniformPrior = opennlp.model.UniformPrior;


    /// <summary>
    /// An implementation of Generalized Iterative Scaling.  The reference paper
    /// for this implementation was Adwait Ratnaparkhi's tech report at the
    /// University of Pennsylvania's Institute for Research in Cognitive Science,
    /// and is available at <a href ="ftp://ftp.cis.upenn.edu/pub/ircs/tr/97-08.ps.Z"><code>ftp://ftp.cis.upenn.edu/pub/ircs/tr/97-08.ps.Z</code></a>. 
    /// 
    /// The slack parameter used in the above implementation has been removed by default
    /// from the computation and a method for updating with Gaussian smoothing has been
    /// added per Investigating GIS and Smoothing for Maximum Entropy Taggers, Clark and Curran (2002).  
    /// <a href="http://acl.ldc.upenn.edu/E/E03/E03-1071.pdf"><code>http://acl.ldc.upenn.edu/E/E03/E03-1071.pdf</code></a>
    /// The slack parameter can be used by setting <code>useSlackParameter</code> to true.
    /// Gaussian smoothing can be used by setting <code>useGaussianSmoothing</code> to true. 
    /// 
    /// A prior can be used to train models which converge to the distribution which minimizes the
    /// relative entropy between the distribution specified by the empirical constraints of the training
    /// data and the specified prior.  By default, the uniform distribution is used as the prior.
    /// </summary>
    public class GISTrainer
    {
        /// <summary>
        /// Specifies whether unseen context/outcome pairs should be estimated as occur very infrequently.
        /// </summary>
        private bool useSimpleSmoothing = false;

        /// <summary>
        /// Specified whether parameter updates should prefer a distribution of parameters which
        /// is gaussian.
        /// </summary>
        private bool useGaussianSmoothing = false;

        private double sigma = 2.0;

        // If we are using smoothing, this is used as the "number" of
        // times we want the trainer to imagine that it saw a feature that it
        // actually didn't see.  Defaulted to 0.1.
        private double _smoothingObservation = 0.1;

        private readonly bool printMessages;

        /// <summary>
        /// Number of unique events which occured in the event set. 
        /// </summary>
        private int numUniqueEvents;

        /// <summary>
        /// Number of predicates. 
        /// </summary>
        private int numPreds;

        /// <summary>
        /// Number of outcomes. 
        /// </summary>
        private int numOutcomes;

        /// <summary>
        /// Records the array of predicates seen in each event.
        /// </summary>
        private int[][] contexts;

        /// <summary>
        /// The value associated with each context. If null then context values are assumes to be 1.
        /// </summary>
        private float[][] values;

        /// <summary>
        /// List of outcomes for each event i, in context[i].
        /// </summary>
        private int[] outcomeList;

        /// <summary>
        /// Records the num of times an event has been seen for each event i, in context[i].
        /// </summary>
        private int[] numTimesEventsSeen;

        /// <summary>
        /// The number of times a predicate occured in the training data.
        /// </summary>
        private int[] predicateCounts;

        private int cutoff;

        /// <summary>
        /// Stores the String names of the outcomes. The GIS only tracks outcomes as
        /// ints, and so this array is needed to save the model to disk and thereby
        /// allow users to know what the outcome was in human understandable terms.
        /// </summary>
        private string[] outcomeLabels;

        /// <summary>
        /// Stores the String names of the predicates. The GIS only tracks predicates
        /// as ints, and so this array is needed to save the model to disk and thereby
        /// allow users to know what the outcome was in human understandable terms.
        /// </summary>
        private string[] predLabels;

        /// <summary>
        /// Stores the observed expected values of the features based on training data.
        /// </summary>
        private MutableContext[] observedExpects;

        /// <summary>
        /// Stores the estimated parameter value of each predicate during iteration
        /// </summary>
        private MutableContext[] @params;

        /// <summary>
        /// Stores the expected values of the features based on the current models 
        /// </summary>
        private MutableContext[][] modelExpects;

        /// <summary>
        /// This is the prior distribution that the model uses for training.
        /// </summary>
        private Prior prior;

        private const double LLThreshold = 0.0001;

        /// <summary>
        /// Initial probability for all outcomes.
        /// </summary>
        private EvalParameters evalParams;

        /// <summary>
        /// Creates a new <code>GISTrainer</code> instance which does not print
        /// progress messages about training to STDOUT.
        /// 
        /// </summary>
        internal GISTrainer()
        {
            printMessages = false;
        }

        /// <summary>
        /// Creates a new <code>GISTrainer</code> instance.
        /// </summary>
        /// <param name="printMessages"> sends progress messages about training to
        ///                      STDOUT when true; trains silently otherwise. </param>
        public GISTrainer(bool printMessages)
        {
            this.printMessages = printMessages;
        }

        /// <summary>
        /// Sets whether this trainer will use smoothing while training the model.
        /// This can improve model accuracy, though training will potentially take
        /// longer and use more memory.  Model size will also be larger.
        /// </summary>
        /// <param name="smooth"> true if smoothing is desired, false if not </param>
        public virtual bool Smoothing
        {
            set { useSimpleSmoothing = value; }
        }

        /// <summary>
        /// Sets whether this trainer will use smoothing while training the model.
        /// This can improve model accuracy, though training will potentially take
        /// longer and use more memory.  Model size will also be larger.
        /// </summary>
        /// <param name="timesSeen"> the "number" of times we want the trainer to imagine
        ///                  it saw a feature that it actually didn't see </param>
        public virtual double SmoothingObservation
        {
            set { _smoothingObservation = value; }
        }

        /// <summary>
        /// Sets whether this trainer will use smoothing while training the model.
        /// This can improve model accuracy, though training will potentially take
        /// longer and use more memory.  Model size will also be larger.
        /// 
        /// </summary>
        public virtual double GaussianSigma
        {
            set
            {
                useGaussianSmoothing = true;
                sigma = value;
            }
        }

        /// <summary>
        /// Trains a GIS model on the event in the specified event stream, using the specified number
        /// of iterations and the specified count cutoff. </summary>
        /// <param name="eventStream"> A stream of all events. </param>
        /// <param name="iterations"> The number of iterations to use for GIS. </param>
        /// <param name="cutoff"> The number of times a feature must occur to be included. </param>
        /// <returns> A GIS model trained with specified  </returns>
        public virtual GISModel trainModel(EventStream eventStream, int iterations, int cutoff)
        {
            return trainModel(iterations, new OnePassDataIndexer(eventStream, cutoff), cutoff);
        }

        /// <summary>
        /// Train a model using the GIS algorithm.
        /// </summary>
        /// <param name="iterations">  The number of GIS iterations to perform. </param>
        /// <param name="di"> The data indexer used to compress events in memory. </param>
        /// <returns> The newly trained model, which can be used immediately or saved
        ///         to disk using an opennlp.maxent.io.GISModelWriter object. </returns>
        public virtual GISModel trainModel(int iterations, DataIndexer di, int cutoff)
        {
            return trainModel(iterations, di, new UniformPrior(), cutoff, 1);
        }

        /// <summary>
        /// Train a model using the GIS algorithm.
        /// </summary>
        /// <param name="iterations">  The number of GIS iterations to perform. </param>
        /// <param name="di"> The data indexer used to compress events in memory. </param>
        /// <param name="modelPrior"> The prior distribution used to train this model. </param>
        /// <returns> The newly trained model, which can be used immediately or saved
        ///         to disk using an opennlp.maxent.io.GISModelWriter object. </returns>
        public virtual GISModel trainModel(int iterations, DataIndexer di, Prior modelPrior, int cutoff, int threads)
        {
            if (threads <= 0)
            {
                throw new System.ArgumentException("threads must be at least one or greater but is " + threads + "!");
            }

            modelExpects = new MutableContext[threads][];

            /// <summary>
            ///************ Incorporate all of the needed info ***************** </summary>
            display("Incorporating indexed data for training...  \n");
            contexts = di.Contexts;
            values = di.Values;
            this.cutoff = cutoff;
            predicateCounts = di.PredCounts;
            numTimesEventsSeen = di.NumTimesEventsSeen;
            numUniqueEvents = contexts.Length;
            this.prior = modelPrior;
            //printTable(contexts);

            // determine the correction constant and its inverse
            double correctionConstant = 0;
            for (int ci = 0; ci < contexts.Length; ci++)
            {
                if (values == null || values[ci] == null)
                {
                    if (contexts[ci].Length > correctionConstant)
                    {
                        correctionConstant = contexts[ci].Length;
                    }
                }
                else
                {
                    float cl = values[ci][0];
                    for (int vi = 1; vi < values[ci].Length; vi++)
                    {
                        cl += values[ci][vi];
                    }

                    if (cl > correctionConstant)
                    {
                        correctionConstant = cl;
                    }
                }
            }
            display("done.\n");

            outcomeLabels = di.OutcomeLabels;
            outcomeList = di.OutcomeList;
            numOutcomes = outcomeLabels.Length;

            predLabels = di.PredLabels;
            prior.setLabels(outcomeLabels, predLabels);
            numPreds = predLabels.Length;

            display("\tNumber of Event Tokens: " + numUniqueEvents + "\n");
            display("\t    Number of Outcomes: " + numOutcomes + "\n");
            display("\t  Number of Predicates: " + numPreds + "\n");

            // set up feature arrays
            float[][] predCount = RectangularArrays.ReturnRectangularFloatArray(numPreds, numOutcomes);
            for (int ti = 0; ti < numUniqueEvents; ti++)
            {
                for (int j = 0; j < contexts[ti].Length; j++)
                {
                    if (values != null && values[ti] != null)
                    {
                        predCount[contexts[ti][j]][outcomeList[ti]] += numTimesEventsSeen[ti]*values[ti][j];
                    }
                    else
                    {
                        predCount[contexts[ti][j]][outcomeList[ti]] += numTimesEventsSeen[ti];
                    }
                }
            }

            //printTable(predCount);
            di = null; // don't need it anymore

            // A fake "observation" to cover features which are not detected in
            // the data.  The default is to assume that we observed "1/10th" of a
            // feature during training.
            double smoothingObservation = _smoothingObservation;

            // Get the observed expectations of the features. Strictly speaking,
            // we should divide the counts by the number of Tokens, but because of
            // the way the model's expectations are approximated in the
            // implementation, this is cancelled out when we compute the next
            // iteration of a parameter, making the extra divisions wasteful.
            @params = new MutableContext[numPreds];
            for (int i = 0; i < modelExpects.Length; i++)
            {
                modelExpects[i] = new MutableContext[numPreds];
            }
            observedExpects = new MutableContext[numPreds];

            // The model does need the correction constant and the correction feature. The correction constant
            // is only needed during training, and the correction feature is not necessary.
            // For compatibility reasons the model contains form now on a correction constant of 1, 
            // and a correction param 0.
            evalParams = new EvalParameters(@params, 0, 1, numOutcomes);
            int[] activeOutcomes = new int[numOutcomes];
            int[] outcomePattern;
            int[] allOutcomesPattern = new int[numOutcomes];
            for (int oi = 0; oi < numOutcomes; oi++)
            {
                allOutcomesPattern[oi] = oi;
            }
            int numActiveOutcomes = 0;
            for (int pi = 0; pi < numPreds; pi++)
            {
                numActiveOutcomes = 0;
                if (useSimpleSmoothing)
                {
                    numActiveOutcomes = numOutcomes;
                    outcomePattern = allOutcomesPattern;
                }
                else //determine active outcomes
                {
                    for (int oi = 0; oi < numOutcomes; oi++)
                    {
                        if (predCount[pi][oi] > 0 && predicateCounts[pi] >= cutoff)
                        {
                            activeOutcomes[numActiveOutcomes] = oi;
                            numActiveOutcomes++;
                        }
                    }
                    if (numActiveOutcomes == numOutcomes)
                    {
                        outcomePattern = allOutcomesPattern;
                    }
                    else
                    {
                        outcomePattern = new int[numActiveOutcomes];
                        for (int aoi = 0; aoi < numActiveOutcomes; aoi++)
                        {
                            outcomePattern[aoi] = activeOutcomes[aoi];
                        }
                    }
                }
                @params[pi] = new MutableContext(outcomePattern, new double[numActiveOutcomes]);
                for (int i = 0; i < modelExpects.Length; i++)
                {
                    modelExpects[i][pi] = new MutableContext(outcomePattern, new double[numActiveOutcomes]);
                }
                observedExpects[pi] = new MutableContext(outcomePattern, new double[numActiveOutcomes]);
                for (int aoi = 0; aoi < numActiveOutcomes; aoi++)
                {
                    int oi = outcomePattern[aoi];
                    @params[pi].setParameter(aoi, 0.0);
                    foreach (MutableContext[] modelExpect in modelExpects)
                    {
                        modelExpect[pi].setParameter(aoi, 0.0);
                    }
                    if (predCount[pi][oi] > 0)
                    {
                        observedExpects[pi].setParameter(aoi, predCount[pi][oi]);
                    }
                    else if (useSimpleSmoothing)
                    {
                        observedExpects[pi].setParameter(aoi, smoothingObservation);
                    }
                }
            }

            predCount = null; // don't need it anymore

            display("...done.\n");

            /// <summary>
            ///*************** Find the parameters *********************** </summary>
            if (threads == 1)
            {
                display("Computing model parameters ...\n");
            }
            else
            {
                display("Computing model parameters in " + threads + " threads...\n");
            }

            findParameters(iterations, correctionConstant);

            /// <summary>
            ///************* Create and return the model ***************** </summary>
            // To be compatible with old models the correction constant is always 1
            return new GISModel(@params, predLabels, outcomeLabels, 1, evalParams.CorrectionParam);
        }

        /* Estimate and return the model parameters. */

        private void findParameters(int iterations, double correctionConstant)
        {
            double prevLL = 0.0;
            double currLL = 0.0;
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
                currLL = nextIteration(correctionConstant);
                if (i > 1)
                {
                    if (prevLL > currLL)
                    {
                        Console.Error.WriteLine("Model Diverging: loglikelihood decreased");
                        break;
                    }
                    if (currLL - prevLL < LLThreshold)
                    {
                        break;
                    }
                }
                prevLL = currLL;
            }

            // kill a bunch of these big objects now that we don't need them
            observedExpects = null;
            modelExpects = null;
            numTimesEventsSeen = null;
            contexts = null;
        }

        //modeled on implementation in  Zhang Le's maxent kit
        private double gaussianUpdate(int predicate, int oid, int n, double correctionConstant)
        {
            double param = @params[predicate].Parameters[oid];
            double x0 = 0.0;
            double modelValue = modelExpects[0][predicate].Parameters[oid];
            double observedValue = observedExpects[predicate].Parameters[oid];
            for (int i = 0; i < 50; i++)
            {
                double tmp = modelValue*Math.Exp(correctionConstant*x0);
                double f = tmp + (param + x0)/sigma - observedValue;
                double fp = tmp*correctionConstant + 1/sigma;
                if (fp == 0)
                {
                    break;
                }
                double x = x0 - f/fp;
                if (Math.Abs(x - x0) < 0.000001)
                {
                    x0 = x;
                    break;
                }
                x0 = x;
            }
            return x0;
        }

        private class ModelExpactationComputeTask : Callable<ModelExpactationComputeTask>
        {
            private readonly GISTrainer outerInstance;


            internal readonly int startIndex;
            internal readonly int length;

            internal double loglikelihood = 0;

            internal int numEvents = 0;
            internal int numCorrect = 0;

            internal readonly int threadIndex;

            // startIndex to compute, number of events to compute
            internal ModelExpactationComputeTask(GISTrainer outerInstance, int threadIndex, int startIndex, int length)
            {
                this.outerInstance = outerInstance;
                this.startIndex = startIndex;
                this.length = length;
                this.threadIndex = threadIndex;
            }

            public virtual ModelExpactationComputeTask call()
            {
                double[] modelDistribution = new double[outerInstance.numOutcomes];


                for (int ei = startIndex; ei < startIndex + length; ei++)
                {
                    // TODO: check interruption status here, if interrupted set a poisoned flag and return

                    if (outerInstance.values != null)
                    {
                        outerInstance.prior.logPrior(modelDistribution, outerInstance.contexts[ei],
                            outerInstance.values[ei]);
                        GISModel.eval(outerInstance.contexts[ei], outerInstance.values[ei], modelDistribution,
                            outerInstance.evalParams);
                    }
                    else
                    {
                        outerInstance.prior.logPrior(modelDistribution, outerInstance.contexts[ei]);
                        GISModel.eval(outerInstance.contexts[ei], modelDistribution, outerInstance.evalParams);
                    }
                    for (int j = 0; j < outerInstance.contexts[ei].Length; j++)
                    {
                        int pi = outerInstance.contexts[ei][j];
                        if (outerInstance.predicateCounts[pi] >= outerInstance.cutoff)
                        {
                            int[] activeOutcomes = outerInstance.modelExpects[threadIndex][pi].Outcomes;
                            for (int aoi = 0; aoi < activeOutcomes.Length; aoi++)
                            {
                                int oi = activeOutcomes[aoi];

                                // numTimesEventsSeen must also be thread safe
                                if (outerInstance.values != null && outerInstance.values[ei] != null)
                                {
                                    outerInstance.modelExpects[threadIndex][pi].updateParameter(aoi,
                                        modelDistribution[oi]*outerInstance.values[ei][j]*
                                        outerInstance.numTimesEventsSeen[ei]);
                                }
                                else
                                {
                                    outerInstance.modelExpects[threadIndex][pi].updateParameter(aoi,
                                        modelDistribution[oi]*outerInstance.numTimesEventsSeen[ei]);
                                }
                            }
                        }
                    }

                    loglikelihood += Math.Log(modelDistribution[outerInstance.outcomeList[ei]])*
                                     outerInstance.numTimesEventsSeen[ei];

                    numEvents += outerInstance.numTimesEventsSeen[ei];
                    if (outerInstance.printMessages)
                    {
                        int max = 0;
                        for (int oi = 1; oi < outerInstance.numOutcomes; oi++)
                        {
                            if (modelDistribution[oi] > modelDistribution[max])
                            {
                                max = oi;
                            }
                        }
                        if (max == outerInstance.outcomeList[ei])
                        {
                            numCorrect += outerInstance.numTimesEventsSeen[ei];
                        }
                    }
                }

                return this;
            }

            internal virtual int NumEvents
            {
                get
                {
                    lock (this)
                    {
                        return numEvents;
                    }
                }
            }

            internal virtual int NumCorrect
            {
                get
                {
                    lock (this)
                    {
                        return numCorrect;
                    }
                }
            }

            internal virtual double Loglikelihood
            {
                get
                {
                    lock (this)
                    {
                        return loglikelihood;
                    }
                }
            }
        }

        /* Compute one iteration of GIS and retutn log-likelihood.*/

        private double nextIteration(double correctionConstant)
        {
            // compute contribution of p(a|b_i) for each feature and the new
            // correction parameter
            double loglikelihood = 0.0;
            int numEvents = 0;
            int numCorrect = 0;

            int numberOfThreads = modelExpects.Length;

//TODO JAVA specific code 
//		ExecutorService executor = Executors.newFixedThreadPool(numberOfThreads);

            int taskSize = numUniqueEvents/numberOfThreads;

            int leftOver = numUniqueEvents%numberOfThreads;

//TODO TASK: Java wildcard generics are not converted to .NET:
            /*
        IList<Future<?>> futures = new List<Future<?>>();

		for (int i = 0; i < numberOfThreads; i++)
		{
		  if (i != numberOfThreads - 1)
		  {
			futures.Add(executor.submit(new ModelExpactationComputeTask(this, i, i * taskSize, taskSize)));
		  }
		  else
		  {
			futures.Add(executor.submit(new ModelExpactationComputeTask(this, i, i * taskSize, taskSize + leftOver)));
		  }
		}

// TODO TASK: Java wildcard generics are not converted to .NET:
		foreach (Future<?> future in futures)
		{
		  ModelExpactationComputeTask finishedTask = null;
		  try
		  {
			finishedTask = (ModelExpactationComputeTask) future.get();
		  }
		  catch (InterruptedException e)
		  {
			// TODO: We got interrupted, but that is currently not really supported!
			// For now we just print the exception and fail hard. We hopefully soon
			// handle this case properly!
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			throw new IllegalStateException("Interruption is not supported!", e);
		  }
		  catch (ExecutionException e)
		  {
			// Only runtime exception can be thrown during training, if one was thrown
			// it should be re-thrown. That could for example be a NullPointerException
			// which is caused through a bug in our implementation.
			throw new Exception("Exception during training: " + e.Message, e);
		  }

		  // When they are done, retrieve the results ...
		  numEvents += finishedTask.NumEvents;
		  numCorrect += finishedTask.NumCorrect;
		  loglikelihood += finishedTask.Loglikelihood;
		}

		executor.shutdown();

		display(".");

		// merge the results of the two computations
		for (int pi = 0; pi < numPreds; pi++)
		{
		  int[] activeOutcomes = @params[pi].Outcomes;

		  for (int aoi = 0;aoi < activeOutcomes.Length;aoi++)
		  {
			for (int i = 1; i < modelExpects.Length; i++)
			{
			  modelExpects[0][pi].updateParameter(aoi, modelExpects[i][pi].Parameters[aoi]);
			}
		  }
		}

		display(".");

		// compute the new parameter values
		for (int pi = 0; pi < numPreds; pi++)
		{
		  double[] observed = observedExpects[pi].Parameters;
		  double[] model = modelExpects[0][pi].Parameters;
		  int[] activeOutcomes = @params[pi].Outcomes;
		  for (int aoi = 0;aoi < activeOutcomes.Length;aoi++)
		  {
			if (useGaussianSmoothing)
			{
			  @params[pi].updateParameter(aoi,gaussianUpdate(pi,aoi,numEvents,correctionConstant));
			}
			else
			{
			  if (model[aoi] == 0)
			  {
				Console.Error.WriteLine("Model expects == 0 for " + predLabels[pi] + " " + outcomeLabels[aoi]);
			  }
			  //params[pi].updateParameter(aoi,(Math.log(observed[aoi]) - Math.log(model[aoi])));
			  @params[pi].updateParameter(aoi,((Math.Log(observed[aoi]) - Math.Log(model[aoi])) / correctionConstant));
			}

			foreach (MutableContext[] modelExpect in modelExpects)
			{
			  modelExpect[pi].setParameter(aoi, 0.0); // re-initialize to 0.0's
			}

		  }
		}

		display(". loglikelihood=" + loglikelihood + "\t" + ((double) numCorrect / numEvents) + "\n");
*/
            return loglikelihood;
        }

        private void display(string s)
        {
            if (printMessages)
            {
                Console.Write(s);
            }
        }
    }
}