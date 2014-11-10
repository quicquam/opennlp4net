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


namespace opennlp.tools.coref.mention
{


	using ResolverUtils = opennlp.tools.coref.resolver.ResolverUtils;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// Provides default implementation of many of the methods in the <seealso cref="MentionFinder"/> interface.
	/// </summary>
	public abstract class AbstractMentionFinder : MentionFinder
	{

	  protected internal HeadFinder headFinder;

	  protected internal bool collectPrenominalNamedEntities_Renamed;
	  protected internal bool collectCoordinatedNounPhrases;

	  private void gatherHeads(Parse p, IDictionary<Parse, Parse> heads)
	  {
		Parse head = headFinder.getHead(p);
		//System.err.println("AbstractMention.gatherHeads: "+head+" -> ("+p.hashCode()+") "+p);
		//if (head != null) { System.err.println("head.hashCode()="+head.hashCode());}
		if (head != null)
		{
		  heads[head] = p;
		}
	  }

	  /// <summary>
	  /// Assigns head relations between noun phrases and the child np
	  ///  which is their head. </summary>
	  ///  <param name="nps"> List of valid nps for this mention finder. </param>
	  ///  <returns> mapping from noun phrases and the child np which is their head
	  ///  </returns>
	  protected internal virtual IDictionary<Parse, Parse> constructHeadMap(IList<Parse> nps)
	  {
		IDictionary<Parse, Parse> headMap = new Dictionary<Parse, Parse>();
		for (int ni = 0; ni < nps.Count; ni++)
		{
		  Parse np = nps[ni];
		  gatherHeads(np, headMap);
		}
		return headMap;
	  }

	  public virtual bool PrenominalNamedEntityCollection
	  {
		  get
		  {
			return collectPrenominalNamedEntities_Renamed;
		  }
		  set
		  {
			collectPrenominalNamedEntities_Renamed = value;
		  }
	  }


	  protected internal virtual bool isBasalNounPhrase(Parse np)
	  {
		return np.NounPhrases.Count == 0;
	  }

	  protected internal virtual bool isPossessive(Parse np)
	  {
		IList<Parse> parts = np.SyntacticChildren;
		if (parts.Count > 1)
		{
		  Parse child0 = parts[0];
		  if (child0.NounPhrase)
		  {
			IList<Parse> ctoks = child0.Tokens;
			Parse tok = ctoks[ctoks.Count - 1];
			if (tok.SyntacticType.Equals("POS"))
			{
			  return true;
			}
		  }
		}
		if (parts.Count > 2)
		{
		  Parse child0 = parts[0];
		  Parse child1 = parts[1];
		  Parse child2 = parts[2];
		  if (child1.Token && child1.SyntacticType.Equals("POS") && child0.NounPhrase && child2.NounPhrase)
		  {
			return true;
		  }
		}
		return false;
	  }

	  protected internal virtual bool isOfPrepPhrase(Parse np)
	  {
		IList<Parse> parts = np.SyntacticChildren;
		if (parts.Count == 2)
		{
		  Parse child0 = parts[0];
		  if (child0.NounPhrase)
		  {
			Parse child1 = parts[1];
			IList<Parse> cparts = child1.SyntacticChildren;
			if (cparts.Count == 2)
			{
			  Parse child2 = cparts[0];
			  if (child2.Token && child2.ToString().Equals("of"))
			  {
				return true;
			  }
			}
		  }
		}
		return false;
	  }

	  protected internal virtual bool isConjoinedBasal(Parse np)
	  {
		IList<Parse> parts = np.SyntacticChildren;
		bool allToken = true;
		bool hasConjunction = false;
		for (int ti = 0; ti < parts.Count; ti++)
		{
		  Parse c = parts[ti];
		  if (c.Token)
		  {
			if (c.SyntacticType.Equals("CC"))
			{
			  hasConjunction = true;
			}
		  }
		  else
		  {
			allToken = false;
			break;
		  }
		}
		return allToken && hasConjunction;
	  }

	  private void collectCoordinatedNounPhraseMentions(Parse np, IList<Mention> entities)
	  {
		//System.err.println("collectCoordNp: "+np);
		//exclude nps with UCPs inside.
		IList<Parse> sc = np.SyntacticChildren;
		for (IEnumerator<Parse> sci = sc.GetEnumerator(); sci.MoveNext();)
		{
		  Parse scp = sci.Current;
		  if (scp.SyntacticType.Equals("UCP") || scp.SyntacticType.Equals("NX"))
		  {
			return;
		  }
		}
		IList<Parse> npTokens = np.Tokens;
		bool inCoordinatedNounPhrase = false;
		int lastNpTokenIndex = headFinder.getHeadIndex(np);
		for (int ti = lastNpTokenIndex - 1; ti >= 0; ti--)
		{
		  Parse tok = npTokens[ti];
		  string tokStr = tok.ToString();
		  if ((tokStr.Equals("and") || tokStr.Equals("or")) && !isPartOfName(tok))
		  {
			if (lastNpTokenIndex != ti)
			{
			  if (ti - 1 >= 0 && (npTokens[ti - 1]).SyntacticType.StartsWith("NN", StringComparison.Ordinal))
			  {
				Span npSpan = new Span((npTokens[ti + 1]).Span.Start, (npTokens[lastNpTokenIndex]).Span.End);
				Mention snpExtent = new Mention(npSpan, npSpan, tok.EntityId, null,"CNP");
				entities.Add(snpExtent);
				//System.err.println("adding extent for conjunction in: "+np+" preeceeded by "+((Parse) npTokens.get(ti-1)).getSyntacticType());
				inCoordinatedNounPhrase = true;
			  }
			  else
			  {
				break;
			  }
			}
			lastNpTokenIndex = ti - 1;
		  }
		  else if (inCoordinatedNounPhrase && tokStr.Equals(","))
		  {
			if (lastNpTokenIndex != ti)
			{
			  Span npSpan = new Span((npTokens[ti + 1]).Span.Start, (npTokens[lastNpTokenIndex]).Span.End);
			  Mention snpExtent = new Mention(npSpan, npSpan, tok.EntityId, null,"CNP");
			  entities.Add(snpExtent);
			  //System.err.println("adding extent for comma in: "+np);
			}
			lastNpTokenIndex = ti - 1;
		  }
		  else if (inCoordinatedNounPhrase && ti == 0 && lastNpTokenIndex >= 0)
		  {
			Span npSpan = new Span((npTokens[ti]).Span.Start, (npTokens[lastNpTokenIndex]).Span.End);
			Mention snpExtent = new Mention(npSpan, npSpan, tok.EntityId, null,"CNP");
			entities.Add(snpExtent);
			//System.err.println("adding extent for start coord in: "+np);
		  }
		}
	  }

	  private bool handledPronoun(string tok)
	  {
		return ResolverUtils.singularThirdPersonPronounPattern.matcher(tok).find() || ResolverUtils.pluralThirdPersonPronounPattern.matcher(tok).find() || ResolverUtils.speechPronounPattern.matcher(tok).find();
	  }

	  private void collectPossesivePronouns(Parse np, IList<Mention> entities)
	  {
		//TODO: Look at how training is done and examine whether this is needed or can be accomidated in a different way.
		/*
		List snps = np.getSubNounPhrases();
		if (snps.size() != 0) {
		  //System.err.println("AbstractMentionFinder: Found existing snps");
		  for (int si = 0, sl = snps.size(); si < sl; si++) {
		    Parse snp = (Parse) snps.get(si);
		    Extent ppExtent = new Extent(snp.getSpan(), snp.getSpan(), snp.getEntityId(), null,Linker.PRONOUN_MODIFIER);
		    entities.add(ppExtent);
		  }
		}
		else {
		*/
		  //System.err.println("AbstractEntityFinder.collectPossesivePronouns: "+np);
		  IList<Parse> npTokens = np.Tokens;
		  Parse headToken = headFinder.getHeadToken(np);
		  for (int ti = npTokens.Count - 2; ti >= 0; ti--)
		  {
			Parse tok = npTokens[ti];
			if (tok == headToken)
			{
			  continue;
			}
			if (tok.SyntacticType.StartsWith("PRP", StringComparison.Ordinal) && handledPronoun(tok.ToString()))
			{
			  Mention ppExtent = new Mention(tok.Span, tok.Span, tok.EntityId, null,opennlp.tools.coref.Linker_Fields.PRONOUN_MODIFIER);
			  //System.err.println("AbstractEntityFinder.collectPossesivePronouns: adding possesive pronoun: "+tok+" "+tok.getEntityId());
			  entities.Add(ppExtent);
			  //System.err.println("AbstractMentionFinder: adding pos-pro: "+ppExtent);
			  break;
			}
		  }
		//}
	  }

	  private void removeDuplicates(IList<Mention> extents)
	  {
		Mention lastExtent = null;
		for (IEnumerator<Mention> ei = extents.GetEnumerator(); ei.MoveNext();)
		{
		  Mention e = ei.Current;
		  if (lastExtent != null && e.Span.Equals(lastExtent.Span))
		  {
		      extents.Remove(e);
		  }
		  else
		  {
			lastExtent = e;
		  }
		}
	  }

	  private bool isHeadOfExistingMention(Parse np, IDictionary<Parse, Parse> headMap, HashSet<Parse> mentions)
	  {
		Parse head = headMap[np];
		while (head != null)
		{
		  if (mentions.Contains(head))
		  {
			return true;
		  }
		  head = headMap[head];
		}
		return false;
	  }


	  private void clearMentions(HashSet<Parse> mentions, Parse np)
	  {
		Span npSpan = np.Span;
		for (IEnumerator<Parse> mi = mentions.GetEnumerator(); mi.MoveNext();)
		{
		  Parse mention = mi.Current;
		  if (!mention.Span.contains(npSpan))
		  {
			//System.err.println("clearing "+mention+" for "+np);
			mi.remove();
		  }
		}
	  }

	  private Mention[] collectMentions(IList<Parse> nps, IDictionary<Parse, Parse> headMap)
	  {
		IList<Mention> mentions = new List<Mention>(nps.Count);
		HashSet<Parse> recentMentions = new HashSet<Parse>();
		//System.err.println("AbtractMentionFinder.collectMentions: "+headMap);
		for (int npi = 0, npl = nps.Count; npi < npl; npi++)
		{
		  Parse np = nps[npi];
		  //System.err.println("AbstractMentionFinder: collectMentions: np[" + npi + "]=" + np + " head=" + headMap.get(np));
		  if (!isHeadOfExistingMention(np,headMap, recentMentions))
		  {
			clearMentions(recentMentions, np);
			if (!isPartOfName(np))
			{
			  Parse head = headFinder.getLastHead(np);
			  Mention extent = new Mention(np.Span, head.Span, head.EntityId, np, null);
			  //System.err.println("adding "+np+" with head "+head);
			  mentions.Add(extent);
			  recentMentions.Add(np);
			  // determine name-entity type
			  string entityType = getEntityType(headFinder.getHeadToken(head));
			  if (entityType != null)
			  {
				extent.NameType = entityType;
			  }
			}
			else
			{
			  //System.err.println("AbstractMentionFinder.collectMentions excluding np as part of name. np=" + np);
			}
		  }
			 else
			 {
			//System.err.println("AbstractMentionFinder.collectMentions excluding np as head of previous mention. np=" + np);
			 }
		  if (isBasalNounPhrase(np))
		  {
			if (collectPrenominalNamedEntities_Renamed)
			{
			  collectPrenominalNamedEntities(np, mentions);
			}
			if (collectCoordinatedNounPhrases)
			{
			  collectCoordinatedNounPhraseMentions(np, mentions);
			}
			collectPossesivePronouns(np, mentions);
		  }
		  else
		  {
			// Could use to get NP -> tokens CON structures for basal nps including NP -> NAC tokens
			//collectComplexNounPhrases(np,mentions);
		  }
		}
		mentions.Sort();
		removeDuplicates(mentions);
		return mentions.ToArray();
	  }

	  /// <summary>
	  /// Adds a mention for the non-treebank-labeled possesive noun phrases. </summary>
	  /// <param name="possesiveNounPhrase"> The possesive noun phase which may require an additional mention. </param>
	  /// <param name="mentions"> The list of mentions into which a new mention can be added. </param>
	//  private void addPossesiveMentions(Parse possesiveNounPhrase, List<Mention> mentions) {
	//    List<Parse> kids = possesiveNounPhrase.getSyntacticChildren();
	//    if (kids.size() >1) {
	//      Parse firstToken = kids.get(1);
	//      if (firstToken.isToken() && !firstToken.getSyntacticType().equals("POS")) {
	//        Parse lastToken = kids.get(kids.size()-1);
	//        if (lastToken.isToken()) {
	//          Span extentSpan = new Span(firstToken.getSpan().getStart(),lastToken.getSpan().getEnd());
	//          Mention extent = new Mention(extentSpan, extentSpan, -1, null, null);
	//          mentions.add(extent);
	//        }
	//        else {
	//          System.err.println("AbstractMentionFinder.addPossesiveMentions: odd parse structure: "+possesiveNounPhrase);
	//        }
	//      }
	//    }
	//  }

	  private void collectPrenominalNamedEntities(Parse np, IList<Mention> extents)
	  {
		Parse htoken = headFinder.getHeadToken(np);
		IList<Parse> nes = np.NamedEntities;
		Span headTokenSpan = htoken.Span;
		for (int nei = 0, nel = nes.Count; nei < nel; nei++)
		{
		  Parse ne = nes[nei];
		  if (!ne.Span.contains(headTokenSpan))
		  {
			//System.err.println("adding extent for prenominal ne: "+ne);
			Mention extent = new Mention(ne.Span, ne.Span, ne.EntityId,null,"NAME");
			extent.NameType = ne.EntityType;
			extents.Add(extent);
		  }
		}
	  }

	  private string getEntityType(Parse headToken)
	  {
		string entityType;
		for (Parse parent = headToken.Parent; parent != null; parent = parent.Parent)
		{
		  entityType = parent.EntityType;
		  if (entityType != null)
		  {
			return entityType;
		  }
		  if (parent.Sentence)
		  {
			break;
		  }
		}
		IList<Parse> tc = headToken.Children;
		int tcs = tc.Count;
		if (tcs > 0)
		{
		  Parse tchild = tc[tcs - 1];
		  entityType = tchild.EntityType;
		  if (entityType != null)
		  {
			return entityType;
		  }
		}
		return null;
	  }

	  private bool isPartOfName(Parse np)
	  {
		string entityType;
		for (Parse parent = np.Parent; parent != null; parent = parent.Parent)
		{
		  entityType = parent.EntityType;
		  //System.err.println("AbstractMentionFinder.isPartOfName: entityType="+entityType);
		  if (entityType != null)
		  {
			//System.err.println("npSpan = "+np.getSpan()+" parentSpan="+parent.getSpan());
			if (!np.Span.contains(parent.Span))
			{
			  return true;
			}
		  }
		  if (parent.Sentence)
		  {
			break;
		  }
		}
		return false;
	  }

	  /// <summary>
	  /// Return all noun phrases which are contained by <code>p</code>. </summary>
	  /// <param name="p"> The parse in which to find the noun phrases. </param>
	  /// <returns> A list of <code>Parse</code> objects which are noun phrases contained by <code>p</code>. </returns>
	  //protected abstract List getNounPhrases(Parse p);

	  public virtual IList<Parse> getNamedEntities(Parse p)
	  {
		return p.NamedEntities;
	  }

	  public virtual Mention[] getMentions(Parse p)
	  {
		IList<Parse> nps = p.NounPhrases;
		Array.Sort(nps.ToArray());
		IDictionary<Parse, Parse> headMap = constructHeadMap(nps);
		//System.err.println("AbstractMentionFinder.getMentions: got " + nps.size()); // + " nps, and " + nes.size() + " named entities");
		Mention[] mentions = collectMentions(nps, headMap);
		return mentions;
	  }

	  public virtual bool CoordinatedNounPhraseCollection
	  {
		  get
		  {
			return collectCoordinatedNounPhrases;
		  }
		  set
		  {
			collectCoordinatedNounPhrases = value;
		  }
	  }

	}

}