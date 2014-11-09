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

namespace opennlp.tools.coref
{

	using MentionContext = opennlp.tools.coref.mention.MentionContext;
	using GenderEnum = opennlp.tools.coref.sim.GenderEnum;
	using NumberEnum = opennlp.tools.coref.sim.NumberEnum;

	/// <summary>
	/// Represents an entity in a discourse model.
	/// </summary>
	public class DiscourseEntity : DiscourseElement
	{

	  private string category = null;
	  private GenderEnum gender;
	  private double genderProb;
	  private NumberEnum number;
	  private double numberProb;

	  /// <summary>
	  /// Creates a new entity based on the specified mention and its specified gender and number properties.
	  /// </summary>
	  /// <param name="mention"> The first mention of this entity. </param>
	  /// <param name="gender"> The gender of this entity. </param>
	  /// <param name="genderProb"> The probability that the specified gender is correct. </param>
	  /// <param name="number"> The number for this entity. </param>
	  /// <param name="numberProb"> The probability that the specified number is correct. </param>
	  public DiscourseEntity(MentionContext mention, GenderEnum gender, double genderProb, NumberEnum number, double numberProb) : base(mention)
	  {
		this.gender = gender;
		this.genderProb = genderProb;
		this.number = number;
		this.numberProb = numberProb;
	  }

	  /// <summary>
	  /// Creates a new entity based on the specified mention.
	  /// </summary>
	  /// <param name="mention"> The first mention of this entity. </param>
	  public DiscourseEntity(MentionContext mention) : base(mention)
	  {
		gender = GenderEnum.UNKNOWN;
		number = NumberEnum.UNKNOWN;
	  }

	  /// <summary>
	  /// Returns the semantic category of this entity.
	  /// This field is used to associated named-entity categories with an entity.
	  /// </summary>
	  /// <returns> the semantic category of this entity. </returns>
	  public virtual string Category
	  {
		  get
		  {
			return (category);
		  }
		  set
		  {
			category = value;
		  }
	  }


	  /// <summary>
	  /// Returns the gender associated with this entity.
	  /// </summary>
	  /// <returns> the gender associated with this entity. </returns>
	  public virtual GenderEnum Gender
	  {
		  get
		  {
			return gender;
		  }
		  set
		  {
			this.gender = value;
		  }
	  }

	  /// <summary>
	  /// Returns the probability for the gender associated with this entity.
	  /// </summary>
	  /// <returns> the probability for the gender associated with this entity. </returns>
	  public virtual double GenderProbability
	  {
		  get
		  {
			return genderProb;
		  }
		  set
		  {
			genderProb = value;
		  }
	  }

	  /// <summary>
	  /// Returns the number associated with this entity.
	  /// </summary>
	  /// <returns> the number associated with this entity. </returns>
	  public virtual NumberEnum Number
	  {
		  get
		  {
			return number;
		  }
		  set
		  {
			this.number = value;
		  }
	  }

	  /// <summary>
	  /// Returns the probability for the number associated with this entity.
	  /// </summary>
	  /// <returns> the probability for the number associated with this entity. </returns>
	  public virtual double NumberProbability
	  {
		  get
		  {
			return numberProb;
		  }
		  set
		  {
			numberProb = value;
		  }
	  }




	}

}