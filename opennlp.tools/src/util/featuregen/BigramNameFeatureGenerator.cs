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

	public class BigramNameFeatureGenerator : FeatureGeneratorAdapter
	{

	  public override void createFeatures(IList<string> features, string[] tokens, int index, string[] previousOutcomes)
	  {
		string wc = FeatureGeneratorUtil.tokenFeature(tokens[index]);
		//bi-gram features 
		if (index > 0)
		{
		  features.Add("pw,w=" + tokens[index - 1] + "," + tokens[index]);
		  string pwc = FeatureGeneratorUtil.tokenFeature(tokens[index - 1]);
		  features.Add("pwc,wc=" + pwc + "," + wc);
		}
		if (index + 1 < tokens.Length)
		{
		  features.Add("w,nw=" + tokens[index] + "," + tokens[index + 1]);
		  string nwc = FeatureGeneratorUtil.tokenFeature(tokens[index + 1]);
		  features.Add("wc,nc=" + wc + "," + nwc);
		}
	  }
	}
}