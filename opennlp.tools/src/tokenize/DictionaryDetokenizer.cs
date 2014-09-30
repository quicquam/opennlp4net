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
using j4n.Exceptions;

namespace opennlp.tools.tokenize
{


	/// <summary>
	/// A rule based detokenizer. Simple rules which indicate in which direction a token should be
	/// moved are looked up in a <seealso cref="DetokenizationDictionary"/> object.
	/// </summary>
	/// <seealso cref= Detokenizer </seealso>
	/// <seealso cref= DetokenizationDictionary </seealso>
	public class DictionaryDetokenizer : Detokenizer
	{

	  private readonly DetokenizationDictionary dict;

	  public DictionaryDetokenizer(DetokenizationDictionary dict)
	  {
		this.dict = dict;
	  }

	  public virtual Detokenizer_DetokenizationOperation[] detokenize(string[] tokens)
	  {

		Detokenizer_DetokenizationOperation[] operations = new Detokenizer_DetokenizationOperation[tokens.Length];

		HashSet<string> matchingTokens = new HashSet<string>();

		for (int i = 0; i < tokens.Length; i++)
		{
		  DetokenizationDictionary.Operation dictOperation = dict.getOperation(tokens[i]);

		  if (dictOperation == null)
		  {
			operations[i] = Detokenizer_DetokenizationOperation.NO_OPERATION;
		  }
		  else if (DetokenizationDictionary.Operation.MOVE_LEFT.Equals(dictOperation))
		  {
			operations[i] = Detokenizer_DetokenizationOperation.MERGE_TO_LEFT;
		  }
		  else if (DetokenizationDictionary.Operation.MOVE_RIGHT.Equals(dictOperation))
		  {
			operations[i] = Detokenizer_DetokenizationOperation.MERGE_TO_RIGHT;
		  }
		  else if (DetokenizationDictionary.Operation.MOVE_BOTH.Equals(dictOperation))
		  {
			operations[i] = Detokenizer_DetokenizationOperation.MERGE_BOTH;
		  }
		  else if (DetokenizationDictionary.Operation.RIGHT_LEFT_MATCHING.Equals(dictOperation))
		  {

			if (matchingTokens.Contains(tokens[i]))
			{
			  // The token already occurred once, move it to the left
			  // and clear the occurrence flag
			  operations[i] = Detokenizer_DetokenizationOperation.MERGE_TO_LEFT;
			  matchingTokens.Remove(tokens[i]);
			}
			else
			{
			  // First time this token is seen, move it to the right
			  // and remember it
			  operations[i] = Detokenizer_DetokenizationOperation.MERGE_TO_RIGHT;
			  matchingTokens.Add(tokens[i]);
			}
		  }
		  else
		  {
			throw new IllegalStateException("Unknown operation: " + dictOperation);
		  }
		}

		return operations;
	  }

	  public virtual string detokenize(string[] tokens, string splitMarker)
	  {

		Detokenizer_DetokenizationOperation[] operations = detokenize(tokens);

		if (tokens.Length != operations.Length)
		{
		  throw new System.ArgumentException("tokens and operations array must have same length: tokens=" + tokens.Length + ", operations=" + operations.Length + "!");
		}


		StringBuilder untokenizedString = new StringBuilder();

		for (int i = 0; i < tokens.Length; i++)
		{

		  // attach token to string buffer
		  untokenizedString.Append(tokens[i]);

		  bool isAppendSpace;
		  bool isAppendSplitMarker;

		  // if this token is the last token do not attach a space
		  if (i + 1 == operations.Length)
		  {
			isAppendSpace = false;
			isAppendSplitMarker = false;
		  }
		  // if next token move left, no space after this token,
		  // its safe to access next token
		  else if (operations[i + 1].Equals(Detokenizer_DetokenizationOperation.MERGE_TO_LEFT) || operations[i + 1].Equals(Detokenizer_DetokenizationOperation.MERGE_BOTH))
		  {
			isAppendSpace = false;
			isAppendSplitMarker = true;
		  }
		  // if this token is move right, no space 
		  else if (operations[i].Equals(Detokenizer_DetokenizationOperation.MERGE_TO_RIGHT) || operations[i].Equals(Detokenizer_DetokenizationOperation.MERGE_BOTH))
		  {
			isAppendSpace = false;
			isAppendSplitMarker = true;
		  }
		  else
		  {
			isAppendSpace = true;
			isAppendSplitMarker = false;
		  }

		  if (isAppendSpace)
		  {
			untokenizedString.Append(' ');
		  }

		  if (isAppendSplitMarker && splitMarker != null)
		  {
			untokenizedString.Append(splitMarker);
		  }
		}

		return untokenizedString.ToString();
	  }
	}

}