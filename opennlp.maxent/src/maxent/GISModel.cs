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
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.Reader;
using j4n.Utils;
using opennlp.maxent.io;
using opennlp.nonjava.helperclasses;

namespace opennlp.maxent
{
    using AbstractModel = opennlp.model.AbstractModel;
    using Context = opennlp.model.Context;
    using EvalParameters = opennlp.model.EvalParameters;
    using Prior = opennlp.model.Prior;
    using UniformPrior = opennlp.model.UniformPrior;

    /// <summary>
    /// A maximum entropy model which has been trained using the Generalized
    /// Iterative Scaling procedure (implemented in GIS.java).
    /// </summary>
    public sealed class GISModel : AbstractModel
    {
        /// <summary>
        /// Creates a new model with the specified parameters, outcome names, and
        /// predicate/feature labels.
        /// </summary>
        /// <param name="params">
        ///          The parameters of the model. </param>
        /// <param name="predLabels">
        ///          The names of the predicates used in this model. </param>
        /// <param name="outcomeNames">
        ///          The names of the outcomes this model predicts. </param>
        /// <param name="correctionConstant">
        ///          The maximum number of active features which occur in an event. </param>
        /// <param name="correctionParam">
        ///          The parameter associated with the correction feature. </param>
        public GISModel(Context[] @params, string[] predLabels, string[] outcomeNames, int correctionConstant,
            double correctionParam)
            : this(@params, predLabels, outcomeNames, correctionConstant, correctionParam, new UniformPrior())
        {
        }

        /// <summary>
        /// Creates a new model with the specified parameters, outcome names, and
        /// predicate/feature labels.
        /// </summary>
        /// <param name="params">
        ///          The parameters of the model. </param>
        /// <param name="predLabels">
        ///          The names of the predicates used in this model. </param>
        /// <param name="outcomeNames">
        ///          The names of the outcomes this model predicts. </param>
        /// <param name="correctionConstant">
        ///          The maximum number of active features which occur in an event. </param>
        /// <param name="correctionParam">
        ///          The parameter associated with the correction feature. </param>
        /// <param name="prior">
        ///          The prior to be used with this model. </param>
        public GISModel(Context[] @params, string[] predLabels, string[] outcomeNames, int correctionConstant,
            double correctionParam, Prior prior)
            : base(@params, predLabels, outcomeNames, correctionConstant, correctionParam)
        {
            this.prior = prior;
            prior.setLabels(outcomeNames, predLabels);
            modelType = ModelTypeEnum.Maxent;
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given that context.
        /// </summary>
        /// <param name="context">
        ///          The names of the predicates which have been observed at the
        ///          present decision point. </param>
        /// <returns> The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i). </returns>
        public override double[] eval(string[] context)
        {
            return (eval(context, new double[evalParams.NumOutcomes]));
        }

        public override double[] eval(string[] context, float[] values)
        {
            return (eval(context, values, new double[evalParams.NumOutcomes]));
        }

        public override double[] eval(string[] context, double[] outsums)
        {
            return eval(context, null, outsums);
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given that context.
        /// </summary>
        /// <param name="context">
        ///          The names of the predicates which have been observed at the
        ///          present decision point. </param>
        /// <param name="outsums">
        ///          This is where the distribution is stored. </param>
        /// <returns> The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i). </returns>
        public double[] eval(string[] context, float[] values, double[] outsums)
        {
            int[] scontexts = new int[context.Length];
            for (int i = 0; i < context.Length; i++)
            {
                int? ci = pmap.get(context[i]);
                scontexts[i] = !ci.HasValue ? - 1 : ci.GetValueOrDefault();
            }
            prior.logPrior(outsums, scontexts, values);
            return GISModel.eval(scontexts, values, outsums, evalParams);
        }


        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given the specified context and the specified parameters.
        /// </summary>
        /// <param name="context">
        ///          The integer values of the predicates which have been observed at
        ///          the present decision point. </param>
        /// <param name="prior">
        ///          The prior distribution for the specified context. </param>
        /// <param name="model">
        ///          The set of parametes used in this computation. </param>
        /// <returns> The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i). </returns>
        public static double[] eval(int[] context, double[] prior, EvalParameters model)
        {
            return eval(context, null, prior, model);
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given the specified context and the specified parameters.
        /// </summary>
        /// <param name="context">
        ///          The integer values of the predicates which have been observed at
        ///          the present decision point. </param>
        /// <param name="values">
        ///          The values for each of the parameters. </param>
        /// <param name="prior">
        ///          The prior distribution for the specified context. </param>
        /// <param name="model">
        ///          The set of parametes used in this computation. </param>
        /// <returns> The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i). </returns>
        public static double[] eval(int[] context, float[] values, double[] prior, EvalParameters model)
        {
            Context[] @params = model.Params;
            int[] numfeats = new int[model.NumOutcomes];
            int[] activeOutcomes;
            double[] activeParameters;
            double value = 1;
            for (int ci = 0; ci < context.Length; ci++)
            {
                if (context[ci] >= 0)
                {
                    Context predParams = @params[context[ci]];
                    activeOutcomes = predParams.Outcomes;
                    activeParameters = predParams.Parameters;
                    if (values != null)
                    {
                        value = values[ci];
                    }
                    for (int ai = 0; ai < activeOutcomes.Length; ai++)
                    {
                        int oid = activeOutcomes[ai];
                        numfeats[oid]++;
                        prior[oid] += activeParameters[ai]*value;
                    }
                }
            }

            double normal = 0.0;
            for (int oid = 0; oid < model.NumOutcomes; oid++)
            {
                if (model.CorrectionParam != 0)
                {
                    prior[oid] =
                        Math.Exp(prior[oid]*model.ConstantInverse +
                                 ((1.0 - ((double) numfeats[oid]/model.CorrectionConstant))*model.CorrectionParam));
                }
                else
                {
                    prior[oid] = Math.Exp(prior[oid]*model.ConstantInverse);
                }
                normal += prior[oid];
            }

            for (int oid = 0; oid < model.NumOutcomes; oid++)
            {
                prior[oid] /= normal;
            }
            return prior;
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: GISModel modelname < contexts");
                Environment.Exit(1);
            }
            AbstractModel m = (new SuffixSensitiveGISModelReader(new Jfile(args[0]))).Model;
            BufferedReader @in = new BufferedReader(new InputStreamReader(new InputStream(Console.OpenStandardInput())));
                // TODO get from System.in
            DecimalFormat df = new DecimalFormat(".###");
            for (string line = @in.readLine(); line != null; line = @in.readLine())
            {
                string[] context = line.Split(" ", true);
                double[] dist = m.eval(context);
                for (int oi = 0; oi < dist.Length; oi++)
                {
                    Console.Write("[" + m.getOutcome(oi) + " " + df.format(dist[oi]) + "] ");
                }
                Console.WriteLine();
            }
        }
    }
}