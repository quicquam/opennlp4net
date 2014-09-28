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

namespace opennlp.model
{

	/// <summary>
	/// Object which compresses events in memory and performs feature selection.
	/// </summary>
	public interface DataIndexer
	{
	  /// <summary>
	  /// Returns the array of predicates seen in each event. </summary>
	  /// <returns> a 2-D array whose first dimension is the event index and array this refers to contains
	  /// the contexts for that event.  </returns>
	  int[][] Contexts {get;}

	  /// <summary>
	  /// Returns an array indicating the number of times a particular event was seen. </summary>
	  /// <returns> an array indexed by the event index indicating the number of times a particular event was seen. </returns>
	  int[] NumTimesEventsSeen {get;}

	  /// <summary>
	  /// Returns an array indicating the outcome index for each event. </summary>
	  /// <returns> an array indicating the outcome index for each event. </returns>
	  int[] OutcomeList {get;}

	  /// <summary>
	  /// Returns an array of predicate/context names. </summary>
	  /// <returns> an array of predicate/context names indexed by context index.  These indices are the
	  /// value of the array returned by <code>getContexts</code>. </returns>
	  string[] PredLabels {get;}

	  /// <summary>
	  /// Returns an array of the count of each predicate in the events. </summary>
	  /// <returns> an array of the count of each predicate in the events. </returns>
	  int[] PredCounts {get;}

	  /// <summary>
	  /// Returns an array of outcome names. </summary>
	  /// <returns> an array of outcome names indexed by outcome index. </returns>
	  string[] OutcomeLabels {get;}

	  /// <summary>
	  /// Returns the values associated with each event context or null if integer values are to be used. </summary>
	  /// <returns> the values associated with each event context. </returns>
	  float[][] Values {get;}

	  /// <summary>
	  /// Returns the number of total events indexed. </summary>
	  /// <returns> The number of total events indexed. </returns>
	  int NumEvents {get;}
	}
}