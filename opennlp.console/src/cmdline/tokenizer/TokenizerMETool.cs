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

namespace opennlp.tools.cmdline.tokenizer
{

	using TokenizerModel = opennlp.tools.tokenize.TokenizerModel;

	public sealed class TokenizerMETool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "learnable tokenizer";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " model < sentences";
		  }
	  }

	  public override void run(string[] args)
	  {
		if (args.Length != 1)
		{
		  Console.WriteLine(Help);
		}
		else
		{

		  TokenizerModel model = (new TokenizerModelLoader()).load(new File(args[0]));

		  CommandLineTokenizer tokenizer = new CommandLineTokenizer(new opennlp.tools.tokenize.TokenizerME(model));

		  tokenizer.process();
		}
	  }
	}

}