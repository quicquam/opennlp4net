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
using System.IO;
using j4n.Exceptions;
using j4n.IO.InputStream;

namespace opennlp.tools.namefind
{


//	using ChunkerContextGenerator = opennlp.tools.chunker.ChunkerContextGenerator;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using FeatureGeneratorCreationError = opennlp.tools.namefind.TokenNameFinderModel.FeatureGeneratorCreationError;
//	using POSTaggerFactory = opennlp.tools.postag.POSTaggerFactory;
//	using TagDictionary = opennlp.tools.postag.TagDictionary;
	using BaseToolFactory = opennlp.tools.util.BaseToolFactory;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using SequenceCodec = opennlp.tools.util.SequenceCodec<string>;
	using SequenceValidator = opennlp.tools.util.SequenceValidator<string>;
    using ExtensionLoader = opennlp.tools.util.ext.ExtensionLoader<TokenNameFinderFactory>;
	using AdaptiveFeatureGenerator = opennlp.tools.util.featuregen.AdaptiveFeatureGenerator;
	using AdditionalContextFeatureGenerator = opennlp.tools.util.featuregen.AdditionalContextFeatureGenerator;
	using AggregatedFeatureGenerator = opennlp.tools.util.featuregen.AggregatedFeatureGenerator;
	using FeatureGeneratorResourceProvider = opennlp.tools.util.featuregen.FeatureGeneratorResourceProvider;
	using GeneratorFactory = opennlp.tools.util.featuregen.GeneratorFactory;

	// Idea of this factory is that most resources/impls used by the name finder
	// can be modified through this class!
	// That only works if thats the central class used for training/runtime

	public class TokenNameFinderFactory : BaseToolFactory
	{

	  private sbyte[] featureGeneratorBytes;
	  private IDictionary<string, object> resources;
	  private SequenceCodec seqCodec;

	  /// <summary>
	  /// Creates a <seealso cref="TokenNameFinderFactory"/> that provides the default implementation
	  /// of the resources.
	  /// </summary>
	  public TokenNameFinderFactory()
	  {
		this.seqCodec = new BioCodec();
	  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public TokenNameFinderFactory(byte[] featureGeneratorBytes, final java.util.Map<String, Object> resources, opennlp.tools.util.SequenceCodec<String> seqCodec)
	  public TokenNameFinderFactory(sbyte[] featureGeneratorBytes, IDictionary<string, object> resources, SequenceCodec seqCodec)
	  {
		init(featureGeneratorBytes, resources, seqCodec);
	  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: void init(byte[] featureGeneratorBytes, final java.util.Map<String, Object> resources, opennlp.tools.util.SequenceCodec<String> seqCodec)
	  internal virtual void init(sbyte[] featureGeneratorBytes, IDictionary<string, object> resources, SequenceCodec seqCodec)
	  {
		this.featureGeneratorBytes = featureGeneratorBytes;
		this.resources = resources;
		this.seqCodec = seqCodec;
	  }

	  protected internal virtual SequenceCodec SequenceCodec
	  {
		  get
		  {
			return seqCodec;
		  }
	  }

	  protected internal virtual IDictionary<string, object> Resources
	  {
		  get
		  {
			return resources;
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenNameFinderFactory create(String subclassName, byte[] featureGeneratorBytes, final java.util.Map<String, Object> resources, opennlp.tools.util.SequenceCodec<String> seqCodec) throws opennlp.tools.util.InvalidFormatException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  public static TokenNameFinderFactory create(string subclassName, sbyte[] featureGeneratorBytes, IDictionary<string, object> resources, SequenceCodec seqCodec)
	  {
		if (subclassName == null)
		{
		  // will create the default factory
		  return new TokenNameFinderFactory();
		}
		try
		{
           TokenNameFinderFactory theFactory = ExtensionLoader.instantiateExtension(subclassName);
		  theFactory.init(featureGeneratorBytes, resources, seqCodec);
		  return theFactory;
		}
		catch (Exception e)
		{
		  string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
		  Console.Error.WriteLine(msg);
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		  throw new InvalidFormatException(msg, e);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void validateArtifactMap() throws opennlp.tools.util.InvalidFormatException
	  public override void validateArtifactMap()
	  {
		// no additional artifacts
	  }

	  public virtual SequenceCodec createSequenceCodec()
	  {

		if (artifactProvider != null)
		{
		  string sequeceCodecImplName = artifactProvider.getManifestProperty(TokenNameFinderModel.SEQUENCE_CODEC_CLASS_NAME_PARAMETER);
		  return instantiateSequenceCodec(sequeceCodecImplName);
		}
		else
		{
		  return seqCodec;
		}
	  }

	  public virtual NameContextGenerator createContextGenerator()
	  {

		AdaptiveFeatureGenerator featureGenerator = createFeatureGenerators();

		if (featureGenerator == null)
		{
		  featureGenerator = NameFinderME.createFeatureGenerator();
		}

		return new DefaultNameContextGenerator(featureGenerator);
	  }

	  /// <summary>
	  /// Creates the <seealso cref="AdaptiveFeatureGenerator"/>. Usually this
	  /// is a set of generators contained in the <seealso cref="AggregatedFeatureGenerator"/>.
	  /// 
	  /// Note:
	  /// The generators are created on every call to this method.
	  /// </summary>
	  /// <returns> the feature generator or null if there is no descriptor in the model </returns>
	  // TODO: During training time the resources need to be loaded from the resources map!
	  public virtual AdaptiveFeatureGenerator createFeatureGenerators()
	  {

		sbyte[] descriptorBytes = null;
		if (featureGeneratorBytes == null && artifactProvider != null)
		{
		  descriptorBytes = (sbyte[]) artifactProvider.getArtifact<sbyte[]>(TokenNameFinderModel.GENERATOR_DESCRIPTOR_ENTRY_NAME);
		}
		else
		{
		  descriptorBytes = featureGeneratorBytes;
		}

		if (descriptorBytes != null)
		{
		  InputStream descriptorIn = new ByteArrayInputStream(descriptorBytes);

		  AdaptiveFeatureGenerator generator = null;
		  try
		  {
			generator = GeneratorFactory.create(descriptorIn, new FeatureGeneratorResourceProviderAnonymousInnerClassHelper(this));
		  }
		  catch (InvalidFormatException e)
		  {
			// It is assumed that the creation of the feature generation does not
			// fail after it succeeded once during model loading.

			// But it might still be possible that such an exception is thrown,
			// in this case the caller should not be forced to handle the exception
			// and a Runtime Exception is thrown instead.

			// If the re-creation of the feature generation fails it is assumed
			// that this can only be caused by a programming mistake and therefore
			// throwing a Runtime Exception is reasonable

			throw new FeatureGeneratorCreationError(e);
		  }
		  catch (IOException e)
		  {
			throw new IllegalStateException("Reading from mem cannot result in an I/O error", e);
		  }

		  return generator;
		}
		else
		{
		  return null;
		}
	  }

	  private class FeatureGeneratorResourceProviderAnonymousInnerClassHelper : FeatureGeneratorResourceProvider
	  {
		  private readonly TokenNameFinderFactory outerInstance;

		  public FeatureGeneratorResourceProviderAnonymousInnerClassHelper(TokenNameFinderFactory outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }


		  public virtual object getResource(string key)
		  {
/*			if (artifactProvider != null)
			{
			  return artifactProvider.getArtifact(key);
			}
			else */
			{
			  return outerInstance.resources[key];
			}
		  }
	  }

	  public static SequenceCodec instantiateSequenceCodec(string sequenceCodecImplName)
	  {

	/*	if (sequenceCodecImplName != null)
		{
		  return ExtensionLoader.instantiateExtension<TokenNameFinderModel>(sequenceCodecImplName);
		}
		else */
		{
		  // If nothing is specified return old default!
		  return new BioCodec();
		}
	  }
	}


}