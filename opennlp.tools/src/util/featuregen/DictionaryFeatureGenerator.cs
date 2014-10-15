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

	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using DictionaryNameFinder = opennlp.tools.namefind.DictionaryNameFinder;

	/// <summary>
	/// The <seealso cref="DictionaryFeatureGenerator"/> uses the <seealso cref="DictionaryNameFinder"/>
	/// to generated features for detected names based on the <seealso cref="InSpanGenerator"/>.
	/// </summary>
	/// <seealso cref= Dictionary </seealso>
	/// <seealso cref= DictionaryNameFinder </seealso>
	/// <seealso cref= InSpanGenerator </seealso>
	public class DictionaryFeatureGenerator : FeatureGeneratorAdapter
	{

	  private InSpanGenerator isg;

	  public DictionaryFeatureGenerator(Dictionary dict) : this("",dict)
	  {
	  }
	  public DictionaryFeatureGenerator(string prefix, Dictionary dict)
	  {
		setDictionary(prefix,dict);
	  }

	  public virtual Dictionary Dictionary
	  {
		  set
		  {
			setDictionary("",value);
		  }
	  }

	  public virtual void setDictionary(string name, Dictionary dict)
	  {
		isg = new InSpanGenerator(name, new DictionaryNameFinder(dict));
	  }

	  public override void createFeatures(List<string> features, string[] tokens, int index, string[] previousOutcomes)
	  {
		isg.createFeatures(features, tokens, index, previousOutcomes);
	  }

	}

}