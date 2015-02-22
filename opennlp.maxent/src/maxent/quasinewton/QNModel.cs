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

namespace opennlp.maxent.quasinewton
{
    using AbstractModel = opennlp.model.AbstractModel;
    using Context = opennlp.model.Context;
    using EvalParameters = opennlp.model.EvalParameters;
    using UniformPrior = opennlp.model.UniformPrior;


    public class QNModel : AbstractModel
    {
        private const double SMOOTHING_VALUE = 0.1;
        private double[] parameters;
        // FROM trainer
        public QNModel(LogLikelihoodFunction monitor, double[] parameters)
            : base(null, monitor.PredLabels, monitor.OutcomeLabels)
        {
            int[][] outcomePatterns = monitor.OutcomePatterns;
            Context[] cParameters = new Context[monitor.PredLabels.Length];
            for (int ci = 0; ci < parameters.Length; ci++)
            {
                int[] outcomePattern = outcomePatterns[ci];
                double[] alpha = new double[outcomePattern.Length];
                for (int oi = 0; oi < outcomePattern.Length; oi++)
                {
                    alpha[oi] = parameters[ci + (outcomePattern[oi]*monitor.PredLabels.Length)];
                }
                cParameters[ci] = new Context(outcomePattern, alpha);
            }
            this.evalParams = new EvalParameters(cParameters, monitor.OutcomeLabels.Length);
            this.prior = new UniformPrior();
            this.modelType = ModelTypeEnum.MaxentQn;

            this.parameters = parameters;
        }

        // FROM model reader
        public QNModel(string[] predNames, string[] outcomeNames, Context[] cParameters, double[] parameters)
            : base(cParameters, predNames, outcomeNames)
        {
            this.prior = new UniformPrior();
            this.modelType = ModelTypeEnum.MaxentQn;

            this.parameters = parameters;
        }

        public override double[] eval(string[] context)
        {
            return eval(context, new double[evalParams.NumOutcomes]);
        }

        private int getPredIndex(string predicate)
        {
            return pmap.get(predicate);
        }

        public override double[] eval(string[] context, double[] probs)
        {
            return eval(context, null, probs);
        }

        public override double[] eval(string[] context, float[] values)
        {
            return eval(context, values, new double[evalParams.NumOutcomes]);
        }

        // TODO need implments for handlling with "probs".
        private double[] eval(string[] context, float[] values, double[] probs)
        {
            double[] result = new double[outcomeNames.Length];
            double[] table = new double[outcomeNames.Length + 1];
            for (int pi = 0; pi < context.Length; pi++)
            {
                int predIdx = getPredIndex(context[pi]);

                for (int oi = 0; oi < outcomeNames.Length; oi++)
                {
                    int paraIdx = oi*pmap.size() + predIdx;

                    double predValue = 1.0;
                    if (values != null)
                    {
                        predValue = values[pi];
                    }
                    if (paraIdx < 0)
                    {
                        table[oi] += predValue*SMOOTHING_VALUE;
                    }
                    else
                    {
                        table[oi] += predValue*parameters[paraIdx];
                    }
                }
            }

            for (int oi = 0; oi < outcomeNames.Length; oi++)
            {
                table[oi] = Math.Exp(table[oi]);
                table[outcomeNames.Length] += table[oi];
            }
            for (int oi = 0; oi < outcomeNames.Length; oi++)
            {
                result[oi] = table[oi]/table[outcomeNames.Length];
            }
            return result;
            //    double[] table = new double[outcomeNames.length];
            //    Arrays.fill(table, 1.0 / outcomeNames.length);
            //    return table;
        }

        public override int NumOutcomes
        {
            get { return this.outcomeNames.Length; }
        }

        public virtual double[] Parameters
        {
            get { return this.parameters; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is QNModel))
            {
                return false;
            }

            QNModel objModel = (QNModel) obj;
            if (this.outcomeNames.Length != objModel.outcomeNames.Length)
            {
                return false;
            }
            for (int i = 0; i < this.outcomeNames.Length; i++)
            {
                if (!this.outcomeNames[i].Equals(objModel.outcomeNames[i]))
                {
                    return false;
                }
            }

            if (this.pmap.size() != objModel.pmap.size())
            {
                return false;
            }
            string[] pmapArray = new string[pmap.size()];
            pmap.toArray(pmapArray);
            for (int i = 0; i < this.pmap.size(); i++)
            {
                if (i != objModel.pmap.get(pmapArray[i]))
                {
                    return false;
                }
            }

            // compare evalParameters
            Context[] contextComparing = objModel.evalParams.Params;
            if (this.evalParams.Params.Length != contextComparing.Length)
            {
                return false;
            }
            for (int i = 0; i < this.evalParams.Params.Length; i++)
            {
                if (this.evalParams.Params[i].Outcomes.Length != contextComparing[i].Outcomes.Length)
                {
                    return false;
                }
                for (int j = 0; i < this.evalParams.Params[i].Outcomes.Length; i++)
                {
                    if (this.evalParams.Params[i].Outcomes[j] != contextComparing[i].Outcomes[j])
                    {
                        return false;
                    }
                }

                if (this.evalParams.Params[i].Parameters.Length != contextComparing[i].Parameters.Length)
                {
                    return false;
                }
                for (int j = 0; i < this.evalParams.Params[i].Parameters.Length; i++)
                {
                    if (this.evalParams.Params[i].Parameters[j] != contextComparing[i].Parameters[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}