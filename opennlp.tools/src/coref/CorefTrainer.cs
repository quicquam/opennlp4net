using System;
using System.Collections.Generic;
using System.Linq;
using j4n.Serialization;

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

    // was opennlp.tools.coref.mention.DefaultParse
	using DefaultParse = opennlp.tools.coref.mention.StubDefaultParse;
	using Mention = opennlp.tools.coref.mention.Mention;
	using MentionContext = opennlp.tools.coref.mention.MentionContext;
	using MentionFinder = opennlp.tools.coref.mention.MentionFinder;
	using MaxentResolver = opennlp.tools.coref.resolver.MaxentResolver;
	using GenderModel = opennlp.tools.coref.sim.GenderModel;
	using NumberModel = opennlp.tools.coref.sim.NumberModel;
	using SimilarityModel = opennlp.tools.coref.sim.SimilarityModel;
	using TrainSimilarityModel = opennlp.tools.coref.sim.TrainSimilarityModel;
	using Parse = opennlp.tools.parser.Parse;
	using opennlp.tools.util;

	public class CorefTrainer
	{

	  private static bool containsToken(string token, Parse p)
	  {
		foreach (Parse node in p.TagNodes)
		{
		  if (node.CoveredText.Equals(token))
		  {
			return true;
		  }
		}
		return false;
	  }

	  private static Mention[] getMentions(CorefSample sample, MentionFinder mentionFinder)
	  {

		IList<Mention> mentions = new List<Mention>();

		foreach (opennlp.tools.coref.mention.Parse corefParse in sample.Parses)
		{

		  Parse p = ((DefaultParse) corefParse).Parse;

		  Mention[] extents = mentionFinder.getMentions(corefParse);

		  for (int ei = 0, en = extents.Length; ei < en;ei++)
		  {

			if (extents[ei].Parse == null)
			{

			  Stack<Parse> nodes = new Stack<Parse>();
			  nodes.Push(p);

			  while (nodes.Count > 0)
			  {

				Parse node = nodes.Pop();

				if (node.Span.Equals(extents[ei].Span) && node.Type.StartsWith("NML", StringComparison.Ordinal))
				{
				  DefaultParse corefParseNode = new DefaultParse(node, corefParse.SentenceNumber);
				  extents[ei].Parse = corefParseNode;
				  extents[ei].Id = corefParseNode.EntityId;
				  break;
				}
                foreach (var child in node.Children)
			      {
			          nodes.Push(child);
			      }
			  }
			}
		  }
		    foreach (var mention in extents)
		    {
		        mentions.Add(mention);
		    }
		}

		return mentions.ToArray();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void train(String modelDirectory, opennlp.tools.util.ObjectStream<CorefSample> samples, boolean useTreebank, boolean useDiscourseModel) throws java.io.IOException
	  public static void train(string modelDirectory, ObjectStream<CorefSample> samples, bool useTreebank, bool useDiscourseModel)
	  {

		TrainSimilarityModel simTrain = SimilarityModel.trainModel(modelDirectory + "/coref/sim");
		TrainSimilarityModel genTrain = GenderModel.trainModel(modelDirectory + "/coref/gen");
		TrainSimilarityModel numTrain = NumberModel.trainModel(modelDirectory + "/coref/num");

		useTreebank = true;

		Linker simLinker;

		if (useTreebank)
		{
		  simLinker = new TreebankLinker(modelDirectory + "/coref/", LinkerMode.SIM);
		}
		else
		{
		  simLinker = new DefaultLinker(modelDirectory + "/coref/",LinkerMode.SIM);
		}

		// TODO: Feed with training data ...
		for (CorefSample sample = samples.read(); sample != null; sample = samples.read())
		{

		  Mention[] mentions = getMentions(sample, simLinker.MentionFinder);
		  MentionContext[] extentContexts = simLinker.constructMentionContexts(mentions);

		  simTrain.Extents = extentContexts;
		  genTrain.Extents = extentContexts;
		  numTrain.Extents = extentContexts;
		}

		simTrain.trainModel();
		genTrain.trainModel();
		numTrain.trainModel();

		MaxentResolver.SimilarityModel = SimilarityModel.testModel(modelDirectory + "/coref" + "/sim");

		// Done with similarity training

		// Now train the linkers

		// Training data needs to be read in again and the stream must be reset
		samples.reset();

		// Now train linkers
		Linker trainLinker;
		if (useTreebank)
		{
		  trainLinker = new TreebankLinker(modelDirectory + "/coref/", LinkerMode.TRAIN, useDiscourseModel);
		}
		else
		{
		  trainLinker = new DefaultLinker(modelDirectory + "/coref/", LinkerMode.TRAIN, useDiscourseModel);
		}

		for (CorefSample sample = samples.read(); sample != null; sample = samples.read())
		{

		  Mention[] mentions = getMentions(sample, trainLinker.MentionFinder);
		  trainLinker.Entities = mentions;
		}

		trainLinker.train();
	  }
	}

}