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


namespace opennlp.tools.util
{
    using Event = opennlp.model.Event;
    using EventStream = opennlp.model.EventStream;

    /// <summary>
    /// This is a base class for <seealso cref="EventStream"/> classes.
    /// It takes an <seealso cref="Iterator"/> of sample objects as input and
    /// outputs the events creates by a subclass.
    /// </summary>
    public abstract class AbstractEventStream<T> : opennlp.model.AbstractEventStream
    {
        private ObjectStream<T> samples;

        private IEnumerator<Event> events = System.Linq.Enumerable.Empty<Event>().GetEnumerator();

        /// <summary>
        /// Initializes the current instance with a sample <seealso cref="Iterator"/>.
        /// </summary>
        /// <param name="samples"> the sample <seealso cref="Iterator"/>. </param>
        public AbstractEventStream(ObjectStream<T> samples)
        {
            this.samples = samples;
        }

        /// <summary>
        /// Creates events for the provided sample.
        /// </summary>
        /// <param name="sample"> the sample for which training <seealso cref="Event"/>s
        /// are be created.
        /// </param>
        /// <returns> an <seealso cref="Iterator"/> of training events or
        /// an empty <seealso cref="Iterator"/>. </returns>
        protected internal abstract IEnumerator<Event> createEvents(T sample);

        /// <summary>
        /// Checks if there are more training events available.
        /// 
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public final boolean hasNext() throws java.io.IOException
        public override bool hasNext()
        {
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            if (events.MoveNext())
            {
                return true;
            }
            else
            {
                // search next event iterator which is not empty
                T sample = default(T);
                while (!events.MoveNext() && (sample = samples.read()) != null)
                {
                    events = createEvents(sample);
                }

                //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                return events.MoveNext();
            }
        }

        public override Event next()
        {
            events.MoveNext();
            return events.Current;
        }
    }
}