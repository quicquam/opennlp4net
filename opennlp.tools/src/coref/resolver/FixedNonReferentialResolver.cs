﻿/*
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

namespace opennlp.tools.coref.resolver
{

	using MentionContext = opennlp.tools.coref.mention.MentionContext;

	/// <summary>
	/// Implementation of non-referential classifier which uses a fixed-value threshold.
	/// </summary>
	public class FixedNonReferentialResolver : NonReferentialResolver
	{

	  private double nonReferentialProbability;

	  public FixedNonReferentialResolver(double nonReferentialProbability)
	  {
		this.nonReferentialProbability = nonReferentialProbability;
	  }

	  public virtual double getNonReferentialProbability(MentionContext mention)
	  {
		return this.nonReferentialProbability;
	  }

	  public virtual void addEvent(MentionContext mention)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void train() throws java.io.IOException
	  public virtual void train()
	  {
	  }
	}

}