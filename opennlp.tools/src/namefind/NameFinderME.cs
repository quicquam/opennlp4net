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
using j4n.IO.InputStream;
using j4n.Lang;
using j4n.Serialization;


namespace opennlp.tools.namefind
{


	using GIS = opennlp.maxent.GIS;
	using GISModel = opennlp.maxent.GISModel;
	using AbstractModel = opennlp.model.AbstractModel;
	using EventStream = opennlp.model.EventStream;
	using MaxentModel = opennlp.model.MaxentModel;
	using TrainUtil = opennlp.model.TrainUtil;
	using TwoPassDataIndexer = opennlp.model.TwoPassDataIndexer;
	using opennlp.tools.util;
	using opennlp.tools.util;
	using Sequence = opennlp.tools.util.Sequence;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;
	using TrainingParameters = opennlp.tools.util.TrainingParameters;
	using AdaptiveFeatureGenerator = opennlp.tools.util.featuregen.AdaptiveFeatureGenerator;
	using AdditionalContextFeatureGenerator = opennlp.tools.util.featuregen.AdditionalContextFeatureGenerator;
	using BigramNameFeatureGenerator = opennlp.tools.util.featuregen.BigramNameFeatureGenerator;
	using CachedFeatureGenerator = opennlp.tools.util.featuregen.CachedFeatureGenerator;
	using FeatureGeneratorResourceProvider = opennlp.tools.util.featuregen.FeatureGeneratorResourceProvider;
	using GeneratorFactory = opennlp.tools.util.featuregen.GeneratorFactory;
	using OutcomePriorFeatureGenerator = opennlp.tools.util.featuregen.OutcomePriorFeatureGenerator;
	using PreviousMapFeatureGenerator = opennlp.tools.util.featuregen.PreviousMapFeatureGenerator;
	using SentenceFeatureGenerator = opennlp.tools.util.featuregen.SentenceFeatureGenerator;
	using TokenClassFeatureGenerator = opennlp.tools.util.featuregen.TokenClassFeatureGenerator;
	using TokenFeatureGenerator = opennlp.tools.util.featuregen.TokenFeatureGenerator;
	using WindowFeatureGenerator = opennlp.tools.util.featuregen.WindowFeatureGenerator;
	using ModelUtil = opennlp.tools.util.model.ModelUtil;

	/// <summary>
	/// Class for creating a maximum-entropy-based name finder.
	/// </summary>
	public class NameFinderME : TokenNameFinder
	{

	  private static string[][] EMPTY = new string[0][];
	  public const int DEFAULT_BEAM_SIZE = 3;
	  private static readonly Pattern typedOutcomePattern = Pattern.compile("(.+)-\\w+");



	  public const string START = "start";
	  public const string CONTINUE = "cont";
	  public const string OTHER = "other";

	  protected internal MaxentModel model;
	  protected internal NameContextGenerator contextGenerator;
	  private Sequence bestSequence;
	  private BeamSearch<string> beam;

	  private AdditionalContextFeatureGenerator additionalContextFeatureGenerator = new AdditionalContextFeatureGenerator();

	  public NameFinderME(TokenNameFinderModel model) : this(model, DEFAULT_BEAM_SIZE)
	  {
	  }

	  /// <summary>
	  /// Initializes the name finder with the specified model.
	  /// </summary>
	  /// <param name="model"> </param>
	  /// <param name="beamSize"> </param>
	  public NameFinderME(TokenNameFinderModel model, AdaptiveFeatureGenerator generator, int beamSize, SequenceValidator<string> sequenceValidator)
	  {
		this.model = model.NameFinderModel;

		// If generator is provided always use that one
		if (generator != null)
		{
		  contextGenerator = new DefaultNameContextGenerator(generator);
		}
		else
		{
		  // If model has a generator use that one, otherwise create default
		  AdaptiveFeatureGenerator featureGenerator = model.createFeatureGenerators();

		  if (featureGenerator == null)
		  {
			featureGenerator = createFeatureGenerator();
		  }

		  contextGenerator = new DefaultNameContextGenerator(featureGenerator);
		}

		contextGenerator.addFeatureGenerator(new WindowFeatureGenerator(additionalContextFeatureGenerator, 8, 8));

		if (sequenceValidator == null)
		{
		  sequenceValidator = new NameFinderSequenceValidator();
		}

		beam = new BeamSearch<string>(beamSize, contextGenerator, this.model, sequenceValidator, beamSize);
	  }

	  public NameFinderME(TokenNameFinderModel model, AdaptiveFeatureGenerator generator, int beamSize) : this(model, generator, beamSize, null)
	  {
	  }

	  public NameFinderME(TokenNameFinderModel model, int beamSize) : this(model, null, beamSize)
	  {
	  }


	  /// <summary>
	  /// Creates a new name finder with the specified model.
	  /// </summary>
	  /// <param name="mod"> The model to be used to find names.
	  /// </param>
	  /// @deprecated Use the new model API! 
	  [Obsolete("Use the new model API!")]
	  public NameFinderME(MaxentModel mod) : this(mod, new DefaultNameContextGenerator(), DEFAULT_BEAM_SIZE)
	  {
	  }

	  /// <summary>
	  /// Creates a new name finder with the specified model and context generator.
	  /// </summary>
	  /// <param name="mod"> The model to be used to find names. </param>
	  /// <param name="cg"> The context generator to be used with this name finder. </param>
	  [Obsolete]
	  public NameFinderME(MaxentModel mod, NameContextGenerator cg) : this(mod, cg, DEFAULT_BEAM_SIZE)
	  {
	  }

	  /// <summary>
	  /// Creates a new name finder with the specified model and context generator.
	  /// </summary>
	  /// <param name="mod"> The model to be used to find names. </param>
	  /// <param name="cg"> The context generator to be used with this name finder. </param>
	  /// <param name="beamSize"> The size of the beam to be used in decoding this model. </param>
	  [Obsolete]
	  public NameFinderME(MaxentModel mod, NameContextGenerator cg, int beamSize)
	  {
		model = mod;
		contextGenerator = cg;

		contextGenerator.addFeatureGenerator(new WindowFeatureGenerator(additionalContextFeatureGenerator, 8, 8));
		beam = new BeamSearch<string>(beamSize, cg, mod, new NameFinderSequenceValidator(), beamSize);
	  }

	  public static AdaptiveFeatureGenerator createFeatureGenerator()
	  {
	   return new CachedFeatureGenerator(new AdaptiveFeatureGenerator[]{ new WindowFeatureGenerator(new TokenFeatureGenerator(), 2, 2), new WindowFeatureGenerator(new TokenClassFeatureGenerator(true), 2, 2), new OutcomePriorFeatureGenerator(), new PreviousMapFeatureGenerator(), new BigramNameFeatureGenerator(), new SentenceFeatureGenerator(true, false)
	  });
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static opennlp.tools.util.featuregen.AdaptiveFeatureGenerator createFeatureGenerator(byte[] generatorDescriptor, final java.util.Map<String, Object> resources) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  private static AdaptiveFeatureGenerator createFeatureGenerator(sbyte[] generatorDescriptor, IDictionary<string, object> resources)
	  {
		AdaptiveFeatureGenerator featureGenerator;

		if (generatorDescriptor != null)
		{
		  featureGenerator = GeneratorFactory.create(new ByteArrayInputStream(generatorDescriptor), new FeatureGeneratorResourceProviderAnonymousInnerClassHelper(resources));
		}
		else
		{
		  featureGenerator = null;
		}

		return featureGenerator;
	  }

	  private class FeatureGeneratorResourceProviderAnonymousInnerClassHelper : FeatureGeneratorResourceProvider
	  {
		  private IDictionary<string, object> resources;

		  public FeatureGeneratorResourceProviderAnonymousInnerClassHelper(IDictionary<string, object> resources)
		  {
			  this.resources = resources;
		  }


		  public virtual object getResource(string key)
		  {
			if (resources != null)
			{
			  return resources[key];
			}
			return null;
		  }
	  }

	  public virtual Span[] find(string[] tokens)
	  {
		return find(tokens, EMPTY);
	  }

	  /// <summary>
	  /// Generates name tags for the given sequence, typically a sentence,
	  /// returning token spans for any identified names.
	  /// </summary>
	  /// <param name="tokens"> an array of the tokens or words of the sequence,
	  ///     typically a sentence. </param>
	  /// <param name="additionalContext"> features which are based on context outside
	  ///     of the sentence but which should also be used.
	  /// </param>
	  /// <returns> an array of spans for each of the names identified. </returns>
	  public virtual Span[] find(string[] tokens, string[][] additionalContext)
	  {
		additionalContextFeatureGenerator.CurrentContext = additionalContext;
		bestSequence = beam.bestSequence(tokens, additionalContext);

		IList<string> c = bestSequence.Outcomes;

		contextGenerator.updateAdaptiveData(tokens, c.ToArray());

		int start = -1;
		int end = -1;
		IList<Span> spans = new List<Span>(tokens.Length);
		for (int li = 0; li < c.Count; li++)
		{
		  string chunkTag = c[li];
		  if (chunkTag.EndsWith(NameFinderME.START, StringComparison.Ordinal))
		  {
			if (start != -1)
			{
			  spans.Add(new Span(start, end, extractNameType(c[li - 1])));
			}

			start = li;
			end = li + 1;

		  }
		  else if (chunkTag.EndsWith(NameFinderME.CONTINUE, StringComparison.Ordinal))
		  {
			end = li + 1;
		  }
		  else if (chunkTag.EndsWith(NameFinderME.OTHER, StringComparison.Ordinal))
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

	  /// <summary>
	  /// Forgets all adaptive data which was collected during previous
	  /// calls to one of the find methods.
	  /// 
	  /// This method is typical called at the end of a document.
	  /// </summary>
	  public virtual void clearAdaptiveData()
	  {
	   contextGenerator.clearAdaptiveData();
	  }

	  /// <summary>
	  /// Populates the specified array with the probabilities of the last decoded
	  /// sequence. The sequence was determined based on the previous call to
	  /// <code>chunk</code>. The specified array should be at least as large as
	  /// the number of tokens in the previous call to <code>chunk</code>.
	  /// </summary>
	  /// <param name="probs">
	  ///          An array used to hold the probabilities of the last decoded
	  ///          sequence. </param>
	   public virtual void probs(double[] probs)
	   {
		 bestSequence.getProbs(probs);
	   }

	  /// <summary>
	  /// Returns an array with the probabilities of the last decoded sequence.  The
	  /// sequence was determined based on the previous call to <code>chunk</code>.
	  /// </summary>
	  /// <returns> An array with the same number of probabilities as tokens were sent to <code>chunk</code>
	  /// when it was last called. </returns>
	   public virtual double[] probs()
	   {
		 return bestSequence.Probs;
	   }

	   /// <summary>
	   /// Returns an array of probabilities for each of the specified spans which is the arithmetic mean
	   /// of the probabilities for each of the outcomes which make up the span.
	   /// </summary>
	   /// <param name="spans"> The spans of the names for which probabilities are desired.
	   /// </param>
	   /// <returns> an array of probabilities for each of the specified spans. </returns>
	   public virtual double[] probs(Span[] spans)
	   {

		 double[] sprobs = new double[spans.Length];
		 double[] probs = bestSequence.Probs;

		 for (int si = 0; si < spans.Length; si++)
		 {

		   double p = 0;

		   for (int oi = spans[si].Start; oi < spans[si].End; oi++)
		   {
			 p += probs[oi];
		   }

		   p /= spans[si].length();

		   sprobs[si] = p;
		 }

		 return sprobs;
	   }

	   /// <summary>
	   /// Trains a name finder model.
	   /// </summary>
	   /// <param name="languageCode">
	   ///          the language of the training data </param>
	   /// <param name="type">
	   ///          null or an override type for all types in the training data </param>
	   /// <param name="samples">
	   ///          the training data </param>
	   /// <param name="trainParams">
	   ///          machine learning train parameters </param>
	   /// <param name="generator">
	   ///          null or the feature generator </param>
	   /// <param name="resources">
	   ///          the resources for the name finder or null if none
	   /// </param>
	   /// <returns> the newly trained model
	   /// </returns>
	   /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenNameFinderModel train(String languageCode, String type, opennlp.tools.util.ObjectStream<NameSample> samples, opennlp.tools.util.TrainingParameters trainParams, opennlp.tools.util.featuregen.AdaptiveFeatureGenerator generator, final java.util.Map<String, Object> resources) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	   public static TokenNameFinderModel train(string languageCode, string type, ObjectStream<NameSample> samples, TrainingParameters trainParams, AdaptiveFeatureGenerator generator, IDictionary<string, object> resources)
	   {

		 if (languageCode == null)
		 {
		   throw new System.ArgumentException("languageCode must not be null!");
		 }

		 IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

		 AdaptiveFeatureGenerator featureGenerator;

		 if (generator != null)
		 {
		   featureGenerator = generator;
		 }
		 else
		 {
		   featureGenerator = createFeatureGenerator();
		 }

		 AbstractModel nameFinderModel;

		 if (!TrainUtil.isSequenceTraining(trainParams.Settings))
		 {
		   EventStream eventStream = new NameFinderEventStream(samples, type, new DefaultNameContextGenerator(featureGenerator));

		   nameFinderModel = TrainUtil.train(eventStream, trainParams.Settings, manifestInfoEntries);
		 }
		 else
		 {
		   NameSampleSequenceStream ss = new NameSampleSequenceStream(samples, featureGenerator);

             // TODO replace line removed for clean compile
		     nameFinderModel = null;// TrainUtil.train(ss, trainParams.Settings, manifestInfoEntries);
		 }

		 return new TokenNameFinderModel(languageCode, nameFinderModel, resources, manifestInfoEntries);
	   }

	  /// <summary>
	  /// Trains a name finder model.
	  /// </summary>
	  /// <param name="languageCode">
	  ///          the language of the training data </param>
	  /// <param name="type">
	  ///          null or an override type for all types in the training data </param>
	  /// <param name="samples">
	  ///          the training data </param>
	  /// <param name="trainParams">
	  ///          machine learning train parameters </param>
	  /// <param name="featureGeneratorBytes">
	  ///          descriptor to configure the feature generation or null </param>
	  /// <param name="resources">
	  ///          the resources for the name finder or null if none
	  /// </param>
	  /// <returns> the newly trained model
	  /// </returns>
	  /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenNameFinderModel train(String languageCode, String type, opennlp.tools.util.ObjectStream<NameSample> samples, opennlp.tools.util.TrainingParameters trainParams, byte[] featureGeneratorBytes, final java.util.Map<String, Object> resources) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  public static TokenNameFinderModel train(string languageCode, string type, ObjectStream<NameSample> samples, TrainingParameters trainParams, sbyte[] featureGeneratorBytes, IDictionary<string, object> resources)
	  {

		TokenNameFinderModel model = train(languageCode, type, samples, trainParams, createFeatureGenerator(featureGeneratorBytes, resources), resources);

		// place the descriptor in the model
		if (featureGeneratorBytes != null)
		{
		  model = model.updateFeatureGenerator(featureGeneratorBytes);
		}

		return model;
	  }

	   /// <summary>
	   /// Trains a name finder model.
	   /// </summary>
	   /// <param name="languageCode"> the language of the training data </param>
	   /// <param name="type"> null or an override type for all types in the training data </param>
	   /// <param name="samples"> the training data </param>
	   /// <param name="iterations"> the number of iterations </param>
	   /// <param name="cutoff"> </param>
	   /// <param name="resources"> the resources for the name finder or null if none
	   /// </param>
	   /// <returns> the newly trained model
	   /// </returns>
	   /// <exception cref="IOException"> </exception>
	   /// <exception cref="ObjectStreamException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenNameFinderModel train(String languageCode, String type, opennlp.tools.util.ObjectStream<NameSample> samples, opennlp.tools.util.featuregen.AdaptiveFeatureGenerator generator, final java.util.Map<String, Object> resources, int iterations, int cutoff) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	   public static TokenNameFinderModel train(string languageCode, string type, ObjectStream<NameSample> samples, AdaptiveFeatureGenerator generator, IDictionary<string, object> resources, int iterations, int cutoff)
	   {
		 return train(languageCode, type, samples, ModelUtil.createTrainingParameters(iterations, cutoff), generator, resources);
	   }

	   /// @deprecated use <seealso cref="#train(String, String, ObjectStream, TrainingParameters, AdaptiveFeatureGenerator, Map)"/>
	   /// instead and pass in a TrainingParameters object. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("use <seealso cref="#train(String, String, opennlp.tools.util.ObjectStream, opennlp.tools.util.TrainingParameters, opennlp.tools.util.featuregen.AdaptiveFeatureGenerator, java.util.Map)"/>") public static TokenNameFinderModel train(String languageCode, String type, opennlp.tools.util.ObjectStream<NameSample> samples, final java.util.Map<String, Object> resources, int iterations, int cutoff) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  [Obsolete("use <seealso cref=\"#train(String, String, opennlp.tools.util.ObjectStream, opennlp.tools.util.TrainingParameters, opennlp.tools.util.featuregen.AdaptiveFeatureGenerator, java.util.Map)\"/>")]
	  public static TokenNameFinderModel train(string languageCode, string type, ObjectStream<NameSample> samples, IDictionary<string, object> resources, int iterations, int cutoff)
	  {
		 return train(languageCode, type, samples, (AdaptiveFeatureGenerator) null, resources, iterations, cutoff);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenNameFinderModel train(String languageCode, String type, opennlp.tools.util.ObjectStream<NameSample> samples, final java.util.Map<String, Object> resources) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	   public static TokenNameFinderModel train(string languageCode, string type, ObjectStream<NameSample> samples, IDictionary<string, object> resources)
	   {
		 return NameFinderME.train(languageCode, type, samples, resources, 100, 5);
	   }

	   /// @deprecated use <seealso cref="#train(String, String, ObjectStream, TrainingParameters, byte[], Map)"/>
	   /// instead and pass in a TrainingParameters object. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("use <seealso cref="#train(String, String, opennlp.tools.util.ObjectStream, opennlp.tools.util.TrainingParameters, byte[] , java.util.Map)"/>") public static TokenNameFinderModel train(String languageCode, String type, opennlp.tools.util.ObjectStream<NameSample> samples, byte[] generatorDescriptor, final java.util.Map<String, Object> resources, int iterations, int cutoff) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  [Obsolete("use <seealso cref=\"#train(String, String, opennlp.tools.util.ObjectStream, opennlp.tools.util.TrainingParameters, byte[] , java.util.Map)\"/>")]
	  public static TokenNameFinderModel train(string languageCode, string type, ObjectStream<NameSample> samples, sbyte[] generatorDescriptor, IDictionary<string, object> resources, int iterations, int cutoff)
	  {

		 // TODO: Pass in resource manager ...

		 AdaptiveFeatureGenerator featureGenerator = createFeatureGenerator(generatorDescriptor, resources);

		 TokenNameFinderModel model = train(languageCode, type, samples, featureGenerator, resources, iterations, cutoff);

		 if (generatorDescriptor != null)
		 {
		   model = model.updateFeatureGenerator(generatorDescriptor);
		 }

		 return model;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static opennlp.maxent.GISModel train(opennlp.model.EventStream es, int iterations, int cut) throws java.io.IOException
	  [Obsolete]
	  public static GISModel train(EventStream es, int iterations, int cut)
	  {
		return GIS.trainModel(iterations, new TwoPassDataIndexer(es, cut));
	  }

	  /// <summary>
	  /// Gets the name type from the outcome </summary>
	  /// <param name="outcome"> the outcome </param>
	  /// <returns> the name type, or null if not set </returns>
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

	  /// <summary>
	  /// Removes spans with are intersecting or crossing in anyway.
	  /// 
	  /// <para>
	  /// The following rules are used to remove the spans:<br>
	  /// Identical spans: The first span in the array after sorting it remains<br>
	  /// Intersecting spans: The first span after sorting remains<br>
	  /// Contained spans: All spans which are contained by another are removed<br>
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="spans">
	  /// </param>
	  /// <returns> non-overlapping spans </returns>
	  public static Span[] dropOverlappingSpans(Span[] spans)
	  {

		var sortedArray = spans.ToArray();

        Array.Sort(sortedArray);

	      var sortedSpans = sortedArray.ToList();

		var it = sortedSpans.GetEnumerator();


		Span lastSpan = null;

		while (it.MoveNext())
		{
		  Span span = it.Current;

		  if (lastSpan != null)
		  {
			if (lastSpan.intersects(span))
			{
			  sortedSpans.Remove(span);
			  span = lastSpan;
			}
		  }

		  lastSpan = span;
		}

		return sortedSpans.ToArray();
	  }
}

}