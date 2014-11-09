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
using j4n.Lang;

namespace opennlp.tools.coref.resolver
{


	using MentionContext = opennlp.tools.coref.mention.MentionContext;

	/// <summary>
	/// This class resolver singular pronouns such as "he", "she", "it" and their various forms.
	/// </summary>
	public class SingularPronounResolver : MaxentResolver
	{

	  internal int mode;

	  internal Pattern PronounPattern;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SingularPronounResolver(String projectName, ResolverMode m) throws java.io.IOException
	  public SingularPronounResolver(string projectName, ResolverMode m) : base(projectName, "pmodel", m, 30)
	  {
		this.numSentencesBack = 2;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SingularPronounResolver(String projectName, ResolverMode m, NonReferentialResolver nonReferentialResolver) throws java.io.IOException
	  public SingularPronounResolver(string projectName, ResolverMode m, NonReferentialResolver nonReferentialResolver) : base(projectName, "pmodel", m, 30,nonReferentialResolver)
	  {
		this.numSentencesBack = 2;
	  }

	  public override bool canResolve(MentionContext mention)
	  {
		//System.err.println("MaxentSingularPronounResolver.canResolve: ec= ("+mention.id+") "+ mention.toText());
		string tag = mention.HeadTokenTag;
		return (tag != null && tag.StartsWith("PRP", StringComparison.Ordinal) && ResolverUtils.singularThirdPersonPronounPattern.matcher(mention.HeadTokenText).matches());
	  }

	  protected internal override IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
	  {
		IList<string> features = new List<string>();
		features.AddRange(base.getFeatures(mention, entity));
		if (entity != null) //generate pronoun w/ referent features
		{
		  MentionContext cec = entity.LastExtent;
		  //String gen = getPronounGender(pronoun);
		  features.AddRange(ResolverUtils.getPronounMatchFeatures(mention,entity));
		  features.AddRange(ResolverUtils.getContextFeatures(cec));
		  features.AddRange(ResolverUtils.getDistanceFeatures(mention,entity));
		  features.Add(ResolverUtils.getMentionCountFeature(entity));
		  /*
		  //lexical features
		  Set featureSet = new HashSet();
		  for (Iterator ei = entity.getExtents(); ei.hasNext();) {
		    MentionContext ec = (MentionContext) ei.next();
		    List toks = ec.tokens;
		    Parse tok;
		    int headIndex = PTBHeadFinder.getInstance().getHeadIndex(toks);
		    for (int ti = 0; ti < headIndex; ti++) {
		      tok = (Parse) toks.get(ti);
		      featureSet.add(gen + "mw=" + tok.toString().toLowerCase());
		      featureSet.add(gen + "mt=" + tok.getSyntacticType());
		    }
		    tok = (Parse) toks.get(headIndex);
		    featureSet.add(gen + "hw=" + tok.toString().toLowerCase());
		    featureSet.add(gen + "ht=" + tok.getSyntacticType());
		    //semantic features
		    if (ec.neType != null) {
		      featureSet.add(gen + "," + ec.neType);
		    }
		    else {
		      for (Iterator si = ec.synsets.iterator(); si.hasNext();) {
		        Integer synset = (Integer) si.next();
		        featureSet.add(gen + "," + synset);
		      }
		    }
		  }
		  Iterator fset = featureSet.iterator();
		  while (fset.hasNext()) {
		    String f = (String) fset.next();
		    features.add(f);
		  }
		  */
		}
		return (features);
	  }

	  public override bool excluded(MentionContext mention, DiscourseEntity entity)
	  {
		if (base.excluded(mention, entity))
		{
		  return (true);
		}
		string mentionGender = null;

		for (IEnumerator<MentionContext> ei = entity.Mentions; ei.MoveNext();)
		{
		  MentionContext entityMention = ei.Current;
		  string tag = entityMention.HeadTokenTag;
		  if (tag != null && tag.StartsWith("PRP", StringComparison.Ordinal) && ResolverUtils.singularThirdPersonPronounPattern.matcher(mention.HeadTokenText).matches())
		  {
			if (mentionGender == null) //lazy initialization
			{
			  mentionGender = ResolverUtils.getPronounGender(mention.HeadTokenText);
			}
			string entityGender = ResolverUtils.getPronounGender(entityMention.HeadTokenText);
			if (!entityGender.Equals("u") && !mentionGender.Equals(entityGender))
			{
			  return (true);
			}
		  }
		}
		return (false);
	  }

	  protected internal override bool outOfRange(MentionContext mention, DiscourseEntity entity)
	  {
		MentionContext cec = entity.LastExtent;
		//System.err.println("MaxentSingularPronounresolve.outOfRange: ["+entity.getLastExtent().toText()+" ("+entity.getId()+")] ["+mention.toText()+" ("+mention.getId()+")] entity.sentenceNumber=("+entity.getLastExtent().getSentenceNumber()+")-mention.sentenceNumber=("+mention.getSentenceNumber()+") > "+numSentencesBack);
		return (mention.SentenceNumber - cec.SentenceNumber > numSentencesBack);
	  }
	}

}