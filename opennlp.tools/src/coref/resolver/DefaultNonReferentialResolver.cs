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
using j4n.IO.InputStream;
using j4n.IO.Writer;
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.coref.resolver
{


	using GIS = opennlp.maxent.GIS;
	using BinaryGISModelReader = opennlp.maxent.io.BinaryGISModelReader;
	using SuffixSensitiveGISModelReader = opennlp.maxent.io.SuffixSensitiveGISModelReader;
	using SuffixSensitiveGISModelWriter = opennlp.maxent.io.SuffixSensitiveGISModelWriter;
	using Event = opennlp.model.Event;
	using MaxentModel = opennlp.model.MaxentModel;
	using MentionContext = opennlp.tools.coref.mention.MentionContext;
	using Parse = opennlp.tools.coref.mention.Parse;
	using CollectionEventStream = opennlp.tools.util.CollectionEventStream;

	/// <summary>
	/// Default implementation of the <seealso cref="NonReferentialResolver"/> interface.
	/// </summary>
	public class DefaultNonReferentialResolver : NonReferentialResolver
	{

	  private MaxentModel model;
	  private IList<Event> events;
	  private bool loadAsResource;
	  private bool debugOn = false;
	  private ResolverMode mode;
	  private string modelName;
	  private string modelExtension = ".bin.gz";
	  private int nonRefIndex;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DefaultNonReferentialResolver(String projectName, String name, ResolverMode mode) throws java.io.IOException
	  public DefaultNonReferentialResolver(string projectName, string name, ResolverMode mode)
	  {
		this.mode = mode;
		this.modelName = projectName + "/" + name + ".nr";
		if (mode == ResolverMode.TRAIN)
		{
		  events = new List<Event>();
		}
		else if (mode == ResolverMode.TEST)
		{
		  if (loadAsResource)
		  {
			model = (new BinaryGISModelReader(new DataInputStream(this.GetType().getResourceAsStream(modelName)))).Model;
		  }
		  else
		  {
			model = (new SuffixSensitiveGISModelReader(new Jfile(modelName + modelExtension))).Model;
		  }
		  nonRefIndex = model.getIndex(MaxentResolver.SAME);
		}
		else
		{
		  throw new Exception("unexpected mode " + mode);
		}
	  }

	  public virtual double getNonReferentialProbability(MentionContext mention)
	  {
		IList<string> features = getFeatures(mention);
		double r = model.eval(features.ToArray())[nonRefIndex];
		if (debugOn)
		{
			Console.Error.WriteLine(this + " " + mention.toText() + " ->  null " + r + " " + features);
		}
		return r;
	  }

	  public virtual void addEvent(MentionContext ec)
	  {
		IList<string> features = getFeatures(ec);
		if (-1 == ec.Id)
		{
		  events.Add(new Event(MaxentResolver.SAME, features.ToArray()));
		}
		else
		{
		  events.Add(new Event(MaxentResolver.DIFF, features.ToArray()));
		}
	  }

	  protected internal virtual IList<string> getFeatures(MentionContext mention)
	  {
		IList<string> features = new List<string>();
		features.Add(MaxentResolver.DEFAULT);
		features.AddRange(getNonReferentialFeatures(mention));
		return features;
	  }

	  /// <summary>
	  /// Returns a list of features used to predict whether the specified mention is non-referential. </summary>
	  /// <param name="mention"> The mention under consideration. </param>
	  /// <returns> a list of features used to predict whether the specified mention is non-referential. </returns>
	  protected internal virtual IList<string> getNonReferentialFeatures(MentionContext mention)
	  {
		IList<string> features = new List<string>();
		Parse[] mtokens = mention.TokenParses;
		//System.err.println("getNonReferentialFeatures: mention has "+mtokens.length+" tokens");
		for (int ti = 0; ti <= mention.HeadTokenIndex; ti++)
		{
		  Parse tok = mtokens[ti];
		  IList<string> wfs = ResolverUtils.getWordFeatures(tok);
		  for (int wfi = 0; wfi < wfs.Count; wfi++)
		  {
			features.Add("nr" + wfs[wfi]);
		  }
		}
		features.AddRange(ResolverUtils.getContextFeatures(mention));
		return features;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void train() throws java.io.IOException
	  public virtual void train()
	  {
		if (ResolverMode.TRAIN == mode)
		{
		  Console.Error.WriteLine(this + " referential");
		  if (debugOn)
		  {
			FileWriter writer = new FileWriter(modelName + ".events");
			for (IEnumerator<Event> ei = events.GetEnumerator(); ei.MoveNext();)
			{
			  Event e = ei.Current;
			  writer.write(e.ToString() + "\n");
			}
			writer.close();
		  }(new SuffixSensitiveGISModelWriter(GIS.trainModel(new CollectionEventStream(events),100,10),new Jfile(modelName + modelExtension))).persist();
		}
	  }
	}

}