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
using j4n.IO.File;

namespace opennlp.tools.cmdline
{


	using Span = opennlp.tools.util.Span;
	using opennlp.tools.util.eval;

	/// <summary>
	/// This listener will gather detailed information about the sample under evaluation and will
	/// allow detailed FMeasure for each outcome.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public abstract class DetailedFMeasureListener<T> : EvaluationMonitor<T>
	{
		private bool InstanceFieldsInitialized = false;

		public DetailedFMeasureListener()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			generalStats = new Stats(this);
		}


	  private int samples = 0;
	  private Stats generalStats;
	  private IDictionary<string, Stats> statsForOutcome = new Dictionary<string, Stats>();

	  protected internal abstract Span[] asSpanArray(T sample);

	  public virtual void correctlyClassified(T reference, T prediction)
	  {
		samples++;
		// add all true positives!
		Span[] spans = asSpanArray(reference);
		foreach (Span span in spans)
		{
		  addTruePositive(span.Type);
		}
	  }

	  public virtual void missclassified(T reference, T prediction)
	  {
		samples++;
		Span[] references = asSpanArray(reference);
		Span[] predictions = asSpanArray(prediction);

		HashSet<Span> refSet = new HashSet<Span>(Arrays.asList(references));
		HashSet<Span> predSet = new HashSet<Span>(Arrays.asList(predictions));

		foreach (Span @ref in refSet)
		{
		  if (predSet.Contains(@ref))
		  {
			addTruePositive(@ref.Type);
		  }
		  else
		  {
			addFalseNegative(@ref.Type);
		  }
		}

		foreach (Span pred in predSet)
		{
		  if (!refSet.Contains(pred))
		  {
			addFalsePositive(pred.Type);
		  }
		}
	  }

	  private void addTruePositive(string type)
	  {
		Stats s = initStatsForOutcomeAndGet(type);
		s.incrementTruePositive();
		s.incrementTarget();

		generalStats.incrementTruePositive();
		generalStats.incrementTarget();
	  }

	  private void addFalsePositive(string type)
	  {
		Stats s = initStatsForOutcomeAndGet(type);
		s.incrementFalsePositive();
		generalStats.incrementFalsePositive();
	  }

	  private void addFalseNegative(string type)
	  {
		Stats s = initStatsForOutcomeAndGet(type);
		s.incrementTarget();
		generalStats.incrementTarget();

	  }

	  private Stats initStatsForOutcomeAndGet(string type)
	  {
		if (!statsForOutcome.ContainsKey(type))
		{
		  statsForOutcome[type] = new Stats(this);
		}
		return statsForOutcome[type];
	  }

	  private const string PERCENT = "%\u00207.2f%%";
	  private static readonly string FORMAT = "%12s: precision: " + PERCENT + ";  recall: " + PERCENT + "; F1: " + PERCENT + ".";
	  private static readonly string FORMAT_EXTRA = FORMAT + " [target: %3d; tp: %3d; fp: %3d]";

	  public virtual string createReport()
	  {
		return createReport(Locale.Default);
	  }

	  public virtual string createReport(Locale locale)
	  {
		StringBuilder ret = new StringBuilder();
		int tp = generalStats.TruePositives;
		int found = generalStats.FalsePositives + tp;
		ret.Append("Evaluated " + samples + " samples with " + generalStats.Target + " entities; found: " + found + " entities; correct: " + tp + ".\n");

		ret.Append(string.format(locale, FORMAT, "TOTAL", zeroOrPositive(generalStats.PrecisionScore * 100), zeroOrPositive(generalStats.RecallScore * 100), zeroOrPositive(generalStats.FMeasure * 100)));
		ret.Append("\n");
		SortedSet<string> set = new SortedSet<string>(new F1Comparator(this));
		set.addAll(statsForOutcome.Keys);
		foreach (string type in set)
		{

		  ret.Append(string.format(locale, FORMAT_EXTRA, type, zeroOrPositive(statsForOutcome[type].PrecisionScore * 100), zeroOrPositive(statsForOutcome[type].RecallScore * 100), zeroOrPositive(statsForOutcome[type].FMeasure * 100), statsForOutcome[type].Target, statsForOutcome[type].TruePositives, statsForOutcome[type].FalsePositives));
		  ret.Append("\n");
		}

		return ret.ToString();
	  }

	  public override string ToString()
	  {
		return createReport();
	  }

	  private double zeroOrPositive(double v)
	  {
		if (v < 0)
		{
		  return 0;
		}
		return v;
	  }

	  private class F1Comparator : IComparer<string>
	  {
		  private readonly DetailedFMeasureListener<T> outerInstance;

		  public F1Comparator(DetailedFMeasureListener<T> outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		public virtual int Compare(string o1, string o2)
		{
		  if (o1.Equals(o2))
		  {
			return 0;
		  }
		  double t1 = 0;
		  double t2 = 0;

		  if (outerInstance.statsForOutcome.ContainsKey(o1))
		  {
			t1 += outerInstance.statsForOutcome[o1].FMeasure;
		  }
		  if (outerInstance.statsForOutcome.ContainsKey(o2))
		  {
			t2 += outerInstance.statsForOutcome[o2].FMeasure;
		  }

		  t1 = outerInstance.zeroOrPositive(t1);
		  t2 = outerInstance.zeroOrPositive(t2);

		  if (t1 + t2 > 0d)
		  {
			if (t1 > t2)
			{
			  return -1;
			}
			return 1;
		  }
		  return o1.CompareTo(o2);
		}

	  }

	  /// <summary>
	  /// Store the statistics.
	  /// </summary>
	  private class Stats
	  {
		  private readonly DetailedFMeasureListener<T> outerInstance;

		  public Stats(DetailedFMeasureListener<T> outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }


		// maybe we could use FMeasure class, but it wouldn't allow us to get
		// details like total number of false positives and true positives.

		internal int falsePositiveCounter = 0;
		internal int truePositiveCounter = 0;
		internal int targetCounter = 0;

		public virtual void incrementFalsePositive()
		{
		  falsePositiveCounter++;
		}

		public virtual void incrementTruePositive()
		{
		  truePositiveCounter++;
		}

		public virtual void incrementTarget()
		{
		  targetCounter++;
		}

		public virtual int FalsePositives
		{
			get
			{
			  return falsePositiveCounter;
			}
		}

		public virtual int TruePositives
		{
			get
			{
			  return truePositiveCounter;
			}
		}

		public virtual int Target
		{
			get
			{
			  return targetCounter;
			}
		}

		/// <summary>
		/// Retrieves the arithmetic mean of the precision scores calculated for each
		/// evaluated sample.
		/// </summary>
		/// <returns> the arithmetic mean of all precision scores </returns>
		public virtual double PrecisionScore
		{
			get
			{
			  int tp = TruePositives;
			  int selected = tp + FalsePositives;
			  return selected > 0 ? (double) tp / (double) selected : 0;
			}
		}

		/// <summary>
		/// Retrieves the arithmetic mean of the recall score calculated for each
		/// evaluated sample.
		/// </summary>
		/// <returns> the arithmetic mean of all recall scores </returns>
		public virtual double RecallScore
		{
			get
			{
			  int target = Target;
			  int tp = TruePositives;
			  return target > 0 ? (double) tp / (double) target : 0;
			}
		}

		/// <summary>
		/// Retrieves the f-measure score.
		/// 
		/// f-measure = 2 * precision * recall / (precision + recall)
		/// </summary>
		/// <returns> the f-measure or -1 if precision + recall <= 0 </returns>
		public virtual double FMeasure
		{
			get
			{
    
			  if (PrecisionScore + RecallScore > 0)
			  {
				return 2 * (PrecisionScore * RecallScore) / (PrecisionScore + RecallScore);
			  }
			  else
			  {
				// cannot divide by zero, return error code
				return -1;
			  }
			}
		}

	  }

	}

}