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
using j4n.Serialization;
using opennlp.tools.nonjava;
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.tokenize
{


	using AbstractModel = opennlp.model.AbstractModel;
	using EventStream = opennlp.model.EventStream;
	using MaxentModel = opennlp.model.MaxentModel;
	using TrainUtil = opennlp.model.TrainUtil;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using Factory = opennlp.tools.tokenize.lang.Factory;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;
	using TrainingParameters = opennlp.tools.util.TrainingParameters;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	/// <summary>
	/// A Tokenizer for converting raw text into separated tokens.  It uses
	/// Maximum Entropy to make its decisions.  The features are loosely
	/// based off of Jeff Reynar's UPenn thesis "Topic Segmentation:
	/// Algorithms and Applications.", which is available from his
	/// homepage: <http://www.cis.upenn.edu/~jcreynar>.
	/// <para>
	/// This tokenizer needs a statistical model to tokenize a text which reproduces
	/// the tokenization observed in the training data used to create the model.
	/// The <seealso cref="TokenizerModel"/> class encapsulates the model and provides
	/// methods to create it from the binary representation. 
	/// </para>
	/// <para>
	/// A tokenizer instance is not thread safe. For each thread one tokenizer
	/// must be instantiated which can share one <code>TokenizerModel</code> instance
	/// to safe memory.
	/// </para>
	/// <para>
	/// To train a new model {<seealso cref="#train(String, ObjectStream, boolean, TrainingParameters)"/> method
	/// can be used.
	/// </para>
	/// <para>
	/// Sample usage:
	/// </para>
	/// <para>
	/// <code>
	/// InputStream modelIn;<br>
	/// <br>
	/// ...<br>
	/// <br>
	/// TokenizerModel model = TokenizerModel(modelIn);<br>
	/// <br>
	/// Tokenizer tokenizer = new TokenizerME(model);<br>
	/// <br>
	/// String tokens[] = tokenizer.tokenize("A sentence to be tokenized.");
	/// </code>
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= Tokenizer </seealso>
	/// <seealso cref= TokenizerModel </seealso>
	/// <seealso cref= TokenSample </seealso>
	public class TokenizerME : AbstractTokenizer
	{

	  /// <summary>
	  /// Constant indicates a token split.
	  /// </summary>
	  public const string SPLIT = "T";

	  /// <summary>
	  /// Constant indicates no token split.
	  /// </summary>
	  public const string NO_SPLIT = "F";

	  /// <summary>
	  /// Alpha-Numeric Pattern </summary>
	  /// @deprecated As of release 1.5.2, replaced by <seealso cref="Factory#getAlphanumeric(String)"/> 
	  [Obsolete("As of release 1.5.2, replaced by <seealso cref=\"Factory#getAlphanumeric(String)\"/>")]
	  public static readonly Pattern alphaNumeric = Pattern.compile(Factory.DEFAULT_ALPHANUMERIC);

	  private readonly Pattern alphanumeric;

	  /// <summary>
	  /// The maximum entropy model to use to evaluate contexts.
	  /// </summary>
	  private MaxentModel model;

	  /// <summary>
	  /// The context generator.
	  /// </summary>
	  private readonly TokenContextGenerator cg;

	  /// <summary>
	  /// Optimization flag to skip alpha numeric tokens for further
	  /// tokenization
	  /// </summary>
	  private bool useAlphaNumericOptimization_Renamed;

	  /// <summary>
	  /// List of probabilities for each token returned from a call to
	  /// <code>tokenize</code> or <code>tokenizePos</code>.
	  /// </summary>
	  private IList<double?> tokProbs;

	  private IList<Span> newTokens;

	  public TokenizerME(TokenizerModel model)
	  {
		TokenizerFactory factory = model.Factory;
		this.alphanumeric = factory.AlphaNumericPattern;
		this.cg = factory.ContextGenerator;
		this.model = model.MaxentModel;
		this.useAlphaNumericOptimization_Renamed = factory.UseAlphaNumericOptmization;

		newTokens = new List<Span>();
		tokProbs = new List<double?>(50);
	  }

	  /// @deprecated use <seealso cref="TokenizerFactory"/> to extend the Tokenizer
	  ///             functionality 
	  public TokenizerME(TokenizerModel model, Factory factory)
	  {
		string languageCode = model.Language;

		this.alphanumeric = factory.getAlphanumeric(languageCode);
		this.cg = factory.createTokenContextGenerator(languageCode, getAbbreviations(model.Abbreviations));

		this.model = model.MaxentModel;
		useAlphaNumericOptimization_Renamed = model.useAlphaNumericOptimization();

		newTokens = new List<Span>();
		tokProbs = new List<double?>(50);
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
	  /// Returns the probabilities associated with the most recent
	  /// calls to <seealso cref="TokenizerME#tokenize(String)"/> or <seealso cref="TokenizerME#tokenizePos(String)"/>.
	  /// </summary>
	  /// <returns> probability for each token returned for the most recent
	  /// call to tokenize.  If not applicable an empty array is
	  /// returned. </returns>
	  public virtual double[] TokenProbabilities
	  {
		  get
		  {
			double[] tokProbArray = new double[tokProbs.Count];
			for (int i = 0; i < tokProbArray.Length; i++)
			{
			  tokProbArray[i] = tokProbs[i].GetValueOrDefault();
			}
			return tokProbArray;
		  }
	  }

	  /// <summary>
	  /// Tokenizes the string.
	  /// </summary>
	  /// <param name="d">  The string to be tokenized.
	  /// </param>
	  /// <returns>   A span array containing individual tokens as elements. </returns>
	  public override Span[] tokenizePos(string d)
	  {
		Span[] tokens = WhitespaceTokenizer.INSTANCE.tokenizePos(d);
		newTokens.Clear();
		tokProbs.Clear();
		for (int i = 0, il = tokens.Length; i < il; i++)
		{
		  Span s = tokens[i];
		  string tok = StringHelperClass.SubstringSpecial(d, s.Start, s.End);
		  // Can't tokenize single characters
		  if (tok.Length < 2)
		  {
			newTokens.Add(s);
			tokProbs.Add(1d);
		  }
		  else if (useAlphaNumericOptimization() && alphanumeric.matcher(tok).matches())
		  {
			newTokens.Add(s);
			tokProbs.Add(1d);
		  }
		  else
		  {
			int start = s.Start;
			int end = s.End;
            int origStart = s.Start;
			double tokenProb = 1.0;
			for (int j = origStart + 1; j < end; j++)
			{
			  double[] probs = model.eval(cg.getContext(tok, j - origStart));
			  string best = model.getBestOutcome(probs);
			  tokenProb *= probs[model.getIndex(best)];
			  if (best.Equals(TokenizerME.SPLIT))
			  {
				newTokens.Add(new Span(start, j));
				tokProbs.Add(tokenProb);
				start = j;
				tokenProb = 1.0;
			  }
			}
			newTokens.Add(new Span(start, end));
			tokProbs.Add(tokenProb);
		  }
		}

		return newTokens.ToArray();
	  }

	  /// <summary>
	  /// Trains a model for the <seealso cref="TokenizerME"/>.
	  /// </summary>
	  /// <param name="samples">
	  ///          the samples used for the training. </param>
	  /// <param name="factory">
	  ///          a <seealso cref="TokenizerFactory"/> to get resources from </param>
	  /// <param name="mlParams">
	  ///          the machine learning train parameters </param>
	  /// <returns> the trained <seealso cref="TokenizerModel"/> </returns>
	  /// <exception cref="IOException">
	  ///           it throws an <seealso cref="IOException"/> if an <seealso cref="IOException"/> is
	  ///           thrown during IO operations on a temp file which is created
	  ///           during training. Or if reading from the <seealso cref="ObjectStream"/>
	  ///           fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenizerModel train(opennlp.tools.util.ObjectStream<TokenSample> samples, TokenizerFactory factory, opennlp.tools.util.TrainingParameters mlParams) throws java.io.IOException
	  public static TokenizerModel train(ObjectStream<TokenSample> samples, TokenizerFactory factory, TrainingParameters mlParams)
	  {

		IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

		EventStream eventStream = new TokSpanEventStream(samples, factory.UseAlphaNumericOptmization, factory.AlphaNumericPattern, factory.ContextGenerator);

		AbstractModel maxentModel = TrainUtil.train(eventStream, mlParams.Settings, manifestInfoEntries);

		return new TokenizerModel(maxentModel, manifestInfoEntries, factory);
	  }

	  /// <summary>
	  /// Trains a model for the <seealso cref="TokenizerME"/>.
	  /// </summary>
	  /// <param name="languageCode"> the language of the natural text </param>
	  /// <param name="samples"> the samples used for the training. </param>
	  /// <param name="useAlphaNumericOptimization"> - if true alpha numerics are skipped </param>
	  /// <param name="mlParams"> the machine learning train parameters
	  /// </param>
	  /// <returns> the trained <seealso cref="TokenizerModel"/>
	  /// </returns>
	  /// <exception cref="IOException"> it throws an <seealso cref="IOException"/> if an <seealso cref="IOException"/>
	  /// is thrown during IO operations on a temp file which is created during training.
	  /// Or if reading from the <seealso cref="ObjectStream"/> fails.
	  /// </exception>
	  /// @deprecated Use 
	  ///    <seealso cref="#train(String, ObjectStream, TokenizerFactory, TrainingParameters)"/> 
	  ///    and pass in a <seealso cref="TokenizerFactory"/> 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenizerModel train(String languageCode, opennlp.tools.util.ObjectStream<TokenSample> samples, boolean useAlphaNumericOptimization, opennlp.tools.util.TrainingParameters mlParams) throws java.io.IOException
	  public static TokenizerModel train(string languageCode, ObjectStream<TokenSample> samples, bool useAlphaNumericOptimization, TrainingParameters mlParams)
	  {
		return train(languageCode, samples, null, useAlphaNumericOptimization, mlParams);
	  }

	  /// <summary>
	  /// Trains a model for the <seealso cref="TokenizerME"/>.
	  /// </summary>
	  /// <param name="languageCode"> the language of the natural text </param>
	  /// <param name="samples"> the samples used for the training. </param>
	  /// <param name="abbreviations"> an abbreviations dictionary </param>
	  /// <param name="useAlphaNumericOptimization"> - if true alpha numerics are skipped </param>
	  /// <param name="mlParams"> the machine learning train parameters
	  /// </param>
	  /// <returns> the trained <seealso cref="TokenizerModel"/>
	  /// </returns>
	  /// <exception cref="IOException"> it throws an <seealso cref="IOException"/> if an <seealso cref="IOException"/>
	  /// is thrown during IO operations on a temp file which is created during training.
	  /// Or if reading from the <seealso cref="ObjectStream"/> fails.
	  /// </exception>
	  /// @deprecated Use 
	  ///    <seealso cref="#train(String, ObjectStream, TokenizerFactory, TrainingParameters)"/> 
	  ///    and pass in a <seealso cref="TokenizerFactory"/> 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenizerModel train(String languageCode, opennlp.tools.util.ObjectStream<TokenSample> samples, opennlp.tools.dictionary.Dictionary abbreviations, boolean useAlphaNumericOptimization, opennlp.tools.util.TrainingParameters mlParams) throws java.io.IOException
	  public static TokenizerModel train(string languageCode, ObjectStream<TokenSample> samples, Dictionary abbreviations, bool useAlphaNumericOptimization, TrainingParameters mlParams)
	  {
		Factory factory = new Factory();

		IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

		EventStream eventStream = new TokSpanEventStream(samples, useAlphaNumericOptimization, factory.getAlphanumeric(languageCode), factory.createTokenContextGenerator(languageCode, getAbbreviations(abbreviations)));

		AbstractModel maxentModel = TrainUtil.train(eventStream, mlParams.Settings, manifestInfoEntries);

		return new TokenizerModel(languageCode, maxentModel, abbreviations, useAlphaNumericOptimization, manifestInfoEntries);
	  }

	  /// <summary>
	  /// Trains a model for the <seealso cref="TokenizerME"/>.
	  /// </summary>
	  /// <param name="languageCode"> the language of the natural text </param>
	  /// <param name="samples"> the samples used for the training. </param>
	  /// <param name="useAlphaNumericOptimization"> - if true alpha numerics are skipped </param>
	  /// <param name="cutoff"> number of times a feature must be seen to be considered </param>
	  /// <param name="iterations"> number of iterations to train the maxent model
	  /// </param>
	  /// <returns> the trained <seealso cref="TokenizerModel"/>
	  /// </returns>
	  /// <exception cref="IOException"> it throws an <seealso cref="IOException"/> if an <seealso cref="IOException"/>
	  /// is thrown during IO operations on a temp file which is created during training.
	  /// Or if reading from the <seealso cref="ObjectStream"/> fails.
	  /// </exception>
	  /// @deprecated Use 
	  ///    <seealso cref="#train(String, ObjectStream, TokenizerFactory, TrainingParameters)"/> 
	  ///    and pass in a <seealso cref="TokenizerFactory"/> 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("Use") public static TokenizerModel train(String languageCode, opennlp.tools.util.ObjectStream<TokenSample> samples, boolean useAlphaNumericOptimization, int cutoff, int iterations) throws java.io.IOException
	  [Obsolete("Use")]
	  public static TokenizerModel train(string languageCode, ObjectStream<TokenSample> samples, bool useAlphaNumericOptimization, int cutoff, int iterations)
	  {

		return train(languageCode, samples, useAlphaNumericOptimization, ModelUtil.createTrainingParameters(iterations, cutoff));
	  }


	  /// <summary>
	  /// Trains a model for the <seealso cref="TokenizerME"/> with a default cutoff of 5 and 100 iterations.
	  /// </summary>
	  /// <param name="languageCode"> the language of the natural text </param>
	  /// <param name="samples"> the samples used for the training. </param>
	  /// <param name="useAlphaNumericOptimization"> - if true alpha numerics are skipped
	  /// </param>
	  /// <returns> the trained <seealso cref="TokenizerModel"/>
	  /// </returns>
	  /// <exception cref="IOException"> it throws an <seealso cref="IOException"/> if an <seealso cref="IOException"/>
	  /// is thrown during IO operations on a temp file which is
	  /// </exception>
	  /// <exception cref="ObjectStreamException"> if reading from the <seealso cref="ObjectStream"/> fails
	  /// created during training.
	  /// 
	  /// </exception>
	  /// @deprecated Use 
	  ///    <seealso cref="#train(String, ObjectStream, TokenizerFactory, TrainingParameters)"/> 
	  ///    and pass in a <seealso cref="TokenizerFactory"/> 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenizerModel train(String languageCode, opennlp.tools.util.ObjectStream<TokenSample> samples, boolean useAlphaNumericOptimization) throws java.io.IOException, java.io.ObjectStreamException
	  public static TokenizerModel train(string languageCode, ObjectStream<TokenSample> samples, bool useAlphaNumericOptimization)
	  {
		return train(languageCode, samples, useAlphaNumericOptimization, 5, 100);
	  }

	  /// <summary>
	  /// Returns the value of the alpha-numeric optimization flag.
	  /// </summary>
	  /// <returns> true if the tokenizer should use alpha-numeric optimization, false otherwise. </returns>
	  public virtual bool useAlphaNumericOptimization()
	  {
		return useAlphaNumericOptimization_Renamed;
	  }
	}

}