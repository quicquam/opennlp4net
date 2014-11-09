using System;

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

namespace opennlp.tools.coref.sim
{

	/// <summary>
	/// Model of mention compatibiltiy using a maxent model.
	/// </summary>
	public class MaxentCompatibilityModel
	{

	  private readonly double minGenderProb = 0.66;
	  private readonly double minNumberProb = 0.66;

	  private static TestGenderModel genModel;
	  private static TestNumberModel numModel;

	  private bool debugOn = false;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MaxentCompatibilityModel(String corefProject) throws java.io.IOException
	  public MaxentCompatibilityModel(string corefProject)
	  {
		genModel = GenderModel.PrepAttachDataUtil.testModel(corefProject + "/gen");
		numModel = NumberModel.PrepAttachDataUtil.testModel(corefProject + "/num");
	  }

	  public virtual Gender computeGender(Context c)
	  {
		Gender gender;
		double[] gdist = genModel.genderDistribution(c);
		if (debugOn)
		{
		  Console.Error.WriteLine("MaxentCompatibilityModel.computeGender: " + c.ToString() + " m=" + gdist[genModel.MaleIndex] + " f=" + gdist[genModel.FemaleIndex] + " n=" + gdist[genModel.NeuterIndex]);
		}
		if (genModel.MaleIndex >= 0 && gdist[genModel.MaleIndex] > minGenderProb)
		{
		  gender = new Gender(GenderEnum.MALE,gdist[genModel.MaleIndex]);
		}
		else if (genModel.FemaleIndex >= 0 && gdist[genModel.FemaleIndex] > minGenderProb)
		{
		  gender = new Gender(GenderEnum.FEMALE,gdist[genModel.FemaleIndex]);
		}
		else if (genModel.NeuterIndex >= 0 && gdist[genModel.NeuterIndex] > minGenderProb)
		{
		  gender = new Gender(GenderEnum.NEUTER,gdist[genModel.NeuterIndex]);
		}
		else
		{
		  gender = new Gender(GenderEnum.UNKNOWN,minGenderProb);
		}
		return gender;
	  }

	  public virtual Number computeNumber(Context c)
	  {
		double[] dist = numModel.numberDist(c);
		Number number;
		//System.err.println("MaxentCompatibiltyResolver.computeNumber: "+c+" sing="+dist[numModel.getSingularIndex()]+" plural="+dist[numModel.getPluralIndex()]);
		if (dist[numModel.SingularIndex] > minNumberProb)
		{
		  number = new Number(NumberEnum.SINGULAR,dist[numModel.SingularIndex]);
		}
		else if (dist[numModel.PluralIndex] > minNumberProb)
		{
		  number = new Number(NumberEnum.PLURAL,dist[numModel.PluralIndex]);
		}
		else
		{
		  number = new Number(NumberEnum.UNKNOWN,minNumberProb);
		}
		return number;
	  }
	}

}