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

namespace opennlp.tools.parser
{


	using ChunkerModel = opennlp.tools.chunker.ChunkerModel;
	using Parser = opennlp.tools.parser.chunking.Parser;
	using opennlp.tools.util;

	public class ParserChunkerSequenceValidator : SequenceValidator<string>
	{

	  private IDictionary<string, string> continueStartMap;

	  public ParserChunkerSequenceValidator(ChunkerModel model)
	  {

		continueStartMap = new Dictionary<string, string>(model.getChunkerModel().NumOutcomes);
		for (int oi = 0, on = model.getChunkerModel().NumOutcomes; oi < on; oi++)
		{
		  string outcome = model.getChunkerModel().getOutcome(oi);
		  if (outcome.StartsWith(Parser.CONT, StringComparison.Ordinal))
		  {
			continueStartMap[outcome] = Parser.START + outcome.Substring(Parser.CONT.Length);
		  }
		}
	  }

	  public virtual bool validSequence(int i, string[] inputSequence, string[] tagList, string outcome)
	  {
		if (continueStartMap.ContainsKey(outcome))
		{
		  int lti = tagList.Length - 1;

		  if (lti == -1)
		  {
			return false;
		  }
		  else
		  {
			string lastTag = tagList[lti];

			if (lastTag.Equals(outcome))
			{
			   return true;
			}

			if (lastTag.Equals(continueStartMap[outcome]))
			{
			  return true;
			}

			if (lastTag.Equals(Parser.OTHER))
			{
			  return false;
			}
			return false;
		  }
		}
		return true;
	  }
	}
}