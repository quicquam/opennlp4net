﻿using System;

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

namespace opennlp.tools.chunker
{

	using opennlp.tools.util;

	public class DefaultChunkerSequenceValidator : SequenceValidator<string>
	{

	  private bool validOutcome(string outcome, string prevOutcome)
	  {
		if (outcome.StartsWith("I-", StringComparison.Ordinal))
		{
		  if (prevOutcome == null)
		  {
			return (false);
		  }
		  else
		  {
			if (prevOutcome.Equals("O"))
			{
			  return (false);
			}
			if (!prevOutcome.Substring(2).Equals(outcome.Substring(2)))
			{
			  return (false);
			}
		  }
		}
		return true;
	  }

	  protected internal virtual bool validOutcome(string outcome, string[] sequence)
	  {
		string prevOutcome = null;
		if (sequence.Length > 0)
		{
		  prevOutcome = sequence[sequence.Length - 1];
		}
		return validOutcome(outcome,prevOutcome);
	  }

	  public virtual bool validSequence(int i, string[] sequence, string[] s, string outcome)
	  {
		return validOutcome(outcome, s);
	  }

	}

}