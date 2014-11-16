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
using j4n.Serialization;

namespace opennlp.tools.formats
{

	using opennlp.tools.cmdline;

	/// <summary>
	/// Base class for sample stream factories.
	/// </summary>
	public abstract class AbstractSampleStreamFactory : ObjectStreamFactory
	{
	    public Type getParameters<TP>()
	    {
	        throw new NotImplementedException();
	    }

	    public abstract ObjectStream<T> create<T>(string[] args);
	    public Type Parameters { get; set; }

// ReSharper disable once InconsistentNaming
	  protected internal Type @params;

	  public AbstractSampleStreamFactory(Type @params)
	    {
	        
	    }

	  public virtual string Lang
	  {
		  get
		  {
			return "en";
		  }
	  }
	}
}