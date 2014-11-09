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


namespace opennlp.tools.coref.resolver
{

	/// <summary>
	/// This class allows you to share a single instance of a non-referential resolver
	/// among several resolvers.
	/// </summary>
	public class SingletonNonReferentialResolver : DefaultNonReferentialResolver
	{

	  private static SingletonNonReferentialResolver resolver;
	  private static bool trained;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private SingletonNonReferentialResolver(String projectName, ResolverMode mode) throws java.io.IOException
	  private SingletonNonReferentialResolver(string projectName, ResolverMode mode) : base(projectName, "nonref", mode)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static SingletonNonReferentialResolver getInstance(String modelName, ResolverMode mode) throws java.io.IOException
	  public static SingletonNonReferentialResolver getInstance(string modelName, ResolverMode mode)
	  {
		if (resolver == null)
		{
		  resolver = new SingletonNonReferentialResolver(modelName, mode);
		}
		return resolver;
	  }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void train() throws java.io.IOException
	  public override void train()
	  {
		if (!trained)
		{
		  base.train();
		  trained = true;
		}
	  }
	}

}