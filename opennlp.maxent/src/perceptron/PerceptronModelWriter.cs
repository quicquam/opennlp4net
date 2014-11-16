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
using opennlp.nonjava.helperclasses;

namespace opennlp.perceptron
{
    using AbstractModel = opennlp.model.AbstractModel;
    using AbstractModelWriter = opennlp.model.AbstractModelWriter;
    using ComparablePredicate = opennlp.model.ComparablePredicate;
    using Context = opennlp.model.Context;
    using opennlp.model;

    /// <summary>
    /// Abstract parent class for Perceptron writers.  It provides the persist method
    /// which takes care of the structure of a stored document, and requires an
    /// extending class to define precisely how the data should be stored.
    /// 
    /// </summary>
    public abstract class PerceptronModelWriter : AbstractModelWriter
    {
        protected internal Context[] PARAMS;
        protected internal string[] OUTCOME_LABELS;
        protected internal string[] PRED_LABELS;
        internal int numOutcomes;

        public PerceptronModelWriter(AbstractModel model)
        {
            object[] data = model.DataStructures;
            this.numOutcomes = model.NumOutcomes;
            PARAMS = (Context[]) data[0];
            IndexHashTable<string> pmap = (IndexHashTable<string>) data[1];
            OUTCOME_LABELS = (string[]) data[2];

            PRED_LABELS = new string[pmap.size()];
            pmap.toArray(PRED_LABELS);
        }

        protected internal virtual ComparablePredicate[] sortValues()
        {
            ComparablePredicate[] sortPreds;
            ComparablePredicate[] tmpPreds = new ComparablePredicate[PARAMS.Length];
            int[] tmpOutcomes = new int[numOutcomes];
            double[] tmpParams = new double[numOutcomes];
            int numPreds = 0;
            //remove parameters with 0 weight and predicates with no parameters 
            for (int pid = 0; pid < PARAMS.Length; pid++)
            {
                int numParams = 0;
                double[] predParams = PARAMS[pid].Parameters;
                int[] outcomePattern = PARAMS[pid].Outcomes;
                for (int pi = 0; pi < predParams.Length; pi++)
                {
                    if (predParams[pi] != 0d)
                    {
                        tmpOutcomes[numParams] = outcomePattern[pi];
                        tmpParams[numParams] = predParams[pi];
                        numParams++;
                    }
                }

                int[] activeOutcomes = new int[numParams];
                double[] activeParams = new double[numParams];

                for (int pi = 0; pi < numParams; pi++)
                {
                    activeOutcomes[pi] = tmpOutcomes[pi];
                    activeParams[pi] = tmpParams[pi];
                }
                if (numParams != 0)
                {
                    tmpPreds[numPreds] = new ComparablePredicate(PRED_LABELS[pid], activeOutcomes, activeParams);
                    numPreds++;
                }
            }
            Console.Error.WriteLine("Compressed " + PARAMS.Length + " parameters to " + numPreds);
            sortPreds = new ComparablePredicate[numPreds];
            Array.Copy(tmpPreds, 0, sortPreds, 0, numPreds);
            Array.Sort(sortPreds);
            return sortPreds;
        }


        protected internal virtual IList<IList<ComparablePredicate>> computeOutcomePatterns(ComparablePredicate[] sorted)
        {
            ComparablePredicate cp = sorted[0];
            IList<IList<ComparablePredicate>> outcomePatterns = new List<IList<ComparablePredicate>>();
            IList<ComparablePredicate> newGroup = new List<ComparablePredicate>();
            foreach (ComparablePredicate predicate in sorted)
            {
                if (cp.CompareTo(predicate) == 0)
                {
                    newGroup.Add(predicate);
                }
                else
                {
                    cp = predicate;
                    outcomePatterns.Add(newGroup);
                    newGroup = new List<ComparablePredicate>();
                    newGroup.Add(predicate);
                }
            }
            outcomePatterns.Add(newGroup);
            Console.Error.WriteLine(outcomePatterns.Count + " outcome patterns");
            return outcomePatterns;
        }

        /// <summary>
        /// Writes the model to disk, using the <code>writeX()</code> methods
        /// provided by extending classes.
        /// 
        /// <para>If you wish to create a PerceptronModelWriter which uses a different
        /// structure, it will be necessary to override the persist method in
        /// addition to implementing the <code>writeX()</code> methods.
        /// </para>
        /// </summary>
        public override void persist()
        {
            // the type of model (Perceptron)
            writeUTF("Perceptron");

            // the mapping from outcomes to their integer indexes
            writeInt(OUTCOME_LABELS.Length);

            foreach (string label in OUTCOME_LABELS)
            {
                writeUTF(label);
            }

            // the mapping from predicates to the outcomes they contributed to.
            // The sorting is done so that we actually can write this out more
            // compactly than as the entire list.
            ComparablePredicate[] sorted = sortValues();
            IList<IList<ComparablePredicate>> compressed = computeOutcomePatterns(sorted);

            writeInt(compressed.Count);

            foreach (IList<ComparablePredicate> a in compressed)
            {
                writeUTF(a.Count + a[0].ToString());
            }

            // the mapping from predicate names to their integer indexes
            writeInt(sorted.Length);

            foreach (ComparablePredicate s in sorted)
            {
                writeUTF(s.name);
            }

            // write out the parameters
            for (int i = 0; i < sorted.Length; i++)
            {
                for (int j = 0; j < sorted[i].@params.Length; j++)
                {
                    writeDouble(sorted[i].@params[j]);
                }
            }

            close();
        }
    }
}