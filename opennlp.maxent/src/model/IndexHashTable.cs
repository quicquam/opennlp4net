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

using j4n.Object;

namespace opennlp.model
{

	/// <summary>
	/// The <seealso cref="IndexHashTable"/> is a hash table which maps entries
	/// of an array to their index in the array. All entries in the array must
	/// be unique otherwise a well-defined mapping is not possible.
	/// <para>
	/// The entry objects must implement <seealso cref="Object#equals(Object)"/> and
	/// <seealso cref="Object#hashCode()"/> otherwise the behavior of this class is
	/// undefined.
	/// </para>
	/// <para>
	/// The implementation uses a hash table with open addressing and linear probing.
	/// </para>
	/// <para>
	/// The table is thread safe and can concurrently accessed by multiple threads,
	/// thread safety is achieved through immutability. Though its not strictly immutable
	/// which means, that the table must still be safely published to other threads.
	/// </para>
	/// </summary>
	public class IndexHashTable<T>
	{

	  private readonly object[] keys;
	  private readonly int[] values;

	  private readonly int size_Renamed;

	  /// <summary>
	  /// Initializes the current instance. The specified array is copied into the
	  /// table and later changes to the array do not affect this table in any way.
	  /// </summary>
	  /// <param name="mapping">
	  ///          the values to be indexed, all values must be unique otherwise a
	  ///          well-defined mapping of an entry to an index is not possible </param>
	  /// <param name="loadfactor">
	  ///          the load factor, usually 0.7
	  /// </param>
	  /// <exception cref="IllegalArgumentException">
	  ///           if the entries are not unique </exception>
	  public IndexHashTable(T[] mapping, double loadfactor)
	  {
		if (loadfactor <= 0 || loadfactor > 1)
		{
		  throw new System.ArgumentException("loadfactor must be larger than 0 " + "and equal to or smaller than 1 but is " + loadfactor + "!");
		}

		int arraySize = (int)(mapping.Length / loadfactor) + 1;

		keys = new object[arraySize];
		values = new int[arraySize];

		size_Renamed = mapping.Length;

		for (int i = 0; i < mapping.Length; i++)
		{
            var s = mapping[i] as string;
		    var hash = s != null ? s.hashCode() : mapping[i].GetHashCode();
		    int startIndex = indexForHash(hash, keys.Length);

		  int index = searchKey(startIndex, null, true);

		  if (index == -1)
		  {
			throw new System.ArgumentException("Array must contain only unique keys!");
		  }

		  keys[index] = mapping[i];
		  values[index] = i;
		}
	  }

	  private static int indexForHash(int h, int length)
	  {
		return (h & 0x7fffffff) % length;
	  }

	  private int searchKey(int startIndex, object key, bool insert)
	  {

		for (int index = startIndex; true; index = (index + 1) % keys.Length)
		{

		  // The keys array contains at least one null element, which guarantees
		  // termination of the loop
		  if (keys[index] == null)
		  {
			if (insert)
			{
			  return index;
			}
			else
			{
			  return -1;
			}
		  }

		  if (keys[index].Equals(key))
		  {
			if (!insert)
			{
			  return index;
			}
			else
			{
			  return -1;
			}
		  }
		}
	  }

	  /// <summary>
	  /// Retrieves the index for the specified key.
	  /// </summary>
	  /// <param name="key"> </param>
	  /// <returns> the index or -1 if there is no entry to the keys </returns>
	  public virtual int get(T key)
	  {

        var s = key as string;
        var hash = s != null ? s.hashCode() : key.GetHashCode();
        int startIndex = indexForHash(hash, keys.Length);

		int index = searchKey(startIndex, key, false);

		if (index != -1)
		{
		  return values[index];
		}
		else
		{
		  return -1;
		}
	  }

	  /// <summary>
	  /// Retrieves the size.
	  /// </summary>
	  /// <returns> the number of elements in this map. </returns>
	  public virtual int size()
	  {
		return size_Renamed;
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public T[] toArray(T array[])
	  public virtual T[] toArray(T[] array)
	  {
		for (int i = 0; i < keys.Length; i++)
		{
		  if (keys[i] != null)
		  {
			array[values[i]] = (T) keys[i];
		  }
		}

		return array;
	  }
	}

}