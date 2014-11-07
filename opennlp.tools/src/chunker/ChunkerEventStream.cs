using System;
/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.IO;
using j4n.Serialization;

namespace opennlp.tools.chunker
{

	using Event = opennlp.model.Event;
	using opennlp.tools.util;

	/// <summary>
	/// Class for creating an event stream out of data files for training a chunker.
	/// </summary>
	public class ChunkerEventStream : opennlp.model.AbstractEventStream
	{

	  private ChunkerContextGenerator cg;
	  private ObjectStream<ChunkSample> data;
	  private Event[] events;
	  private int ei;


	  /// <summary>
	  /// Creates a new event stream based on the specified data stream using the specified context generator. </summary>
	  /// <param name="d"> The data stream for this event stream. </param>
	  /// <param name="cg"> The context generator which should be used in the creation of events for this event stream. </param>
	  public ChunkerEventStream(ObjectStream<ChunkSample> d, ChunkerContextGenerator cg)
	  {
		this.cg = cg;
		data = d;
		ei = 0;
		addNewEvents();
	  }

	  /// <summary>
	  /// Creates a new event stream based on the specified data stream. </summary>
	  /// <param name="d"> The data stream for this event stream.
	  /// </param>
	  /// @deprecated Use <seealso cref="#ChunkerEventStream(ObjectStream, ChunkerContextGenerator)"/> instead. 
	  public ChunkerEventStream(ObjectStream<ChunkSample> d) : this(d, new DefaultChunkerContextGenerator())
	  {
	  }

	  public override Event next()
	  {

		hasNext();

		return events[ei++];
	  }

	  public override bool hasNext()
	  {
		if (ei == events.Length)
		{
		  addNewEvents();
		  ei = 0;
		}
		return ei < events.Length;
	  }

	  private void addNewEvents()
	  {

		ChunkSample sample;
		try
		{
		  sample = data.read();
		}
		catch (IOException e)
		{
		  throw new Exception(e.Message);
		}

		if (sample != null)
		{
		  events = new Event[sample.Sentence.Length];
		  string[] toksArray = sample.Sentence;
		  string[] tagsArray = sample.Tags;
		  string[] predsArray = sample.Preds;
		  for (int ei = 0, el = events.Length; ei < el; ei++)
		  {
			events[ei] = new Event(predsArray[ei], cg.getContext(ei,toksArray,tagsArray,predsArray));
		  }
		}
		else
		{
		  events = new Event[0];
		}
	  }
	}

}