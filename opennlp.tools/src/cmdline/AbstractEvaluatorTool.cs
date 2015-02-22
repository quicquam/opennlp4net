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

using System;
using opennlp.tools.util;

namespace opennlp.tools.cmdline
{
    /// <summary>
	/// Base class for evaluator tools.
	/// </summary>
	public class AbstractEvaluatorTool<T, P> : AbstractTypedParamTool<T, P>
	{

	  protected internal P parameters;
	  protected internal ObjectStreamFactory<T> factory;
	  protected internal ObjectStream<T> sampleStream;

	  /// <summary>
	  /// Constructor with type parameters.
	  /// </summary>
	  /// <param name="sampleType"> class of the template parameter </param>
	  /// <param name="params">     interface with parameters </param>
	  protected internal AbstractEvaluatorTool(Type sampleType, Type parameters) : base(sampleType, parameters)
	  {
	  }

	  public override void run(string format, string[] args)
	  {
		validateAllArgs<T>(args, this.paramsClass, format);

		parameters = ArgumentParser.parse<P>(ArgumentParser.filter(args, this.paramsClass), this.paramsClass);

		factory = getStreamFactory(format);
		string[] fargs = ArgumentParser.filter(args, factory.getParameters());
		validateFactoryArgs(factory, fargs);
		sampleStream = factory.create(fargs);
	  }
	}
}