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

namespace opennlp.tools.cmdline
{

	using opennlp.tools.util;
	using TrainingParameters = opennlp.tools.util.TrainingParameters;

	/// <summary>
	/// Base class for trainer tools.
	/// </summary>
	public class AbstractTrainerTool<T, P> : AbstractTypedParamTool<T, P>
	{

	  protected internal P @params;
	  protected internal TrainingParameters mlParams;
	  protected internal ObjectStreamFactory<T> factory;
	  protected internal ObjectStream<T> sampleStream;

	  /// <summary>
	  /// Constructor with type parameters.
	  /// </summary>
	  /// <param name="sampleType"> class of the template parameter </param>
	  /// <param name="params">     interface with parameters </param>
	  protected internal AbstractTrainerTool(Type sampleType, Type @params) : base(sampleType, @params)
	  {
	  }

	  public override void run(string format, string[] args)
	  {
		validateAllArgs(args, this.paramsClass, format);

		@params = ArgumentParser.parse(ArgumentParser.filter(args, this.paramsClass), this.paramsClass);

		factory = getStreamFactory(format);
		string[] fargs = ArgumentParser.filter(args, factory.Parameters);
		validateFactoryArgs(factory, fargs);
		sampleStream = factory.create(fargs);
	  }
	}
}