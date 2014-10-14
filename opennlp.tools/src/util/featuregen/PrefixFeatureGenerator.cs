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

namespace opennlp.tools.util.featuregen
{

	public class PrefixFeatureGenerator : FeatureGeneratorAdapter
	{

	  private const int PREFIX_LENGTH = 4;

	  public static string[] getPrefixes(string lex)
	  {
		string[] prefs = new string[PREFIX_LENGTH];
		for (int li = 0, ll = PREFIX_LENGTH; li < ll; li++)
		{
		  prefs[li] = lex.Substring(0, Math.Min(li + 1, lex.Length));
		}
		return prefs;
	  }

	  public override void createFeatures(IList<string> features, string[] tokens, int index, string[] previousOutcomes)
	  {
		string[] prefs = PrefixFeatureGenerator.getPrefixes(tokens[index]);
		foreach (string pref in prefs)
		{
		  features.Add("pre=" + pref);
		}
	  }
	}

}