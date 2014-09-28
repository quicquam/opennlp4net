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
using j4n.Utils;

namespace opennlp.tools.sentdetect
{


	using Event = opennlp.model.Event;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;

	public class SDEventStream : AbstractEventStream<SentenceSample>
	{

	  private SDContextGenerator cg;
	  private EndOfSentenceScanner scanner;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="samples"> </param>
	  public SDEventStream(ObjectStream<SentenceSample> samples, SDContextGenerator cg, EndOfSentenceScanner scanner) : base(samples)
	  {

		this.cg = cg;
		this.scanner = scanner;
	  }

	  protected internal override IEnumerator<Event> createEvents(SentenceSample sample)
	  {

		ICollection<Event> events = new List<Event>();

		foreach (Span sentenceSpan in sample.Sentences)
		{
		  string sentenceString = sentenceSpan.getCoveredText(sample.Document).ToString();

		  for (IEnumerator<int?> it = scanner.getPositions(sentenceString).GetEnumerator(); it.MoveNext();)
		  {

			int candidate = it.Current;
			string type = SentenceDetectorME.NO_SPLIT;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			if (!it.hasNext())
			{
			  type = SentenceDetectorME.SPLIT;
			}

			events.Add(new Event(type, cg.getContext(sample.Document, sentenceSpan.Start + candidate)));
		  }
		}


		return events.GetEnumerator();
	  }

	    public override bool hasNext()
	    {
	        throw new System.NotImplementedException();
	    }

	    public override Event next()
	    {
	        throw new System.NotImplementedException();
	    }
	}

}