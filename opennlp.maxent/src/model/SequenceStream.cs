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

using System.Collections.Generic;
namespace opennlp.model
{

	/// <summary>
	///  Interface for streams of sequences used to train sequence models. 
	/// </summary>
	public interface SequenceStream<T> : IEnumerable<Sequence<T>>
	{
	  /// <summary>
	  /// Creates a new event array based on the outcomes predicted by the specified parameters 
	  /// for the specified sequence. </summary>
	  /// <param name="sequence"> The sequence to be evaluated. </param>
	  /// <returns> event array </returns>
	  Event[] updateContext(Sequence<T> sequence, AbstractModel model);

	}

}