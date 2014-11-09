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

namespace opennlp.tools.coref.mention
{



	/// <summary>
	/// Finds head information from Penn Treebank style parses.
	/// </summary>
	public sealed class PTBHeadFinder : HeadFinder
	{

	  private static PTBHeadFinder instance;
	  private static HashSet<string> skipSet = new HashSet<string>();
	  static PTBHeadFinder()
	  {
		skipSet.Add("POS");
		skipSet.Add(",");
		skipSet.Add(":");
		skipSet.Add(".");
		skipSet.Add("''");
		skipSet.Add("-RRB-");
		skipSet.Add("-RCB-");
	  }

	  private PTBHeadFinder()
	  {
	  }

	  /// <summary>
	  /// Returns an instance of this head finder. </summary>
	  /// <returns> an instance of this head finder. </returns>
	  public static HeadFinder Instance
	  {
		  get
		  {
			if (instance == null)
			{
			  instance = new PTBHeadFinder();
			}
			return instance;
		  }
	  }

	  public Parse getHead(Parse p)
	  {
		if (p == null)
		{
		  return null;
		}
		if (p.NounPhrase)
		{
		  IList<Parse> parts = p.SyntacticChildren;
		  //shallow parse POS
		  if (parts.Count > 2)
		  {
			Parse child0 = parts[0];
			Parse child1 = parts[1];
			Parse child2 = parts[2];
			if (child1.Token && child1.SyntacticType.Equals("POS") && child0.NounPhrase && child2.NounPhrase)
			{
			  return child2;
			}
		  }
		  //full parse POS
		  if (parts.Count > 1)
		  {
			Parse child0 = parts[0];
			if (child0.NounPhrase)
			{
			  IList<Parse> ctoks = child0.Tokens;
			  if (ctoks.Count == 0)
			  {
				Console.Error.WriteLine("PTBHeadFinder: NP " + child0 + " with no tokens");
			  }
			  Parse tok = ctoks[ctoks.Count - 1];
			  if (tok.SyntacticType.Equals("POS"))
			  {
				return null;
			  }
			}
		  }
		  //coordinated nps are their own entities
		  if (parts.Count > 1)
		  {
			for (int pi = 1; pi < parts.Count - 1; pi++)
			{
			  Parse child = parts[pi];
			  if (child.Token && child.SyntacticType.Equals("CC"))
			  {
				return null;
			  }
			}
		  }
		  //all other NPs
		  for (int pi = 0; pi < parts.Count; pi++)
		  {
			Parse child = parts[pi];
			//System.err.println("PTBHeadFinder.getHead: "+p.getSyntacticType()+" "+p+" child "+pi+"="+child.getSyntacticType()+" "+child);
			if (child.NounPhrase)
			{
			  return child;
			}
		  }
		  return null;
		}
		else
		{
		  return null;
		}
	  }

	  public int getHeadIndex(Parse p)
	  {
		IList<Parse> sChildren = p.SyntacticChildren;
		bool countTokens = false;
		int tokenCount = 0;
		//check for NP -> NN S type structures and return last token before S as head.
		for (int sci = 0,scn = sChildren.Count;sci < scn;sci++)
		{
		  Parse sc = sChildren[sci];
		  //System.err.println("PTBHeadFinder.getHeadIndex "+p+" "+p.getSyntacticType()+" sChild "+sci+" type = "+sc.getSyntacticType());
		  if (sc.SyntacticType.StartsWith("S", StringComparison.Ordinal))
		  {
			if (sci != 0)
			{
			  countTokens = true;
			}
			else
			{
			  //System.err.println("PTBHeadFinder.getHeadIndex(): NP -> S production assuming right-most head");
			}
		  }
		  if (countTokens)
		  {
			tokenCount += sc.Tokens.Count;
		  }
		}
		IList<Parse> toks = p.Tokens;
		if (toks.Count == 0)
		{
		  Console.Error.WriteLine("PTBHeadFinder.getHeadIndex(): empty tok list for parse " + p);
		}
		for (int ti = toks.Count - tokenCount - 1; ti >= 0; ti--)
		{
		  Parse tok = toks[ti];
		  if (!skipSet.Contains(tok.SyntacticType))
		  {
			return ti;
		  }
		}
		//System.err.println("PTBHeadFinder.getHeadIndex: "+p+" hi="+toks.size()+"-"+tokenCount+" -1 = "+(toks.size()-tokenCount -1));
		return toks.Count - tokenCount - 1;
	  }

	  /// <summary>
	  /// Returns the bottom-most head of a <code>Parse</code>.  If no
	  ///    head is available which is a child of <code>p</code> then
	  ///    <code>p</code> is returned. 
	  /// </summary>
	  public Parse getLastHead(Parse p)
	  {
		Parse head;
		//System.err.print("EntityFinder.getLastHead: "+p);

		while (null != (head = getHead(p)))
		{
		  //System.err.print(" -> "+head);
		  //if (p.getEntityId() != -1 && head.getEntityId() != p.getEntityId()) {	System.err.println(p+" ("+p.getEntityId()+") -> "+head+" ("+head.getEntityId()+")");      }
		  p = head;
		}
		//System.err.println(" -> null");
		return p;
	  }

	  public Parse getHeadToken(Parse p)
	  {
		IList<Parse> toks = p.Tokens;
		return toks[getHeadIndex(p)];
	  }
	}

}