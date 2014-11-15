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
using System.Linq;
using j4n.Serialization;
using opennlp.model;

namespace opennlp.tools.postag
{
    using AbstractModel = opennlp.model.AbstractModel;
    using Event = opennlp.model.Event;
    using opennlp.tools.util;

    public class POSSampleSequenceStream : SequenceStream<Event>
    {
        private POSContextGenerator pcg;
        private IList<POSSample> samples;

        public POSSampleSequenceStream(ObjectStream<POSSample> psi) : this(psi, new DefaultPOSContextGenerator(null))
        {
        }

        public POSSampleSequenceStream(ObjectStream<POSSample> psi, POSContextGenerator pcg)
        {
            samples = new List<POSSample>();

            POSSample sample;
            while ((sample = psi.read()) != null)
            {
                samples.Add(sample);
            }
            Console.Error.WriteLine("Got " + samples.Count + " sequences");
            this.pcg = pcg;
        }


        public virtual Event[] updateContext(Sequence<POSSample> sequence, AbstractModel model)
        {
            Sequence<POSSample> pss = sequence;
            POSTagger tagger = new POSTaggerME(new POSModel("x-unspecified", model, null, new POSTaggerFactory()));
            string[] sentence = pss.Source.Sentence;
            object[] ac = pss.Source.AddictionalContext;
            string[] tags = tagger.tag(pss.Source.Sentence);
            Event[] events = POSSampleEventStream.generateEvents(sentence, tags, ac, pcg).ToArray();
            return events;
        }

        public virtual IEnumerator<Sequence> iterator()
        {
            return new POSSampleSequenceIterator(samples.GetEnumerator());
        }

        public Event[] updateContext(Sequence<Event> sequence, AbstractModel model)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Sequence<Event>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class POSSampleSequenceIterator : IEnumerator<Sequence>
    {
        private IEnumerator<POSSample> psi;
        private POSContextGenerator cg;

        public POSSampleSequenceIterator(IEnumerator<POSSample> psi)
        {
            this.psi = psi;
            cg = new DefaultPOSContextGenerator(null);
        }

        public virtual bool hasNext()
        {

            return psi.MoveNext();
        }

        public virtual Sequence<POSSample> next()
        {

            POSSample sample = psi.Current;

            string[] sentence = sample.Sentence;
            string[] tags = sample.Tags;
            Event[] events = new Event[sentence.Length];

            for (int i = 0; i < sentence.Length; i++)
            {
                // it is safe to pass the tags as previous tags because
                // the context generator does not look for non predicted tags
                string[] context = cg.getContext(i, sentence, tags, null);

                events[i] = new Event(tags[i], context);
            }
            Sequence<POSSample> sequence = new Sequence<POSSample>(events, sample);
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

        public Sequence Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}