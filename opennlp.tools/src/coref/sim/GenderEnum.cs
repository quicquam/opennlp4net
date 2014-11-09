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
	/// Enumeration of gender types.
	/// </summary>
	public class GenderEnum
	{
	  private string gender;

	  /// <summary>
	  /// Male gender. </summary>
	  public static readonly GenderEnum MALE = new GenderEnum("male");
	  /// <summary>
	  /// Female gender. </summary>
	  public static readonly GenderEnum FEMALE = new GenderEnum("female");
	  /// <summary>
	  /// Nueter gender. </summary>
	  public static readonly GenderEnum NEUTER = new GenderEnum("neuter");
	  /// <summary>
	  /// Unknown gender. </summary>
	  public static readonly GenderEnum UNKNOWN = new GenderEnum("unknown");

	  private GenderEnum(string g)
	  {
		gender = g;
	  }

	  public override string ToString()
	  {
		return gender;
	  }
	}

}