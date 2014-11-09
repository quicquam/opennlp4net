using System;

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

namespace opennlp.tools.coref
{

	using MentionContext = opennlp.tools.coref.mention.MentionContext;
	using PTBHeadFinder = opennlp.tools.coref.mention.PTBHeadFinder;
	using ShallowParseMentionFinder = opennlp.tools.coref.mention.ShallowParseMentionFinder;
	using AbstractResolver = opennlp.tools.coref.resolver.AbstractResolver;
	using CommonNounResolver = opennlp.tools.coref.resolver.CommonNounResolver;
	using DefiniteNounResolver = opennlp.tools.coref.resolver.DefiniteNounResolver;
	using FixedNonReferentialResolver = opennlp.tools.coref.resolver.FixedNonReferentialResolver;
	using IsAResolver = opennlp.tools.coref.resolver.IsAResolver;
	using MaxentResolver = opennlp.tools.coref.resolver.MaxentResolver;
	using NonReferentialResolver = opennlp.tools.coref.resolver.NonReferentialResolver;
	using PerfectResolver = opennlp.tools.coref.resolver.PerfectResolver;
	using PluralNounResolver = opennlp.tools.coref.resolver.PluralNounResolver;
	using PluralPronounResolver = opennlp.tools.coref.resolver.PluralPronounResolver;
	using ProperNounResolver = opennlp.tools.coref.resolver.ProperNounResolver;
	using ResolverMode = opennlp.tools.coref.resolver.ResolverMode;
	using SingularPronounResolver = opennlp.tools.coref.resolver.SingularPronounResolver;
	using SpeechPronounResolver = opennlp.tools.coref.resolver.SpeechPronounResolver;
	using Gender = opennlp.tools.coref.sim.Gender;
	using MaxentCompatibilityModel = opennlp.tools.coref.sim.MaxentCompatibilityModel;
	using Number = opennlp.tools.coref.sim.Number;
	using SimilarityModel = opennlp.tools.coref.sim.SimilarityModel;

	/// <summary>
	/// This class perform coreference for treebank style parses or for noun-phrase chunked data.
	/// Non-constituent entities such as pre-nominal named-entities and sub entities in simple coordinated
	/// noun phases will be created.  This linker requires that named-entity information also be provided.
	/// This information can be added to the parse using the -parse option with EnglishNameFinder.
	/// </summary>
	public class DefaultLinker : AbstractLinker
	{

	  protected internal MaxentCompatibilityModel mcm;

	  /// <summary>
	  /// Creates a new linker with the specified model directory, running in the specified mode. </summary>
	  /// <param name="modelDirectory"> The directory where the models for this linker are kept. </param>
	  /// <param name="mode"> The mode that this linker is running in. </param>
	  /// <exception cref="IOException"> when the models can not be read or written to based on the mode. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DefaultLinker(String modelDirectory, LinkerMode mode) throws java.io.IOException
	  public DefaultLinker(string modelDirectory, LinkerMode mode) : this(modelDirectory,mode,true,-1)
	  {
	  }

	  /// <summary>
	  /// Creates a new linker with the specified model directory, running in the specified mode which uses a discourse model
	  /// based on the specified parameter. </summary>
	  /// <param name="modelDirectory"> The directory where the models for this linker are kept. </param>
	  /// <param name="mode"> The mode that this linker is running in. </param>
	  /// <param name="useDiscourseModel"> Whether the model should use a discourse model or not. </param>
	  /// <exception cref="IOException"> when the models can not be read or written to based on the mode. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DefaultLinker(String modelDirectory, LinkerMode mode, boolean useDiscourseModel) throws java.io.IOException
	  public DefaultLinker(string modelDirectory, LinkerMode mode, bool useDiscourseModel) : this(modelDirectory,mode,useDiscourseModel,-1)
	  {
	  }

	  /// <summary>
	  /// Creates a new linker with the specified model directory, running in the specified mode which uses a discourse model
	  /// based on the specified parameter and uses the specified fixed non-referential probability. </summary>
	  /// <param name="modelDirectory"> The directory where the models for this linker are kept. </param>
	  /// <param name="mode"> The mode that this linker is running in. </param>
	  /// <param name="useDiscourseModel"> Whether the model should use a discourse model or not. </param>
	  /// <param name="fixedNonReferentialProbability"> The probability which resolvers are required to exceed to positi a coreference relationship. </param>
	  /// <exception cref="IOException"> when the models can not be read or written to based on the mode. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DefaultLinker(String modelDirectory, LinkerMode mode, boolean useDiscourseModel, double fixedNonReferentialProbability) throws java.io.IOException
	  public DefaultLinker(string modelDirectory, LinkerMode mode, bool useDiscourseModel, double fixedNonReferentialProbability) : base(modelDirectory, mode, useDiscourseModel)
	  {
		if (mode != LinkerMode.SIM)
		{
		  mcm = new MaxentCompatibilityModel(corefProject);
		}
		initHeadFinder();
		initMentionFinder();
		if (mode != LinkerMode.SIM)
		{
		  initResolvers(mode, fixedNonReferentialProbability);
		  entities = new DiscourseEntity[resolvers.Length];
		}
	  }

	  /// <summary>
	  /// Initializes the resolvers used by this linker. </summary>
	  /// <param name="mode"> The mode in which this linker is being used. </param>
	  /// <param name="fixedNonReferentialProbability"> </param>
	  /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void initResolvers(LinkerMode mode, double fixedNonReferentialProbability) throws java.io.IOException
	  protected internal virtual void initResolvers(LinkerMode mode, double fixedNonReferentialProbability)
	  {
		if (mode == LinkerMode.TRAIN)
		{
		  mentionFinder.PrenominalNamedEntityCollection = false;
		  mentionFinder.CoordinatedNounPhraseCollection = false;
		}
		SINGULAR_PRONOUN = 0;
		if (LinkerMode.TEST == mode || LinkerMode.EVAL == mode)
		{
		  if (fixedNonReferentialProbability < 0)
		  {
			resolvers = new MaxentResolver[] {new SingularPronounResolver(corefProject, ResolverMode.TEST), new ProperNounResolver(corefProject, ResolverMode.TEST), new DefiniteNounResolver(corefProject, ResolverMode.TEST), new IsAResolver(corefProject, ResolverMode.TEST), new PluralPronounResolver(corefProject, ResolverMode.TEST), new PluralNounResolver(corefProject, ResolverMode.TEST), new CommonNounResolver(corefProject, ResolverMode.TEST), new SpeechPronounResolver(corefProject, ResolverMode.TEST)};
		  }
		  else
		  {
			NonReferentialResolver nrr = new FixedNonReferentialResolver(fixedNonReferentialProbability);
			resolvers = new MaxentResolver[] {new SingularPronounResolver(corefProject, ResolverMode.TEST,nrr), new ProperNounResolver(corefProject, ResolverMode.TEST,nrr), new DefiniteNounResolver(corefProject, ResolverMode.TEST,nrr), new IsAResolver(corefProject, ResolverMode.TEST,nrr), new PluralPronounResolver(corefProject, ResolverMode.TEST,nrr), new PluralNounResolver(corefProject, ResolverMode.TEST,nrr), new CommonNounResolver(corefProject, ResolverMode.TEST,nrr), new SpeechPronounResolver(corefProject, ResolverMode.TEST,nrr)};
		  }
		  if (LinkerMode.EVAL == mode)
		  {
			//String[] names = {"Pronoun", "Proper", "Def-NP", "Is-a", "Plural Pronoun"};
			//eval = new Evaluation(names);
		  }
		  MaxentResolver.SimilarityModel = SimilarityModel.PrepAttachDataUtil.testModel(corefProject + "/sim");
		}
		else if (LinkerMode.TRAIN == mode)
		{
		  resolvers = new AbstractResolver[9];
		  resolvers[0] = new SingularPronounResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[1] = new ProperNounResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[2] = new DefiniteNounResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[3] = new IsAResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[4] = new PluralPronounResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[5] = new PluralNounResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[6] = new CommonNounResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[7] = new SpeechPronounResolver(corefProject, ResolverMode.TRAIN);
		  resolvers[8] = new PerfectResolver();
		}
		else
		{
		  Console.Error.WriteLine("DefaultLinker: Invalid Mode");
		}
	  }

	  /// <summary>
	  /// Initializes the head finder for this linker.
	  /// </summary>
	  protected internal virtual void initHeadFinder()
	  {
		headFinder = PTBHeadFinder.Instance;
	  }
	  /// <summary>
	  /// Initializes the mention finder for this linker.
	  /// This can be over-ridden to change the space of mentions used for coreference.
	  /// </summary>
	  protected internal virtual void initMentionFinder()
	  {
		mentionFinder = ShallowParseMentionFinder.getInstance(headFinder);
	  }

	  protected internal override Gender computeGender(MentionContext mention)
	  {
		return mcm.computeGender(mention);
	  }

	  protected internal override Number computeNumber(MentionContext mention)
	  {
		return mcm.computeNumber(mention);
	  }
	}

}