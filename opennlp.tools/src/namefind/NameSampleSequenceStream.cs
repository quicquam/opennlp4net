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

        public NameSampleSequenceStream(ObjectStream<NameSample> psi)
            : this(psi, new DefaultNameContextGenerator((AdaptiveFeatureGenerator) null))
        {
        }

        public NameSampleSequenceStream(ObjectStream<NameSample> psi, AdaptiveFeatureGenerator featureGen)
            : this(psi, new DefaultNameContextGenerator(featureGen))
        {
        }

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


        public virtual Event[] updateContext(Sequence<NameSample> sequence, AbstractModel model)
        {
            Sequence<NameSample> pss = sequence;
            TokenNameFinder tagger =
                new NameFinderME(new TokenNameFinderModel("x-unspecified", model, new Dictionary<string, object>(), null));
            string[] sentence = pss.Source.Sentence;
            string[] tags = NameFinderEventStream.generateOutcomes(tagger.find(sentence), null, sentence.Length);
            Event[] events = new Event[sentence.Length];

            events = NameFinderEventStream.generateEvents(sentence, tags, pcg).ToArray();

            return events;
        }

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

            return psi.Current != null;
        }

        public virtual Sequence<NameSample> next()
        {

            psi.MoveNext();
            NameSample sample = psi.Current;

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
            Sequence<NameSample> sequence = new Sequence<NameSample>(events, sample);
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