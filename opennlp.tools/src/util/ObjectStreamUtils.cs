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


	public class ObjectStreamUtils
	{

	  /// <summary>
	  /// Creates an <seealso cref="ObjectStream"/> form an array.
	  /// </summary>
	  /// @param <T> </param>
	  /// <param name="array">
	  /// </param>
	  /// <returns> the object stream over the array elements </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static <T> ObjectStream<T> createObjectStream(final T... array)
        public static ObjectStream<Object> createObjectStream<T>(params Object[] array)
	  {

		return new ObjectStreamAnonymousInnerClassHelper(array);
	  }

	  private class ObjectStreamAnonymousInnerClassHelper : ObjectStream<Object>
	  {
		  private Object[] array;

          public ObjectStreamAnonymousInnerClassHelper(Object[] array)
		  {
			  this.array = array;
		  }


		  private int index = 0;

          public virtual Object read()
		  {
			if (index < array.Length)
			{
			  return array[index++];
			}
			else
			{
			  return null;
			}
		  }

		  public virtual void reset()
		  {
			index = 0;
		  }

		  public virtual void close()
		  {
		  }
	  }

	  /// <summary>
	  /// Creates an <seealso cref="ObjectStream"/> form a collection.
	  /// </summary>
	  /// @param <T> </param>
	  /// <param name="collection">
	  /// </param>
	  /// <returns> the object stream over the collection elements </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static <T> ObjectStream<T> createObjectStream(final java.util.Collection<T> collection)
      public static ObjectStream<Object> createObjectStream(ICollection<Object> collection)
	  {

		return new ObjectStreamAnonymousInnerClassHelper2(collection);
	  }

      private class ObjectStreamAnonymousInnerClassHelper2 : ObjectStream<Object>
	  {
          private ICollection<Object> collection;

          public ObjectStreamAnonymousInnerClassHelper2(ICollection<Object> collection)
		  {
			  this.collection = collection;
		  }


          private IEnumerator<Object> iterator = collection.GetEnumerator();

          public virtual Object read()
		  {
			if (iterator.hasNext())
			{
			  return iterator.next();
			}
			else
			{
			  return null;
			}
		  }

		  public virtual void reset()
		  {
			iterator = collection.GetEnumerator();
		  }

		  public virtual void close()
		  {
		  }
	  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static <T> ObjectStream<T> createObjectStream(final ObjectStream<T>... streams)
      public static ObjectStream<Object> createObjectStream<T>(params ObjectStream<Object>[] streams)
	  {

          foreach (ObjectStream<Object> stream in streams)
		{
		  if (stream == null)
		  {
			throw new System.NullReferenceException("stream cannot be null");
		  }
		}

		return new ObjectStreamAnonymousInnerClassHelper3(streams);
	  }

      private class ObjectStreamAnonymousInnerClassHelper3 : ObjectStream<Object>
	  {
          private opennlp.tools.util.ObjectStream<Object>[] streams;

          public ObjectStreamAnonymousInnerClassHelper3(opennlp.tools.util.ObjectStream<Object>[] streams)
		  {
			  this.streams = streams;
		  }


		  private int streamIndex = 0;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public T read() throws java.io.IOException
          public virtual Object read()
		  {

              Object @object = null;

			while (streamIndex < streams.Length && @object == null)
			{
			  @object = streams[streamIndex].read();

			  if (@object == null)
			  {
				  streamIndex++;
			  }
			}

			return @object;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws java.io.IOException, UnsupportedOperationException
		  public virtual void reset()
		  {
			streamIndex = 0;

            foreach (ObjectStream<Object> stream in streams)
			{
			  stream.reset();
			}
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		  public virtual void close()
		  {

              foreach (ObjectStream<Object> stream in streams)
			{
			  stream.close();
			}
		  }
	  }
	}

}