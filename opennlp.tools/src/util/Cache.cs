using System;
using System.Collections;
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
using System.Collections.Generic;
using opennlp.tools.dictionary;

namespace opennlp.tools.util
{


    /// <summary>
    /// Provides fixed size, pre-allocated, least recently used replacement cache.
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @SuppressWarnings("unchecked") public class Cache implements java.util.Map
    public class Cache
    {

        /// <summary>
        /// The element in the linked list which was most recently used. * </summary>
        private DoubleLinkedListElement first;
        /// <summary>
        /// The element in the linked list which was least recently used. * </summary>
        private DoubleLinkedListElement last;
        /// <summary>
        /// Temporary holder of the key of the least-recently-used element. </summary>
        private object lastKey;
        /// <summary>
        /// Temporary value used in swap. </summary>
        private ObjectWrapper temp;
        /// <summary>
        /// Holds the object wrappers which the keys are mapped to. </summary>
        private ObjectWrapper[] wrappers;
        /// <summary>
        /// Map which stores the keys and values of the cache. </summary>
        private IDictionary map;
        /// <summary>
        /// The size of the cache. </summary>
        private int size_Renamed;

        /// <summary>
        /// Creates a new cache of the specified size. </summary>
        /// <param name="size"> The size of the cache. </param>
        public Cache(int size)
        {
            map = new Hashtable(size);
            wrappers = new ObjectWrapper[size];
            this.size_Renamed = size;
            object o = new object();
            first = new DoubleLinkedListElement(null, null, o);
            map[o] = new ObjectWrapper(null, first);
            wrappers[0] = new ObjectWrapper(null, first);

            DoubleLinkedListElement e = first;
            for (int i = 1; i < size; i++)
            {
                o = new object();
                e = new DoubleLinkedListElement(e, null, o);
                wrappers[i] = new ObjectWrapper(null, e);
                map[o] = wrappers[i];
                e.prev.next = e;
            }
            last = e;
        }

        public bool Contains(object key)
        {
            throw new NotImplementedException();
        }

        public void Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        public virtual void Clear()
        {
            map.Clear();
            DoubleLinkedListElement e = first;
            for (int oi = 0; oi < size_Renamed; oi++)
            {
                wrappers[oi].@object = null;
                object o = new object();
                map[o] = wrappers[oi];
                e.@object = o;
                e = e.next;
            }
        }

        public virtual object put(object key, object value)
        {
            ObjectWrapper o = (ObjectWrapper)map[key];
            if (o != null)
            {
                /*
                 * this should never be the case, we only do a put on a cache miss which
                 * means the current value wasn't in the cache. However if the user screws
                 * up or wants to use this as a fixed size hash and puts the same thing in
                 * the list twice then we update the value and more the key to the front of the
                 * most recently used list.
                 */

                // Move o's partner in the list to front
                DoubleLinkedListElement e = o.listItem;

                //move to front
                if (e != first)
                {
                    //remove list item
                    e.prev.next = e.next;
                    if (e.next != null)
                    {
                        e.next.prev = e.prev;
                    }
                    else //were moving last
                    {
                        last = e.prev;
                    }

                    //put list item in front
                    e.next = first;
                    first.prev = e;
                    e.prev = null;

                    //update first
                    first = e;
                }
                return o.@object;
            }
            // Put o in the front and remove the last one
            lastKey = last.@object; // store key to remove from hash later
            last.@object = key; //update list element with new key

            // connect list item to front of list
            last.next = first;
            first.prev = last;

            // update first and last value
            first = last;
            last = last.prev;
            first.prev = null;
            last.next = null;

            // remove old value from cache
            temp = (ObjectWrapper)map[lastKey];
            map.Remove(lastKey);
            //update wrapper
            temp.@object = value;
            temp.listItem = first;

            map[key] = temp;
            return null;
        }

        public virtual object get(object key)
        {
            ObjectWrapper o = (ObjectWrapper)map[key];
            if (o != null)
            {
                // Move it to the front
                DoubleLinkedListElement e = o.listItem;

                //move to front
                if (e != first)
                {
                    //remove list item
                    e.prev.next = e.next;
                    if (e.next != null)
                    {
                        e.next.prev = e.prev;
                    }
                    else //were moving last
                    {
                        last = e.prev;
                    }
                    //put list item in front
                    e.next = first;
                    first.prev = e;
                    e.prev = null;

                    //update first
                    first = e;
                }
                return o.@object;
            }
            else
            {
                return null;
            }
        }


        public virtual bool ContainsKey(object key)
        {
            return map.Contains(key);
        }

        public virtual bool containsValue(object value)
        {
            return map.Contains(value);
        }

        public virtual HashSet<Object> entrySet()
        {
            //return map.SetOfKeyValuePairs<Object>();
            throw new NotFiniteNumberException();
        }

        public virtual bool Empty
        {
            get
            {
                return map.Count == 0;
            }
        }

        public virtual HashSet<Object> keySet()
        {
            throw new NotImplementedException();
            //return map.Keys;
        }

        public virtual void putAll(IDictionary t)
        {
            map = t;
        }

        public virtual object remove(object key)
        {
            var o = map[key];
            map.Remove(key);
            return o;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public virtual int Count
        {
            get
            {
                return map.Count;
            }
        }

        public object SyncRoot { get; private set; }
        public bool IsSynchronized { get; private set; }

        public ICollection Keys { get; private set; }

        public virtual ICollection Values
        {
            get
            {
                return map.Values;
            }
        }

        public bool IsReadOnly { get; private set; }
        public bool IsFixedSize { get; private set; }

        public string[] this[string cacheKey]
        {
            get { return map[cacheKey] as string[]; }
            set { map.Add(cacheKey, value); }
        }
    }

    internal class ObjectWrapper
    {

        public object @object;
        public DoubleLinkedListElement listItem;

        public ObjectWrapper(object o, DoubleLinkedListElement li)
        {
            @object = o;
            listItem = li;
        }

        public virtual object Object
        {
            get
            {
                return @object;
            }
            set
            {
                @object = value;
            }
        }

        public virtual DoubleLinkedListElement ListItem
        {
            get
            {
                return listItem;
            }
            set
            {
                listItem = value;
            }
        }



        public virtual bool eqauls(object o)
        {
            return @object.Equals(o);
        }
    }

    internal class DoubleLinkedListElement
    {

        public DoubleLinkedListElement prev;
        public DoubleLinkedListElement next;
        public object @object;

        public DoubleLinkedListElement(DoubleLinkedListElement p, DoubleLinkedListElement n, object o)
        {
            prev = p;
            next = n;
            @object = o;

            if (p != null)
            {
                p.next = this;
            }

            if (n != null)
            {
                n.prev = this;
            }
        }
    }

    internal class DoubleLinkedList
    {

        internal DoubleLinkedListElement first;
        internal DoubleLinkedListElement last;
        internal DoubleLinkedListElement current;

        public DoubleLinkedList()
        {
            first = null;
            last = null;
            current = null;
        }

        public virtual void addFirst(object o)
        {
            first = new DoubleLinkedListElement(null, first, o);

            if (current.next == null)
            {
                last = current;
            }
        }

        public virtual void addLast(object o)
        {
            last = new DoubleLinkedListElement(last, null, o);

            if (current.prev == null)
            {
                first = current;
            }
        }

        public virtual void insert(object o)
        {
            if (current == null)
            {
                current = new DoubleLinkedListElement(null, null, o);
            }
            else
            {
                current = new DoubleLinkedListElement(current.prev, current, o);
            }

            if (current.prev == null)
            {
                first = current;
            }

            if (current.next == null)
            {
                last = current;
            }
        }

        public virtual DoubleLinkedListElement First
        {
            get
            {
                current = first;
                return first;
            }
        }

        public virtual DoubleLinkedListElement Last
        {
            get
            {
                current = last;
                return last;
            }
        }

        public virtual DoubleLinkedListElement Current
        {
            get
            {
                return current;
            }
        }

        public virtual DoubleLinkedListElement next()
        {
            if (current.next != null)
            {
                current = current.next;
            }
            return current;
        }

        public virtual DoubleLinkedListElement prev()
        {
            if (current.prev != null)
            {
                current = current.prev;
            }
            return current;
        }

        public override string ToString()
        {
            DoubleLinkedListElement e = first;
            string s = "[" + e.@object.ToString();

            e = e.next;

            while (e != null)
            {
                s = s + ", " + e.@object.ToString();
                e = e.next;
            }

            s = s + "]";

            return s;
        }
    }

}