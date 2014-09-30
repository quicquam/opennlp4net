using System.Collections;
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
using System.Linq;


namespace opennlp.tools.tokenize
{


	using StringUtil = opennlp.tools.util.StringUtil;

	/// <summary>
	/// Generate events for maxent decisions for tokenization.
	/// </summary>
	public class DefaultTokenContextGenerator : TokenContextGenerator
	{

	  protected internal readonly HashSet<string> inducedAbbreviations;

	  /// <summary>
	  /// Creates a default context generator for tokenizer.
	  /// </summary>
	  public DefaultTokenContextGenerator() : this(new HashSet<string>())
	  {
	  }

	  /// <summary>
	  /// Creates a default context generator for tokenizer.
	  /// </summary>
	  /// <param name="inducedAbbreviations"> the induced abbreviations </param>
	  public DefaultTokenContextGenerator(HashSet<string> inducedAbbreviations)
	  {
		this.inducedAbbreviations = inducedAbbreviations;
	  }

	  /* (non-Javadoc)
	   * @see opennlp.tools.tokenize.TokenContextGenerator#getContext(java.lang.String, int)
	   */
	  public virtual string[] getContext(string sentence, int index)
	  {
		IList<string> preds = createContext(sentence, index);
		return preds.ToArray();
	  }

	  /// <summary>
	  /// Returns an <seealso cref="ArrayList"/> of features for the specified sentence string
	  /// at the specified index. Extensions of this class can override this method
	  /// to create a customized <seealso cref="TokenContextGenerator"/>
	  /// </summary>
	  /// <param name="sentence">
	  ///          the token been analyzed </param>
	  /// <param name="index">
	  ///          the index of the character been analyzed </param>
	  /// <returns> an <seealso cref="ArrayList"/> of features for the specified sentence string
	  ///         at the specified index. </returns>
	  protected internal virtual IList<string> createContext(string sentence, int index)
	  {
		IList<string> preds = new List<string>();
		string prefix = sentence.Substring(0, index);
		string suffix = sentence.Substring(index);
		preds.Add("p=" + prefix);
		preds.Add("s=" + suffix);
		if (index > 0)
		{
		  addCharPreds("p1", sentence[index - 1], preds);
		  if (index > 1)
		  {
			addCharPreds("p2", sentence[index - 2], preds);
			preds.Add("p21=" + sentence[index - 2] + sentence[index - 1]);
		  }
		  else
		  {
			preds.Add("p2=bok");
		  }
		  preds.Add("p1f1=" + sentence[index - 1] + sentence[index]);
		}
		else
		{
		  preds.Add("p1=bok");
		}
		addCharPreds("f1", sentence[index], preds);
		if (index + 1 < sentence.Length)
		{
		  addCharPreds("f2", sentence[index + 1], preds);
		  preds.Add("f12=" + sentence[index] + sentence[index + 1]);
		}
		else
		{
		  preds.Add("f2=bok");
		}
		if (sentence[0] == '&' && sentence[sentence.Length - 1] == ';')
		{
		  preds.Add("cc"); //character code
		}

		if (index == sentence.Length - 1 && inducedAbbreviations.Contains(sentence))
		{
		  preds.Add("pabb");
		}

		return preds;
	  }


	  /// <summary>
	  /// Helper function for getContext.
	  /// </summary>
	  protected internal virtual void addCharPreds(string key, char c, IList<string> preds)
	  {
		preds.Add(key + "=" + c);
		if (char.IsLetter(c))
		{
		  preds.Add(key + "_alpha");
		  if (char.IsUpper(c))
		  {
			preds.Add(key + "_caps");
		  }
		}
		else if (char.IsDigit(c))
		{
		  preds.Add(key + "_num");
		}
		else if (StringUtil.isWhitespace(c))
		{
		  preds.Add(key + "_ws");
		}
		else
		{
		  if (c == '.' || c == '?' || c == '!')
		  {
			preds.Add(key + "_eos");
		  }
		  else if (c == '`' || c == '"' || c == '\'')
		  {
			preds.Add(key + "_quote");
		  }
		  else if (c == '[' || c == '{' || c == '(')
		  {
			preds.Add(key + "_lp");
		  }
		  else if (c == ']' || c == '}' || c == ')')
		  {
			preds.Add(key + "_rp");
		  }
		}
	  }
	}

}