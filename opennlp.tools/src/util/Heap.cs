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

namespace opennlp.tools.util
{
    /// <summary>
    /// Interface for interacting with a Heap data structure.
    /// This implementation extract objects from smallest to largest based on either
    /// their natural ordering or the comparator provided to an implementation.
    /// While this is a typical of a heap it allows this objects natural ordering to
    /// match that of other sorted collections.
    /// 
    /// </summary>
    public interface Heap<E>
    {
        /// <summary>
        /// Removes the smallest element from the heap and returns it. </summary>
        /// <returns> The smallest element from the heap. </returns>
        E extract();

        /// <summary>
        /// Returns the smallest element of the heap. </summary>
        /// <returns> The top element of the heap. </returns>
        E first();

        /// <summary>
        /// Returns the largest element of the heap. </summary>
        /// <returns> The largest element of the heap. </returns>
        E last();

        /// <summary>
        /// Adds the specified object to the heap. </summary>
        /// <param name="o"> The object to add to the heap. </param>
        void add(E o);

        /// <summary>
        /// Returns the size of the heap. </summary>
        /// <returns> The size of the heap. </returns>
        int size();

        /// <summary>
        /// Returns whether the heap is empty. </summary>
        /// <returns> true if the heap is empty; false otherwise. </returns>
        bool Empty { get; }

        /// <summary>
        /// Returns an iterator over the elements of the heap.  No specific ordering of these
        /// elements is guaranteed. </summary>
        /// <returns> An iterator over the elements of the heap. </returns>
        IEnumerator<E> iterator();

        /// <summary>
        /// Clears the contents of the heap.
        /// </summary>
        void clear();
    }
}