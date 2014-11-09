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
using j4n.Exceptions;
using j4n.Interfaces;
using j4n.IO.InputStream;


namespace opennlp.tools.util.model
{


	using FeatureGeneratorFactory = opennlp.tools.util.featuregen.FeatureGeneratorFactory;

	[Obsolete]
	public class FeatureGeneratorFactorySerializer : ArtifactSerializer<FeatureGeneratorFactory>
	{
	    private class ClassSerializer
	    {
	        public Type create(InputStream @in)
	        {
	            throw new NotImplementedException();
	        }

	        public void serialize(Type getType, OutputStream @out)
	        {
	            throw new NotImplementedException();
	        }
	    }

	    private ClassSerializer classSerializer;

	  public FeatureGeneratorFactorySerializer()
	  {
		classSerializer = new ClassSerializer();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.util.featuregen.FeatureGeneratorFactory create(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
	  public virtual FeatureGeneratorFactory create(InputStream @in)
	  {

		Type generatorFactoryClass = classSerializer.create(@in);

		try
		{
		  return (FeatureGeneratorFactory) generatorFactoryClass.newInstance();
		}
		catch (InstantiationException e)
		{
		  throw new InvalidFormatException(e);
		}
		catch (IllegalAccessException e)
		{
		  throw new InvalidFormatException(e);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void serialize(opennlp.tools.util.featuregen.FeatureGeneratorFactory artifact, java.io.OutputStream out) throws java.io.IOException
	  public virtual void serialize(FeatureGeneratorFactory artifact, OutputStream @out)
	  {
		classSerializer.serialize(artifact.GetType(), @out);
	  }
	}

}