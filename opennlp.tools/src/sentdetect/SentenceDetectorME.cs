using System;
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
using j4n.Serialization;


namespace opennlp.tools.sentdetect
{


	using AbstractModel = opennlp.model.AbstractModel;
	using EventStream = opennlp.model.EventStream;
	using MaxentModel = opennlp.model.MaxentModel;
	using TrainUtil = opennlp.model.TrainUtil;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using Factory = opennlp.tools.sentdetect.lang.Factory;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;
	using StringUtil = opennlp.tools.util.StringUtil;
	using TrainingParameters = opennlp.tools.util.TrainingParameters;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	/// <summary>
	/// A sentence detector for splitting up raw text into sentences.
	/// <para>
	/// A maximum entropy model is used to evaluate the characters ".", "!", and "?" in a
	/// string to determine if they signify the end of a sentence.
	/// </para>
	/// </summary>
	public class SentenceDetectorME : SentenceDetector
	{

	  /// <summary>
	  /// Constant indicates a sentence split.
	  /// </summary>
	  public const string SPLIT = "s";

	  /// <summary>
	  /// Constant indicates no sentence split.
	  /// </summary>
	  public const string NO_SPLIT = "n";

	  // Note: That should be inlined when doing a re-factoring!
	  private static readonly double? ONE = new double?(1);

	  /// <summary>
	  /// The maximum entropy model to use to evaluate contexts.
	  /// </summary>
	  private MaxentModel model;

	  /// <summary>
	  /// The feature context generator.
	  /// </summary>
	  private readonly SDContextGenerator cgen;

	  /// <summary>
	  /// The <seealso cref="EndOfSentenceScanner"/> to use when scanning for end of sentence offsets.
	  /// </summary>
	  private readonly EndOfSentenceScanner scanner;

	  /// <summary>
	  /// The list of probabilities associated with each decision.
	  /// </summary>
	  private IList<double?> sentProbs = new List<double?>();

	  protected internal bool useTokenEnd;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="model"> the <seealso cref="SentenceModel"/> </param>
	  public SentenceDetectorME(SentenceModel model)
	  {
		SentenceDetectorFactory sdFactory = model.Factory;
		this.model = model.MaxentModel;
		cgen = sdFactory.SDContextGenerator;
		scanner = sdFactory.EndOfSentenceScanner;
		useTokenEnd = sdFactory.UseTokenEnd;
	  }

	  /// @deprecated Use a <seealso cref="SentenceDetectorFactory"/> to extend
	  ///             SentenceDetector functionality. 
	  public SentenceDetectorME(SentenceModel model, Factory factory)
	  {
		this.model = model.MaxentModel;
		// if the model has custom EOS characters set, use this to get the context
		// generator and the EOS scanner; otherwise use language-specific defaults
		char[] customEOSCharacters = model.EosCharacters;
		if (customEOSCharacters == null)
		{
		  cgen = factory.createSentenceContextGenerator(model.Language, getAbbreviations(model.Abbreviations));
		  scanner = factory.createEndOfSentenceScanner(model.Language);
		}
		else
		{
		  cgen = factory.createSentenceContextGenerator(getAbbreviations(model.Abbreviations), customEOSCharacters);
		  scanner = factory.createEndOfSentenceScanner(customEOSCharacters);
		}
		useTokenEnd = model.useTokenEnd();
	  }

	  private static HashSet<string> getAbbreviations(Dictionary abbreviations)
	  {
		if (abbreviations == null)
		{
		  return new HashSet<string>();
		}
		return abbreviations.asStringSet();
	  }

	  /// <summary>
	  /// Detect sentences in a String.
	  /// </summary>
	  /// <param name="s">  The string to be processed.
	  /// </param>
	  /// <returns>   A string array containing individual sentences as elements. </returns>
	  public virtual string[] sentDetect(string s)
	  {
		Span[] spans = sentPosDetect(s);
		string[] sentences;
		if (spans.Length != 0)
		{

		  sentences = new string[spans.Length];

		  for (int si = 0; si < spans.Length; si++)
		  {
			sentences[si] = spans[si].getCoveredText(s);
		  }
		}
		else
		{
		  sentences = new string[] {};
		}
		return sentences;
	  }

	  private int getFirstWS(string s, int pos)
	  {
		while (pos < s.Length && !StringUtil.isWhitespace(s[pos]))
		{
		  pos++;
		}
		return pos;
	  }

	  private int getFirstNonWS(string s, int pos)
	  {
		while (pos < s.Length && StringUtil.isWhitespace(s[pos]))
		{
		  pos++;
		}
		return pos;
	  }

	  /// <summary>
	  /// Detect the position of the first words of sentences in a String.
	  /// </summary>
	  /// <param name="s">  The string to be processed. </param>
	  /// <returns>   A integer array containing the positions of the end index of
	  ///          every sentence
	  ///  </returns>
	  public virtual Span[] sentPosDetect(string s)
	  {
		sentProbs.Clear();
		StringBuilder sb = new StringBuilder(s);
		IList<int?> enders = scanner.getPositions(s);
		IList<int?> positions = new List<int?>(enders.Count);

		for (int i = 0, end = enders.Count, index = 0; i < end; i++)
		{
		  int? candidate = enders[i];
		  int cint = candidate.Value;
		  // skip over the leading parts of non-token final delimiters
		  int fws = getFirstWS(s,cint + 1);
		  if (i + 1 < end && enders[i + 1] < fws)
		  {
			continue;
		  }

		  double[] probs = model.eval(cgen.getContext(sb, cint));
		  string bestOutcome = model.getBestOutcome(probs);

		  if (bestOutcome.Equals(SPLIT) && isAcceptableBreak(s, index, cint))
		  {
			if (index != cint)
			{
			  if (useTokenEnd)
			  {
				positions.Add(getFirstNonWS(s, getFirstWS(s,cint + 1)));
			  }
			  else
			  {
				positions.Add(getFirstNonWS(s,cint));
			  }
			  sentProbs.Add(probs[model.getIndex(bestOutcome)]);
			}
			index = cint + 1;
		  }
		}

		int[] starts = new int[positions.Count];
		for (int i = 0; i < starts.Length; i++)
		{
		  starts[i] = positions[i].GetValueOrDefault();
		}

		// string does not contain sentence end positions
		if (starts.Length == 0)
		{

			// remove leading and trailing whitespace
			int start = 0;
			int end = s.Length;

			while (start < s.Length && StringUtil.isWhitespace(s[start]))
			{
			  start++;
			}

			while (end > 0 && StringUtil.isWhitespace(s[end - 1]))
			{
			  end--;
			}

			if ((end - start) > 0)
			{
			  sentProbs.Add(1d);
			  return new Span[] {new Span(start, end)};
			}
			else
			{
			  return new Span[0];
			}
		}

		// Now convert the sent indexes to spans
		bool leftover = starts[starts.Length - 1] != s.Length;
		Span[] spans = new Span[leftover? starts.Length + 1 : starts.Length];
		for (int si = 0;si < starts.Length;si++)
		{
		  int start, end;
		  if (si == 0)
		  {
			start = 0;

			while (si < starts.Length && StringUtil.isWhitespace(s[start]))
			{
			  start++;
			}
		  }
		  else
		  {
			start = starts[si - 1];
		  }
		  end = starts[si];
		  while (end > 0 && StringUtil.isWhitespace(s[end - 1]))
		  {
			end--;
		  }
		  spans[si] = new Span(start,end);
		}

		if (leftover)
		{
		  spans[spans.Length - 1] = new Span(starts[starts.Length - 1],s.Length);
		  sentProbs.Add(ONE);
		}

		return spans;
	  }

	  /// <summary>
	  /// Returns the probabilities associated with the most recent
	  /// calls to sentDetect().
	  /// </summary>
	  /// <returns> probability for each sentence returned for the most recent
	  /// call to sentDetect.  If not applicable an empty array is
	  /// returned. </returns>
	  public virtual double[] SentenceProbabilities
	  {
		  get
		  {
			double[] sentProbArray = new double[sentProbs.Count];
			for (int i = 0; i < sentProbArray.Length; i++)
			{
			  sentProbArray[i] = sentProbs[i].GetValueOrDefault();
			}
			return sentProbArray;
		  }
	  }

	  /// <summary>
	  /// Allows subclasses to check an overzealous (read: poorly
	  /// trained) model from flagging obvious non-breaks as breaks based
	  /// on some boolean determination of a break's acceptability.
	  /// 
	  /// <para>The implementation here always returns true, which means
	  /// that the MaxentModel's outcome is taken as is.</para>
	  /// </summary>
	  /// <param name="s"> the string in which the break occurred. </param>
	  /// <param name="fromIndex"> the start of the segment currently being evaluated </param>
	  /// <param name="candidateIndex"> the index of the candidate sentence ending </param>
	  /// <returns> true if the break is acceptable </returns>
	  protected internal virtual bool isAcceptableBreak(string s, int fromIndex, int candidateIndex)
	  {
		return true;
	  }

	  /// @deprecated Use
	  ///             <seealso cref="#train(String, ObjectStream, SentenceDetectorFactory, TrainingParameters)"/>
	  ///             and pass in af <seealso cref="SentenceDetectorFactory"/>. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static SentenceModel train(String languageCode, opennlp.tools.util.ObjectStream<SentenceSample> samples, boolean useTokenEnd, opennlp.tools.dictionary.Dictionary abbreviations, opennlp.tools.util.TrainingParameters mlParams) throws java.io.IOException
	  public static SentenceModel train(string languageCode, ObjectStream<SentenceSample> samples, bool useTokenEnd, Dictionary abbreviations, TrainingParameters mlParams)
	  {
		SentenceDetectorFactory sdFactory = new SentenceDetectorFactory(languageCode, useTokenEnd, abbreviations, null);
		return train(languageCode, samples, sdFactory, mlParams);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static SentenceModel train(String languageCode, opennlp.tools.util.ObjectStream<SentenceSample> samples, SentenceDetectorFactory sdFactory, opennlp.tools.util.TrainingParameters mlParams) throws java.io.IOException
	  public static SentenceModel train(string languageCode, ObjectStream<SentenceSample> samples, SentenceDetectorFactory sdFactory, TrainingParameters mlParams)
	  {

		IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

		// TODO: Fix the EventStream to throw exceptions when training goes wrong
		EventStream eventStream = new SDEventStream(samples, sdFactory.SDContextGenerator, sdFactory.EndOfSentenceScanner);

		AbstractModel sentModel = TrainUtil.train(eventStream, mlParams.Settings, manifestInfoEntries);

		return new SentenceModel(languageCode, sentModel, manifestInfoEntries, sdFactory);
	  }

	  /// @deprecated Use
	  ///             <seealso cref="#train(String, ObjectStream, SentenceDetectorFactory, TrainingParameters)"/>
	  ///             and pass in af <seealso cref="SentenceDetectorFactory"/>. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("Use") public static SentenceModel train(String languageCode, opennlp.tools.util.ObjectStream<SentenceSample> samples, boolean useTokenEnd, opennlp.tools.dictionary.Dictionary abbreviations, int cutoff, int iterations) throws java.io.IOException
	  [Obsolete("Use")]
	  public static SentenceModel train(string languageCode, ObjectStream<SentenceSample> samples, bool useTokenEnd, Dictionary abbreviations, int cutoff, int iterations)
	  {
		return train(languageCode, samples, useTokenEnd, abbreviations, ModelUtil.createTrainingParameters(iterations, cutoff));
	  }

	  /// @deprecated Use
	  ///             <seealso cref="#train(String, ObjectStream, SentenceDetectorFactory, TrainingParameters)"/>
	  ///             and pass in af <seealso cref="SentenceDetectorFactory"/>. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static SentenceModel train(String languageCode, opennlp.tools.util.ObjectStream<SentenceSample> samples, boolean useTokenEnd, opennlp.tools.dictionary.Dictionary abbreviations) throws java.io.IOException
	  public static SentenceModel train(string languageCode, ObjectStream<SentenceSample> samples, bool useTokenEnd, Dictionary abbreviations)
	  {
		return train(languageCode, samples, useTokenEnd, abbreviations,5,100);
	  }
	}

}