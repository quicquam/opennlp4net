using System.Collections.Generic;
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
using j4n.Serialization;


namespace opennlp.tools.postag
{


	using Event = opennlp.model.Event;
	using opennlp.tools.util;
	using opennlp.tools.util;

	/// <summary>
	/// This class reads the <seealso cref="POSSample"/>s from the given <seealso cref="Iterator"/>
	/// and converts the <seealso cref="POSSample"/>s into <seealso cref="Event"/>s which
	/// can be used by the maxent library for training.
	/// </summary>
	public class POSSampleEventStream : AbstractEventStream<POSSample>
	{

	  /// <summary>
	  /// The <seealso cref="POSContextGenerator"/> used
	  /// to create the training <seealso cref="Event"/>s.
	  /// </summary>
	  private POSContextGenerator cg;

	  /// <summary>
	  /// Initializes the current instance with the given samples and the
	  /// given <seealso cref="POSContextGenerator"/>.
	  /// </summary>
	  /// <param name="samples"> </param>
	  /// <param name="cg"> </param>
	  public POSSampleEventStream(ObjectStream<POSSample> samples, POSContextGenerator cg) : base(samples)
	  {

		this.cg = cg;
	  }

	  /// <summary>
	  /// Initializes the current instance with given samples
	  /// and a <seealso cref="DefaultPOSContextGenerator"/>. </summary>
	  /// <param name="samples"> </param>
	  public POSSampleEventStream(ObjectStream<POSSample> samples) : this(samples, new DefaultPOSContextGenerator(null))
	  {
	  }

	  protected internal override IEnumerator<Event> createEvents(POSSample sample)
	  {
		string[] sentence = sample.Sentence;
		string[] tags = sample.Tags;
		object[] ac = sample.AddictionalContext;
		IList<Event> events = generateEvents(sentence, tags, ac, cg);
		return events.GetEnumerator();
	  }

	  public static IList<Event> generateEvents(string[] sentence, string[] tags, object[] additionalContext, POSContextGenerator cg)
	  {
		IList<Event> events = new List<Event>(sentence.Length);

		for (int i = 0; i < sentence.Length; i++)
		{

		  // it is safe to pass the tags as previous tags because
		  // the context generator does not look for non predicted tags
		  string[] context = cg.getContext(i, sentence, tags, additionalContext);

		  events.Add(new Event(tags[i], context));
		}
		return events;
	  }

	  public static IList<Event> generateEvents(string[] sentence, string[] tags, POSContextGenerator cg)
	  {
		return generateEvents(sentence, tags, null, cg);
	  }
	}

}