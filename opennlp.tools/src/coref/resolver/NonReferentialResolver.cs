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

namespace opennlp.tools.coref.resolver
{

	using MentionContext = opennlp.tools.coref.mention.MentionContext;

	/// <summary>
	/// Provides the interface for a object to provide a resolver with a non-referential
	/// probability.  Non-referential resolvers compute the probability that a particular mention refers
	/// to no antecedent.  This probability can then compete with the probability that
	/// a mention refers with a specific antecedent.
	/// </summary>
	public interface NonReferentialResolver
	{

	  /// <summary>
	  /// Returns the probability that the specified mention doesn't refer to any previous mention.
	  /// </summary>
	  /// <param name="mention"> The mention under consideration. </param>
	  /// <returns> A probability that the specified mention doesn't refer to any previous mention. </returns>
	  double getNonReferentialProbability(MentionContext mention);

	  /// <summary>
	  /// Designates that the specified mention be used for training.
	  /// </summary>
	  /// <param name="mention"> The mention to be used.  The mention id is used to determine
	  /// whether this mention is referential or non-referential. </param>
	  void addEvent(MentionContext mention);

	  /// <summary>
	  /// Trains a model based on the events given to this resolver via #addEvent.
	  /// </summary>
	  /// <exception cref="IOException"> When the model can not be written out. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void train() throws java.io.IOException;
	  void train();
	}

}