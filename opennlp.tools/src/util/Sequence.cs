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

namespace opennlp.tools.util
{


	/// <summary>
	/// Represents a weighted sequence of outcomes. </summary>
	public class Sequence : IComparable<Sequence>
	{
	  private double score;
	  private List<string> outcomes;
	  private List<double?> probs;
	  private double? ONE = 1.0d;

	  /// <summary>
	  /// Creates a new sequence of outcomes. </summary>
	  public Sequence()
	  {
		outcomes = new List<string>(1);
		probs = new List<double?>(1);
		score = 0d;
	  }

	  public Sequence(Sequence s)
	  {
		outcomes = new List<string>(s.outcomes.Count + 1);
		outcomes.AddRange(s.outcomes);
		probs = new List<double?>(s.probs.Count + 1);
		probs.AddRange(s.probs);
		score = s.score;
	  }

	  public Sequence(Sequence s, string outcome, double p)
	  {
		  outcomes = new List<string>(s.outcomes.Count + 1);
		  outcomes.AddRange(s.outcomes);
		  outcomes.Add(outcome);
		  probs = new List<double?>(s.probs.Count + 1);
		  probs.AddRange(s.probs);
		  probs.Add(p);
		  score = s.score + Math.Log(p);
	  }

	  public Sequence(List<string> outcomes)
	  {
		this.outcomes = outcomes;
		this.probs = new List<double?>();
	      for (var i = 0; i < outcomes.Count; i++)
	      {
              probs.Add(ONE);
	      }
	  }

	  public virtual int CompareTo(Sequence s)
	  {
		if (score < s.score)
		{
		  return 1;
		}
		if (score > s.score)
		{
		  return -1;
		}
		return 0;
	  }

	  /// <summary>
	  /// Adds an outcome and probability to this sequence. </summary>
	  /// <param name="outcome"> the outcome to be added. </param>
	  /// <param name="p"> the probability associated with this outcome. </param>
	  public virtual void add(string outcome, double p)
	  {
		outcomes.Add(outcome);
		probs.Add(p);
		score += Math.Log(p);
	  }

	  /// <summary>
	  /// Returns a list of outcomes for this sequence. </summary>
	  /// <returns> a list of outcomes. </returns>
	  public virtual IList<string> Outcomes
	  {
		  get
		  {
			return outcomes;
		  }
	  }

	  /// <summary>
	  /// Returns an array of probabilities associated with the outcomes of this sequence. </summary>
	  /// <returns> an array of probabilities. </returns>
	  public virtual double[] Probs
	  {
		  get
		  {
			double[] ps = new double[probs.Count];
			getProbs(ps);
			return ps;
		  }
	  }

	  /// <summary>
	  /// Returns the score of this sequence. </summary>
	  /// <returns> The score of this sequence. </returns>
	  public virtual double Score
	  {
		  get
		  {
			return score;
		  }
	  }

	  /// <summary>
	  /// Populates  an array with the probabilities associated with the outcomes of this sequence. </summary>
	  /// <param name="ps"> a pre-allocated array to use to hold the values of the probabilities of the outcomes for this sequence. </param>
	  public virtual void getProbs(double[] ps)
	  {
		for (int pi = 0,pl = probs.Count;pi < pl;pi++)
		{
		  ps[pi] = probs[pi].GetValueOrDefault();
		}
	  }

	  public override string ToString()
	  {
		return score + " " + outcomes;
	  }
	}

}