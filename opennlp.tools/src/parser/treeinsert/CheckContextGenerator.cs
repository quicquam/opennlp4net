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


namespace opennlp.tools.parser.treeinsert
{



	public class CheckContextGenerator : AbstractContextGenerator
	{

	  private Parse[] leftNodes;

	  public CheckContextGenerator(HashSet<string> punctSet)
	  {
		this.punctSet = punctSet;
		leftNodes = new Parse[2];
	  }

	  public virtual string[] getContext(object arg0)
	  {
		// TODO Auto-generated method stub
		return null;
	  }

	  public virtual string[] getContext(Parse parent, Parse[] constituents, int index, bool trimFrontier)
	  {
		IList<string> features = new List<string>(100);
		//default
		features.Add("default");
		Parse[] children = AbstractBottomUpParser.collapsePunctuation(parent.Children,punctSet);
		Parse pstart = children[0];
		Parse pend = children[children.Length - 1];
		string type = parent.Type;
		checkcons(pstart, "begin", type, features);
		checkcons(pend, "last", type, features);
		string production = "p=" + production(parent,false);
		string punctProduction = "pp=" + production(parent,true);
		features.Add(production);
		features.Add(punctProduction);


		Parse p1 = null;
		Parse p2 = null;
		Parse p_1 = null;
		Parse p_2 = null;
		ICollection<Parse> p1s = constituents[index].NextPunctuationSet;
		ICollection<Parse> p2s = null;
		ICollection<Parse> p_1s = constituents[index].PreviousPunctuationSet;
		ICollection<Parse> p_2s = null;
		IList<Parse> rf;
		if (index == 0)
		{
		  rf = Collections.emptyList();
		}
		else
		{
		  rf = Parser.getRightFrontier(constituents[0], punctSet);
		  if (trimFrontier)
		  {
			int pi = rf.IndexOf(parent);
			if (pi == -1)
			{
			  throw new Exception("Parent not found in right frontier:" + parent + " rf=" + rf);
			}
			else
			{
			  for (int ri = 0;ri <= pi;ri++)
			  {
				//System.err.println(pi+" removing "+((Parse)rf.get(0)).getType()+" "+rf.get(0)+" "+(rf.size()-1)+" remain");
				rf.RemoveAt(0);
			  }
			}
		  }
		}

		getFrontierNodes(rf,leftNodes);
		p_1 = leftNodes[0];
		p_2 = leftNodes[1];
		int ps = constituents.Length;
		if (p_1 != null)
		{
		  p_2s = p_1.PreviousPunctuationSet;
		}
		if (index + 1 < ps)
		{
		  p1 = constituents[index + 1];
		  p2s = p1.NextPunctuationSet;
		}
		if (index + 2 < ps)
		{
		  p2 = constituents[index + 2];
		}
		surround(p_1, -1, type, p_1s, features);
		surround(p_2, -2, type, p_2s, features);
		surround(p1, 1, type, p1s, features);
		surround(p2, 2, type, p2s, features);

		return features.ToArray();
	  }

	}

}