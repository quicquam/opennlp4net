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
	/// Class which models the gender of a particular mentions and entities made up of mentions.
	/// </summary>
	public class GenderModel : TestGenderModel, TrainSimilarityModel
	{

	  private int maleIndex;
	  private int femaleIndex;
	  private int neuterIndex;

	  private string modelName;
	  private string modelExtension = ".bin.gz";
	  private MaxentModel testModel_Renamed;
	  private IList<Event> events;
	  private bool debugOn = true;

	  private HashSet<string> maleNames;
	  private HashSet<string> femaleNames;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TestGenderModel PrepAttachDataUtil.testModel(String name) throws java.io.IOException
	  public static TestGenderModel testModel(string name)
	  {
		GenderModel gm = new GenderModel(name, false);
		return gm;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TrainSimilarityModel trainModel(String name) throws java.io.IOException
	  public static TrainSimilarityModel trainModel(string name)
	  {
		GenderModel gm = new GenderModel(name, true);
		return gm;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.Set<String> readNames(String nameFile) throws java.io.IOException
	  private HashSet<string> readNames(string nameFile)
	  {
		HashSet<string> names = new HashSet<string>();
		BufferedReader nameReader = new BufferedReader(new FileReader(nameFile));
		for (string line = nameReader.readLine(); line != null; line = nameReader.readLine())
		{
		  names.Add(line);
		}
		return names;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private GenderModel(String modelName, boolean train) throws java.io.IOException
	  private GenderModel(string modelName, bool train)
	  {
		this.modelName = modelName;
		maleNames = readNames(modelName + ".mas");
		femaleNames = readNames(modelName + ".fem");
		if (train)
		{
		  events = new List<Event>();
		}
		else
		{
		  //if (MaxentResolver.loadAsResource()) {
		  //  testModel = (new BinaryGISModelReader(new DataInputStream(this.getClass().getResourceAsStream(modelName)))).getModel();
		  //}
		  testModel_Renamed = (new SuffixSensitiveGISModelReader(new Jfile(modelName + modelExtension))).Model;
		  maleIndex = testModel_Renamed.getIndex(GenderEnum.MALE.ToString());
		  femaleIndex = testModel_Renamed.getIndex(GenderEnum.FEMALE.ToString());
		  neuterIndex = testModel_Renamed.getIndex(GenderEnum.NEUTER.ToString());
		}
	  }

	  private IList<string> getFeatures(Context np1)
	  {
		IList<string> features = new List<string>();
		features.Add("default");
		for (int ti = 0, tl = np1.HeadTokenIndex; ti < tl; ti++)
		{
		  features.Add("mw=" + np1.Tokens[ti].ToString());
		}
		features.Add("hw=" + np1.HeadTokenText);
		features.Add("n=" + np1.NameType);
		if (np1.NameType != null && np1.NameType.Equals("person"))
		{
		  object[] tokens = np1.Tokens;
		  //System.err.println("GenderModel.getFeatures: person name="+np1);
		  for (int ti = 0;ti < np1.HeadTokenIndex || ti == 0;ti++)
		  {
			string name = tokens[ti].ToString().ToLower();
			if (femaleNames.Contains(name))
			{
			  features.Add("fem");
			  //System.err.println("GenderModel.getFeatures: person (fem) "+np1);
			}
			if (maleNames.Contains(name))
			{
			  features.Add("mas");
			  //System.err.println("GenderModel.getFeatures: person (mas) "+np1);
			}
		  }
		}

		foreach (string si in np1.Synsets)
		{
		  features.Add("ss=" + si);
		}
		return features;
	  }

	  private void addEvent(string outcome, Context np1)
	  {
		IList<string> feats = getFeatures(np1);
		events.Add(new Event(outcome, feats.ToArray()));
	  }

	  /// <summary>
	  /// Heuristic computation of gender for a mention context using pronouns and honorifics. </summary>
	  /// <param name="mention"> The mention whose gender is to be computed. </param>
	  /// <returns> The heuristically determined gender or unknown. </returns>
	  private GenderEnum getGender(Context mention)
	  {
		if (ResolverUtils.malePronounPattern.matcher(mention.HeadTokenText).matches())
		{
		  return GenderEnum.MALE;
		}
		else if (ResolverUtils.femalePronounPattern.matcher(mention.HeadTokenText).matches())
		{
		  return GenderEnum.FEMALE;
		}
		else if (ResolverUtils.neuterPronounPattern.matcher(mention.HeadTokenText).matches())
		{
		  return GenderEnum.NEUTER;
		}
		object[] mtokens = mention.Tokens;
		for (int ti = 0, tl = mtokens.Length - 1; ti < tl; ti++)
		{
		  string token = mtokens[ti].ToString();
		  if (token.Equals("Mr.") || token.Equals("Mr"))
		  {
			return GenderEnum.MALE;
		  }
		  else if (token.Equals("Mrs.") || token.Equals("Mrs") || token.Equals("Ms.") || token.Equals("Ms"))
		  {
			return GenderEnum.FEMALE;
		  }
		}

		return GenderEnum.UNKNOWN;
	  }

	  private GenderEnum getGender(IList<Context> entity)
	  {
		for (IEnumerator<Context> ci = entity.GetEnumerator(); ci.MoveNext();)
		{
		  Context ec = ci.Current;
		  GenderEnum ge = getGender(ec);
		  if (ge != GenderEnum.UNKNOWN)
		  {
			return ge;
		  }
		}

		return GenderEnum.UNKNOWN;
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public void setExtents(Context[] extentContexts)
	  public virtual Context[] Extents
	  {
		  set
		  {
			HashList entities = new HashList();
			IList<Context> singletons = new List<Context>();
			for (int ei = 0, el = value.Length; ei < el; ei++)
			{
			  Context ec = value[ei];
			  //System.err.println("GenderModel.setExtents: ec("+ec.getId()+") "+ec.toText());
			  if (ec.Id != -1)
			  {
				entities[ec.Id] = ec;
			  }
			  else
			  {
				singletons.Add(ec);
			  }
			}
			IList<Context> males = new List<Context>();
			IList<Context> females = new List<Context>();
			IList<Context> eunuches = new List<Context>();
			//coref entities
			for (var ei = entities.Keys.GetEnumerator(); ei.MoveNext();)
			{
			  var key = ei.Current;
			  IList<Context> entityContexts = (IList<Context>) entities[key];
			  GenderEnum gender = getGender(entityContexts);
			  if (gender != null)
			  {
				if (gender == GenderEnum.MALE)
				{
				  males.AddRange(entityContexts);
				}
				else if (gender == GenderEnum.FEMALE)
				{
				  females.AddRange(entityContexts);
				}
				else if (gender == GenderEnum.NEUTER)
				{
				  eunuches.AddRange(entityContexts);
				}
			  }
			}
			//non-coref entities
			for (IEnumerator<Context> ei = singletons.GetEnumerator(); ei.MoveNext();)
			{
			  Context ec = ei.Current;
			  GenderEnum gender = getGender(ec);
			  if (gender == GenderEnum.MALE)
			  {
				males.Add(ec);
			  }
			  else if (gender == GenderEnum.FEMALE)
			  {
				females.Add(ec);
			  }
			  else if (gender == GenderEnum.NEUTER)
			  {
				eunuches.Add(ec);
			  }
			}
			for (IEnumerator<Context> mi = males.GetEnumerator(); mi.MoveNext();)
			{
			  Context ec = mi.Current;
			  addEvent(GenderEnum.MALE.ToString(), ec);
			}
			for (IEnumerator<Context> fi = females.GetEnumerator(); fi.MoveNext();)
			{
			  Context ec = fi.Current;
			  addEvent(GenderEnum.FEMALE.ToString(), ec);
			}
			for (IEnumerator<Context> ei = eunuches.GetEnumerator(); ei.MoveNext();)
			{
			  Context ec = ei.Current;
			  addEvent(GenderEnum.NEUTER.ToString(), ec);
			}
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
	  public static void Main(string[] args)
	  {
		if (args.Length == 0)
		{
		  Console.Error.WriteLine("Usage: GenderModel modelName < tiger/NN bear/NN");
		  Environment.Exit(1);
		}
		string modelName = args[0];
		GenderModel model = new GenderModel(modelName, false);
		//Context.wn = new WordNet(System.getProperty("WNHOME"), true);
		//Context.morphy = new Morphy(Context.wn);
        BufferedReader @in = new BufferedReader(new InputStreamReader(Console.OpenStandardInput(), "TODO Encoding"));
		for (string line = @in.readLine(); line != null; line = @in.readLine())
		{
		  string[] words = line.Split(" ", true);
		  double[] dist = model.genderDistribution(Context.parseContext(words[0]));
		  Console.WriteLine("m=" + dist[model.MaleIndex] + " f=" + dist[model.FemaleIndex] + " n=" + dist[model.NeuterIndex] + " " + model.getFeatures(Context.parseContext(words[0])));
		}
	  }

	  public virtual double[] genderDistribution(Context np1)
	  {
		IList<string> features = getFeatures(np1);
		if (debugOn)
		{
		  //System.err.println("GenderModel.genderDistribution: "+features);
		}
		return testModel_Renamed.eval(features.ToArray());
	  }

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
		}
		(new SuffixSensitiveGISModelWriter(GIS.trainModel(new CollectionEventStream(events), true), new Jfile(modelName + modelExtension))).persist();
	  }

	  public virtual int FemaleIndex
	  {
		  get
		  {
			return femaleIndex;
		  }
	  }

	  public virtual int MaleIndex
	  {
		  get
		  {
			return maleIndex;
		  }
	  }

	  public virtual int NeuterIndex
	  {
		  get
		  {
			return neuterIndex;
		  }
	  }
	}

}