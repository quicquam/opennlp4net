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
    /// predicates.
    /// </summary>
    public class OnePassDataIndexer : AbstractDataIndexer
    {
        /// <summary>
        /// One argument constructor for DataIndexer which calls the two argument
        /// constructor assuming no cutoff.
        /// </summary>
        /// <param name="eventStream">
        ///          An Event[] which contains the a list of all the Events seen in the
        ///          training data. </param>
        public OnePassDataIndexer(EventStream eventStream) : this(eventStream, 0)
        {
        }

        public OnePassDataIndexer(EventStream eventStream, int cutoff) : this(eventStream, cutoff, true)
        {
        }

        /// <summary>
        /// Two argument constructor for DataIndexer.
        /// </summary>
        /// <param name="eventStream">
        ///          An Event[] which contains the a list of all the Events seen in the
        ///          training data. </param>
        /// <param name="cutoff">
        ///          The minimum number of times a predicate must have been observed in
        ///          order to be included in the model. </param>
        public OnePassDataIndexer(EventStream eventStream, int cutoff, bool sort)
        {
            IDictionary<string, int?> predicateIndex = new Dictionary<string, int?>();
            LinkedList<Event> events;
            IList<ComparableEvent> eventsToCompare;

            Console.WriteLine("Indexing events using cutoff of " + cutoff + "\n");

            Console.Write("\tComputing event counts...  ");
            events = computeEventCounts(eventStream, predicateIndex, cutoff);
            Console.WriteLine("done. " + events.Count + " events");

            Console.Write("\tIndexing...  ");
            eventsToCompare = index(events, predicateIndex) as IList<ComparableEvent>;
            // done with event list
            events = null;
            // done with predicates
            predicateIndex = null;

            Console.WriteLine("done.");

            Console.Write("Sorting and merging events... ");
            sortAndMerge(eventsToCompare, sort);
            Console.WriteLine("Done indexing.");
        }

        /// <summary>
        /// Reads events from <tt>eventStream</tt> into a linked list. The predicates
        /// associated with each event are counted and any which occur at least
        /// <tt>cutoff</tt> times are added to the <tt>predicatesInOut</tt> map along
        /// with a unique integer index.
        /// </summary>
        /// <param name="eventStream">
        ///          an <code>EventStream</code> value </param>
        /// <param name="predicatesInOut">
        ///          a <code>TObjectIntHashMap</code> value </param>
        /// <param name="cutoff">
        ///          an <code>int</code> value </param>
        /// <returns> a <code>TLinkedList</code> value </returns>
        private LinkedList<Event> computeEventCounts(EventStream eventStream, IDictionary<string, int?> predicatesInOut,
            int cutoff)
        {
            HashSet<string> predicateSet = new HashSet<string>();
            IDictionary<string, int?> counter = new Dictionary<string, int?>();
            LinkedList<Event> events = new LinkedList<Event>();
            while (eventStream.hasNext())
            {
                Event ev = eventStream.next();
                events.AddLast(ev);
                update(ev.Context, predicateSet, counter, cutoff);
            }
            predCounts = new int[predicateSet.Count];
            int index = 0;
            for (IEnumerator<string> pi = predicateSet.GetEnumerator(); pi.MoveNext(); index++)
            {
                string predicate = pi.Current;
                predCounts[index] = (int) counter[predicate];
                predicatesInOut[predicate] = index;
            }
            return events;
        }

        protected internal virtual IList index(LinkedList<Event> events, IDictionary<string, int?> predicateIndex)
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

                // drop events with no active features
                if (indexedContext.Count > 0)
                {
                    int[] cons = new int[indexedContext.Count];
                    for (int ci = 0; ci < cons.Length; ci++)
                    {
                        cons[ci] = indexedContext[ci].GetValueOrDefault();
                    }
                    ce = new ComparableEvent(ocID, cons);
                    eventsToCompare.Add(ce);
                }
                else
                {
                    Console.Error.WriteLine("Dropped event " + ev.Outcome + ":" + Arrays.asList(ev.Context));
                }
                // recycle the TIntArrayList
                indexedContext.Clear();
            }
            outcomeLabels = toIndexedStringArray(omap);
            predLabels = toIndexedStringArray(predicateIndex);
            return eventsToCompare as IList;
        }
    }
}