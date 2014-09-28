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
using System.IO;

using j4n.IO.File;
using j4n.IO.OutputStream;
using j4n.IO.Writer;
using opennlp.nonjava.helperclasses;

namespace opennlp.model
{



	/// <summary>
	/// Collecting event and context counts by making two passes over the events.  The
	/// first pass determines which contexts will be used by the model, and the
	/// second pass creates the events in memory containing only the contexts which 
	/// will be used.  This greatly reduces the amount of memory required for storing
	/// the events.  During the first pass a temporary event file is created which
	/// is read during the second pass.
	/// </summary>
	public class TwoPassDataIndexer : AbstractDataIndexer
	{

	  /// <summary>
	  /// One argument constructor for DataIndexer which calls the two argument
	  /// constructor assuming no cutoff.
	  /// </summary>
	  /// <param name="eventStream"> An Event[] which contains the a list of all the Events
	  ///               seen in the training data. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TwoPassDataIndexer(EventStream eventStream) throws java.io.IOException
	  public TwoPassDataIndexer(EventStream eventStream) : this(eventStream, 0)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TwoPassDataIndexer(EventStream eventStream, int cutoff) throws java.io.IOException
	  public TwoPassDataIndexer(EventStream eventStream, int cutoff) : this(eventStream,cutoff,true)
	  {
	  }
	  /// <summary>
	  /// Two argument constructor for DataIndexer.
	  /// </summary>
	  /// <param name="eventStream"> An Event[] which contains the a list of all the Events
	  ///               seen in the training data. </param>
	  /// <param name="cutoff"> The minimum number of times a predicate must have been
	  ///               observed in order to be included in the model. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TwoPassDataIndexer(EventStream eventStream, int cutoff, boolean sort) throws java.io.IOException
	  public TwoPassDataIndexer(EventStream eventStream, int cutoff, bool sort)
	  {
		IDictionary<string, int?> predicateIndex = new Dictionary<string, int?>();
		IList<ComparableEvent> eventsToCompare;

		Console.WriteLine("Indexing events using cutoff of " + cutoff + "\n");

		Console.Write("\tComputing event counts...  ");
		try
		{
		  Jfile tmp = Jfile.createTempFile("events", null);
		  tmp.deleteOnExit();
		  Writer osw = new BufferedWriter(new OutputStreamWriter(new FileOutputStream(tmp),"UTF8"));
		  int numEvents = computeEventCounts(eventStream, osw, predicateIndex, cutoff);
		  Console.WriteLine("done. " + numEvents + " events");

		  Console.Write("\tIndexing...  ");

		  FileEventStream fes = new FileEventStream(tmp);
		  try
		  {
			eventsToCompare = index(numEvents, fes, predicateIndex);
		  }
		  finally
		  {
			fes.close();
		  }
		  // done with predicates
		  predicateIndex = null;
		  tmp.delete();
		  Console.WriteLine("done.");

		  if (sort)
		  {
			Console.Write("Sorting and merging events... ");
		  }
		  else
		  {
			Console.Write("Collecting events... ");
		  }
		  sortAndMerge(eventsToCompare,sort);
		  Console.WriteLine("Done indexing.");
		}
		catch (IOException e)
		{
		  Console.Error.WriteLine(e);
		}
	  }

	  /// <summary>
	  /// Reads events from <tt>eventStream</tt> into a linked list.  The
	  /// predicates associated with each event are counted and any which
	  /// occur at least <tt>cutoff</tt> times are added to the
	  /// <tt>predicatesInOut</tt> map along with a unique integer index.
	  /// </summary>
	  /// <param name="eventStream"> an <code>EventStream</code> value </param>
	  /// <param name="eventStore"> a writer to which the events are written to for later processing. </param>
	  /// <param name="predicatesInOut"> a <code>TObjectIntHashMap</code> value </param>
	  /// <param name="cutoff"> an <code>int</code> value </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int computeEventCounts(EventStream eventStream, java.io.Writer eventStore, java.util.Map<String,Integer> predicatesInOut, int cutoff) throws java.io.IOException
	  private int computeEventCounts(EventStream eventStream, Writer eventStore, IDictionary<string, int?> predicatesInOut, int cutoff)
	  {
		IDictionary<string, int?> counter = new Dictionary<string, int?>();
		int eventCount = 0;
		HashSet<string> predicateSet = new HashSet<string>();
		while (eventStream.hasNext())
		{
		  Event ev = eventStream.next();
		  eventCount++;
		  eventStore.write(FileEventStream.toLine(ev));
		  string[] ec = ev.Context;
		  update(ec,predicateSet,counter,cutoff);
		}
		predCounts = new int[predicateSet.Count];
		int index = 0;
		for (IEnumerator<string> pi = predicateSet.GetEnumerator(); pi.MoveNext(); index++)
		{
		  string predicate = pi.Current;
		  predCounts[index] = counter[predicate].GetValueOrDefault();
		  predicatesInOut[predicate] = index;
		}
		eventStore.close();
		return eventCount;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.List<ComparableEvent> index(int numEvents, EventStream es, java.util.Map<String,Integer> predicateIndex) throws java.io.IOException
	  private IList<ComparableEvent> index(int numEvents, EventStream es, IDictionary<string, int?> predicateIndex)
	  {
		IDictionary<string, int?> omap = new Dictionary<string, int?>();
		int outcomeCount = 0;
		IList<ComparableEvent> eventsToCompare = new List<ComparableEvent>(numEvents);
		IList<int?> indexedContext = new List<int?>();
		while (es.hasNext())
		{
		  Event ev = es.next();
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
			for (int ci = 0;ci < cons.Length;ci++)
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
		return eventsToCompare;
	  }

	}


}