using System;
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
using System.IO;
using j4n.IO.OutputStream;
using j4n.IO.Writer;


namespace opennlp.tools.util
{
    /// <summary>
    /// Set which counts the number of times a values are added to it.
    /// This value can be accessed with the #getCount method.
    /// </summary>
    public class CountedSet<E> : HashSet<E>
    {
        private IDictionary<E, int?> cset;

        /// <summary>
        /// Creates a new counted set.
        /// </summary>
        public CountedSet()
        {
            cset = new Dictionary<E, int?>();
        }

        /// <summary>
        /// Creates a new counted set of the specified initial size.
        /// </summary>
        /// <param name="size"> The initial size of this set. </param>
        public CountedSet(int size)
        {
            cset = new Dictionary<E, int?>(size);
        }

        public virtual bool add(E o)
        {
            int? count = cset[o];
            if (count == null)
            {
                cset[o] = 1;
                return true;
            }
            else
            {
                cset[o] = count + 1;
                return false;
            }
        }

        /// <summary>
        /// Reduces the count associated with this object by 1.  If this causes the count
        /// to become 0, then the object is removed form the set.
        /// </summary>
        /// <param name="o"> The object whose count is being reduced. </param>
        public virtual void subtract(E o)
        {
            int? count = cset[o];
            if (count != null)
            {
                int c = count.GetValueOrDefault() - 1;
                if (c == 0)
                {
                    cset.Remove(o);
                }
                else
                {
                    cset[o] = c;
                }
            }
        }

        /// <summary>
        /// Assigns the specified object the specified count in the set.
        /// </summary>
        /// <param name="o"> The object to be added or updated in the set. </param>
        /// <param name="c"> The count of the specified object. </param>
        public virtual void setCount(E o, int c)
        {
            cset[o] = c;
        }

        /// <summary>
        /// Return the count of the specified object.
        /// </summary>
        /// <param name="o"> the object whose count needs to be determined. </param>
        /// <returns> the count of the specified object. </returns>
        public virtual int getCount(E o)
        {
            int? count = cset[o];
            if (count == null)
            {
                return 0;
            }
            else
            {
                return count.Value;
            }
        }

        /// <summary>
        /// This methods is deprecated use opennlp.toolsdictionary.serialization
        /// package for writing a <seealso cref="CountedSet"/>.
        /// </summary>
        /// <param name="fileName"> </param>
        /// <param name="countCutoff"> </param>
        [Obsolete]
        public virtual void write(string fileName, int countCutoff)
        {
            write(fileName, countCutoff, " ");
        }

        /// <summary>
        /// This methods is deprecated use opennlp.toolsdictionary.serialization
        /// package for writing a <seealso cref="CountedSet"/>.
        /// </summary>
        /// <param name="fileName"> </param>
        /// <param name="countCutoff"> </param>
        /// <param name="delim"> </param>
        [Obsolete]
        public virtual void write(string fileName, int countCutoff, string delim)
        {
            write(fileName, countCutoff, delim, null);
        }

        /// <summary>
        /// This methods is deprecated use opennlp.toolsdictionary.serialization
        /// package for writing a <seealso cref="CountedSet"/>.
        /// </summary>
        /// <param name="fileName"> </param>
        /// <param name="countCutoff"> </param>
        /// <param name="delim"> </param>
        /// <param name="encoding"> </param>
        [Obsolete]
        public virtual void write(string fileName, int countCutoff, string delim, string encoding)
        {
            PrintWriter @out = null;
            try
            {
                if (encoding != null)
                {
                    @out = new PrintWriter(new OutputStreamWriter(new FileOutputStream(fileName), encoding));
                }
                else
                {
                    @out = new PrintWriter(new FileWriter(fileName));
                }

                for (IEnumerator<E> e = cset.Keys.GetEnumerator(); e.MoveNext();)
                {
                    E key = e.Current;
                    int count = this.getCount(key);
                    if (count >= countCutoff)
                    {
                        @out.println(count + delim + key);
                    }
                }
                @out.close();
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public virtual bool addAll<T1>(ICollection<T1> c) where T1 : E
        {
            bool changed = false;

            foreach (E element in c)
            {
                changed = changed || add(element);
            }

            return changed;
        }

        public virtual void clear()
        {
            cset.Clear();
        }

        public virtual bool contains(E o)
        {
            return cset.Keys.Contains(o);
        }

        public virtual bool containsAll<T1>(ICollection<T1> c)
        {
            throw new NotImplementedException();
        }

        public virtual bool Empty
        {
            get { return cset.Count == 0; }
        }

        public virtual IEnumerator<E> iterator()
        {
            return cset.Keys.GetEnumerator();
        }

        public virtual bool remove(E o)
        {
            return cset.ContainsKey(o) && cset.Remove(o) != null;
        }

        public virtual bool removeAll<T1>(ICollection<T1> c)
        {
            bool changed = false;
            for (IEnumerator<E> ki = cset.Keys.GetEnumerator(); ki.MoveNext();)
            {
                changed = changed || cset.Remove(ki.Current) != null;
            }
            return changed;
        }

        public virtual bool retainAll<T1>(ICollection<T1> c)
        {
            throw new NotImplementedException();
        }

        public virtual int size()
        {
            return cset.Count;
        }

        public virtual object[] toArray()
        {
            throw new NotImplementedException();
        }

        public virtual T[] toArray<T>(T[] a)
        {
            throw new NotImplementedException();
        }
    }
}