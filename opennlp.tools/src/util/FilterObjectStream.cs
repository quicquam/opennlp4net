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
    /// <summary>
    /// Abstract base class for filtering <seealso cref="ObjectStream"/>s.
    /// <para>
    /// Filtering streams take an existing stream and convert 
    /// its output to something else.
    /// 
    /// </para>
    /// </summary>
    /// @param <S> the type of the source/input stream </param>
    /// @param <T> the type of this stream </param>
    public abstract class FilterObjectStream<S, T> : ObjectStream<T>
    {
        public abstract T read();

        protected internal readonly ObjectStream<S> samples;

        protected internal FilterObjectStream(ObjectStream<S> samples)
        {
            if (samples == null)
            {
                throw new System.ArgumentException("samples must not be null!");
            }

            this.samples = samples;
        }

        protected FilterObjectStream(ObjectStream<sbyte[]> objectStream)
        {
            throw new System.NotImplementedException();
        }

        public virtual void reset()
        {
            samples.reset();
        }

        public virtual void close()
        {
            samples.close();
        }
    }
}