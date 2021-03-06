﻿using System;

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

namespace opennlp.tools.formats
{

	/// <summary>
	/// Stream factory for those streams which carry language.
	/// </summary>
	public abstract class LanguageSampleStreamFactory : AbstractSampleStreamFactory
	{

	  protected internal string language;

	    protected LanguageSampleStreamFactory(Type @params, string language) : base(@params)
	    {
	        this.language = language;
	    }

	    protected internal LanguageSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  public override string Lang
	  {
		  get
		  {
			return language;
		  }
	  }
	}

}