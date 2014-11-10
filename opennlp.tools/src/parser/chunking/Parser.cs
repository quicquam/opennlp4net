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
using j4n.Serialization;


namespace opennlp.tools.parser.chunking
{


	using AbstractModel = opennlp.model.AbstractModel;
	using MaxentModel = opennlp.model.MaxentModel;
	using TrainUtil = opennlp.model.TrainUtil;
	using TwoPassDataIndexer = opennlp.model.TwoPassDataIndexer;
	using Chunker = opennlp.tools.chunker.Chunker;
	using ChunkerME = opennlp.tools.chunker.ChunkerME;
	using ChunkerModel = opennlp.tools.chunker.ChunkerModel;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using POSModel = opennlp.tools.postag.POSModel;
	using POSTagger = opennlp.tools.postag.POSTagger;
	using POSTaggerME = opennlp.tools.postag.POSTaggerME;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;
	using TrainingParameters = opennlp.tools.util.TrainingParameters;

	/// <summary>
	/// Class for a shift reduce style parser based on Adwait Ratnaparkhi's 1998 thesis.
	/// </summary>
	public class Parser : AbstractBottomUpParser
	{

	  private MaxentModel buildModel;
	  private MaxentModel checkModel;

	  private BuildContextGenerator buildContextGenerator;
	  private CheckContextGenerator checkContextGenerator;

	  private double[] bprobs;
	  private double[] cprobs;

	  private static readonly string TOP_START = START + TOP_NODE;
	  private int topStartIndex;
	  private IDictionary<string, string> startTypeMap;
	  private IDictionary<string, string> contTypeMap;

	  private int completeIndex;
	  private int incompleteIndex;

	  public Parser(ParserModel model, int beamSize, double advancePercentage) : this(model.BuildModel, model.CheckModel, new POSTaggerME(model.ParserTaggerModel, 10, 0), new ChunkerME(model.ParserChunkerModel, ChunkerME.DEFAULT_BEAM_SIZE, new ParserChunkerSequenceValidator(model.ParserChunkerModel), new ChunkContextGenerator(ChunkerME.DEFAULT_BEAM_SIZE)), model.HeadRules, beamSize, advancePercentage)
	  {
	  }

	  public Parser(ParserModel model) : this(model, defaultBeamSize, defaultAdvancePercentage)
	  {
	  }

	  /// <summary>
	  /// Creates a new parser using the specified models and head rules. </summary>
	  /// <param name="buildModel"> The model to assign constituent labels. </param>
	  /// <param name="checkModel"> The model to determine a constituent is complete. </param>
	  /// <param name="tagger"> The model to assign pos-tags. </param>
	  /// <param name="chunker"> The model to assign flat constituent labels. </param>
	  /// <param name="headRules"> The head rules for head word perculation. </param>
	  [Obsolete]
	  public Parser(MaxentModel buildModel, MaxentModel checkModel, POSTagger tagger, Chunker chunker, HeadRules headRules) : this(buildModel,checkModel,tagger,chunker,headRules,defaultBeamSize,defaultAdvancePercentage)
	  {
	  }

	  /// <summary>
	  /// Creates a new parser using the specified models and head rules using the specified beam size and advance percentage. </summary>
	  /// <param name="buildModel"> The model to assign constituent labels. </param>
	  /// <param name="checkModel"> The model to determine a constituent is complete. </param>
	  /// <param name="tagger"> The model to assign pos-tags. </param>
	  /// <param name="chunker"> The model to assign flat constituent labels. </param>
	  /// <param name="headRules"> The head rules for head word perculation. </param>
	  /// <param name="beamSize"> The number of different parses kept during parsing. </param>
	  /// <param name="advancePercentage"> The minimal amount of probability mass which advanced outcomes must represent.
	  /// Only outcomes which contribute to the top "advancePercentage" will be explored. </param>
	  [Obsolete]
	  public Parser(MaxentModel buildModel, MaxentModel checkModel, POSTagger tagger, Chunker chunker, HeadRules headRules, int beamSize, double advancePercentage) : base(tagger, chunker, headRules, beamSize, advancePercentage)
	  {
		this.buildModel = buildModel;
		this.checkModel = checkModel;
		bprobs = new double[buildModel.NumOutcomes];
		cprobs = new double[checkModel.NumOutcomes];
		this.buildContextGenerator = new BuildContextGenerator();
		this.checkContextGenerator = new CheckContextGenerator();
		startTypeMap = new Dictionary<string, string>();
		contTypeMap = new Dictionary<string, string>();
		for (int boi = 0, bon = buildModel.NumOutcomes; boi < bon; boi++)
		{
		  string outcome = buildModel.getOutcome(boi);
		  if (outcome.StartsWith(START, StringComparison.Ordinal))
		  {
			//System.err.println("startMap "+outcome+"->"+outcome.substring(START.length()));
			startTypeMap[outcome] = outcome.Substring(START.Length);
		  }
		  else if (outcome.StartsWith(CONT, StringComparison.Ordinal))
		  {
			//System.err.println("contMap "+outcome+"->"+outcome.substring(CONT.length()));
			contTypeMap[outcome] = outcome.Substring(CONT.Length);
		  }
		}
		topStartIndex = buildModel.getIndex(TOP_START);
		completeIndex = checkModel.getIndex(COMPLETE);
		incompleteIndex = checkModel.getIndex(INCOMPLETE);
	  }

	  protected internal override void advanceTop(Parse p)
	  {
		buildModel.eval(buildContextGenerator.getContext(p.Children, 0), bprobs);
		p.addProb(Math.Log(bprobs[topStartIndex]));
		checkModel.eval(checkContextGenerator.getContext(p.Children, TOP_NODE, 0, 0), cprobs);
		p.addProb(Math.Log(cprobs[completeIndex]));
		p.Type = TOP_NODE;
	  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override protected opennlp.tools.parser.Parse[] advanceParses(final opennlp.tools.parser.Parse p, double probMass)
	  protected internal override Parse[] advanceParses(Parse p, double probMass)
	  {
		double q = 1 - probMass;
		/// <summary>
		/// The closest previous node which has been labeled as a start node. </summary>
		Parse lastStartNode = null;
		/// <summary>
		/// The index of the closest previous node which has been labeled as a start node. </summary>
		int lastStartIndex = -1;
		/// <summary>
		/// The type of the closest previous node which has been labeled as a start node. </summary>
		string lastStartType = null;
		/// <summary>
		/// The index of the node which will be labeled in this iteration of advancing the parse. </summary>
		int advanceNodeIndex;
		/// <summary>
		/// The node which will be labeled in this iteration of advancing the parse. </summary>
		Parse advanceNode = null;
		Parse[] originalChildren = p.Children;
		Parse[] children = collapsePunctuation(originalChildren,punctSet);
		int numNodes = children.Length;
		if (numNodes == 0)
		{
		  return null;
		}
		//determines which node needs to be labeled and prior labels.
		for (advanceNodeIndex = 0; advanceNodeIndex < numNodes; advanceNodeIndex++)
		{
		  advanceNode = children[advanceNodeIndex];
		  if (advanceNode.Label == null)
		  {
			break;
		  }
		  else if (startTypeMap.ContainsKey(advanceNode.Label))
		  {
			lastStartType = startTypeMap[advanceNode.Label];
			lastStartNode = advanceNode;
			lastStartIndex = advanceNodeIndex;
			//System.err.println("lastStart "+i+" "+lastStart.label+" "+lastStart.prob);
		  }
		}
		int originalAdvanceIndex = mapParseIndex(advanceNodeIndex,children,originalChildren);
		IList<Parse> newParsesList = new List<Parse>(buildModel.NumOutcomes);
		//call build
		buildModel.eval(buildContextGenerator.getContext(children, advanceNodeIndex), bprobs);
		double bprobSum = 0;
		while (bprobSum < probMass)
		{
		  // The largest unadvanced labeling.
		  int max = 0;
		  for (int pi = 1; pi < bprobs.Length; pi++) //for each build outcome
		  {
			if (bprobs[pi] > bprobs[max])
			{
			  max = pi;
			}
		  }
		  if (bprobs[max] == 0)
		  {
			break;
		  }
		  double bprob = bprobs[max];
		  bprobs[max] = 0; //zero out so new max can be found
		  bprobSum += bprob;
		  string tag = buildModel.getOutcome(max);
		  //System.out.println("trying "+tag+" "+bprobSum+" lst="+lst);
		  if (max == topStartIndex) // can't have top until complete
		  {
			continue;
		  }
		  //System.err.println(i+" "+tag+" "+bprob);
		  if (startTypeMap.ContainsKey(tag)) //update last start
		  {
			lastStartIndex = advanceNodeIndex;
			lastStartNode = advanceNode;
			lastStartType = startTypeMap[tag];
		  }
		  else if (contTypeMap.ContainsKey(tag))
		  {
			if (lastStartNode == null || !lastStartType.Equals(contTypeMap[tag]))
			{
			  continue; //Cont must match previous start or continue
			}
		  }
		  Parse newParse1 = (Parse) p.Clone(); //clone parse
		  if (createDerivationString)
		  {
			  newParse1.Derivation.Append(max).Append("-");
		  }
		  newParse1.setChild(originalAdvanceIndex,tag); //replace constituent being labeled to create new derivation
		  newParse1.addProb(Math.Log(bprob));
		  //check
		  //String[] context = checkContextGenerator.getContext(newParse1.getChildren(), lastStartType, lastStartIndex, advanceNodeIndex);
		  checkModel.eval(checkContextGenerator.getContext(collapsePunctuation(newParse1.Children,punctSet), lastStartType, lastStartIndex, advanceNodeIndex), cprobs);
		  //System.out.println("check "+lastStartType+" "+cprobs[completeIndex]+" "+cprobs[incompleteIndex]+" "+tag+" "+java.util.Arrays.asList(context));
		  Parse newParse2 = newParse1;
		  if (cprobs[completeIndex] > q) //make sure a reduce is likely
		  {
			newParse2 = (Parse) newParse1.clone(null);
			if (createDerivationString)
			{
				newParse2.Derivation.Append(1).Append(".");
			}
			newParse2.addProb(Math.Log(cprobs[completeIndex]));
			Parse[] cons = new Parse[advanceNodeIndex - lastStartIndex + 1];
			bool flat = true;
			//first
			cons[0] = lastStartNode;
			flat &= cons[0].PosTag;
			//last
			cons[advanceNodeIndex - lastStartIndex] = advanceNode;
			flat &= cons[advanceNodeIndex - lastStartIndex].PosTag;
			//middle
			for (int ci = 1; ci < advanceNodeIndex - lastStartIndex; ci++)
			{
			  cons[ci] = children[ci + lastStartIndex];
			  flat &= cons[ci].PosTag;
			}
			if (!flat) //flat chunks are done by chunker
			{
			  if (lastStartIndex == 0 && advanceNodeIndex == numNodes - 1) //check for top node to include end and begining punctuation
			  {
				//System.err.println("ParserME.advanceParses: reducing entire span: "+new Span(lastStartNode.getSpan().getStart(), advanceNode.getSpan().getEnd())+" "+lastStartType+" "+java.util.Arrays.asList(children));
				newParse2.insert(new Parse(p.Text, p.Span, lastStartType, cprobs[1], headRules.getHead(cons, lastStartType)));
			  }
			  else
			  {
				newParse2.insert(new Parse(p.Text, new Span(lastStartNode.Span.Start, advanceNode.Span.End), lastStartType, cprobs[1], headRules.getHead(cons, lastStartType)));
			  }
			  newParsesList.Add(newParse2);
			}
		  }
		  if (cprobs[incompleteIndex] > q) //make sure a shift is likely
		  {
			if (createDerivationString)
			{
				newParse1.Derivation.Append(0).Append(".");
			}
			if (advanceNodeIndex != numNodes - 1) //can't shift last element
			{
			  newParse1.addProb(Math.Log(cprobs[incompleteIndex]));
			  newParsesList.Add(newParse1);
			}
		  }
		}
        Parse[] newParses = newParsesList.ToArray();		
		return newParses;
	  }

	  /// @deprecated Please do not use anymore, use the ObjectStream train methods instead! This method
	  /// will be removed soon. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("Please do not use anymore, use the ObjectStream train methods instead! This method") public static opennlp.model.AbstractModel train(opennlp.model.EventStream es, int iterations, int cut) throws java.io.IOException
	  [Obsolete("Please do not use anymore, use the ObjectStream train methods instead! This method")]
	  public static AbstractModel train(opennlp.model.EventStream es, int iterations, int cut)
	  {
		return opennlp.maxent.GIS.trainModel(iterations, new TwoPassDataIndexer(es, cut));
	  }

	  public static void mergeReportIntoManifest(IDictionary<string, string> manifest, IDictionary<string, string> report, string @namespace)
	  {

		foreach (KeyValuePair<string, string> entry in report)
		{
		  manifest[@namespace + "." + entry.Key] = entry.Value;
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.tools.parser.ParserModel train(String languageCode, opennlp.tools.util.ObjectStream<opennlp.tools.parser.Parse> parseSamples, opennlp.tools.parser.HeadRules rules, opennlp.tools.util.TrainingParameters mlParams) throws java.io.IOException
	  public static ParserModel train(string languageCode, ObjectStream<Parse> parseSamples, HeadRules rules, TrainingParameters mlParams)
	  {

		Console.Error.WriteLine("Building dictionary");

		Dictionary mdict = buildDictionary(parseSamples, rules, mlParams);

		parseSamples.reset();

		IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

		// build
		Console.Error.WriteLine("Training builder");
		opennlp.model.EventStream bes = new ParserEventStream(parseSamples, rules, ParserEventTypeEnum.BUILD, mdict);
		IDictionary<string, string> buildReportMap = new Dictionary<string, string>();
		AbstractModel buildModel = TrainUtil.train(bes, mlParams.getSettings("build"), buildReportMap);
		mergeReportIntoManifest(manifestInfoEntries, buildReportMap, "build");

		parseSamples.reset();

		// tag
		POSModel posModel = POSTaggerME.train(languageCode, new PosSampleStream(parseSamples), mlParams.getParameters("tagger"), null, null);

		parseSamples.reset();

		// chunk
		ChunkerModel chunkModel = ChunkerME.train(languageCode, new ChunkSampleStream(parseSamples), new ChunkContextGenerator(), mlParams.getParameters("chunker"));

		parseSamples.reset();

		// check
		Console.Error.WriteLine("Training checker");
		opennlp.model.EventStream kes = new ParserEventStream(parseSamples, rules, ParserEventTypeEnum.CHECK);
		IDictionary<string, string> checkReportMap = new Dictionary<string, string>();
		AbstractModel checkModel = TrainUtil.train(kes, mlParams.getSettings("check"), checkReportMap);
		mergeReportIntoManifest(manifestInfoEntries, checkReportMap, "check");

		// TODO: Remove cast for HeadRules
		return new ParserModel(languageCode, buildModel, checkModel, posModel, chunkModel, (opennlp.tools.parser.lang.en.HeadRules) rules, ParserType.CHUNKING, manifestInfoEntries);
	  }

	  /// @deprecated use <seealso cref="#train(String, ObjectStream, HeadRules, TrainingParameters)"/>
	  /// instead and pass in a TrainingParameters object. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("use <seealso cref="#train(String, opennlp.tools.util.ObjectStream, opennlp.tools.parser.HeadRules, opennlp.tools.util.TrainingParameters)"/>") public static opennlp.tools.parser.ParserModel train(String languageCode, opennlp.tools.util.ObjectStream<opennlp.tools.parser.Parse> parseSamples, opennlp.tools.parser.HeadRules rules, int iterations, int cut) throws java.io.IOException
	  [Obsolete("use <seealso cref=\"#train(String, opennlp.tools.util.ObjectStream, opennlp.tools.parser.HeadRules, opennlp.tools.util.TrainingParameters)\"/>")]
	  public static ParserModel train(string languageCode, ObjectStream<Parse> parseSamples, HeadRules rules, int iterations, int cut)
	  {

		TrainingParameters @params = new TrainingParameters();
		@params.put("dict", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));

        @params.put("tagger", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
        @params.put("tagger", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));
        @params.put("chunker", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
        @params.put("chunker", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));
        @params.put("check", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
        @params.put("check", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));
        @params.put("build", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
        @params.put("build", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));

		return train(languageCode, parseSamples, rules, @params);
	  }
	}

}