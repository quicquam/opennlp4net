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

namespace opennlp.tools.util
{


	/// <summary>
	/// An implementation of the Heap interface based on <seealso cref="java.util.SortedSet"/>.
	/// This implementation will not allow multiple objects which are equal to be added to the heap.
	/// Only use this implementation when object in the heap can be totally ordered (no duplicates).
	/// </summary>
	/// @deprecated not used anymore, when there is need for a heap use ListHeap instead 
	[Obsolete("not used anymore, when there is need for a heap use ListHeap instead")]
	public class TreeHeap<E> : Heap<E>
	{

	  private SortedSet<E> tree;

	  /// <summary>
	  /// Creates a new tree heap.
	  /// </summary>
	  public TreeHeap()
	  {
		tree = new SortedSet<E>();
	  }

	  /// <summary>
	  /// Creates a new tree heap of the specified size. </summary>
	  /// <param name="size"> The size of the new tree heap. </param>
	  public TreeHeap(int size)
	  {
		tree = new SortedSet<E>();
	  }

	  public virtual E extract()
	  {
		E rv = tree.first();
		tree.remove(rv);
		return rv;
	  }

	  public virtual E first()
	  {
		return tree.first();
	  }

	  public virtual E last()
	  {
		return tree.last();
	  }

	  public virtual IEnumerator<E> iterator()
	  {
		return tree.GetEnumerator();
	  }

	  public virtual void add(E o)
	  {
		tree.add(o);
	  }

	  public virtual int size()
	  {
		return tree.size();
	  }

	  public virtual void clear()
	  {
		tree.clear();
	  }

	  public virtual bool Empty
	  {
		  get
		  {
			return this.tree.Empty;
		  }
	  }

	  public static void Main(string[] args)
	  {
		Heap<int?> heap = new TreeHeap<int?>(5);
		for (int ai = 0;ai < args.Length;ai++)
		{
		  heap.add(Convert.ToInt32(args[ai]));
		}
		while (!heap.Empty)
		{
		  Console.Write(heap.extract() + " ");
		}
		Console.WriteLine();
	  }
	}

}