using System;
using System.Collections;
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
using opennlp.model;

namespace opennlp.tools.namefind
 {


	using AbstractModel = opennlp.model.AbstractModel;
	using Event = opennlp.model.Event;
	using opennlp.tools.util;
	using AdaptiveFeatureGenerator = opennlp.tools.util.featuregen.AdaptiveFeatureGenerator;

     public class NameSampleSequenceStream : SequenceStream<NameSample>
	{

	  private NameContextGenerator pcg;
	  private IList<NameSample> samples;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NameSampleSequenceStream(opennlp.tools.util.ObjectStream<NameSample> psi) throws java.io.IOException
	  public NameSampleSequenceStream(ObjectStream<NameSample> psi) : this(psi, new DefaultNameContextGenerator((AdaptiveFeatureGenerator) null))
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NameSampleSequenceStream(opennlp.tools.util.ObjectStream<NameSample> psi, opennlp.tools.util.featuregen.AdaptiveFeatureGenerator featureGen) throws java.io.IOException
	  public NameSampleSequenceStream(ObjectStream<NameSample> psi, AdaptiveFeatureGenerator featureGen) : this(psi, new DefaultNameContextGenerator(featureGen))
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NameSampleSequenceStream(opennlp.tools.util.ObjectStream<NameSample> psi, NameContextGenerator pcg) throws java.io.IOException
	  public NameSampleSequenceStream(ObjectStream<NameSample> psi, NameContextGenerator pcg)
	  {
		samples = new List<NameSample>();

		NameSample sample;
		while ((sample = psi.read()) != null)
		{
		  samples.Add(sample);
		}

		Console.Error.WriteLine("Got " + samples.Count + " sequences");

		this.pcg = pcg;
	  }


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public opennlp.model.Event[] updateContext(opennlp.model.Sequence sequence, opennlp.model.AbstractModel model)
	  public virtual Event[] updateContext(Sequence<NameSample> sequence, AbstractModel model)
	  {
		Sequence<NameSample> pss = sequence;
		TokenNameFinder tagger = new NameFinderME(new TokenNameFinderModel("x-unspecified", model, new Dictionary<string, object>(), null));
		string[] sentence = pss.Source.Sentence;
		string[] tags = NameFinderEventStream.generateOutcomes(tagger.find(sentence), null, sentence.Length);
		Event[] events = new Event[sentence.Length];

		events = NameFinderEventStream.generateEvents(sentence,tags,pcg).ToArray();

		return events;
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public java.util.Iterator<opennlp.model.Sequence> iterator()
	  public virtual IEnumerator<Sequence<NameSample>> iterator()
	  {
		return new NameSampleSequenceIterator(samples.GetEnumerator());
	  }

      public IEnumerator<Sequence<NameSample>> GetEnumerator()
         {
             throw new NotImplementedException();
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
             return GetEnumerator();
         }
	}

	internal class NameSampleSequenceIterator : IEnumerator<Sequence<NameSample>>
	{

	  private IEnumerator<NameSample> psi;
	  private NameContextGenerator cg;

	  public NameSampleSequenceIterator(IEnumerator<NameSample> psi)
	  {
		this.psi = psi;
		cg = new DefaultNameContextGenerator(null);
	  }

	  public virtual bool hasNext()
	  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
		return psi.hasNext();
	  }

	  public virtual Sequence<NameSample> next()
	  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
		NameSample sample = psi.next();

		string[] sentence = sample.Sentence;
		string[] tags = NameFinderEventStream.generateOutcomes(sample.Names, null, sentence.Length);
		Event[] events = new Event[sentence.Length];

		for (int i = 0; i < sentence.Length; i++)
		{

		  // it is safe to pass the tags as previous tags because
		  // the context generator does not look for non predicted tags
		  string[] context = cg.getContext(i, sentence, tags, null);

		  events[i] = new Event(tags[i], context);
		}
		Sequence<NameSample> sequence = new Sequence<NameSample>(events,sample);
		return sequence;
	  }

	  public virtual void remove()
	  {
		throw new System.NotSupportedException();
	  }

	    public void Dispose()
	    {
	        throw new NotImplementedException();
	    }

	    public bool MoveNext()
	    {
	        throw new NotImplementedException();
	    }

	    public void Reset()
	    {
	        throw new NotImplementedException();
	    }

	    public Sequence<NameSample> Current { get; private set; }

	    object IEnumerator.Current
	    {
	        get { return Current; }
	    }
	}


 }