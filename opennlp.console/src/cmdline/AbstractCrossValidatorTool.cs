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

namespace opennlp.console.cmdline
{

	/// <summary>
	/// Base class for cross validator tools.
	/// </summary>
	public abstract class AbstractCrossValidatorTool<T, P> : AbstractTrainerTool<T, P>
	{

	  /// <summary>
	  /// Constructor with type parameters.
	  /// </summary>
	  /// <param name="sampleType"> class of the template parameter </param>
	  /// <param name="params">     interface with parameters </param>
	  protected internal AbstractCrossValidatorTool(Type sampleType, Type @params) : base(sampleType, @params)
	  {
	  }
	}

}