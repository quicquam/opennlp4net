﻿/*
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

using j4n.IO.InputStream;
using opennlp.tools.sentdetect;

namespace opennlp.console.cmdline.sentdetect
{
    /// <summary>
	/// Loads a Tokenizer Model for the command line tools.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	internal sealed class SentenceModelLoader : ModelLoader<SentenceModel>
	{

	  public SentenceModelLoader() : base("Sentence Detector")
	  {
	  }

	  protected internal override SentenceModel loadModel(InputStream modelIn)
	  {
		return new SentenceModel(modelIn);
	  }

	}

}