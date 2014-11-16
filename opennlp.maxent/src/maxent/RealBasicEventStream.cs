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
using j4n.IO.Reader;
using opennlp.nonjava.helperclasses;

namespace opennlp.maxent
{
    using AbstractEventStream = opennlp.model.AbstractEventStream;
    using Event = opennlp.model.Event;
    using EventStream = opennlp.model.EventStream;
    using RealValueFileEventStream = opennlp.model.RealValueFileEventStream;

    public class RealBasicEventStream : AbstractEventStream
    {
        internal ContextGenerator cg = new BasicContextGenerator();
        internal DataStream ds;
        internal Event next_Renamed;

        public RealBasicEventStream(DataStream ds)
        {
            this.ds = ds;
            if (this.ds.hasNext())
            {
                next_Renamed = createEvent((string) this.ds.nextToken());
            }
        }

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
            int lastSpace = obs.LastIndexOf(' ');
            if (lastSpace == -1)
            {
                return null;
            }
            else
            {
                string[] contexts = obs.Substring(0, lastSpace).Split("\\s+", true);
                float[] values = RealValueFileEventStream.parseContexts(contexts);
                return new Event(obs.Substring(lastSpace + 1), contexts, values);
            }
        }


        public static void Main(string[] args)
        {
            EventStream es = new RealBasicEventStream(new PlainTextByLineDataStream(new FileReader(args[0])));
            while (es.hasNext())
            {
                Console.WriteLine(es.next());
            }
        }
    }
}