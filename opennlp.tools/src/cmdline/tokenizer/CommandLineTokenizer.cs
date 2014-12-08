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
using System.IO;
using j4n.IO.Reader;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.tools.cmdline.tokenizer
{
    internal sealed class CommandLineTokenizer
	{

	  private readonly Tokenizer tokenizer;

	  internal CommandLineTokenizer(Tokenizer tokenizer)
	  {
		this.tokenizer = tokenizer;
	  }

	  internal void process()
	  {

		ObjectStream<string> untokenizedLineStream = new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput()));

		ObjectStream<string> tokenizedLineStream = new WhitespaceTokenStream(new TokenizerStream(tokenizer, untokenizedLineStream));

		PerformanceMonitor perfMon = new PerformanceMonitor(Console.Error, "sent");
		perfMon.start();

		try
		{
		  string tokenizedLine;
		  while ((tokenizedLine = tokenizedLineStream.read()) != null)
		  {
			Console.WriteLine(tokenizedLine);
			perfMon.incrementCounter();
		  }
		}
		catch (IOException e)
		{
		  CmdLineUtil.handleStdinIoError(e);
		}

		perfMon.stopAndPrintFinalResult();
	  }
	}

}