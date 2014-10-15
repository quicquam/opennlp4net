﻿using System.Collections.Generic;

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

namespace opennlp.tools.util.featuregen
{


	/// <summary>
	/// The <seealso cref="AdditionalContextFeatureGenerator"/> generates the context from the passed
	/// in additional context.
	/// </summary>
	public class AdditionalContextFeatureGenerator : FeatureGeneratorAdapter
	{

	  private string[][] additionalContext;

	//  public AdditionalContextFeatureGenerator() {
	//  }

	  public override void createFeatures(List<string> features, string[] tokens, int index, string[] preds)
	  {

		if (additionalContext != null && additionalContext.Length != 0)
		{

		  string[] context = additionalContext[index];

		  foreach (string s in context)
		  {
			features.Add("ne=" + s);
		  }
		}
	  }

	  public virtual string[][] CurrentContext
	  {
		  set
		  {
			additionalContext = value;
		  }
	  }
	}

}