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

	public class SemanticEnum
	{

	  private string compatibility;

	  /// <summary>
	  /// Semantically compatible. </summary>
	  public static readonly SemanticEnum COMPATIBLE = new SemanticEnum("compatible");
	  /// <summary>
	  /// Semantically incompatible. </summary>
	  public static readonly SemanticEnum INCOMPATIBLE = new SemanticEnum("incompatible");
	  /// <summary>
	  /// Semantic compatibility Unknown. </summary>
	  public static readonly SemanticEnum UNKNOWN = new SemanticEnum("unknown");

	  private SemanticEnum(string g)
	  {
		compatibility = g;
	  }

	  public override string ToString()
	  {
		return compatibility;
	  }
	}

}