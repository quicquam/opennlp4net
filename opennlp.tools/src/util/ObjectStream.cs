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
	/// Reads <code>Object</code>s from a stream.
	/// <para>
	/// Design Decision:<br>
	/// This interface provides a means for iterating over the
	/// objects in a stream, it does not implement <seealso cref="java.util.Iterator"/> or
	/// <seealso cref="Iterable"/> because:
	/// <ul>
	/// <li><seealso cref="java.util.Iterator#next()"/> and
	/// <seealso cref="java.util.Iterator#hasNext()"/> are declared as throwing no checked
	/// exceptions. Thus the <seealso cref="IOException"/>s thrown by <seealso cref="#read()"/> would
	/// have to be wrapped in <seealso cref="RuntimeException"/>s, and the compiler would be
	/// unable to force users of this code to catch such exceptions.</li>
	/// <li>Implementing <seealso cref="Iterable"/> would mean either silently calling
	/// <seealso cref="#reset()"/> to guarantee that all items were always seen on each
	/// iteration, or documenting that the Iterable only iterates over the remaining
	/// elements of the ObjectStream. In either case, users not reading the
	/// documentation carefully might run into unexpected behavior.</li>
	/// </ul>
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= ObjectStreamException </seealso>
	public interface ObjectStream<T>
	{

	  /// <summary>
	  /// Returns the next object. Calling this method repeatedly until it returns
	  /// null will return each object from the underlying source exactly once. 
	  /// </summary>
	  /// <returns> the next object or null to signal that the stream is exhausted </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: T read() throws java.io.IOException;
	  T read();

	  /// <summary>
	  /// Repositions the stream at the beginning and the previously seen object sequence
	  /// will be repeated exactly. This method can be used to re-read
	  /// the stream if multiple passes over the objects are required.
	  /// 
	  /// The implementation of this method is optional.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void reset() throws java.io.IOException, UnsupportedOperationException;
	  void reset();

	  /// <summary>
	  /// Closes the <code>ObjectStream</code> and releases all allocated
	  /// resources. After close was called its not allowed to call
	  /// read or reset.
	  /// </summary>
	  /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void close() throws java.io.IOException;
	  void close();
	}

}