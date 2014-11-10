using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


namespace opennlp.tools.parser.treeinsert
{



	public class AttachContextGenerator : AbstractContextGenerator
	{


	  public AttachContextGenerator(HashSet<string> punctSet)
	  {
		this.punctSet = punctSet;
	  }

	  public virtual string[] getContext(object o)
	  {
		object[] parts = (object[]) o;
		return getContext((Parse[]) parts[0], (int)parts[1], parts[2] as IList<Parse>, (int) parts[3]);
	  }

	  private bool containsPunct(ICollection<Parse> puncts, string punct)
	  {
		if (puncts != null)
		{
		  for (IEnumerator<Parse> pi = puncts.GetEnumerator(); pi.MoveNext();)
		  {
			Parse p = pi.Current;
			if (p.Type.Equals(punct))
			{
			  return true;
			}
		  }
		}
		return false;
	  }

	  /// 
	  /// <param name="constituents"> The constituents as they have been constructed so far. </param>
	  /// <param name="index"> The constituent index of the node being attached. </param>
	  /// <param name="rightFrontier"> The nodes which have been not attach to so far. </param>
	  /// <returns> A set of contextual features about this attachment. </returns>
	  public virtual string[] getContext(Parse[] constituents, int index, IList<Parse> rightFrontier, int rfi)
	  {
		IList<string> features = new List<string>(100);
		int nodeDistance = rfi;
		Parse fn = rightFrontier[rfi];
		Parse fp = null;
		if (rfi + 1 < rightFrontier.Count)
		{
		  fp = rightFrontier[rfi + 1];
		}
		Parse p_1 = null;
		if (rightFrontier.Count > 0)
		{
		  p_1 = rightFrontier[0];
		}
		Parse p0 = constituents[index];
		Parse p1 = null;
		if (index + 1 < constituents.Length)
		{
		  p1 = constituents[index + 1];
		}

		ICollection<Parse> punct1s = null;
		ICollection<Parse> punct_1s = null;
		ICollection<Parse> punct_1fs = null;
		punct_1fs = fn.PreviousPunctuationSet;
		punct_1s = p0.PreviousPunctuationSet;
		punct1s = p0.NextPunctuationSet;

		string consfp = cons(fp,-3);
		string consf = cons(fn,-2);
		string consp_1 = cons(p_1,-1);
		string consp0 = cons(p0,0);
		string consp1 = cons(p1,1);

		string consbofp = consbo(fp,-3);
		string consbof = consbo(fn,-2);
		string consbop_1 = consbo(p_1,-1);
		string consbop0 = consbo(p0,0);
		string consbop1 = consbo(p1,1);

		Cons cfp = new Cons(consfp,consbofp,-3,true);
		Cons cf = new Cons(consf,consbof,-2,true);
		Cons c_1 = new Cons(consp_1,consbop_1,-1,true);
		Cons c0 = new Cons(consp0,consbop0,0,true);
		Cons c1 = new Cons(consp1,consbop1,1,true);

		//default
		features.Add("default");

		//unigrams
		features.Add(consfp);
		features.Add(consbofp);
		features.Add(consf);
		features.Add(consbof);
		features.Add(consp_1);
		features.Add(consbop_1);
		features.Add(consp0);
		features.Add(consbop0);
		features.Add(consp1);
		features.Add(consbop1);

		//productions
		string prod = production(fn,false);
		//String punctProd = production(fn,true,punctSet);
		features.Add("pn=" + prod);
		features.Add("pd=" + prod + "," + p0.Type);
		features.Add("ps=" + fn.Type + "->" + fn.Type + "," + p0.Type);
		if (punct_1s != null)
		{
		  StringBuilder punctBuf = new StringBuilder(5);
		  for (IEnumerator<Parse> pi = punct_1s.GetEnumerator(); pi.MoveNext();)
		  {
			Parse punct = pi.Current;
			punctBuf.Append(punct.Type).Append(",");
		  }
		  //features.add("ppd="+punctProd+","+punctBuf.toString()+p0.getType());
		  //features.add("pps="+fn.getType()+"->"+fn.getType()+","+punctBuf.toString()+p0.getType());
		}

		//bi-grams
		//cons(fn),cons(0)
		cons2(features,cfp,c0,punct_1s,true);
		cons2(features,cf,c0,punct_1s,true);
		cons2(features,c_1,c0,punct_1s,true);
		cons2(features,c0,c1,punct1s,true);
		cons3(features,cf,c_1,c0,null,punct_1s,true,true,true);
		cons3(features,cf,c0,c1,punct_1s,punct1s,true,true,true);
		cons3(features,cfp,cf,c0,null,punct_1s,true,true,true);
		/*
		for (int ri=0;ri<rfi;ri++) {
		  Parse jn = (Parse) rightFrontier.get(ri);
		  features.add("jn="+jn.getType());
		}
		*/
		int headDistance = (p0.HeadIndex - fn.HeadIndex);
		features.Add("hd=" + headDistance);
		features.Add("nd=" + nodeDistance);

		features.Add("nd=" + p0.Type + "." + nodeDistance);
		features.Add("hd=" + p0.Type + "." + headDistance);
		//features.add("fs="+rightFrontier.size());
		//paired punct features
		if (containsPunct(punct_1s,"''"))
		{
		  if (containsPunct(punct_1fs,"``"))
		  {
			features.Add("quotematch"); //? not generating feature correctly

		  }
		  else
		  {
			//features.add("noquotematch");
		  }
		}
		return features.ToArray();
	  }
	}

}