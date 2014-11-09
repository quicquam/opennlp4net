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
using System.IO;
using j4n.IO.File;
using j4n.IO.InputStream;

namespace opennlp.tools.formats
{

	using TerminateToolException = opennlp.tools.cmdline.TerminateToolException;
	using DetokenizerParameter = opennlp.tools.cmdline.@params.DetokenizerParameter;
	using DetokenizationDictionary = opennlp.tools.tokenize.DetokenizationDictionary;
	using Detokenizer = opennlp.tools.tokenize.Detokenizer;
	using DictionaryDetokenizer = opennlp.tools.tokenize.DictionaryDetokenizer;


	/// <summary>
	/// Base class for factories which need detokenizer.
	/// </summary>
	public abstract class DetokenizerSampleStreamFactory<T> : AbstractSampleStreamFactory<T>
	{

	  protected internal DetokenizerSampleStreamFactory(Type @params) : base(@params)
	  {
	  }

	  protected internal virtual Detokenizer createDetokenizer(DetokenizerParameter p)
	  {
		try
		{
		  return new DictionaryDetokenizer(new DetokenizationDictionary(new FileInputStream(new Jfile(p.Detokenizer))));
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while loading detokenizer dict: " + e.Message, e);
		}
	  }
	}
}