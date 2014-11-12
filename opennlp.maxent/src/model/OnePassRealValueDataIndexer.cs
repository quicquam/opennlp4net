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

namespace opennlp.model
{
    /// <summary>
    /// An indexer for maxent model data which handles cutoffs for uncommon
    /// contextual predicates and provides a unique integer index for each of the
    /// predicates and maintains event values.  
    /// </summary>
    public class OnePassRealValueDataIndexer : OnePassDataIndexer
    {
        internal float[][] values;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public OnePassRealValueDataIndexer(EventStream eventStream, int cutoff, boolean sort) throws java.io.IOException
        public OnePassRealValueDataIndexer(EventStream eventStream, int cutoff, bool sort)
            : base(eventStream, cutoff, sort)
        {
        }

        /// <summary>
        /// Two argument constructor for DataIndexer. </summary>
        /// <param name="eventStream"> An Event[] which contains the a list of all the Events
        ///               seen in the training data. </param>
        /// <param name="cutoff"> The minimum number of times a predicate must have been
        ///               observed in order to be included in the model. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public OnePassRealValueDataIndexer(EventStream eventStream, int cutoff) throws java.io.IOException
        public OnePassRealValueDataIndexer(EventStream eventStream, int cutoff) : base(eventStream, cutoff)
        {
        }

        public override float[][] Values
        {
            get { return values; }
        }

        protected internal override int sortAndMerge(IList<ComparableEvent> eventsToCompare, bool sort)
        {
            int numUniqueEvents = base.sortAndMerge(eventsToCompare, sort);
            values = new float[numUniqueEvents][];
            int numEvents = eventsToCompare.Count;
            for (int i = 0, j = 0; i < numEvents; i++)
            {
                ComparableEvent evt = (ComparableEvent) eventsToCompare[i];
                if (null == evt)
                {
                    continue; // this was a dupe, skip over it.
                }
                values[j++] = evt.values;
            }
            return numUniqueEvents;
        }

        protected internal override IList index(LinkedList<Event> events, IDictionary<string, int?> predicateIndex)
        {
            IDictionary<string, int?> omap = new Dictionary<string, int?>();

            int numEvents = events.Count;
            int outcomeCount = 0;
            IList<ComparableEvent> eventsToCompare = new List<ComparableEvent>(numEvents);
            IList<int?> indexedContext = new List<int?>();

            for (int eventIndex = 0; eventIndex < numEvents; eventIndex++)
            {
                var evNode = events.First;
                Event ev = evNode.Value;
                events.RemoveFirst();
                string[] econtext = ev.Context;
                ComparableEvent ce;

                int ocID;
                string oc = ev.Outcome;

                if (omap.ContainsKey(oc))
                {
                    ocID = omap[oc].GetValueOrDefault();
                }
                else
                {
                    ocID = outcomeCount++;
                    omap[oc] = ocID;
                }

                foreach (string pred in econtext)
                {
                    if (predicateIndex.ContainsKey(pred))
                    {
                        indexedContext.Add(predicateIndex[pred]);
                    }
                }

                //drop events with no active features
                if (indexedContext.Count > 0)
                {
                    int[] cons = new int[indexedContext.Count];
                    for (int ci = 0; ci < cons.Length; ci++)
                    {
                        cons[ci] = indexedContext[ci].GetValueOrDefault();
                    }
                    ce = new ComparableEvent(ocID, cons, ev.Values);
                    eventsToCompare.Add(ce);
                }
                else
                {
                    Console.Error.WriteLine("Dropped event " + ev.Outcome + ":" + Arrays.asList(ev.Context));
                }
                //    recycle the TIntArrayList
                indexedContext.Clear();
            }
            outcomeLabels = toIndexedStringArray(omap);
            predLabels = toIndexedStringArray(predicateIndex);
            return eventsToCompare as IList;
        }
    }
}