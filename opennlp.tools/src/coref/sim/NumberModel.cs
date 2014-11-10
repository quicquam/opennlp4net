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
	/// Class which models the number of particular mentions and the entities made up of mentions.
	/// </summary>
	public class NumberModel : TestNumberModel, TrainSimilarityModel
	{

	  private string modelName;
	  private string modelExtension = ".bin.gz";
	  private MaxentModel testModel_Renamed;
	  private IList<Event> events;

	  private int singularIndex;
	  private int pluralIndex;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TestNumberModel PrepAttachDataUtil.testModel(String name) throws java.io.IOException
	  public static TestNumberModel testModel(string name)
	  {
		NumberModel nm = new NumberModel(name, false);
		return nm;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TrainSimilarityModel trainModel(String modelName) throws java.io.IOException
	  public static TrainSimilarityModel trainModel(string modelName)
	  {
		NumberModel gm = new NumberModel(modelName, true);
		return gm;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private NumberModel(String modelName, boolean train) throws java.io.IOException
	  private NumberModel(string modelName, bool train)
	  {
		this.modelName = modelName;
		if (train)
		{
		  events = new List<Event>();
		}
		else
		{
		  //if (MaxentResolver.loadAsResource()) {
		  //  testModel = (new PlainTextGISModelReader(new BufferedReader(new InputStreamReader(this.getClass().getResourceAsStream(modelName))))).getModel();
		  //}
		  testModel_Renamed = (new SuffixSensitiveGISModelReader(new Jfile(modelName + modelExtension))).Model;
		  singularIndex = testModel_Renamed.getIndex(NumberEnum.SINGULAR.ToString());
		  pluralIndex = testModel_Renamed.getIndex(NumberEnum.PLURAL.ToString());
		}
	  }

	  private IList<string> getFeatures(Context np1)
	  {
		IList<string> features = new List<string>();
		features.Add("default");
		object[] npTokens = np1.Tokens;
		for (int ti = 0, tl = npTokens.Length - 1; ti < tl; ti++)
		{
		  features.Add("mw=" + npTokens[ti].ToString());
		}
		features.Add("hw=" + np1.HeadTokenText.ToLower());
		features.Add("ht=" + np1.HeadTokenTag);
		return features;
	  }

	  private void addEvent(string outcome, Context np1)
	  {
		IList<string> feats = getFeatures(np1);
		events.Add(new Event(outcome, feats.ToArray()));
	  }

	  public virtual NumberEnum getNumber(Context ec)
	  {
		if (ResolverUtils.singularPronounPattern.matcher(ec.HeadTokenText).matches())
		{
		  return NumberEnum.SINGULAR;
		}
		else if (ResolverUtils.pluralPronounPattern.matcher(ec.HeadTokenText).matches())
		{
		  return NumberEnum.PLURAL;
		}
		else
		{
		  return NumberEnum.UNKNOWN;
		}
	  }

	  private NumberEnum getNumber(IList<Context> entity)
	  {
		for (IEnumerator<Context> ci = entity.GetEnumerator(); ci.MoveNext();)
		{
		  Context ec = ci.Current;
		  NumberEnum ne = getNumber(ec);
		  if (ne != NumberEnum.UNKNOWN)
		  {
			return ne;
		  }
		}
		return NumberEnum.UNKNOWN;
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
			  //System.err.println("NumberModel.setExtents: ec("+ec.getId()+") "+ec.toText());
			  if (ec.Id != -1)
			  {
				entities[ec.Id] = ec;
			  }
			  else
			  {
				singletons.Add(ec);
			  }
			}
			IList<Context> singles = new List<Context>();
			IList<Context> plurals = new List<Context>();
			// coref entities
			for (IEnumerator<int?> ei = entities.Keys.GetEnumerator(); ei.MoveNext();)
			{
			  int? key = ei.Current;
			  IList<Context> entityContexts = (IList<Context>) entities[key];
			  NumberEnum number = getNumber(entityContexts);
			  if (number == NumberEnum.SINGULAR)
			  {
				singles.AddRange(entityContexts);
			  }
			  else if (number == NumberEnum.PLURAL)
			  {
				plurals.AddRange(entityContexts);
			  }
			}
			// non-coref entities.
			for (IEnumerator<Context> ei = singletons.GetEnumerator(); ei.MoveNext();)
			{
			  Context ec = ei.Current;
			  NumberEnum number = getNumber(ec);
			  if (number == NumberEnum.SINGULAR)
			  {
				singles.Add(ec);
			  }
			  else if (number == NumberEnum.PLURAL)
			  {
				plurals.Add(ec);
			  }
			}
    
			for (IEnumerator<Context> si = singles.GetEnumerator(); si.MoveNext();)
			{
			  Context ec = si.Current;
			  addEvent(NumberEnum.SINGULAR.ToString(), ec);
			}
			for (IEnumerator<Context> fi = plurals.GetEnumerator(); fi.MoveNext();)
			{
			  Context ec = fi.Current;
			  addEvent(NumberEnum.PLURAL.ToString(),ec);
			}
		  }
	  }

	  public virtual double[] numberDist(Context c)
	  {
		IList<string> feats = getFeatures(c);
		return testModel_Renamed.eval(feats.ToArray());
	  }

	  public virtual int SingularIndex
	  {
		  get
		  {
			return singularIndex;
		  }
	  }

	  public virtual int PluralIndex
	  {
		  get
		  {
			return pluralIndex;
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void trainModel() throws java.io.IOException
	  public virtual void trainModel()
	  {
		(new SuffixSensitiveGISModelWriter(GIS.trainModel(new CollectionEventStream(events),100,10),new Jfile(modelName + modelExtension))).persist();
	  }

	}

}