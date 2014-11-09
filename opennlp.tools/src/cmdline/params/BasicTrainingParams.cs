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

namespace opennlp.tools.cmdline.@params
{

	using OptionalParameter = opennlp.tools.cmdline.ArgumentParser.OptionalParameter;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;

	/// <summary>
	/// Common training parameters.
	/// 
	/// Note: Do not use this class, internal use only!
	/// </summary>
	public interface BasicTrainingParams : LanguageParams
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "num", description = "number of training iterations, ignored if -params is used.") @OptionalParameter(defaultValue="100") Integer getIterations();
	  int? Iterations {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "num", description = "minimal number of times a feature must be seen, ignored if -params is used.") @OptionalParameter(defaultValue="5") Integer getCutoff();
	  int? Cutoff {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "paramsFile", description = "training parameters file.") @OptionalParameter() String getParams();
	  string Params {get;}
	}

}