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

namespace opennlp.tools.util
{


	using MaxentModel = opennlp.model.MaxentModel;

	/// <summary>
	/// Performs k-best search over sequence.  This is based on the description in
	/// Ratnaparkhi (1998), PhD diss, Univ. of Pennsylvania.
	/// </summary>
	/// <seealso cref= Sequence </seealso>
	/// <seealso cref= SequenceValidator </seealso>
	/// <seealso cref= BeamSearchContextGenerator </seealso>
	public class BeamSearch<T>
	{

	  private static readonly object[] EMPTY_ADDITIONAL_CONTEXT = new object[0];

	  protected internal int size;
	  protected internal BeamSearchContextGenerator<T> cg;
	  protected internal MaxentModel model;
	  private SequenceValidator<T> validator;

	  private double[] probs;
	  private Cache contextsCache;
	  private const int zeroLog = -100000;

	  /// <summary>
	  /// Creates new search object.
	  /// </summary>
	  /// <param name="size"> The size of the beam (k). </param>
	  /// <param name="cg"> the context generator for the model. </param>
	  /// <param name="model"> the model for assigning probabilities to the sequence outcomes. </param>
	  public BeamSearch(int size, BeamSearchContextGenerator<T> cg, MaxentModel model) : this(size, cg, model, null, 0)
	  {
	  }

	  public BeamSearch(int size, BeamSearchContextGenerator<T> cg, MaxentModel model, int cacheSize) : this(size, cg, model, null, cacheSize)
	  {
	  }

	  public BeamSearch(int size, BeamSearchContextGenerator<T> cg, MaxentModel model, SequenceValidator<T> validator, int cacheSize)
	  {

		this.size = size;
		this.cg = cg;
		this.model = model;
		this.validator = validator;

		if (cacheSize > 0)
		{
		  contextsCache = new Cache(cacheSize);
		}

		this.probs = new double[model.NumOutcomes];
	  }

	  /// <summary>
	  /// Note:
	  /// This method will be private in the future because clients can now
	  /// pass a validator to validate the sequence.
	  /// </summary>
	  /// <seealso cref= SequenceValidator </seealso>
	  private bool validSequence(int i, T[] inputSequence, string[] outcomesSequence, string outcome)
	  {

		if (validator != null)
		{
		  return validator.validSequence(i, inputSequence, outcomesSequence, outcome);
		}
		else
		{
		  return true;
		}
	  }

	  public virtual Sequence[] bestSequences(int numSequences, T[] sequence, object[] additionalContext)
	  {
		return bestSequences(numSequences, sequence, additionalContext, zeroLog);
	  }

	  /// <summary>
	  /// Returns the best sequence of outcomes based on model for this object.
	  /// </summary>
	  /// <param name="numSequences"> The maximum number of sequences to be returned. </param>
	  /// <param name="sequence"> The input sequence. </param>
	  /// <param name="additionalContext"> An Object[] of additional context.  This is passed to the context generator blindly with the assumption that the context are appropiate. </param>
	  /// <param name="minSequenceScore"> A lower bound on the score of a returned sequence. </param>
	  /// <returns> An array of the top ranked sequences of outcomes. </returns>
	  public virtual Sequence[] bestSequences(int numSequences, T[] sequence, object[] additionalContext, double minSequenceScore)
	  {

		Heap<Sequence> prev = new ListHeap<Sequence>(size);
		Heap<Sequence> next = new ListHeap<Sequence>(size);
		Heap<Sequence> tmp;
		prev.add(new Sequence());

		if (additionalContext == null)
		{
		  additionalContext = EMPTY_ADDITIONAL_CONTEXT;
		}

		for (int i = 0; i < sequence.Length; i++)
		{
		  int sz = Math.Min(size, prev.size());

		  for (int sc = 0; prev.size() > 0 && sc < sz; sc++)
		  {
			Sequence top = prev.extract();
			IList<string> tmpOutcomes = top.Outcomes;
			string[] outcomes = tmpOutcomes.ToArray();
			string[] contexts = cg.getContext(i, sequence, outcomes, additionalContext);
			double[] scores;
			if (contextsCache != null)
			{
			  scores = (double[]) contextsCache[contexts];
			  if (scores == null)
			  {
				scores = model.eval(contexts, probs);
				contextsCache[contexts] = scores;
			  }
			}
			else
			{
			  scores = model.eval(contexts, probs);
			}

			double[] temp_scores = new double[scores.Length];
			for (int c = 0; c < scores.Length; c++)
			{
			  temp_scores[c] = scores[c];
			}

			Array.Sort(temp_scores);

			double min = temp_scores[Math.Max(0,scores.Length - size)];

			for (int p = 0; p < scores.Length; p++)
			{
			  if (scores[p] < min)
			  {
				continue; //only advance first "size" outcomes
			  }
			  string @out = model.getOutcome(p);
			  if (validSequence(i, sequence, outcomes, @out))
			  {
				Sequence ns = new Sequence(top, @out, scores[p]);
				if (ns.Score > minSequenceScore)
				{
				  next.add(ns);
				}
			  }
			}

			if (next.size() == 0) //if no advanced sequences, advance all valid
			{
			  for (int p = 0; p < scores.Length; p++)
			  {
				string @out = model.getOutcome(p);
				if (validSequence(i, sequence, outcomes, @out))
				{
				  Sequence ns = new Sequence(top, @out, scores[p]);
				  if (ns.Score > minSequenceScore)
				  {
					next.add(ns);
				  }
				}
			  }
			}
		  }

		  //    make prev = next; and re-init next (we reuse existing prev set once we clear it)
		  prev.clear();
		  tmp = prev;
		  prev = next;
		  next = tmp;
		}

		int numSeq = Math.Min(numSequences, prev.size());
		Sequence[] topSequences = new Sequence[numSeq];

		for (int seqIndex = 0; seqIndex < numSeq; seqIndex++)
		{
		  topSequences[seqIndex] = prev.extract();
		}

		return topSequences;
	  }

	  /// <summary>
	  /// Returns the best sequence of outcomes based on model for this object.
	  /// </summary>
	  /// <param name="sequence"> The input sequence. </param>
	  /// <param name="additionalContext"> An Object[] of additional context.  This is passed to the context generator blindly with the assumption that the context are appropiate.
	  /// </param>
	  /// <returns> The top ranked sequence of outcomes or null if no sequence could be found </returns>
	  public virtual Sequence bestSequence(T[] sequence, object[] additionalContext)
	  {
		Sequence[] sequences = bestSequences(1, sequence, additionalContext,zeroLog);

		if (sequences.Length > 0)
		{
		  return sequences[0];
		}
		else
		{
		  return null;
		}
	  }
	}

}