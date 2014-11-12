using System;

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

namespace opennlp.maxent
{
    using AbstractEventStream = opennlp.model.AbstractEventStream;
    using Event = opennlp.model.Event;

    /// <summary>
    /// A object which can deliver a stream of training events assuming
    /// that each event is represented as a separated list containing
    /// all the contextual predicates, with the last item being the
    /// outcome. The default separator is the space " ".
    /// e.g.: 
    /// 
    /// <para> cp_1 cp_2 ... cp_n outcome
    /// </para>
    /// <para> cp_1,cp_2,...,cp_n,outcome
    /// </para>
    /// </summary>
    public class BasicEventStream : AbstractEventStream
    {
        internal ContextGenerator cg;
        internal DataStream ds;
        internal Event next_Renamed;

        private string separator = " ";

        public BasicEventStream(DataStream ds, string sep)
        {
            separator = sep;
            cg = new BasicContextGenerator(separator);
            this.ds = ds;
            if (this.ds.hasNext())
            {
                next_Renamed = createEvent((string) this.ds.nextToken());
            }
        }

        public BasicEventStream(DataStream ds) : this(ds, " ")
        {
        }

        /// <summary>
        /// Returns the next Event object held in this EventStream.  Each call to nextEvent advances the EventStream.
        /// </summary>
        /// <returns> the Event object which is next in this EventStream </returns>
        public override Event next()
        {
            while (next_Renamed == null && this.ds.hasNext())
            {
                next_Renamed = createEvent((string) this.ds.nextToken());
            }

            Event current = next_Renamed;
            if (this.ds.hasNext())
            {
                next_Renamed = createEvent((string) this.ds.nextToken());
            }
            else
            {
                next_Renamed = null;
            }
            return current;
        }

        /// <summary>
        /// Test whether there are any Events remaining in this EventStream.
        /// </summary>
        /// <returns> true if this EventStream has more Events </returns>
        public override bool hasNext()
        {
            while (next_Renamed == null && ds.hasNext())
            {
                next_Renamed = createEvent((string) ds.nextToken());
            }
            return next_Renamed != null;
        }

        private Event createEvent(string obs)
        {
            int lastSpace = obs.LastIndexOf(separator, StringComparison.Ordinal);
            if (lastSpace == -1)
            {
                return null;
            }
            else
            {
                return new Event(obs.Substring(lastSpace + 1), cg.getContext(obs.Substring(0, lastSpace)));
            }
        }
    }
}