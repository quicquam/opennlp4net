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
using j4n.IO.File;
using j4n.IO.Reader;
using j4n.IO.Writer;
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.coref.sim
{


	using GIS = opennlp.maxent.GIS;
	using SuffixSensitiveGISModelReader = opennlp.maxent.io.SuffixSensitiveGISModelReader;
	using SuffixSensitiveGISModelWriter = opennlp.maxent.io.SuffixSensitiveGISModelWriter;
	using Event = opennlp.model.Event;
	using MaxentModel = opennlp.model.MaxentModel;
	using ResolverUtils = opennlp.tools.coref.resolver.ResolverUtils;
	using CollectionEventStream = opennlp.tools.util.CollectionEventStream;
	using HashList = opennlp.tools.util.HashList;

	/// <summary>
	/// Models semantic similarity between two mentions and returns a score based on
	/// how semantically comparable the mentions are with one another.
	/// </summary>
	public class SimilarityModel : TestSimilarityModel, TrainSimilarityModel
	{

	  private string modelName;
	  private string modelExtension = ".bin.gz";
	  private MaxentModel testModel_Renamed;
	  private IList<Event> events;
	  private int SAME_INDEX;
	  private const string SAME = "same";
	  private const string DIFF = "diff";
	  private bool debugOn = false;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TestSimilarityModel PrepAttachDataUtil.testModel(String name) throws java.io.IOException
	  public static TestSimilarityModel testModel(string name)
	  {
		return new SimilarityModel(name, false);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TrainSimilarityModel trainModel(String name) throws java.io.IOException
	  public static TrainSimilarityModel trainModel(string name)
	  {
		SimilarityModel sm = new SimilarityModel(name, true);
		return sm;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private SimilarityModel(String modelName, boolean train) throws java.io.IOException
	  private SimilarityModel(string modelName, bool train)
	  {
		this.modelName = modelName;
		if (train)
		{
		  events = new List<Event>();
		}
		else
		{
		  testModel_Renamed = (new SuffixSensitiveGISModelReader(new Jfile(modelName + modelExtension))).Model;
		  SAME_INDEX = testModel_Renamed.getIndex(SAME);
		}
	  }

	  private void addEvent(bool same, Context np1, Context np2)
	  {
		if (same)
		{
		  IList<string> feats = getFeatures(np1, np2);
		  //System.err.println(SAME+" "+np1.headTokenText+" ("+np1.id+") -> "+np2.headTokenText+" ("+np2.id+") "+feats);
		  events.Add(new Event(SAME, feats.ToArray()));
		}
		else
		{
		  IList<string> feats = getFeatures(np1, np2);
		  //System.err.println(DIFF+" "+np1.headTokenText+" ("+np1.id+") -> "+np2.headTokenText+" ("+np2.id+") "+feats);
		  events.Add(new Event(DIFF, feats.ToArray()));
		}
	  }

	  /// <summary>
	  /// Produces a set of head words for the specified list of mentions.
	  /// </summary>
	  /// <param name="mentions"> The mentions to use to construct the
	  /// </param>
	  /// <returns> A set containing the head words of the specified mentions. </returns>
	  private HashSet<string> constructHeadSet(IList<Context> mentions)
	  {
		HashSet<string> headSet = new HashSet<string>();
		for (IEnumerator<Context> ei = mentions.GetEnumerator(); ei.MoveNext();)
		{
		  Context ec = ei.Current;
		  headSet.Add(ec.HeadTokenText.ToLower());
		}
		return headSet;
	  }

	  private bool hasSameHead(HashSet<string> entityHeadSet, HashSet<string> candidateHeadSet)
	  {
		for (IEnumerator<string> hi = entityHeadSet.GetEnumerator(); hi.MoveNext();)
		{
		  if (candidateHeadSet.Contains(hi.Current))
		  {
			return true;
		  }
		}
		return false;
	  }

	  private bool hasSameNameType(HashSet<string> entityNameSet, HashSet<string> candidateNameSet)
	  {
		for (IEnumerator<string> hi = entityNameSet.GetEnumerator(); hi.MoveNext();)
		{
		  if (candidateNameSet.Contains(hi.Current))
		  {
			return true;
		  }
		}
		return false;
	  }

	  private bool hasSuperClass(IList<Context> entityContexts, IList<Context> candidateContexts)
	  {
		for (IEnumerator<Context> ei = entityContexts.GetEnumerator(); ei.MoveNext();)
		{
		  Context ec = ei.Current;
		  for (IEnumerator<Context> cei = candidateContexts.GetEnumerator(); cei.MoveNext();)
		  {
			if (inSuperClass(ec, cei.Current))
			{
			  return true;
			}
		  }
		}
		return false;
	  }

	  /// <summary>
	  /// Constructs a set of entities which may be semantically compatible with the
	  /// entity indicated by the specified entityKey.
	  /// </summary>
	  /// <param name="entityKey"> The key of the entity for which the set is being constructed. </param>
	  /// <param name="entities"> A mapping between entity keys and their mentions. </param>
	  /// <param name="headSets"> A mapping between entity keys and their head sets. </param>
	  /// <param name="nameSets"> A mapping between entity keys and their name sets. </param>
	  /// <param name="singletons"> A list of all entities which consists of a single mentions.
	  /// </param>
	  /// <returns> A set of mentions for all the entities which might be semantically compatible
	  /// with entity indicated by the specified key. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.Set<Context> constructExclusionSet(Integer entityKey, opennlp.tools.util.HashList entities, java.util.Map<Integer, java.util.Set<String>> headSets, java.util.Map<Integer, java.util.Set<String>> nameSets, java.util.List<Context> singletons)
	  private HashSet<Context> constructExclusionSet(int? entityKey, HashList entities, IDictionary<int?, HashSet<string>> headSets, IDictionary<int?, HashSet<string>> nameSets, IList<Context> singletons)
	  {
		HashSet<Context> exclusionSet = new HashSet<Context>();
		HashSet<string> entityHeadSet = headSets[entityKey];
		HashSet<string> entityNameSet = nameSets[entityKey];
		IList<Context> entityContexts = (IList<Context>) entities[entityKey];
		//entities
		for (var ei = entities.Keys.GetEnumerator(); ei.MoveNext();)
		{
		  var key = ei.Current as int?;
		  IList<Context> candidateContexts = (IList<Context>) entities[key];
		  if (key.Equals(entityKey))
		  {
			exclusionSet.addAll(candidateContexts);
		  }
		  else if (nameSets[key].Count == 0)
		  {
			exclusionSet.addAll(candidateContexts);
		  }
		  else if (hasSameHead(entityHeadSet, headSets[key]))
		  {
			exclusionSet.addAll(candidateContexts);
		  }
		  else if (hasSameNameType(entityNameSet, nameSets[key]))
		  {
			exclusionSet.addAll(candidateContexts);
		  }
		  else if (hasSuperClass(entityContexts, candidateContexts))
		  {
			exclusionSet.addAll(candidateContexts);
		  }
		}
		//singles
		IList<Context> singles = new List<Context>(1);
		for (IEnumerator<Context> si = singletons.GetEnumerator(); si.MoveNext();)
		{
		  Context sc = si.Current;
		  singles.Clear();
		  singles.Add(sc);
		  if (entityHeadSet.Contains(sc.HeadTokenText.ToLower()))
		  {
			exclusionSet.Add(sc);
		  }
		  else if (sc.NameType == null)
		  {
			exclusionSet.Add(sc);
		  }
		  else if (entityNameSet.Contains(sc.NameType))
		  {
			exclusionSet.Add(sc);
		  }
		  else if (hasSuperClass(entityContexts, singles))
		  {
			exclusionSet.Add(sc);
		  }
		}
		return exclusionSet;
	  }

	  /// <summary>
	  /// Constructs a mapping between the specified entities and their head set.
	  /// </summary>
	  /// <param name="entities"> Mapping between a key and a list of mentions which compose an entity.
	  /// </param>
	  /// <returns> a mapping between the keys of the specified entity mapping and the head set
	  /// generated from the mentions associated with that key. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.Map<Integer, java.util.Set<String>> constructHeadSets(opennlp.tools.util.HashList entities)
	  private IDictionary<int?, HashSet<string>> constructHeadSets(HashList entities)
	  {
		IDictionary<int?, HashSet<string>> headSets = new Dictionary<int?, HashSet<string>>();
		for (var ei = entities.Keys.GetEnumerator(); ei.MoveNext();)
		{
		  var key = ei.Current as int?;
		  IList<Context> entityContexts = (IList<Context>) entities[key];
		  headSets[key] = constructHeadSet(entityContexts);
		}
		return headSets;
	  }

	  /// <summary>
	  /// Produces the set of name types associated with each of the specified mentions.
	  /// </summary>
	  /// <param name="mentions"> A list of mentions.
	  /// </param>
	  /// <returns> A set set of name types assigned to the specified mentions. </returns>
	  private HashSet<string> constructNameSet(IList<Context> mentions)
	  {
		HashSet<string> nameSet = new HashSet<string>();
		for (IEnumerator<Context> ei = mentions.GetEnumerator(); ei.MoveNext();)
		{
		  Context ec = ei.Current;
		  if (ec.NameType != null)
		  {
			nameSet.Add(ec.NameType);
		  }
		}
		return nameSet;
	  }

	  /// <summary>
	  /// Constructs a mapping between the specified entities and the names associated with these entities.
	  /// </summary>
	  /// <param name="entities"> A mapping between a key and a list of mentions.
	  /// </param>
	  /// <returns> a mapping between each key in the specified entity map and the name types associated with the each mention of that entity. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.Map<Integer, java.util.Set<String>> constructNameSets(opennlp.tools.util.HashList entities)
	  private IDictionary<int?, HashSet<string>> constructNameSets(HashList entities)
	  {
		IDictionary<int?, HashSet<string>> nameSets = new Dictionary<int?, HashSet<string>>();
		for (var ei = entities.Keys.GetEnumerator(); ei.MoveNext();)
		{
		  var key = ei.Current as int?;
		  IList<Context> entityContexts = (IList<Context>) entities[key];
		  nameSets[key] = constructNameSet(entityContexts);
		}
		return nameSets;
	  }

	  private bool inSuperClass(Context ec, Context cec)
	  {
		if (ec.Synsets.Count == 0 || cec.Synsets.Count == 0)
		{
		  return false;
		}
		else
		{
		  int numCommonSynsets = 0;
		  for (IEnumerator<string> si = ec.Synsets.GetEnumerator(); si.MoveNext();)
		  {
			string synset = si.Current;
			if (cec.Synsets.Contains(synset))
			{
			  numCommonSynsets++;
			}
		  }
		  if (numCommonSynsets == 0)
		  {
			return false;
		  }
		  else if (numCommonSynsets == ec.Synsets.Count || numCommonSynsets == cec.Synsets.Count)
		  {
			return true;
		  }
		  else
		  {
			return false;
		  }
		}
	  }

	  /*
	  private boolean isPronoun(MentionContext mention) {
	    return mention.getHeadTokenTag().startsWith("PRP");
	  }
	  */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public void setExtents(Context[] extentContexts)
	  public virtual Context[] Extents
	  {
		  set
		  {
			HashList entities = new HashList();
			/// <summary>
			/// Extents which are not in a coreference chain. </summary>
			IList<Context> singletons = new List<Context>();
			IList<Context> allExtents = new List<Context>();
			//populate data structures
			for (int ei = 0, el = value.Length; ei < el; ei++)
			{
			  Context ec = value[ei];
			  //System.err.println("SimilarityModel: setExtents: ec("+ec.getId()+") "+ec.getNameType()+" "+ec);
			  if (ec.Id == -1)
			  {
				singletons.Add(ec);
			  }
			  else
			  {
				entities[ec.Id] = ec;
			  }
			  allExtents.Add(ec);
			}
    
			int axi = 0;
			IDictionary<int?, HashSet<string>> headSets = constructHeadSets(entities);
			IDictionary<int?, HashSet<string>> nameSets = constructNameSets(entities);
    
			for (var ei = entities.Keys.GetEnumerator(); ei.MoveNext();)
			{
			  var key = ei.Current as int?;
			  HashSet<string> entityNameSet = nameSets[key];
			  if (entityNameSet.Count == 0)
			  {
				continue;
			  }
			  IList<Context> entityContexts = (IList<Context>) entities[key];
			  HashSet<Context> exclusionSet = constructExclusionSet(key, entities, headSets, nameSets, singletons);
			  if (entityContexts.Count == 1)
			  {
			  }
			  for (int xi1 = 0, xl = entityContexts.Count; xi1 < xl; xi1++)
			  {
				Context ec1 = entityContexts[xi1];
				//if (isPronoun(ec1)) {
				//  continue;
				//}
				for (int xi2 = xi1 + 1; xi2 < xl; xi2++)
				{
				  Context ec2 = entityContexts[xi2];
				  //if (isPronoun(ec2)) {
				  //  continue;
				  //}
				  addEvent(true, ec1, ec2);
				  int startIndex = axi;
				  do
				  {
					Context sec1 = allExtents[axi];
					axi = (axi + 1) % allExtents.Count;
					if (!exclusionSet.Contains(sec1))
					{
					  if (debugOn)
					  {
						  Console.Error.WriteLine(ec1.ToString() + " " + entityNameSet + " " + sec1.ToString() + " " + nameSets[sec1.Id]);
					  }
					  addEvent(false, ec1, sec1);
					  break;
					}
				  } while (axi != startIndex);
				}
			  }
			}
		  }
	  }

	  /// <summary>
	  /// Returns a number between 0 and 1 which represents the models belief that the specified mentions are compatible.
	  /// Value closer to 1 are more compatible, while values closer to 0 are less compatible. </summary>
	  /// <param name="mention1"> The first mention to be considered. </param>
	  /// <param name="mention2"> The second mention to be considered. </param>
	  /// <returns> a number between 0 and 1 which represents the models belief that the specified mentions are compatible. </returns>
	  public virtual double compatible(Context mention1, Context mention2)
	  {
		IList<string> feats = getFeatures(mention1, mention2);
		if (debugOn)
		{
			Console.Error.WriteLine("SimilarityModel.compatible: feats=" + feats);
		}
		return (testModel_Renamed.eval(feats.ToArray())[SAME_INDEX]);
	  }

	  /// <summary>
	  /// Train a model based on the previously supplied evidence. </summary>
	  /// <seealso cref= #setExtents(Context[]) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void trainModel() throws java.io.IOException
	  public virtual void trainModel()
	  {
		if (debugOn)
		{
		  FileWriter writer = new FileWriter(modelName + ".events");
		  for (IEnumerator<Event> ei = events.GetEnumerator(); ei.MoveNext();)
		  {
			Event e = ei.Current;
			writer.write(e.ToString() + "\n");
		  }
		  writer.close();
		}(new SuffixSensitiveGISModelWriter(GIS.trainModel(new CollectionEventStream(events),100,10), new Jfile(modelName + modelExtension))).persist();
	  }

	  private bool isName(Context np)
	  {
		return np.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal);
	  }

	  private bool isCommonNoun(Context np)
	  {
		return !np.HeadTokenTag.StartsWith("NNP", StringComparison.Ordinal) && np.HeadTokenTag.StartsWith("NN", StringComparison.Ordinal);
	  }

	  private bool isPronoun(Context np)
	  {
		return np.HeadTokenTag.StartsWith("PRP", StringComparison.Ordinal);
	  }

	  private bool isNumber(Context np)
	  {
		return np.HeadTokenTag.Equals("CD");
	  }

	  private IList<string> getNameCommonFeatures(Context name, Context common)
	  {
		HashSet<string> synsets = common.Synsets;
		IList<string> features = new List<string>(2 + synsets.Count);
		features.Add("nn=" + name.NameType + "," + common.NameType);
		features.Add("nw=" + name.NameType + "," + common.HeadTokenText.ToLower());
		for (IEnumerator<string> si = synsets.GetEnumerator(); si.MoveNext();)
		{
		  features.Add("ns=" + name.NameType + "," + si.Current);
		}
		if (name.NameType == null)
		{
		  //features.addAll(getCommonCommonFeatures(name,common));
		}
		return features;
	  }

	  private IList<string> getNameNumberFeatures(Context name, Context number)
	  {
		IList<string> features = new List<string>(2);
		features.Add("nt=" + name.NameType + "," + number.HeadTokenTag);
		features.Add("nn=" + name.NameType + "," + number.NameType);
		return features;
	  }

	  private IList<string> getNamePronounFeatures(Context name, Context pronoun)
	  {
		IList<string> features = new List<string>(2);
		features.Add("nw=" + name.NameType + "," + pronoun.HeadTokenText.ToLower());
		features.Add("ng=" + name.NameType + "," + ResolverUtils.getPronounGender(pronoun.HeadTokenText.ToLower()));
		return features;
	  }

	  private IList<string> getCommonPronounFeatures(Context common, Context pronoun)
	  {
		IList<string> features = new List<string>();
		HashSet<string> synsets1 = common.Synsets;
		string p = pronoun.HeadTokenText.ToLower();
		string gen = ResolverUtils.getPronounGender(p);
		features.Add("wn=" + p + "," + common.NameType);
		for (IEnumerator<string> si = synsets1.GetEnumerator(); si.MoveNext();)
		{
		  string synset = si.Current;
		  features.Add("ws=" + p + "," + synset);
		  features.Add("gs=" + gen + "," + synset);
		}
		return features;
	  }

	  private IList<string> getCommonNumberFeatures(Context common, Context number)
	  {
		IList<string> features = new List<string>();
		HashSet<string> synsets1 = common.Synsets;
		for (IEnumerator<string> si = synsets1.GetEnumerator(); si.MoveNext();)
		{
		  string synset = si.Current;
		  features.Add("ts=" + number.HeadTokenTag + "," + synset);
		  features.Add("ns=" + number.NameType + "," + synset);
		}
		features.Add("nn=" + number.NameType + "," + common.NameType);
		return features;
	  }

	  private IList<string> getNumberPronounFeatures(Context number, Context pronoun)
	  {
		IList<string> features = new List<string>();
		string p = pronoun.HeadTokenText.ToLower();
		string gen = ResolverUtils.getPronounGender(p);
		features.Add("wt=" + p + "," + number.HeadTokenTag);
		features.Add("wn=" + p + "," + number.NameType);
		features.Add("wt=" + gen + "," + number.HeadTokenTag);
		features.Add("wn=" + gen + "," + number.NameType);
		return features;
	  }

	  private IList<string> getNameNameFeatures(Context name1, Context name2)
	  {
		IList<string> features = new List<string>(1);
		if (name1.NameType == null && name2.NameType == null)
		{
		  features.Add("nn=" + name1.NameType + "," + name2.NameType);
		  //features.addAll(getCommonCommonFeatures(name1,name2));
		}
		else if (name1.NameType == null)
		{
		  features.Add("nn=" + name1.NameType + "," + name2.NameType);
		  //features.addAll(getNameCommonFeatures(name2,name1));
		}
		else if (name2.NameType == null)
		{
		  features.Add("nn=" + name2.NameType + "," + name1.NameType);
		  //features.addAll(getNameCommonFeatures(name1,name2));
		}
		else
		{
		  if (name1.NameType.CompareTo(name2.NameType) < 0)
		  {
			features.Add("nn=" + name1.NameType + "," + name2.NameType);
		  }
		  else
		  {
			features.Add("nn=" + name2.NameType + "," + name1.NameType);
		  }
		  if (name1.NameType.Equals(name2.NameType))
		  {
			features.Add("sameNameType");
		  }
		}
		return features;
	  }

	  private IList<string> getCommonCommonFeatures(Context common1, Context common2)
	  {
		IList<string> features = new List<string>();
		HashSet<string> synsets1 = common1.Synsets;
		HashSet<string> synsets2 = common2.Synsets;

		if (synsets1.Count == 0)
		{
		  //features.add("missing_"+common1.headToken);
		  return features;
		}
		if (synsets2.Count == 0)
		{
		  //features.add("missing_"+common2.headToken);
		  return features;
		}
		int numCommonSynsets = 0;
		for (IEnumerator<string> si = synsets1.GetEnumerator(); si.MoveNext();)
		{
		  string synset = si.Current;
		  if (synsets2.Contains(synset))
		  {
			features.Add("ss=" + synset);
			numCommonSynsets++;
		  }
		}
		if (numCommonSynsets == 0)
		{
		  features.Add("ncss");
		}
		else if (numCommonSynsets == synsets1.Count && numCommonSynsets == synsets2.Count)
		{
		  features.Add("samess");
		}
		else if (numCommonSynsets == synsets1.Count)
		{
		  features.Add("2isa1");
		  //features.add("2isa1-"+(synsets2.size() - numCommonSynsets));
		}
		else if (numCommonSynsets == synsets2.Count)
		{
		  features.Add("1isa2");
		  //features.add("1isa2-"+(synsets1.size() - numCommonSynsets));
		}
		return features;
	  }

	  private IList<string> getPronounPronounFeatures(Context pronoun1, Context pronoun2)
	  {
		IList<string> features = new List<string>();
		string g1 = ResolverUtils.getPronounGender(pronoun1.HeadTokenText);
		string g2 = ResolverUtils.getPronounGender(pronoun2.HeadTokenText);
		if (g1.Equals(g2))
		{
		  features.Add("sameGender");
		}
		else
		{
		  features.Add("diffGender");
		}
		return features;
	  }

	  private IList<string> getFeatures(Context np1, Context np2)
	  {
		IList<string> features = new List<string>();
		features.Add("default");
		//  semantic categories
		string w1 = np1.HeadTokenText.ToLower();
		string w2 = np2.HeadTokenText.ToLower();
		if (w1.CompareTo(w2) < 0)
		{
		  features.Add("ww=" + w1 + "," + w2);
		}
		else
		{
		  features.Add("ww=" + w2 + "," + w1);
		}
		if (w1.Equals(w2))
		{
		  features.Add("sameHead");
		}
		//features.add("tt="+np1.headTag+","+np2.headTag);
		if (isName(np1))
		{
		  if (isName(np2))
		  {
			features.AddRange(getNameNameFeatures(np1, np2));
		  }
		  else if (isCommonNoun(np2))
		  {
			features.AddRange(getNameCommonFeatures(np1, np2));
		  }
		  else if (isPronoun(np2))
		  {
			features.AddRange(getNamePronounFeatures(np1, np2));
		  }
		  else if (isNumber(np2))
		  {
			features.AddRange(getNameNumberFeatures(np1, np2));
		  }
		}
		else if (isCommonNoun(np1))
		{
		  if (isName(np2))
		  {
			features.AddRange(getNameCommonFeatures(np2, np1));
		  }
		  else if (isCommonNoun(np2))
		  {
			features.AddRange(getCommonCommonFeatures(np1, np2));
		  }
		  else if (isPronoun(np2))
		  {
			features.AddRange(getCommonPronounFeatures(np1, np2));
		  }
		  else if (isNumber(np2))
		  {
			features.AddRange(getCommonNumberFeatures(np1, np2));
		  }
		  else
		  {
			//System.err.println("unknown group for " + np1.headTokenText + " -> " + np2.headTokenText);
		  }
		}
		else if (isPronoun(np1))
		{
		  if (isName(np2))
		  {
			features.AddRange(getNamePronounFeatures(np2, np1));
		  }
		  else if (isCommonNoun(np2))
		  {
			features.AddRange(getCommonPronounFeatures(np2, np1));
		  }
		  else if (isPronoun(np2))
		  {
			features.AddRange(getPronounPronounFeatures(np1, np2));
		  }
		  else if (isNumber(np2))
		  {
			features.AddRange(getNumberPronounFeatures(np2, np1));
		  }
		  else
		  {
			//System.err.println("unknown group for " + np1.headTokenText + " -> " + np2.headTokenText);
		  }
		}
		else if (isNumber(np1))
		{
		  if (isName(np2))
		  {
			features.AddRange(getNameNumberFeatures(np2, np1));
		  }
		  else if (isCommonNoun(np2))
		  {
			features.AddRange(getCommonNumberFeatures(np2, np1));
		  }
		  else if (isPronoun(np2))
		  {
			features.AddRange(getNumberPronounFeatures(np1, np2));
		  }
		  else if (isNumber(np2))
		  {
		  }
		  else
		  {
			//System.err.println("unknown group for " + np1.headTokenText + " -> " + np2.headTokenText);
		  }
		}
		else
		{
		  //System.err.println("unknown group for " + np1.headToken);
		}
		return (features);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
	  public static void Main(string[] args)
	  {
		if (args.Length == 0)
		{
		  Console.Error.WriteLine("Usage: SimilarityModel modelName < tiger/NN bear/NN");
		  Environment.Exit(1);
		}
		string modelName = args[0];
		SimilarityModel model = new SimilarityModel(modelName, false);
		//Context.wn = new WordNet(System.getProperty("WNHOME"), true);
		//Context.morphy = new Morphy(Context.wn);
        BufferedReader @in = new BufferedReader(new InputStreamReader(Console.OpenStandardInput(), "TODO Encoding"));
		for (string line = @in.readLine(); line != null; line = @in.readLine())
		{
		  string[] words = line.Split(" ", true);
		  double p = model.compatible(Context.parseContext(words[0]), Context.parseContext(words[1]));
		  Console.WriteLine(p + " " + model.getFeatures(Context.parseContext(words[0]), Context.parseContext(words[1])));
		}
	  }
	}

}