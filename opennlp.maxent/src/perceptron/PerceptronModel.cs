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
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.Reader;
using j4n.Utils;
using opennlp.nonjava.helperclasses;

namespace opennlp.perceptron
{
    using AbstractModel = opennlp.model.AbstractModel;
    using Context = opennlp.model.Context;
    using EvalParameters = opennlp.model.EvalParameters;
    using opennlp.model;

    public class PerceptronModel : AbstractModel
    {
        public PerceptronModel(Context[] @params, string[] predLabels, IndexHashTable<string> pmap,
            string[] outcomeNames) : base(@params, predLabels, pmap, outcomeNames)
        {
            modelType = ModelTypeEnum.Perceptron;
        }

        /// @deprecated use the constructor with the <seealso cref="IndexHashTable"/> instead! 
        [Obsolete("use the constructor with the <seealso cref=\"opennlp.model.IndexHashTable\"/> instead!")]
        public PerceptronModel(Context[] @params, string[] predLabels, IDictionary<string, int?> pmap,
            string[] outcomeNames) : base(@params, predLabels, outcomeNames)
        {
            modelType = ModelTypeEnum.Perceptron;
        }

        public PerceptronModel(Context[] @params, string[] predLabels, string[] outcomeNames)
            : base(@params, predLabels, outcomeNames)
        {
            modelType = ModelTypeEnum.Perceptron;
        }

        public override double[] eval(string[] context)
        {
            return eval(context, new double[evalParams.NumOutcomes]);
        }

        public override double[] eval(string[] context, float[] values)
        {
            return eval(context, values, new double[evalParams.NumOutcomes]);
        }

        public override double[] eval(string[] context, double[] probs)
        {
            return eval(context, null, probs);
        }

        public virtual double[] eval(string[] context, float[] values, double[] outsums)
        {
            int[] scontexts = new int[context.Length];
            Arrays.Fill(outsums, 0);
            for (int i = 0; i < context.Length; i++)
            {
                int? ci = pmap.get(context[i]);
                scontexts[i] = ci.HasValue ? - 1 : ci.GetValueOrDefault();
            }
            return eval(scontexts, values, outsums, evalParams, true);
        }

        public static double[] eval(int[] context, double[] prior, EvalParameters model)
        {
            return eval(context, null, prior, model, true);
        }

        public static double[] eval(int[] context, float[] values, double[] prior, EvalParameters model, bool normalize)
        {
            Context[] @params = model.Params;
            double[] activeParameters;
            int[] activeOutcomes;
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
                        prior[oid] += activeParameters[ai]*value;
                    }
                }
            }
            if (normalize)
            {
                int numOutcomes = model.NumOutcomes;

                double maxPrior = 1;

                for (int oid = 0; oid < numOutcomes; oid++)
                {
                    if (maxPrior < Math.Abs(prior[oid]))
                    {
                        maxPrior = Math.Abs(prior[oid]);
                    }
                }

                double normal = 0.0;
                for (int oid = 0; oid < numOutcomes; oid++)
                {
                    prior[oid] = Math.Exp(prior[oid]/maxPrior);
                    normal += prior[oid];
                }

                for (int oid = 0; oid < numOutcomes; oid++)
                {
                    prior[oid] /= normal;
                }
            }
            return prior;
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: PerceptronModel modelname < contexts");
                Environment.Exit(1);
            }
            AbstractModel m = (new PerceptronModelReader(new Jfile(args[0]))).Model;
            BufferedReader @in = new BufferedReader(new InputStreamReader(new InputStream(Console.OpenStandardInput())));
                // TODO get from stdin
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