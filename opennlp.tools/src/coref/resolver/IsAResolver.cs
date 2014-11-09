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
	///  Resolves coreference between appositives.
	/// </summary>
	public class IsAResolver : MaxentResolver
	{

	  internal Pattern predicativePattern;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IsAResolver(String projectName, ResolverMode m) throws java.io.IOException
	  public IsAResolver(string projectName, ResolverMode m) : base(projectName, "/imodel", m, 20)
	  {
		showExclusions = false;
		//predicativePattern = Pattern.compile("^(,|am|are|is|was|were|--)$");
		predicativePattern = Pattern.compile("^(,|--)$");
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IsAResolver(String projectName, ResolverMode m, NonReferentialResolver nrr) throws java.io.IOException
	  public IsAResolver(string projectName, ResolverMode m, NonReferentialResolver nrr) : base(projectName, "/imodel", m, 20,nrr)
	  {
		showExclusions = false;
		//predicativePattern = Pattern.compile("^(,|am|are|is|was|were|--)$");
		predicativePattern = Pattern.compile("^(,|--)$");
	  }


	  public override bool canResolve(MentionContext ec)
	  {
		if (ec.HeadTokenTag.StartsWith("NN", StringComparison.Ordinal))
		{
		  return (ec.PreviousToken != null && predicativePattern.matcher(ec.PreviousToken.ToString()).matches());
		}
		return false;
	  }

	  protected internal override bool excluded(MentionContext ec, DiscourseEntity de)
	  {
		MentionContext cec = de.LastExtent;
		//System.err.println("IsAResolver.excluded?: ec.span="+ec.getSpan()+" cec.span="+cec.getSpan()+" cec="+cec.toText()+" lastToken="+ec.getNextToken());
		if (ec.SentenceNumber != cec.SentenceNumber)
		{
		  //System.err.println("IsAResolver.excluded: (true) not same sentence");
		  return (true);
		}
		//shallow parse appositives
		//System.err.println("IsAResolver.excluded: ec="+ec.toText()+" "+ec.span+" cec="+cec.toText()+" "+cec.span);
		if (cec.IndexSpan.End == ec.IndexSpan.Start - 2)
		{
		  return (false);
		}
		//full parse w/o trailing comma
		if (cec.IndexSpan.End == ec.IndexSpan.End)
		{
		  //System.err.println("IsAResolver.excluded: (false) spans share end");
		  return (false);
		}
		//full parse w/ trailing comma or period
		if (cec.IndexSpan.End <= ec.IndexSpan.End + 2 && (ec.NextToken != null && (ec.NextToken.ToString().Equals(",") || ec.NextToken.ToString().Equals("."))))
		{
		  //System.err.println("IsAResolver.excluded: (false) spans end + punct");
		  return (false);
		}
		//System.err.println("IsAResolver.excluded: (true) default");
		return (true);
	  }

	  protected internal override bool outOfRange(MentionContext ec, DiscourseEntity de)
	  {
		MentionContext cec = de.LastExtent;
		return (cec.SentenceNumber != ec.SentenceNumber);
	  }

	  protected internal override bool defaultReferent(DiscourseEntity de)
	  {
		return (true);
	  }

	  protected internal override IList<string> getFeatures(MentionContext mention, DiscourseEntity entity)
	  {
		IList<string> features = new List<string>();
		features.AddRange(base.getFeatures(mention, entity));
		if (entity != null)
		{
		  MentionContext ant = entity.LastExtent;
		  IList<string> leftContexts = ResolverUtils.getContextFeatures(ant);
		  for (int ci = 0, cn = leftContexts.Count; ci < cn; ci++)
		  {
			features.Add("l" + leftContexts[ci]);
		  }
		  IList<string> rightContexts = ResolverUtils.getContextFeatures(mention);
		  for (int ci = 0, cn = rightContexts.Count; ci < cn; ci++)
		  {
			features.Add("r" + rightContexts[ci]);
		  }
		  features.Add("hts" + ant.HeadTokenTag + "," + mention.HeadTokenTag);
		}
		/*
		if (entity != null) {
		  //System.err.println("MaxentIsResolver.getFeatures: ["+ec2.toText()+"] -> ["+de.getLastExtent().toText()+"]");
		  //previous word and tag
		  if (ant.prevToken != null) {
		    features.add("pw=" + ant.prevToken);
		    features.add("pt=" + ant.prevToken.getSyntacticType());
		  }
		  else {
		    features.add("pw=<none>");
		    features.add("pt=<none>");
		  }
	
		  //next word and tag
		  if (mention.nextToken != null) {
		    features.add("nw=" + mention.nextToken);
		    features.add("nt=" + mention.nextToken.getSyntacticType());
		  }
		  else {
		    features.add("nw=<none>");
		    features.add("nt=<none>");
		  }
	
		  //modifier word and tag for c1
		  int i = 0;
		  List c1toks = ant.tokens;
		  for (; i < ant.headTokenIndex; i++) {
		    features.add("mw=" + c1toks.get(i));
		    features.add("mt=" + ((Parse) c1toks.get(i)).getSyntacticType());
		  }
		  //head word and tag for c1
		  features.add("mh=" + c1toks.get(i));
		  features.add("mt=" + ((Parse) c1toks.get(i)).getSyntacticType());
	
		  //modifier word and tag for c2
		  i = 0;
		  List c2toks = mention.tokens;
		  for (; i < mention.headTokenIndex; i++) {
		    features.add("mw=" + c2toks.get(i));
		    features.add("mt=" + ((Parse) c2toks.get(i)).getSyntacticType());
		  }
		  //head word and tag for n2
		  features.add("mh=" + c2toks.get(i));
		  features.add("mt=" + ((Parse) c2toks.get(i)).getSyntacticType());
	
		  //word/tag pairs
		  for (i = 0; i < ant.headTokenIndex; i++) {
		    for (int j = 0; j < mention.headTokenIndex; j++) {
		      features.add("w=" + c1toks.get(i) + "|" + "w=" + c2toks.get(j));
		      features.add("w=" + c1toks.get(i) + "|" + "t=" + ((Parse) c2toks.get(j)).getSyntacticType());
		      features.add("t=" + ((Parse) c1toks.get(i)).getSyntacticType() + "|" + "w=" + c2toks.get(j));
		      features.add("t=" + ((Parse) c1toks.get(i)).getSyntacticType() + "|" + "t=" + ((Parse) c2toks.get(j)).getSyntacticType());
		    }
		  }
		  features.add("ht=" + ant.headTokenTag + "|" + "ht=" + mention.headTokenTag);
		  features.add("ht1=" + ant.headTokenTag);
		  features.add("ht2=" + mention.headTokenTag);
		 */
		  //semantic categories
		  /*
		  if (ant.neType != null) {
		    if (re.neType != null) {
		      features.add("sc="+ant.neType+","+re.neType);
		    }
		    else if (!re.headTokenTag.startsWith("NNP") && re.headTokenTag.startsWith("NN")) {
		      Set synsets = re.synsets;
		      for (Iterator si=synsets.iterator();si.hasNext();) {
		        features.add("sc="+ant.neType+","+si.next());
		      }
		    }
		  }
		  else if (!ant.headTokenTag.startsWith("NNP") && ant.headTokenTag.startsWith("NN")) {
		    if (re.neType != null) {
		      Set synsets = ant.synsets;
		      for (Iterator si=synsets.iterator();si.hasNext();) {
		        features.add("sc="+re.neType+","+si.next());
		      }
		    }
		    else if (!re.headTokenTag.startsWith("NNP") && re.headTokenTag.startsWith("NN")) {
		      //System.err.println("MaxentIsaResolover.getFeatures: both common re="+re.parse+" ant="+ant.parse);
		      Set synsets1 = ant.synsets;
		      Set synsets2 = re.synsets;
		      for (Iterator si=synsets1.iterator();si.hasNext();) {
		        Object synset = si.next();
		        if (synsets2.contains(synset)) {
		          features.add("sc="+synset);
		        }
		      }
		    }
		  }
		}
		*/
		//System.err.println("MaxentIsResolver.getFeatures: "+features.toString());
		return (features);
	  }
	}

}