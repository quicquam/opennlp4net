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

namespace opennlp.tools.cmdline.postag
{


	using opennlp.tools.cmdline;
	using POSModel = opennlp.tools.postag.POSModel;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;

	/// <summary>
	/// Loads a POS Tagger Model for the command line tools.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public sealed class POSModelLoader : ModelLoader<POSModel>
	{

	  public POSModelLoader() : base("POS Tagger")
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected opennlp.tools.postag.POSModel loadModel(java.io.InputStream modelIn) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
	  protected internal override POSModel loadModel(InputStream modelIn)
	  {
		return new POSModel(modelIn);
	  }
	}

}