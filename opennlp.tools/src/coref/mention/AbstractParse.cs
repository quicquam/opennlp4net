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

namespace opennlp.tools.coref.mention
{


	/// <summary>
	/// Provides default implemenation of many of the methods in the <seealso cref="Parse"/> interface.
	/// </summary>
	public abstract class AbstractParse : Parse
	{
		public abstract Parse NextToken {get;}
		public abstract Parse PreviousToken {get;}
		public abstract opennlp.tools.util.Span Span {get;}
		public abstract int EntityId {get;}
		public override abstract string ToString();
		public abstract bool Token {get;}
		public abstract bool Sentence {get;}
		public abstract bool NounPhrase {get;}
		public abstract bool NamedEntity {get;}
		public abstract Parse Parent {get;}
		public abstract bool ParentNAC {get;}
		public abstract string EntityType {get;}
		public abstract string SyntacticType {get;}
		public abstract IList<Parse> Tokens {get;}
		public abstract IList<Parse> SyntacticChildren {get;}
		public abstract IList<Parse> Children {get;}
		public abstract IList<Parse> NamedEntities {get;}
		public abstract int SentenceNumber {get;}

	  public virtual bool CoordinatedNounPhrase
	  {
		  get
		  {
			IList<Parse> parts = SyntacticChildren;
			if (parts.Count >= 2)
			{
			  for (int pi = 1; pi < parts.Count; pi++)
			  {
				Parse child = parts[pi];
				string ctype = child.SyntacticType;
				if (ctype != null && ctype.Equals("CC") && !child.ToString().Equals("&"))
				{
				  return true;
				}
			  }
			}
			return false;
		  }
	  }

	  public virtual IList<Parse> NounPhrases
	  {
		  get
		  {
			IList<Parse> parts = SyntacticChildren;
			IList<Parse> nps = new List<Parse>();
			while (parts.Count > 0)
			{
			  IList<Parse> newParts = new List<Parse>();
			  for (int pi = 0,pn = parts.Count;pi < pn;pi++)
			  {
				//System.err.println("AbstractParse.getNounPhrases "+parts.get(pi).getClass());
				Parse cp = parts[pi];
				if (cp.NounPhrase)
				{
				  nps.Add(cp);
				}
				if (!cp.Token)
				{
				  newParts.AddRange(cp.SyntacticChildren);
				}
			  }
			  parts = newParts;
			}
			return nps;
		  }
	  }
	}

}