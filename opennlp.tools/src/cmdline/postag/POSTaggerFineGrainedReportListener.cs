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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using j4n.IO.OutputStream;
using j4n.Utils;
using opennlp.tools.nonjava.helperclasses;
using opennlp.tools.postag;
using opennlp.tools.util;
using opennlp.tools.util.eval;

namespace opennlp.tools.cmdline.postag
{
    /// <summary>
	/// Generates a detailed report for the POS Tagger.
	/// <para>
	/// It is possible to use it from an API and access the statistics using the
	/// provided getters
	/// 
	/// </para>
	/// </summary>
	public class POSTaggerFineGrainedReportListener : POSTaggerEvaluationMonitor
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			stats = new Stats(this);
		}


	  private readonly PrintStream printStream;
	  private Stats stats;

	  private static readonly char[] alpha = new char[] {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};

	  /// <summary>
	  /// Creates a listener that will print to <seealso cref="System#err"/>
	  /// </summary>
	  public POSTaggerFineGrainedReportListener() : this(new OutputStream(Console.OpenStandardError()))
	  {
		  if (!InstanceFieldsInitialized)
		  {
			  InitializeInstanceFields();
			  InstanceFieldsInitialized = true;
		  }
	  }

	  /// <summary>
	  /// Creates a listener that prints to a given <seealso cref="OutputStream"/>
	  /// </summary>
	  public POSTaggerFineGrainedReportListener(OutputStream outputStream)
	  {
		  if (!InstanceFieldsInitialized)
		  {
			  InitializeInstanceFields();
			  InstanceFieldsInitialized = true;
		  }
		this.printStream = new PrintStream(outputStream);
	  }

	  // methods inherited from EvaluationMonitor

	  public virtual void missclassified(POSSample reference, POSSample prediction)
	  {
		stats.add(reference, prediction);
	  }

	  public virtual void correctlyClassified(POSSample reference, POSSample prediction)
	  {
		stats.add(reference, prediction);
	  }

	  /// <summary>
	  /// Writes the report to the <seealso cref="OutputStream"/>. Should be called only after
	  /// the evaluation process
	  /// </summary>
	  public virtual void writeReport()
	  {
		printGeneralStatistics();
		// token stats
		printTokenErrorRank();
		printTokenOcurrenciesRank();
		// tag stats
		printTagsErrorRank();
		// confusion tables
		printGeneralConfusionTable();
		printDetailedConfusionMatrix();
	  }

	  // api methods
	  // general stats

	  public virtual long NumberOfSentences
	  {
		  get
		  {
			return stats.NumberOfSentences;
		  }
	  }

	  public virtual double AverageSentenceSize
	  {
		  get
		  {
			return stats.AverageSentenceSize;
		  }
	  }

	  public virtual int MinSentenceSize
	  {
		  get
		  {
			return stats.MinSentenceSize;
		  }
	  }

	  public virtual int MaxSentenceSize
	  {
		  get
		  {
			return stats.MaxSentenceSize;
		  }
	  }

	  public virtual int NumberOfTags
	  {
		  get
		  {
			return stats.NumberOfTags;
		  }
	  }

	  public virtual double Accuracy
	  {
		  get
		  {
			return stats.Accuracy;
		  }
	  }

	  // token stats

	  public virtual double getTokenAccuracy(string token)
	  {
		return stats.getTokenAccuracy(token);
	  }

	  public virtual SortedSet<string> TokensOrderedByFrequency
	  {
		  get
		  {
			return stats.TokensOrderedByFrequency;
		  }
	  }

	  public virtual int getTokenFrequency(string token)
	  {
		return stats.getTokenFrequency(token);
	  }

	  public virtual int getTokenErrors(string token)
	  {
		return stats.getTokenErrors(token);
	  }

	  public virtual SortedSet<string> TokensOrderedByNumberOfErrors
	  {
		  get
		  {
			return stats.TokensOrderedByNumberOfErrors;
		  }
	  }

	  public virtual SortedSet<string> TagsOrderedByErrors
	  {
		  get
		  {
			return stats.TagsOrderedByErrors;
		  }
	  }

	  public virtual int getTagFrequency(string tag)
	  {
		return stats.getTagFrequency(tag);
	  }

	  public virtual int getTagErrors(string tag)
	  {
		return stats.getTagErrors(tag);
	  }

	  public virtual double getTagPrecision(string tag)
	  {
		return stats.getTagPrecision(tag);
	  }

	  public virtual double getTagRecall(string tag)
	  {
		return stats.getTagRecall(tag);
	  }

	  public virtual double getTagFMeasure(string tag)
	  {
		return stats.getTagFMeasure(tag);
	  }

	  public virtual SortedSet<string> ConfusionMatrixTagset
	  {
		  get
		  {
			return stats.ConfusionMatrixTagset;
		  }
	  }

	  public virtual SortedSet<string> getConfusionMatrixTagset(string token)
	  {
		return stats.getConfusionMatrixTagset(token);
	  }

	  public virtual double[][] ConfusionMatrix
	  {
		  get
		  {
			return stats.ConfusionMatrix;
		  }
	  }

	  public virtual double[][] getConfusionMatrix(string token)
	  {
		return stats.getConfusionMatrix(token);
	  }

	  private string matrixToString(SortedSet<string> tagset, double[][] data, bool filter)
	  {
		// we dont want to print trivial cases (acc=1)
		int initialIndex = 0;
		string[] tags = tagset.ToArray();
		StringBuilder sb = new StringBuilder();
		int minColumnSize = int.MinValue;
		string[][] matrix = RectangularArrays.ReturnRectangularStringArray(data.Length, data[0].Length);
		for (int i = 0; i < data.Length; i++)
		{
		  int j = 0;
		  for (; j < data[i].Length - 1; j++)
		  {
			matrix[i][j] = data[i][j] > 0 ? Convert.ToString((int) data[i][j]) : ".";
			if (minColumnSize < matrix[i][j].Length)
			{
			  minColumnSize = matrix[i][j].Length;
			}
		  }
		  matrix[i][j] = MessageFormat.format("{0,number,#.##%}", data[i][j]);
		  if (data[i][j] == 1 && filter)
		  {
			initialIndex = i + 1;
		  }
		}

		string headerFormat = "%" + (minColumnSize + 2) + "s "; // | 1234567 |
		string cellFormat = "%" + (minColumnSize + 2) + "s "; // | 12345 |
		string diagFormat = " %" + (minColumnSize + 2) + "s";
		for (int i = initialIndex; i < tagset.Count; i++)
		{
		  sb.Append(string.Format(headerFormat, generateAlphaLabel(i - initialIndex).Trim()));
		}
		sb.Append("| Accuracy | <-- classified as\n");
		for (int i = initialIndex; i < data.Length; i++)
		{
		  int j = initialIndex;
		  for (; j < data[i].Length - 1; j++)
		  {
			if (i == j)
			{
			  string val = "<" + matrix[i][j] + ">";
			  sb.Append(string.Format(diagFormat, val));
			}
			else
			{
			  sb.Append(string.Format(cellFormat, matrix[i][j]));
			}
		  }
		  sb.Append(string.Format("|   {0,-6} |   {1,3} = ", matrix[i][j], generateAlphaLabel(i - initialIndex))).Append(tags[i]);
		  sb.Append("\n");
		}
		return sb.ToString();
	  }

	  private void printGeneralStatistics()
	  {
		printHeader("Evaluation summary");
		printStream.append(string.Format("{0,21}: {1,6}", "Number of sentences\n", Convert.ToString(NumberOfSentences)));
		printStream.append(string.Format("{0,21}: {1,6}", "Min sentence size\n", MinSentenceSize));
		printStream.append(string.Format("{0,21}: {1,6}", "Max sentence size\n", MaxSentenceSize));
		printStream.append(string.Format("{0,21}: {1,6}", "Average sentence size\n", MessageFormat.format("{0,number,#.##}", AverageSentenceSize)));
		printStream.append(string.Format("{0,21}: {1,6}", "Tags count\n", NumberOfTags));
		printStream.append(string.Format("{0,21}: {1,6}", "Accuracy\n", MessageFormat.format("{0,number,#.##%}", Accuracy)));
		printFooter("Evaluation Corpus Statistics");
	  }

	  private void printTokenOcurrenciesRank()
	  {
		printHeader("Most frequent tokens");

		SortedSet<string> toks = TokensOrderedByFrequency;
		const int maxLines = 20;

		int maxTokSize = 5;

		int count = 0;
		IEnumerator<string> tokIterator = toks.GetEnumerator();
		while (tokIterator.MoveNext() && count++ < maxLines)
		{
		  string tok = tokIterator.Current;
		  if (tok.Length > maxTokSize)
		  {
			maxTokSize = tok.Length;
		  }
		}

		int tableSize = maxTokSize + 19;
		string format = "| %3s | %6s | %" + maxTokSize + "s |";

		printLine(tableSize);
		printStream.append(string.Format(format, "Pos", "Count", "Token\n"));
		printLine(tableSize);

		// get the first 20 errors
		count = 0;
		tokIterator = toks.GetEnumerator();
		while (tokIterator.MoveNext() && count++ < maxLines)
		{
		  string tok = tokIterator.Current;
		  int ocurrencies = getTokenFrequency(tok);

		  printStream.append(string.Format("{0} {1} {2} {3}\n", format, count, ocurrencies, tok));
		}
		printLine(tableSize);
		printFooter("Most frequent tokens");
	  }

	  private void printTokenErrorRank()
	  {
		printHeader("Tokens with the highest number of errors");
		printStream.append("\n");

		SortedSet<string> toks = TokensOrderedByNumberOfErrors;
		int maxTokenSize = 5;

		int count = 0;
		IEnumerator<string> tokIterator = toks.GetEnumerator();
		while (tokIterator.MoveNext() && count++ < 20)
		{
		  string tok = tokIterator.Current;
		  if (tok.Length > maxTokenSize)
		  {
			maxTokenSize = tok.Length;
		  }
		}

		int tableSize = 31 + maxTokenSize;

		string format = "| %" + maxTokenSize + "s | %6s | %5s | %7s |\n";

		printLine(tableSize);
		printStream.append(string.Format(format, "Token", "Errors", "Count", "% Err"));
		printLine(tableSize);

		// get the first 20 errors
		count = 0;
		tokIterator = toks.GetEnumerator();
		while (tokIterator.MoveNext() && count++ < 20)
		{
		  string tok = tokIterator.Current;
		  int ocurrencies = getTokenFrequency(tok);
		  int errors = getTokenErrors(tok);
		  string rate = MessageFormat.format("{0,number,#.##%}", (double) errors / ocurrencies);

		  printStream.append(string.Format(format, tok, errors, ocurrencies, rate));
		}
		printLine(tableSize);
		printFooter("Tokens with the highest number of errors");
	  }

	  private void printTagsErrorRank()
	  {
		printHeader("Detailed Accuracy By Tag");
		SortedSet<string> tags = TagsOrderedByErrors;
		printStream.append("\n");

		int maxTagSize = 3;

		foreach (string t in tags)
		{
		  if (t.Length > maxTagSize)
		  {
			maxTagSize = t.Length;
		  }
		}

		int tableSize = 65 + maxTagSize;

		string headerFormat = "| %" + maxTagSize + "s | %6s | %6s | %7s | %9s | %6s | %9s |\n";
		string format = "| %" + maxTagSize + "s | %6s | %6s | %-7s | %-9s | %-6s | %-9s |\n";

		printLine(tableSize);
		printStream.append(string.Format(headerFormat, "Tag", "Errors", "Count", "% Err", "Precision", "Recall", "F-Measure"));
		printLine(tableSize);

		IEnumerator<string> tagIterator = tags.GetEnumerator();
		while (tagIterator.MoveNext())
		{
		  string tag = tagIterator.Current;
		  int ocurrencies = getTagFrequency(tag);
		  int errors = getTagErrors(tag);
		  string rate = MessageFormat.format("{0,number,#.###}", (double) errors / ocurrencies);

		  double p = getTagPrecision(tag);
		  double r = getTagRecall(tag);
		  double f = getTagFMeasure(tag);

		  printStream.append(string.Format(format, tag, errors, ocurrencies, rate, MessageFormat.format("{0,number,#.###}", p > 0 ? p : 0), MessageFormat.format("{0,number,#.###}", r > 0 ? r : 0), MessageFormat.format("{0,number,#.###}", f > 0 ? f : 0)));
		}
		printLine(tableSize);

		printFooter("Tags with the highest number of errors");
	  }

	  private void printGeneralConfusionTable()
	  {
		printHeader("Confusion matrix");

		SortedSet<string> labels = ConfusionMatrixTagset;

		double[][] confusionMatrix = ConfusionMatrix;

		printStream.append("\nTags with 100% accuracy: ");
		int line = 0;
		foreach (string label in labels)
		{
		  if (confusionMatrix[line][confusionMatrix[0].Length - 1] == 1)
		  {
			printStream.append(label).append(" (").append(Convert.ToString((int) confusionMatrix[line][line])).append(") ");
		  }
		  line++;
		}

		printStream.append("\n\n");

		printStream.append(matrixToString(labels, confusionMatrix, true));

		printFooter("Confusion matrix");
	  }

	  private void printDetailedConfusionMatrix()
	  {
		printHeader("Confusion matrix for tokens");
		printStream.append("  sorted by number of errors\n");
		SortedSet<string> toks = TokensOrderedByNumberOfErrors;

		foreach (string t in toks)
		{
		  double acc = getTokenAccuracy(t);
		  if (acc < 1)
		  {
			printStream.append("\n[").append(t).append("]\n").append(string.Format("{0,12}: {1,-8}", "Accuracy", MessageFormat.format("{0,number,#.##%}", acc))).append("\n");
			printStream.append(string.Format("{0,12}: {1,-8}", "Ocurrencies", Convert.ToString(getTokenFrequency(t)))).append("\n");
			printStream.append(string.Format("{0,12}: {1,-8}", "Errors", Convert.ToString(getTokenErrors(t)))).append("\n");

			SortedSet<string> labels = getConfusionMatrixTagset(t);

			double[][] confusionMatrix = getConfusionMatrix(t);

			printStream.append(matrixToString(labels, confusionMatrix, false));
		  }
		}
		printFooter("Confusion matrix for tokens");
	  }

	  /// <summary>
	  /// Auxiliary method that prints a emphasised report header </summary>
	  private void printHeader(string text)
	  {
		printStream.append("=== ").append(text).append(" ===\n");
	  }

	  /// <summary>
	  /// Auxiliary method that prints a marker to the end of a report </summary>
	  private void printFooter(string text)
	  {
		printStream.append("\n<-end> ").append(text).append("\n\n");
	  }

	  /// <summary>
	  /// Auxiliary method that prints a horizontal line of a given size </summary>
	  private void printLine(int size)
	  {
		for (int i = 0; i < size; i++)
		{
		  printStream.append("-");
		}
		printStream.append("\n");
	  }

	  private static string generateAlphaLabel(int index)
	  {

		char[] labelChars = new char[3];
		int i;

		for (i = 2; i >= 0; i--)
		{
		  labelChars[i] = alpha[index % alpha.Length];
		  index = index / alpha.Length - 1;
		  if (index < 0)
		  {
			break;
		  }
		}

		return new string(labelChars);
	  }

	  private class Stats
	  {
		  private readonly POSTaggerFineGrainedReportListener outerInstance;

		  public Stats(POSTaggerFineGrainedReportListener outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }


		// general statistics
		internal readonly Mean accuracy = new Mean();
		internal readonly Mean averageSentenceLength = new Mean();
		internal int minimalSentenceLength = int.MaxValue;
		internal int maximumSentenceLength = int.MinValue;

		// token statistics
		internal readonly IDictionary<string, Mean> tokAccuracies = new Dictionary<string, Mean>();
		internal readonly IDictionary<string, Counter> tokOcurrencies = new Dictionary<string, Counter>();
		internal readonly IDictionary<string, Counter> tokErrors = new Dictionary<string, Counter>();

		// tag statistics
		internal readonly IDictionary<string, Counter> tagOcurrencies = new Dictionary<string, Counter>();
		internal readonly IDictionary<string, Counter> tagErrors = new Dictionary<string, Counter>();
		internal readonly IDictionary<string, FMeasure> tagFMeasure = new Dictionary<string, FMeasure>();

		// represents a Confusion Matrix that aggregates all tokens
		internal readonly IDictionary<string, ConfusionMatrixLine> generalConfusionMatrix = new Dictionary<string, ConfusionMatrixLine>();

		// represents a set of Confusion Matrix for each token
		internal readonly IDictionary<string, IDictionary<string, ConfusionMatrixLine>> tokenConfusionMatrix = new Dictionary<string, IDictionary<string, ConfusionMatrixLine>>();

		public virtual void add(POSSample reference, POSSample prediction)
		{
		  int length = reference.Sentence.Length;
		  averageSentenceLength.add(length);

		  if (minimalSentenceLength > length)
		  {
			minimalSentenceLength = length;
		  }
		  if (maximumSentenceLength < length)
		  {
			maximumSentenceLength = length;
		  }

		  string[] toks = reference.Sentence;
		  string[] refs = reference.Tags;
		  string[] preds = prediction.Tags;

		  updateTagFMeasure(refs, preds);

		  for (int i = 0; i < toks.Length; i++)
		  {
			add(toks[i], refs[i], preds[i]);
		  }
		}

		/// <summary>
		/// Includes a new evaluation data
		/// </summary>
		/// <param name="tok">
		///          the evaluated token </param>
		/// <param name="ref">
		///          the reference pos tag </param>
		/// <param name="pred">
		///          the predicted pos tag </param>
		internal virtual void add(string tok, string @ref, string pred)
		{
		  // token stats
		  if (!tokAccuracies.ContainsKey(tok))
		  {
			tokAccuracies[tok] = new Mean();
			tokOcurrencies[tok] = new Counter();
			tokErrors[tok] = new Counter();
		  }
		  tokOcurrencies[tok].increment();

		  // tag stats
		  if (!tagOcurrencies.ContainsKey(@ref))
		  {
			tagOcurrencies[@ref] = new Counter();
			tagErrors[@ref] = new Counter();
		  }
		  tagOcurrencies[@ref].increment();

		  // updates general, token and tag error stats
		  if (@ref.Equals(pred))
		  {
			tokAccuracies[tok].add(1);
			accuracy.add(1);
		  }
		  else
		  {
			tokAccuracies[tok].add(0);
			tokErrors[tok].increment();
			tagErrors[@ref].increment();
			accuracy.add(0);
		  }

		  // populate confusion matrixes
		  if (!generalConfusionMatrix.ContainsKey(@ref))
		  {
			generalConfusionMatrix[@ref] = new ConfusionMatrixLine(@ref);
		  }
		  generalConfusionMatrix[@ref].increment(pred);

		  if (!tokenConfusionMatrix.ContainsKey(tok))
		  {
			tokenConfusionMatrix[tok] = new Dictionary<string, ConfusionMatrixLine>();
		  }
		  if (!tokenConfusionMatrix[tok].ContainsKey(@ref))
		  {
			tokenConfusionMatrix[tok][@ref] = new ConfusionMatrixLine(@ref);
		  }
		  tokenConfusionMatrix[tok][@ref].increment(pred);
		}

		internal virtual void updateTagFMeasure(string[] refs, string[] preds)
		{
		  // create a set with all tags
          HashSet<string> tags = new HashSet<string>(preds);

		  // create samples for each tag
		  foreach (string tag in tags)
		  {
			IList<Span> reference = new List<Span>();
			IList<Span> prediction = new List<Span>();
			for (int i = 0; i < refs.Length; i++)
			{
			  if (refs[i].Equals(tag))
			  {
				reference.Add(new Span(i, i + 1));
			  }
			  if (preds[i].Equals(tag))
			  {
				prediction.Add(new Span(i, i + 1));
			  }
			}
			if (!this.tagFMeasure.ContainsKey(tag))
			{
			  this.tagFMeasure[tag] = new FMeasure();
			}
			// populate the fmeasure
			this.tagFMeasure[tag].updateScores(reference.ToArray(), prediction.ToArray());
		  }
		}

		public virtual double Accuracy
		{
			get
			{
			  return accuracy.mean();
			}
		}

		public virtual int NumberOfTags
		{
			get
			{
			  return this.tagOcurrencies.Keys.Count;
			}
		}

		public virtual long NumberOfSentences
		{
			get
			{
			  return this.averageSentenceLength.count();
			}
		}

		public virtual double AverageSentenceSize
		{
			get
			{
			  return this.averageSentenceLength.mean();
			}
		}

		public virtual int MinSentenceSize
		{
			get
			{
			  return this.minimalSentenceLength;
			}
		}

		public virtual int MaxSentenceSize
		{
			get
			{
			  return this.maximumSentenceLength;
			}
		}

		public virtual double getTokenAccuracy(string token)
		{
		  return tokAccuracies[token].mean();
		}

		public virtual int getTokenErrors(string token)
		{
		  return tokErrors[token].value();
		}

		public virtual int getTokenFrequency(string token)
		{
		  return tokOcurrencies[token].value();
		}

		public virtual SortedSet<string> TokensOrderedByFrequency
		{
			get
			{
			  var toks = new SortedSet<string>(new ComparatorAnonymousInnerClassHelper(this));

			    foreach (var key in tokOcurrencies.Keys)
			    {
			        toks.Add(key);
			    }
    
			  return toks;
			}
		}

		private class ComparatorAnonymousInnerClassHelper : IComparer<string>
		{
			private readonly Stats outerInstance;

			public ComparatorAnonymousInnerClassHelper(Stats outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual int Compare(string o1, string o2)
			{
			  if (o1.Equals(o2))
			  {
				return 0;
			  }
			  int e1 = 0, e2 = 0;
			  if (outerInstance.tokOcurrencies.ContainsKey(o1))
			  {
				e1 = outerInstance.tokOcurrencies[o1].value();
			  }
			  if (outerInstance.tokOcurrencies.ContainsKey(o2))
			  {
				e2 = outerInstance.tokOcurrencies[o2].value();
			  }
			  if (e1 == e2)
			  {
				return o1.CompareTo(o2);
			  }
			  return e2 - e1;
			}
		}

		public virtual SortedSet<string> TokensOrderedByNumberOfErrors
		{
			get
			{
			  SortedSet<string> toks = new SortedSet<string>(new ComparatorAnonymousInnerClassHelper2(this));
			    foreach (var key in tokErrors.Keys)
			    {
			        toks.Add(key);
			    }
			  return toks;
			}
		}

		private class ComparatorAnonymousInnerClassHelper2 : IComparer<string>
		{
			private readonly Stats outerInstance;

			public ComparatorAnonymousInnerClassHelper2(Stats outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual int Compare(string o1, string o2)
			{
			  if (o1.Equals(o2))
			  {
				return 0;
			  }
			  int e1 = 0, e2 = 0;
			  if (outerInstance.tokErrors.ContainsKey(o1))
			  {
				e1 = outerInstance.tokErrors[o1].value();
			  }
			  if (outerInstance.tokErrors.ContainsKey(o2))
			  {
				e2 = outerInstance.tokErrors[o2].value();
			  }
			  if (e1 == e2)
			  {
				return o1.CompareTo(o2);
			  }
			  return e2 - e1;
			}
		}

		public virtual int getTagFrequency(string tag)
		{
		  return tagOcurrencies[tag].value();
		}

		public virtual int getTagErrors(string tag)
		{
		  return tagErrors[tag].value();
		}

		public virtual double getTagFMeasure(string tag)
		{
		  return tagFMeasure[tag].getFMeasure();
		}

		public virtual double getTagRecall(string tag)
		{
		  return tagFMeasure[tag].RecallScore;
		}

		public virtual double getTagPrecision(string tag)
		{
		  return tagFMeasure[tag].PrecisionScore;
		}

		public virtual SortedSet<string> TagsOrderedByErrors
		{
			get
			{
			  SortedSet<string> tags = new SortedSet<string>(new ComparatorAnonymousInnerClassHelper3(this));
			    foreach (var tagError in tagErrors.Keys)
			    {
			        tags.Add(tagError);
			    }
			  return tags;
			}
		}

		private class ComparatorAnonymousInnerClassHelper3 : IComparer<string>
		{
			private readonly Stats outerInstance;

			public ComparatorAnonymousInnerClassHelper3(Stats outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual int Compare(string o1, string o2)
			{
			  if (o1.Equals(o2))
			  {
				return 0;
			  }
			  int e1 = 0, e2 = 0;
			  if (outerInstance.tagErrors.ContainsKey(o1))
			  {
				e1 = outerInstance.tagErrors[o1].value();
			  }
			  if (outerInstance.tagErrors.ContainsKey(o2))
			  {
				e2 = outerInstance.tagErrors[o2].value();
			  }
			  if (e1 == e2)
			  {
				return o1.CompareTo(o2);
			  }
			  return e2 - e1;
			}
		}

		public virtual SortedSet<string> ConfusionMatrixTagset
		{
			get
			{
			  return getConfusionMatrixTagset(generalConfusionMatrix);
			}
		}

		public virtual double[][] ConfusionMatrix
		{
			get
			{
			  return createConfusionMatrix(ConfusionMatrixTagset, generalConfusionMatrix);
			}
		}

		public virtual SortedSet<string> getConfusionMatrixTagset(string token)
		{
		  return getConfusionMatrixTagset(tokenConfusionMatrix[token]);
		}

		public virtual double[][] getConfusionMatrix(string token)
		{
		  return createConfusionMatrix(getConfusionMatrixTagset(token), tokenConfusionMatrix[token]);
		}

		/// <summary>
		/// Creates a matrix with N lines and N + 1 columns with the data from
		/// confusion matrix. The last column is the accuracy.
		/// </summary>
		internal virtual double[][] createConfusionMatrix(SortedSet<string> tagset, IDictionary<string, ConfusionMatrixLine> data)
		{
		  int size = tagset.Count;
		  double[][] matrix = RectangularArrays.ReturnRectangularDoubleArray(size, size + 1);
		  int line = 0;
		  foreach (string @ref in tagset)
		  {
			int column = 0;
			foreach (string pred in tagset)
			{
			  matrix[line][column] = (double)(data[@ref] != null ? data[@ref].getValue(pred) : 0);
			  column++;
			}
			// set accuracy
			matrix[line][column] = (double)(data[@ref] != null ? data[@ref].Accuracy : 0);
			line++;
		  }

		  return matrix;
		}

		internal virtual SortedSet<string> getConfusionMatrixTagset(IDictionary<string, ConfusionMatrixLine> data)
		{
		  SortedSet<string> tags = new SortedSet<string>();
		    foreach (var key in data.Keys)
		    {
		        tags.Add(key);
		    }
		  List<string> col = new List<string>();
		  foreach (string t in tags)
		  {
			col.AddRange(data[t].line.Keys);
		  }
          foreach (var c in col)
          {
              tags.Add(c);
          }
		  return tags;
		}
	  }

	  /// <summary>
	  /// A comparator that sorts the confusion matrix labels according to the
	  /// accuracy of each line
	  /// </summary>
	  private class CategoryComparator : IComparer<string>
	  {

		internal IDictionary<string, ConfusionMatrixLine> confusionMatrix;

		public CategoryComparator(IDictionary<string, ConfusionMatrixLine> confusionMatrix)
		{
		  this.confusionMatrix = confusionMatrix;
		}

		public virtual int Compare(string o1, string o2)
		{
		  if (o1.Equals(o2))
		  {
			return 0;
		  }
		  ConfusionMatrixLine t1 = confusionMatrix[o1];
		  ConfusionMatrixLine t2 = confusionMatrix[o2];
		  if (t1 == null || t2 == null)
		  {
			if (t1 == null)
			{
			  return 1;
			}
			else if (t2 == null)
			{
			  return -1;
			}
			return 0;
		  }
		  double r1 = t1.Accuracy;
		  double r2 = t2.Accuracy;
		  if (r1 == r2)
		  {
			return o1.CompareTo(o2);
		  }
		  if (r2 > r1)
		  {
			return 1;
		  }
		  return -1;
		}

	  }

	  /// <summary>
	  /// Represents a line in the confusion table.
	  /// </summary>
	  private class ConfusionMatrixLine
	  {

		internal IDictionary<string, Counter> line = new Dictionary<string, Counter>();
		internal string @ref;
		internal int total = 0;
		internal int correct = 0;
		internal double acc = -1;

		/// <summary>
		/// Creates a new <seealso cref="ConfusionMatrixLine"/>
		/// </summary>
		/// <param name="ref">
		///          the reference column </param>
		public ConfusionMatrixLine(string @ref)
		{
		  this.@ref = @ref;
		}

		/// <summary>
		/// Increments the counter for the given column and updates the statistics.
		/// </summary>
		/// <param name="column">
		///          the column to be incremented </param>
		public virtual void increment(string column)
		{
		  total++;
		  if (column.Equals(@ref))
		  {
			correct++;
		  }
		  if (!line.ContainsKey(column))
		  {
			line[column] = new Counter();
		  }
		  line[column].increment();
		}

		/// <summary>
		/// Gets the calculated accuracy of this element
		/// </summary>
		/// <returns> the accuracy </returns>
		public virtual double Accuracy
		{
			get
			{
			  // we save the accuracy because it is frequently used by the comparator
			  if (acc == -1)
			  {
				if (total == 0)
				{
				  acc = 0;
				}
				acc = (double) correct / (double) total;
			  }
			  return acc;
			}
		}

		/// <summary>
		/// Gets the value given a column
		/// </summary>
		/// <param name="column">
		///          the column </param>
		/// <returns> the counter value </returns>
		public virtual int getValue(string column)
		{
		  Counter c = line[column];
		  if (c == null)
		  {
			return 0;
		  }
		  return c.value();
		}
	  }

	  /// <summary>
	  /// Implements a simple counter
	  /// </summary>
	  private class Counter
	  {
		internal int c = 0;

		public virtual void increment()
		{
		  c++;
		}

		public virtual int value()
		{
		  return c;
		}
	  }

	}

}