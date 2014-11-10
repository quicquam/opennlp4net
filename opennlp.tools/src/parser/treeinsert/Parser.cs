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

namespace opennlp.tools.parser.treeinsert
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
	using TrainingParameters = opennlp.tools.util.TrainingParameters;

	/// <summary>
	/// Built/attach parser.  Nodes are built when their left-most
	/// child is encountered.  Subsequent children are attached as
	/// daughters.  Attachment is based on node in the right-frontier
	/// of the tree.  After each attachment or building, nodes are
	/// assesed as either complete or incomplete.  Complete nodes
	/// are no longer elligable for daughter attachment.
	/// Complex modifiers which produce additional node
	/// levels of the same type are attached with sister-adjunction.
	/// Attachment can not take place higher in the right-frontier
	/// than an incomplete node.
	/// </summary>
	public class Parser : AbstractBottomUpParser
	{

	  /// <summary>
	  /// Outcome used when a constituent needs an no additional parent node/building. </summary>
	  public const string DONE = "d";

	  /// <summary>
	  /// Outcome used when a node should be attached as a sister to another node. </summary>
	  public const string ATTACH_SISTER = "s";
	  /// <summary>
	  /// Outcome used when a node should be attached as a daughter to another node. </summary>
	  public const string ATTACH_DAUGHTER = "d";
	  /// <summary>
	  /// Outcome used when a node should not be attached to another node. </summary>
	  public const string NON_ATTACH = "n";

	  /// <summary>
	  /// Label used to distinguish build nodes from non-built nodes. </summary>
	  public const string BUILT = "built";
	  private MaxentModel buildModel;
	  private MaxentModel attachModel;
	  private MaxentModel checkModel;

	  internal static bool checkComplete = false;

	  private BuildContextGenerator buildContextGenerator;
	  private AttachContextGenerator attachContextGenerator;
	  private CheckContextGenerator checkContextGenerator;

	  private double[] bprobs;
	  private double[] aprobs;
	  private double[] cprobs;

	  private int doneIndex;
	  private int sisterAttachIndex;
	  private int daughterAttachIndex;
	  private int nonAttachIndex;
	  private int completeIndex;

	  private int[] attachments;

	  public Parser(ParserModel model, int beamSize, double advancePercentage) : this(model.BuildModel, model.AttachModel, model.CheckModel, new POSTaggerME(model.ParserTaggerModel), new ChunkerME(model.ParserChunkerModel, ChunkerME.DEFAULT_BEAM_SIZE, new ParserChunkerSequenceValidator(model.ParserChunkerModel), new ChunkContextGenerator(ChunkerME.DEFAULT_BEAM_SIZE)), model.HeadRules, beamSize, advancePercentage)
	  {
	  }

	  public Parser(ParserModel model) : this(model, defaultBeamSize, defaultAdvancePercentage)
	  {
	  }

	  [Obsolete]
	  public Parser(AbstractModel buildModel, AbstractModel attachModel, AbstractModel checkModel, POSTagger tagger, Chunker chunker, HeadRules headRules, int beamSize, double advancePercentage) : base(tagger,chunker,headRules,beamSize,advancePercentage)
	  {
		this.buildModel = buildModel;
		this.attachModel = attachModel;
		this.checkModel = checkModel;

		this.buildContextGenerator = new BuildContextGenerator();
		this.attachContextGenerator = new AttachContextGenerator(punctSet);
		this.checkContextGenerator = new CheckContextGenerator(punctSet);

		this.bprobs = new double[buildModel.NumOutcomes];
		this.aprobs = new double[attachModel.NumOutcomes];
		this.cprobs = new double[checkModel.NumOutcomes];

		this.doneIndex = buildModel.getIndex(DONE);
		this.sisterAttachIndex = attachModel.getIndex(ATTACH_SISTER);
		this.daughterAttachIndex = attachModel.getIndex(ATTACH_DAUGHTER);
		this.nonAttachIndex = attachModel.getIndex(NON_ATTACH);
		attachments = new int[] {daughterAttachIndex,sisterAttachIndex};
		this.completeIndex = checkModel.getIndex(Parser.COMPLETE);
	  }

	  [Obsolete]
	  public Parser(AbstractModel buildModel, AbstractModel attachModel, AbstractModel checkModel, POSTagger tagger, Chunker chunker, HeadRules headRules) : this(buildModel,attachModel,checkModel, tagger,chunker,headRules,defaultBeamSize,defaultAdvancePercentage)
	  {
	  }

	  /// <summary>
	  /// Returns the right frontier of the specified parse tree with nodes ordered from deepest
	  /// to shallowest. </summary>
	  /// <param name="root"> The root of the parse tree. </param>
	  /// <returns> The right frontier of the specified parse tree. </returns>
	  public static IList<Parse> getRightFrontier(Parse root, HashSet<string> punctSet)
	  {
		IList<Parse> rf = new LinkedList<Parse>();
		Parse top;
		if (root.Type == AbstractBottomUpParser.TOP_NODE || root.Type == AbstractBottomUpParser.INC_NODE)
		{
		  top = collapsePunctuation(root.Children,punctSet)[0];
		}
		else
		{
		  top = root;
		}
		while (!top.PosTag)
		{
		  rf.Insert(0,top);
		  Parse[] kids = top.Children;
		  top = kids[kids.Length - 1];
		}
		return new List<Parse>(rf);
	  }

	  private Parse Built
	  {
		  set
		  {
			string l = value.Label;
			if (l == null)
			{
			  value.Label = Parser.BUILT;
			}
			else
			{
			  if (isComplete(value))
			  {
				value.Label = Parser.BUILT + "." + Parser.COMPLETE;
			  }
			  else
			  {
				value.Label = Parser.BUILT + "." + Parser.INCOMPLETE;
			  }
			}
		  }
	  }

	  private Parse Complete
	  {
		  set
		  {
			string l = value.Label;
			if (!isBuilt(value))
			{
			  value.Label = Parser.COMPLETE;
			}
			else
			{
			  value.Label = Parser.BUILT + "." + Parser.COMPLETE;
			}
		  }
	  }

	  private Parse Incomplete
	  {
		  set
		  {
			if (!isBuilt(value))
			{
			  value.Label = Parser.INCOMPLETE;
			}
			else
			{
			  value.Label = Parser.BUILT + "." + Parser.INCOMPLETE;
			}
		  }
	  }

	  private bool isBuilt(Parse p)
	  {
		string l = p.Label;
		if (l == null)
		{
		  return false;
		}
		else
		{
		  return l.StartsWith(Parser.BUILT, StringComparison.Ordinal);
		}
	  }

	  private bool isComplete(Parse p)
	  {
		string l = p.Label;
		if (l == null)
		{
		  return false;
		}
		else
		{
		  return l.EndsWith(Parser.COMPLETE, StringComparison.Ordinal);
		}
	  }

	  protected internal override Parse[] advanceChunks(Parse p, double minChunkScore)
	  {
		Parse[] parses = base.advanceChunks(p, minChunkScore);
		for (int pi = 0;pi < parses.Length;pi++)
		{
		  Parse[] chunks = parses[pi].Children;
		  for (int ci = 0;ci < chunks.Length;ci++)
		  {
			Complete = chunks[ci];
		  }
		}
		return parses;
	  }

	  protected internal override Parse[] advanceParses(Parse p, double probMass)
	  {
		double q = 1 - probMass;
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
		else if (numNodes == 1) //put sentence initial and final punct in top node
		{
		  if (children[0].PosTag)
		  {
			return null;
		  }
		  else
		  {
			p.expandTopNode(children[0]);
			return new Parse[] {p};
		  }
		}
		//determines which node needs to adanced.
		for (advanceNodeIndex = 0; advanceNodeIndex < numNodes; advanceNodeIndex++)
		{
		  advanceNode = children[advanceNodeIndex];
		  if (!isBuilt(advanceNode))
		  {
			break;
		  }
		}
		int originalZeroIndex = mapParseIndex(0,children,originalChildren);
		int originalAdvanceIndex = mapParseIndex(advanceNodeIndex,children,originalChildren);
		IList<Parse> newParsesList = new List<Parse>();
		//call build model
		buildModel.eval(buildContextGenerator.getContext(children, advanceNodeIndex), bprobs);
		double doneProb = bprobs[doneIndex];
		if (debugOn)
		{
			Console.WriteLine("adi=" + advanceNodeIndex + " " + advanceNode.Type + "." + advanceNode.Label + " " + advanceNode + " choose build=" + (1 - doneProb) + " attach=" + doneProb);
		}
		if (1 - doneProb > q)
		{
		  double bprobSum = 0;
		  while (bprobSum < probMass)
		  {
			/// <summary>
			/// The largest unadvanced labeling. </summary>
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
			if (!tag.Equals(DONE))
			{
			  Parse newParse1 = (Parse) p.clone();
			  Parse newNode = new Parse(p.Text,advanceNode.Span,tag,bprob,advanceNode.Head);
			  newParse1.insert(newNode);
			  newParse1.addProb(Math.Log(bprob));
			  newParsesList.Add(newParse1);
			  if (checkComplete)
			  {
				cprobs = checkModel.eval(checkContextGenerator.getContext(newNode,children,advanceNodeIndex,false));
				if (debugOn)
				{
					Console.WriteLine("building " + tag + " " + bprob + " c=" + cprobs[completeIndex]);
				}
				if (cprobs[completeIndex] > probMass) //just complete advances
				{
				  Complete = newNode;
				  newParse1.addProb(Math.Log(cprobs[completeIndex]));
				  if (debugOn)
				  {
					  Console.WriteLine("Only advancing complete node");
				  }
				}
				else if (1 - cprobs[completeIndex] > probMass) //just incomplete advances
				{
				  Incomplete = newNode;
				  newParse1.addProb(Math.Log(1 - cprobs[completeIndex]));
				  if (debugOn)
				  {
					  Console.WriteLine("Only advancing incomplete node");
				  }
				}
				else //both complete and incomplete advance
				{
				  if (debugOn)
				  {
					  Console.WriteLine("Advancing both complete and incomplete nodes");
				  }
				  Complete = newNode;
				  newParse1.addProb(Math.Log(cprobs[completeIndex]));

				  Parse newParse2 = (Parse) p.clone();
				  Parse newNode2 = new Parse(p.Text,advanceNode.Span,tag,bprob,advanceNode.Head);
				  newParse2.insert(newNode2);
				  newParse2.addProb(Math.Log(bprob));
				  newParsesList.Add(newParse2);
				  newParse2.addProb(Math.Log(1 - cprobs[completeIndex]));
				  Incomplete = newNode2; //set incomplete for non-clone
				}
			  }
			  else
			  {
				if (debugOn)
				{
					Console.WriteLine("building " + tag + " " + bprob);
				}
			  }
			}
		  }
		}
		//advance attaches
		if (doneProb > q)
		{
		  Parse newParse1 = (Parse) p.clone(); //clone parse
		  //mark nodes as built
		  if (checkComplete)
		  {
			if (isComplete(advanceNode))
			{
			  newParse1.setChild(originalAdvanceIndex,Parser.BUILT + "." + Parser.COMPLETE); //replace constituent being labeled to create new derivation
			}
			else
			{
			  newParse1.setChild(originalAdvanceIndex,Parser.BUILT + "." + Parser.INCOMPLETE); //replace constituent being labeled to create new derivation
			}
		  }
		  else
		  {
			newParse1.setChild(originalAdvanceIndex,Parser.BUILT); //replace constituent being labeled to create new derivation
		  }
		  newParse1.addProb(Math.Log(doneProb));
		  if (advanceNodeIndex == 0) //no attach if first node.
		  {
			newParsesList.Add(newParse1);
		  }
		  else
		  {
			IList<Parse> rf = getRightFrontier(p,punctSet);
			for (int fi = 0,fs = rf.Count;fi < fs;fi++)
			{
			  Parse fn = rf[fi];
			  attachModel.eval(attachContextGenerator.getContext(children, advanceNodeIndex,rf,fi), aprobs);
			  if (debugOn)
			  {
				//List cs = java.util.Arrays.asList(attachContextGenerator.getContext(children, advanceNodeIndex,rf,fi,punctSet));
				Console.WriteLine("Frontier node(" + fi + "): " + fn.Type + "." + fn.Label + " " + fn + " <- " + advanceNode.Type + " " + advanceNode + " d=" + aprobs[daughterAttachIndex] + " s=" + aprobs[sisterAttachIndex] + " ");
			  }
			  for (int ai = 0;ai < attachments.Length;ai++)
			  {
				double prob = aprobs[attachments[ai]];
				//should we try an attach if p > threshold and
				// if !checkComplete then prevent daughter attaching to chunk
				// if checkComplete then prevent daughter attacing to complete node or
				//    sister attaching to an incomplete node
				if (prob > q && ((!checkComplete && (attachments[ai] != daughterAttachIndex || !isComplete(fn))) || (checkComplete && ((attachments[ai] == daughterAttachIndex && !isComplete(fn)) || (attachments[ai] == sisterAttachIndex && isComplete(fn))))))
				{
				  Parse newParse2 = newParse1.cloneRoot(fn,originalZeroIndex);
				  Parse[] newKids = Parser.collapsePunctuation(newParse2.Children,punctSet);
				  //remove node from top level since were going to attach it (including punct)
				  for (int ri = originalZeroIndex + 1;ri <= originalAdvanceIndex;ri++)
				  {
					//System.out.println(at"-removing "+(originalZeroIndex+1)+" "+newParse2.getChildren()[originalZeroIndex+1]);
					newParse2.remove(originalZeroIndex + 1);
				  }
				  IList<Parse> crf = getRightFrontier(newParse2,punctSet);
				  Parse updatedNode;
				  if (attachments[ai] == daughterAttachIndex) //attach daughter
				  {
					updatedNode = crf[fi];
					updatedNode.add(advanceNode,headRules);
				  }
				  else //attach sister
				  {
					Parse psite;
					if (fi + 1 < crf.Count)
					{
					  psite = crf[fi + 1];
					  updatedNode = psite.adjoin(advanceNode,headRules);
					}
					else
					{
					  psite = newParse2;
					  updatedNode = psite.adjoinRoot(advanceNode,headRules,originalZeroIndex);
					  newKids[0] = updatedNode;
					}
				  }
				  //update spans affected by attachment
				  for (int ni = fi + 1;ni < crf.Count;ni++)
				  {
					Parse node = crf[ni];
					node.updateSpan();
				  }
				  //if (debugOn) {System.out.print(ai+"-result: ");newParse2.show();System.out.println();}
				  newParse2.addProb(Math.Log(prob));
				  newParsesList.Add(newParse2);
				  if (checkComplete)
				  {
					cprobs = checkModel.eval(checkContextGenerator.getContext(updatedNode,newKids,advanceNodeIndex,true));
					if (cprobs[completeIndex] > probMass)
					{
					  Complete = updatedNode;
					  newParse2.addProb(Math.Log(cprobs[completeIndex]));
					  if (debugOn)
					  {
						  Console.WriteLine("Only advancing complete node");
					  }
					}
					else if (1 - cprobs[completeIndex] > probMass)
					{
					  Incomplete = updatedNode;
					  newParse2.addProb(Math.Log(1 - cprobs[completeIndex]));
					  if (debugOn)
					  {
						  Console.WriteLine("Only advancing incomplete node");
					  }
					}
					else
					{
					  Complete = updatedNode;
					  Parse newParse3 = newParse2.cloneRoot(updatedNode,originalZeroIndex);
					  newParse3.addProb(Math.Log(cprobs[completeIndex]));
					  newParsesList.Add(newParse3);
					  Incomplete = updatedNode;
					  newParse2.addProb(Math.Log(1 - cprobs[completeIndex]));
					  if (debugOn)
					  {
						  Console.WriteLine("Advancing both complete and incomplete nodes; c=" + cprobs[completeIndex]);
					  }
					}
				  }
				}
				else
				{
				  if (debugOn)
				  {
					  Console.WriteLine("Skipping " + fn.Type + "." + fn.Label + " " + fn + " daughter=" + (attachments[ai] == daughterAttachIndex) + " complete=" + isComplete(fn) + " prob=" + prob);
				  }
				}
			  }
			  if (checkComplete && !isComplete(fn))
			  {
				if (debugOn)
				{
					Console.WriteLine("Stopping at incomplete node(" + fi + "): " + fn.Type + "." + fn.Label + " " + fn);
				}
				break;
			  }
			}
		  }
		}
        Parse[] newParses = newParsesList.ToArray();
		return newParses;
	  }

	  protected internal override void advanceTop(Parse p)
	  {
		p.Type = TOP_NODE;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.tools.parser.ParserModel train(String languageCode, opennlp.tools.util.ObjectStream<opennlp.tools.parser.Parse> parseSamples, opennlp.tools.parser.HeadRules rules, opennlp.tools.util.TrainingParameters mlParams) throws java.io.IOException
	  public static ParserModel train(string languageCode, ObjectStream<Parse> parseSamples, HeadRules rules, TrainingParameters mlParams)
	  {

		IDictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();

		Console.Error.WriteLine("Building dictionary");
		Dictionary mdict = buildDictionary(parseSamples, rules, mlParams);

		parseSamples.reset();

		// tag
		POSModel posModel = POSTaggerME.train(languageCode, new PosSampleStream(parseSamples), mlParams.getParameters("tagger"), null, null);

		parseSamples.reset();

		// chunk
		ChunkerModel chunkModel = ChunkerME.train(languageCode, new ChunkSampleStream(parseSamples), new ChunkContextGenerator(), mlParams.getParameters("chunker"));

		parseSamples.reset();

		// build
		Console.Error.WriteLine("Training builder");
		opennlp.model.EventStream bes = new ParserEventStream(parseSamples, rules, ParserEventTypeEnum.BUILD, mdict);
		IDictionary<string, string> buildReportMap = new Dictionary<string, string>();
		AbstractModel buildModel = TrainUtil.train(bes, mlParams.getSettings("build"), buildReportMap);
		opennlp.tools.parser.chunking.Parser.mergeReportIntoManifest(manifestInfoEntries, buildReportMap, "build");

		parseSamples.reset();

		// check
		Console.Error.WriteLine("Training checker");
		opennlp.model.EventStream kes = new ParserEventStream(parseSamples, rules, ParserEventTypeEnum.CHECK);
		IDictionary<string, string> checkReportMap = new Dictionary<string, string>();
		AbstractModel checkModel = TrainUtil.train(kes, mlParams.getSettings("check"), checkReportMap);
		opennlp.tools.parser.chunking.Parser.mergeReportIntoManifest(manifestInfoEntries, checkReportMap, "check");

		parseSamples.reset();

		// attach 
		Console.Error.WriteLine("Training attacher");
		opennlp.model.EventStream attachEvents = new ParserEventStream(parseSamples, rules, ParserEventTypeEnum.ATTACH);
		IDictionary<string, string> attachReportMap = new Dictionary<string, string>();
		AbstractModel attachModel = TrainUtil.train(attachEvents, mlParams.getSettings("attach"), attachReportMap);
		opennlp.tools.parser.chunking.Parser.mergeReportIntoManifest(manifestInfoEntries, attachReportMap, "attach");

		// TODO: Remove cast for HeadRules
		return new ParserModel(languageCode, buildModel, checkModel, attachModel, posModel, chunkModel, (opennlp.tools.parser.lang.en.HeadRules) rules, ParserType.TREEINSERT, manifestInfoEntries);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.tools.parser.ParserModel train(String languageCode, opennlp.tools.util.ObjectStream<opennlp.tools.parser.Parse> parseSamples, opennlp.tools.parser.HeadRules rules, int iterations, int cut) throws java.io.IOException
	  public static ParserModel train(string languageCode, ObjectStream<Parse> parseSamples, HeadRules rules, int iterations, int cut)
	  {

		TrainingParameters @params = new TrainingParameters();
		@params.Put("dict", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));

		@params.Put("tagger", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
		@params.Put("tagger", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));
		@params.Put("chunker", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
		@params.Put("chunker", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));
		@params.Put("check", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
		@params.Put("check", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));
		@params.Put("build", TrainingParameters.CUTOFF_PARAM, Convert.ToString(cut));
		@params.Put("build", TrainingParameters.ITERATIONS_PARAM, Convert.ToString(iterations));

		return train(languageCode, parseSamples, rules, @params);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static opennlp.model.AbstractModel train(opennlp.model.EventStream es, int iterations, int cut) throws java.io.IOException
	  [Obsolete]
	  public static AbstractModel train(opennlp.model.EventStream es, int iterations, int cut)
	  {
		return opennlp.maxent.GIS.trainModel(iterations, new TwoPassDataIndexer(es, cut));
	  }
	}

}