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

namespace opennlp.model
{
    /// <summary>
    /// Abstract class for collecting event and context counts used in training. 
    /// 
    /// </summary>
    public abstract class AbstractDataIndexer : DataIndexer
    {
        private int numEvents;

        /// <summary>
        /// The integer contexts associated with each unique event. </summary>
        protected internal int[][] contexts;

        /// <summary>
        /// The integer outcome associated with each unique event. </summary>
        protected internal int[] outcomeList;

        /// <summary>
        /// The number of times an event occured in the training data. </summary>
        protected internal int[] numTimesEventsSeen;

        /// <summary>
        /// The predicate/context names. </summary>
        protected internal string[] predLabels;

        /// <summary>
        /// The names of the outcomes. </summary>
        protected internal string[] outcomeLabels;

        /// <summary>
        /// The number of times each predicate occured. </summary>
        protected internal int[] predCounts;

        public virtual int[][] Contexts
        {
            get { return contexts; }
        }

        public virtual int[] NumTimesEventsSeen
        {
            get { return numTimesEventsSeen; }
        }

        public virtual int[] OutcomeList
        {
            get { return outcomeList; }
        }

        public virtual string[] PredLabels
        {
            get { return predLabels; }
        }

        public virtual string[] OutcomeLabels
        {
            get { return outcomeLabels; }
        }


        public virtual int[] PredCounts
        {
            get { return predCounts; }
        }

        /// <summary>
        /// Sorts and uniques the array of comparable events and return the number of unique events.
        /// This method will alter the eventsToCompare array -- it does an in place
        /// sort, followed by an in place edit to remove duplicates.
        /// </summary>
        /// <param name="eventsToCompare"> a <code>ComparableEvent[]</code> value </param>
        /// <returns> The number of unique events in the specified list.
        /// @since maxent 1.2.6 </returns>
        protected internal virtual int sortAndMerge(IList<ComparableEvent> eventsToCompare, bool sort)
        {
            int numUniqueEvents = 1;
            numEvents = eventsToCompare.Count;
            if (sort)
            {
                eventsToCompare.Sort();
                if (numEvents <= 1)
                {
                    return numUniqueEvents; // nothing to do; edge case (see assertion)
                }

                ComparableEvent ce = eventsToCompare[0];
                for (int i = 1; i < numEvents; i++)
                {
                    ComparableEvent ce2 = eventsToCompare[i];

                    if (ce.CompareTo(ce2) == 0)
                    {
                        ce.seen++; // increment the seen count
                        eventsToCompare[i] = null; // kill the duplicate
                    }
                    else
                    {
                        ce = ce2; // a new champion emerges...
                        numUniqueEvents++; // increment the # of unique events
                    }
                }
            }
            else
            {
                numUniqueEvents = eventsToCompare.Count;
            }
            if (sort)
            {
                Console.WriteLine("done. Reduced " + numEvents + " events to " + numUniqueEvents + ".");
            }

            contexts = new int[numUniqueEvents][];
            outcomeList = new int[numUniqueEvents];
            numTimesEventsSeen = new int[numUniqueEvents];

            for (int i = 0, j = 0; i < numEvents; i++)
            {
                ComparableEvent evt = eventsToCompare[i];
                if (null == evt)
                {
                    continue; // this was a dupe, skip over it.
                }
                numTimesEventsSeen[j] = evt.seen;
                outcomeList[j] = evt.outcome;
                contexts[j] = evt.predIndexes;
                ++j;
            }
            return numUniqueEvents;
        }


        public virtual int NumEvents
        {
            get { return numEvents; }
        }

        /// <summary>
        /// Updates the set of predicated and counter with the specified event contexts and cutoff. </summary>
        /// <param name="ec"> The contexts/features which occur in a event. </param>
        /// <param name="predicateSet"> The set of predicates which will be used for model building. </param>
        /// <param name="counter"> The predicate counters. </param>
        /// <param name="cutoff"> The cutoff which determines whether a predicate is included. </param>
        protected internal static void update(string[] ec, HashSet<string> predicateSet,
            IDictionary<string, int?> counter, int cutoff)
        {
            foreach (string s in ec)
            {
                int? i = counter[s];
                if (i == null)
                {
                    counter[s] = 1;
                }
                else
                {
                    counter[s] = i + 1;
                }
                if (!predicateSet.Contains(s) && counter[s] >= cutoff)
                {
                    predicateSet.Add(s);
                }
            }
        }

        /// <summary>
        /// Utility method for creating a String[] array from a map whose
        /// keys are labels (Strings) to be stored in the array and whose
        /// values are the indices (Integers) at which the corresponding
        /// labels should be inserted.
        /// </summary>
        /// <param name="labelToIndexMap"> a <code>TObjectIntHashMap</code> value </param>
        /// <returns> a <code>String[]</code> value
        /// @since maxent 1.2.6 </returns>
        protected internal static string[] toIndexedStringArray(IDictionary<string, int?> labelToIndexMap)
        {
            string[] array = new string[labelToIndexMap.Count];
            foreach (string label in labelToIndexMap.Keys)
            {
                array[labelToIndexMap[label].GetValueOrDefault()] = label;
            }
            return array;
        }

        public virtual float[][] Values
        {
            get { return null; }
        }
    }
}