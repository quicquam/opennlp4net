﻿/*
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

using j4n.Exceptions;
using j4n.Security;
using System.Numerics;
using opennlp.nonjava.helperclasses;

namespace opennlp.model
{
    public class HashSumEventStream : EventStream
    {
        private readonly EventStream eventStream;

        private MessageDigest digest;

        public HashSumEventStream(EventStream eventStream)
        {
            this.eventStream = eventStream;

            try
            {
                digest = MessageDigest.getInstance("MD5");
            }
            catch (NoSuchAlgorithmException e)
            {
                // should never happen, does all java runtimes have md5 ?!
                throw new IllegalStateException(e);
            }
        }

        public virtual bool hasNext()
        {
            return eventStream.hasNext();
        }

        public virtual Event next()
        {
            Event @event = eventStream.next();

            try
            {
                digest.update(@event.ToString().GetBytes("UTF-8"));
            }
            catch (UnsupportedEncodingException e)
            {
                throw new IllegalStateException("UTF-8 encoding is not available!", e);
            }

            return @event;
        }

        /// <summary>
        /// Calculates the hash sum of the stream. The method must be
        /// called after the stream is completely consumed.
        /// </summary>
        /// <returns> the hash sum </returns>
        /// <exception cref="IllegalStateException"> if the stream is not consumed completely,
        /// completely means that hasNext() returns false </exception>
        public virtual BigInteger calculateHashSum()
        {
            //    if (hasNext())
            //      throw new IllegalStateException("stream must be consumed completely!");

            return new BigInteger(digest.digest());
        }

        public virtual void remove()
        {
        }
    }
}