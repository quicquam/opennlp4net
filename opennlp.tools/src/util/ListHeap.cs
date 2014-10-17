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
using j4n.Interfaces;

namespace opennlp.tools.util
{


    /// <summary>
    /// This class implements the heap interface using a <seealso cref="java.util.List"/> as the underlying
    /// data structure.  This heap allows values which are equals to be inserted.  The heap will
    /// return the top K values which have been added where K is specified by the size passed to
    /// the constructor. K+1 values are not gaurenteed to be kept in the heap or returned in a
    /// particular order.
    /// </summary>
    public class ListHeap<E> : Heap<E> where E : IComparable<E>
    {
        private IList<E> list;

        private IComparer<E> comp;

        private int size_Renamed;

        private E max = default(E);

        /// <summary>
        /// Creates a new heap with the specified size using the sorted based on the
        /// specified comparator. </summary>
        /// <param name="sz"> The size of the heap. </param>
        /// <param name="c"> The comparator to be used to sort heap elements. </param>
        public ListHeap(int sz, IComparer<E> c)
        {
            size_Renamed = sz;
            comp = c;
            list = new List<E>(sz);
        }

        /// <summary>
        /// Creates a new heap of the specified size. </summary>
        /// <param name="sz"> The size of the new heap. </param>
        public ListHeap(int sz) : this(sz, null)
        {
        }

        private int parent(int i)
        {
            return (i - 1)/2;
        }

        private int left(int i)
        {
            return (i + 1)*2 - 1;
        }

        private int right(int i)
        {
            return (i + 1)*2;
        }

        public virtual int size()
        {
            return list.Count;
        }

        private void swap(int x, int y)
        {
            E ox = list[x];
            E oy = list[y];

            list[y] = ox;
            list[x] = oy;
        }

        private bool lt(E o1, E o2)
        {
            if (comp != null)
            {
                return comp.Compare(o1, o2) < 0;
            }
            else
            {
                return o1.CompareTo(o2) < 0;
            }
        }

        private bool gt(E o1, E o2)
        {
            if (comp != null)
            {
                return comp.Compare(o1, o2) > 0;
            }
            else
            {
                return o1.CompareTo(o2) > 0;
            }
        }

        private void heapify(int i)
        {
            while (true)
            {
                int l = left(i);
                int r = right(i);
                int smallest;

                if (l < list.Count && lt(list[l], list[i]))
                {
                    smallest = l;
                }
                else
                {
                    smallest = i;
                }

                if (r < list.Count && lt(list[r], list[smallest]))
                {
                    smallest = r;
                }

                if (smallest != i)
                {
                    swap(smallest, i);
                    i = smallest;
                }
                else
                {
                    break;
                }
            }
        }

        public virtual E extract()
        {
            if (list.Count == 0)
            {
                throw new Exception("Heap Underflow");
            }
            E top = list[0];
            int last = list.Count - 1;
            if (last != 0)
            {
                var lastItem = list[last];
                list[0] = lastItem;
                list.Remove(lastItem);
                heapify(0);
            }
            else
            {
                list.RemoveAt(last);
            }

            return top;
        }

        public virtual E first()
        {
            if (list.Count == 0)
            {
                throw new Exception("Heap Underflow");
            }
            return list[0];
        }

        public virtual E last()
        {
            if (list.Count == 0)
            {
                throw new Exception("Heap Underflow");
            }
            return max;
        }

        public virtual void add(E o)
        {
            /* keep track of max to prevent unnecessary insertion */
            if (max == null)
            {
                max = o;
            }
            else if (gt(o, max))
            {
                if (list.Count < size_Renamed)
                {
                    max = o;
                }
                else
                {
                    return;
                }
            }
            list.Add(o);

            int i = list.Count - 1;

            //percolate new node to correct position in heap.
            while (i > 0 && gt(list[parent(i)], o))
            {
                list[i] = list[parent(i)];
                i = parent(i);
            }

            list[i] = o;
        }

        public virtual void clear()
        {
            list.Clear();
        }

        public virtual IEnumerator<E> iterator()
        {
            return list.GetEnumerator();
        }

        public virtual bool Empty
        {
            get { return this.list.Count == 0; }
        }

        [Obsolete]
        public static void Main(string[] args)
        {
            throw new NotImplementedException();
            /*   
       Heap<int?> heap = new ListHeap<int?>(5);
	   for (int ai = 0;ai < args.Length;ai++)
	   {
		 heap.add(Convert.ToInt32(args[ai]));
	   }
	   while (!heap.Empty)
	   {
		 Console.Write(heap.extract() + " ");
	   }
	   Console.WriteLine(); */
        }
        
    }

}