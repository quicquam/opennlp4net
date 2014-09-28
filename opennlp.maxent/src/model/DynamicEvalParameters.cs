using System.Collections.Generic;

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

	public class DynamicEvalParameters
	{

	  /// <summary>
	  /// Mapping between outcomes and paramater values for each context. 
	  /// The integer representation of the context can be found using <code>pmap</code>.
	  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: private java.util.List<? extends Context> params;
	  private IList<?> @params;

	  /// <summary>
	  /// The number of outcomes being predicted. </summary>
	  private readonly int numOutcomes;


	  /// <summary>
	  /// Creates a set of paramters which can be evaulated with the eval method. </summary>
	  /// <param name="params"> The parameters of the model. </param>
	  /// <param name="numOutcomes"> The number of outcomes. </param>
	  public DynamicEvalParameters<T1>(IList<T1> @params, int numOutcomes) where T1 : Context
	  {
		this.@params = @params;
		this.numOutcomes = numOutcomes;
	  }

	  public virtual Context[] Params
	  {
		  get
		  {
			return @params.ToArray();
		  }
	  }

	  public virtual int NumOutcomes
	  {
		  get
		  {
			return numOutcomes;
		  }
	  }

	}

}