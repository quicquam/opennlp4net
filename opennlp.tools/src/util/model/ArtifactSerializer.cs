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
using j4n.Interfaces;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;

namespace opennlp.tools.util.model
{


	/// <summary>
	/// Responsible to create an artifact from an <seealso cref="InputStream"/>.
	/// </summary>
	public interface ArtifactSerializer<T>
	{

	  /// <summary>
	  /// Creates the artifact from the provided <seealso cref="InputStream"/>.
	  /// 
	  /// The <seealso cref="InputStream"/> remains open.
	  /// </summary>
	  /// <returns> the artifact
	  /// </returns>
	  /// <exception cref="IOException"> </exception>
	  /// <exception cref="InvalidFormatException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: T create(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException;
	  T create(InputStream @in);

	  /// <summary>
	  /// Serializes the artifact to the provided <seealso cref="OutputStream"/>.
	  /// 
	  /// The <seealso cref="OutputStream"/> remains open.
	  /// </summary>
	  /// <param name="artifact"> </param>
	  /// <param name="out"> </param>
	  /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(T artifact, java.io.OutputStream out) throws java.io.IOException;
	  void serialize(T artifact, OutputStream @out);
	}

}