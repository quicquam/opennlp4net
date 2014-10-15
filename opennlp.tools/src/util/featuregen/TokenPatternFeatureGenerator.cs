using System.Collections.Generic;
using System.Text;
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
using j4n.Lang;


namespace opennlp.tools.util.featuregen
{


	using SimpleTokenizer = opennlp.tools.tokenize.SimpleTokenizer;
	using Tokenizer = opennlp.tools.tokenize.Tokenizer;

	/// <summary>
	/// Partitions tokens into sub-tokens based on character classes and generates
	/// class features for each of the sub-tokens and combinations of those sub-tokens.
	/// </summary>
	public class TokenPatternFeatureGenerator : FeatureGeneratorAdapter
	{

		private Pattern noLetters = Pattern.compile("[^a-zA-Z]");
		private Tokenizer tokenizer;

		/// <summary>
		/// Initializes a new instance.
		/// For tokinization the <seealso cref="SimpleTokenizer"/> is used.
		/// </summary>
		public TokenPatternFeatureGenerator() : this(SimpleTokenizer.INSTANCE)
		{
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="supportTokenizer"> </param>
		public TokenPatternFeatureGenerator(Tokenizer supportTokenizer)
		{
			tokenizer = supportTokenizer;
		}

		public override void createFeatures(List<string> feats, string[] toks, int index, string[] preds)
		{

		  string[] tokenized = tokenizer.tokenize(toks[index]);

		  if (tokenized.Length == 1)
		  {
			feats.Add("st=" + toks[index].ToLower());
			return;
		  }

		  feats.Add("stn=" + tokenized.Length);

		  StringBuilder pattern = new StringBuilder();

		  for (int i = 0; i < tokenized.Length; i++)
		  {

			if (i < tokenized.Length - 1)
			{
			  feats.Add("pt2=" + FeatureGeneratorUtil.tokenFeature(tokenized[i]) + FeatureGeneratorUtil.tokenFeature(tokenized[i + 1]));
			}

			if (i < tokenized.Length - 2)
			{
			  feats.Add("pt3=" + FeatureGeneratorUtil.tokenFeature(tokenized[i]) + FeatureGeneratorUtil.tokenFeature(tokenized[i + 1]) + FeatureGeneratorUtil.tokenFeature(tokenized[i + 2]));
			}

			pattern.Append(FeatureGeneratorUtil.tokenFeature(tokenized[i]));

			if (!noLetters.matcher(tokenized[i]).find())
			{
			  feats.Add("st=" + tokenized[i].ToLower());
			}
		  }

		  feats.Add("pta=" + pattern.ToString());
		}
	}

}