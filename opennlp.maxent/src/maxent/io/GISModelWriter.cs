using System;
using System.Collections;
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

namespace opennlp.maxent.io
{
    using AbstractModel = opennlp.model.AbstractModel;
    using AbstractModelWriter = opennlp.model.AbstractModelWriter;
    using ComparablePredicate = opennlp.model.ComparablePredicate;
    using Context = opennlp.model.Context;
    using opennlp.model;

    /// <summary>
    /// Abstract parent class for GISModel writers.  It provides the persist method
    /// which takes care of the structure of a stored document, and requires an
    /// extending class to define precisely how the data should be stored.
    /// </summary>
    public abstract class GISModelWriter : AbstractModelWriter
    {
        protected internal Context[] PARAMS;
        protected internal string[] OUTCOME_LABELS;
        protected internal int CORRECTION_CONSTANT;
        protected internal double CORRECTION_PARAM;
        protected internal string[] PRED_LABELS;

        public GISModelWriter(AbstractModel model)
        {
            object[] data = model.DataStructures;

            PARAMS = (Context[]) data[0];
            IndexHashTable<string> pmap = (IndexHashTable<string>) data[1];
            OUTCOME_LABELS = (string[]) data[2];
            CORRECTION_CONSTANT = (int) data[3];
            CORRECTION_PARAM = (double) data[4];

            PRED_LABELS = new string[pmap.size()];
            pmap.toArray(PRED_LABELS);
        }


        /// <summary>
        /// Writes the model to disk, using the <code>writeX()</code> methods provided
        /// by extending classes.
        /// 
        /// <para>
        /// If you wish to create a GISModelWriter which uses a different structure, it
        /// will be necessary to override the persist method in addition to
        /// implementing the <code>writeX()</code> methods.
        /// </para>
        /// </summary>

        public override void persist()
        {
            // the type of model (GIS)
            writeUTF("GIS");

            // the value of the correction constant
            writeInt(CORRECTION_CONSTANT);

            // the value of the correction constant
            writeDouble(CORRECTION_PARAM);

            // the mapping from outcomes to their integer indexes
            writeInt(OUTCOME_LABELS.Length);

            for (int i = 0; i < OUTCOME_LABELS.Length; i++)
            {
                writeUTF(OUTCOME_LABELS[i]);
            }

            // the mapping from predicates to the outcomes they contributed to.
            // The sorting is done so that we actually can write this out more
            // compactly than as the entire list.
            ComparablePredicate[] sorted = sortValues();
            IList<IList<ComparablePredicate>> compressed = compressOutcomes(sorted);

            writeInt(compressed.Count);

            for (int i = 0; i < compressed.Count; i++)
            {
                IList a = compressed[i] as IList;
                writeUTF(a.Count + a[0].ToString());
            }

            // the mapping from predicate names to their integer indexes
            writeInt(PARAMS.Length);

            for (int i = 0; i < sorted.Length; i++)
            {
                writeUTF(sorted[i].name);
            }

            // write out the parameters
            for (int i = 0; i < sorted.Length; i++)
            {
                for (int j = 0; j < sorted[i].parameters.Length; j++)
                {
                    writeDouble(sorted[i].parameters[j]);
                }
            }

            close();
        }

        protected internal virtual ComparablePredicate[] sortValues()
        {
            ComparablePredicate[] sortPreds = new ComparablePredicate[PARAMS.Length];

            int numParams = 0;
            for (int pid = 0; pid < PARAMS.Length; pid++)
            {
                int[] predkeys = PARAMS[pid].Outcomes;
                // Array.Sort(predkeys);
                int numActive = predkeys.Length;
                int[] activeOutcomes = predkeys;
                double[] activeParams = PARAMS[pid].Parameters;

                numParams += numActive;
                /*
		   * double[] activeParams = new double[numActive];
		   * 
		   * int id = 0; for (int i=0; i < predkeys.length; i++) { int oid =
		   * predkeys[i]; activeOutcomes[id] = oid; activeParams[id] =
		   * PARAMS[pid].getParams(oid); id++; }
		   */
                sortPreds[pid] = new ComparablePredicate(PRED_LABELS[pid], activeOutcomes, activeParams);
            }

            Array.Sort(sortPreds);
            return sortPreds;
        }

        protected internal virtual IList<IList<ComparablePredicate>> compressOutcomes(ComparablePredicate[] sorted)
        {
            ComparablePredicate cp = sorted[0];
            IList<IList<ComparablePredicate>> outcomePatterns = new List<IList<ComparablePredicate>>();
            IList<ComparablePredicate> newGroup = new List<ComparablePredicate>();
            for (int i = 0; i < sorted.Length; i++)
            {
                if (cp.CompareTo(sorted[i]) == 0)
                {
                    newGroup.Add(sorted[i]);
                }
                else
                {
                    cp = sorted[i];
                    outcomePatterns.Add(newGroup);
                    newGroup = new List<ComparablePredicate>();
                    newGroup.Add(sorted[i]);
                }
            }
            outcomePatterns.Add(newGroup);
            return outcomePatterns;
        }
    }
}