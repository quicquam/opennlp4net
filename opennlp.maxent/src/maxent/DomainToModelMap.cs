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
using j4n.Exceptions;

namespace opennlp.maxent
{


	using MaxentModel = opennlp.model.MaxentModel;

	/// <summary>
	/// A class which stores a mapping from ModelDomain objects to MaxentModels.
	/// This permits an application to replace an old model for a domain with a
	/// newly trained one in a thread-safe manner.  By calling the getModel()
	/// method, the application can create new instances of classes which use the
	/// relevant models.
	/// </summary>
	public class DomainToModelMap
	{

	  // the underlying object which stores the mapping
	  private IDictionary<ModelDomain, MaxentModel> map = new Dictionary<ModelDomain, MaxentModel>();

	  /// <summary>
	  /// Sets the model for the given domain.
	  /// </summary>
	  /// <param name="domain">
	  ///          The ModelDomain object which keys to the model. </param>
	  /// <param name="model">
	  ///          The MaxentModel trained for the domain. </param>
	  public virtual void setModelForDomain(ModelDomain domain, MaxentModel model)
	  {
		map[domain] = model;
	  }

	  /// <summary>
	  /// Get the model mapped to by the given ModelDomain key.
	  /// </summary>
	  /// <param name="domain">
	  ///          The ModelDomain object which keys to the desired model. </param>
	  /// <returns> The MaxentModel corresponding to the given domain. </returns>
	  public virtual MaxentModel getModel(ModelDomain domain)
	  {
		if (map.ContainsKey(domain))
		{
		  return map[domain];
		}
		else
		{
		  throw new NoSuchElementException("No model has been created for " + "domain: " + domain);
		}
	  }

	  /// <summary>
	  /// Removes the mapping for this ModelDomain key from this map if present.
	  /// </summary>
	  /// <param name="domain">
	  ///          The ModelDomain key whose mapping is to be removed from the map. </param>
	  public virtual void removeDomain(ModelDomain domain)
	  {
		map.Remove(domain);
	  }
    }

}