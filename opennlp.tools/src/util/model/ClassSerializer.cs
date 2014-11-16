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


	[Obsolete]
	public class ClassSerializer : ArtifactSerializer<Type>
	{

	  private const string CLASS_SEARCH_NAME = "ClassSearchName";

	  private sbyte[] classBytes;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Class loadClass(final byte[] classBytes) throws opennlp.tools.util.InvalidFormatException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  private static Type loadClass(sbyte[] classBytes)
	  {

		ClassLoader loader = new ClassLoaderAnonymousInnerClassHelper(classBytes);

		try
		{
		  return loader.loadClass(CLASS_SEARCH_NAME);
		}
		catch (ClassNotFoundException e)
		{
		  throw new InvalidFormatException(e);
		}
	  }

	  private class ClassLoaderAnonymousInnerClassHelper : ClassLoader
	  {
		  private sbyte[] classBytes;

		  public ClassLoaderAnonymousInnerClassHelper(sbyte[] classBytes)
		  {
			  this.classBytes = classBytes;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected Class findClass(String name) throws ClassNotFoundException
		  protected internal override Type findClass(string name, object defineClass)
		  {
			if (CLASS_SEARCH_NAME.Equals(name))
			{
			  return defineClass(null, classBytes, 0, classBytes.Length);
			}
			else
			{
			  return base.findClass(name);
			}
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Class create(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
	  public virtual Type create(InputStream @in)
	  {
		classBytes = ModelUtil.read(@in);

		Type factoryClass = loadClass(classBytes);

		return factoryClass;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void serialize(Class artifact, java.io.OutputStream out) throws java.io.IOException
	  public void serialize(Type artifact, OutputStream @out)
	  {
		@out.write(classBytes);
	  }


	}

    internal class ClassLoader
    {
        public Type loadClass(string classSearchName)
        {
            throw new NotImplementedException();
        }

        protected Type findClass(string name)
        {
            throw new NotImplementedException();
        }
    }
}