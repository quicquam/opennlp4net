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
using System.Linq;
using j4n.Lang;
using opennlp.tools.util;

namespace opennlp.tools.namefind
{


	using SequenceCodec = opennlp.tools.util.SequenceCodec<string>;
	using Span = opennlp.tools.util.Span;

	public class BioCodec : SequenceCodec
	{

	  public const string START = "start";
	  public const string CONTINUE = "cont";
	  public const string OTHER = "other";

	  private static readonly Pattern typedOutcomePattern = Pattern.compile("(.+)-\\w+");

	  internal static string extractNameType(string outcome)
	  {
		Matcher matcher = typedOutcomePattern.matcher(outcome);
		if (matcher.matches())
		{
		  string nameType = matcher.group(1);
		  return nameType;
		}

		return null;
	  }

	  public virtual Span[] decode(IList<string> c)
	  {
		int start = -1;
		int end = -1;
		IList<Span> spans = new List<Span>(c.Count);
		for (int li = 0; li < c.Count; li++)
		{
		  string chunkTag = c[li];
		  if (chunkTag.EndsWith(BioCodec.START, StringComparison.Ordinal))
		  {
			if (start != -1)
			{
			  spans.Add(new Span(start, end, extractNameType(c[li - 1])));
			}

			start = li;
			end = li + 1;

		  }
		  else if (chunkTag.EndsWith(BioCodec.CONTINUE, StringComparison.Ordinal))
		  {
			end = li + 1;
		  }
		  else if (chunkTag.EndsWith(BioCodec.OTHER, StringComparison.Ordinal))
		  {
			if (start != -1)
			{
			  spans.Add(new Span(start, end, extractNameType(c[li - 1])));
			  start = -1;
			  end = -1;
			}
		  }
		}

		if (start != -1)
		{
		  spans.Add(new Span(start, end, extractNameType(c[c.Count - 1])));
		}

		return spans.ToArray();
	  }

	  public virtual string[] encode(Span[] names, int length)
	  {
		string[] outcomes = new string[length];
		for (int i = 0; i < outcomes.Length; i++)
		{
		  outcomes[i] = BioCodec.OTHER;
		}
		foreach (Span name in names)
		{
		  if (name.Type == null)
		  {
			outcomes[name.Start] = "default" + "-" + BioCodec.START;
		  }
		  else
		  {
			outcomes[name.Start] = name.Type + "-" + BioCodec.START;
		  }
		  // now iterate from begin + 1 till end
		  for (int i = name.Start + 1; i < name.End; i++)
		  {
			if (name.Type == null)
			{
			  outcomes[i] = "default" + "-" + BioCodec.CONTINUE;
			}
			else
			{
			  outcomes[i] = name.Type + "-" + BioCodec.CONTINUE;
			}
		  }
		}

		return outcomes;
	  }

	  public virtual SequenceValidator<string> createSequenceValidator()
	  {
		return new NameFinderSequenceValidator();
	  }

	  public bool areOutcomesCompatible(string[] outcomes)
	  {
		// We should have *optionally* one outcome named "other", some named xyz-start and sometimes
		// they have a pair xyz-cont. We should not have any other outcome
		// To validate the model we check if we have one outcome named "other", at least
		// one outcome with suffix start. After that we check if all outcomes that ends with
		// "cont" have a pair that ends with "start".
		IList<string> start = new List<string>();
		IList<string> cont = new List<string>();

		for (int i = 0; i < outcomes.Length; i++)
		{
		  string outcome = outcomes[i];
		  if (outcome.EndsWith(NameFinderME.START, StringComparison.Ordinal))
		  {
			start.Add(outcome.Substring(0, outcome.Length - NameFinderME.START.Length));
		  }
		  else if (outcome.EndsWith(NameFinderME.CONTINUE, StringComparison.Ordinal))
		  {
			cont.Add(outcome.Substring(0, outcome.Length - NameFinderME.CONTINUE.Length));
		  }
		  else if (outcome.Equals(NameFinderME.OTHER))
		  {
			// don't fail anymore if couldn't find outcome named OTHER
		  }
		  else
		  {
			// got unexpected outcome
			return false;
		  }
		}

		if (start.Count == 0)
		{
		  return false;
		}
		else
		{
		  foreach (string contPreffix in cont)
		  {
			if (!start.Contains(contPreffix))
			{
			  return false;
			}
		  }
		}

		return true;
	  }
	}

}