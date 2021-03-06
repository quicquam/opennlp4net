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

namespace opennlp.tools.cmdline.sentdetect
{


	using SentenceDetectorME = opennlp.tools.sentdetect.SentenceDetectorME;
	using SentenceModel = opennlp.tools.sentdetect.SentenceModel;
	using opennlp.tools.util;
	using ParagraphStream = opennlp.tools.util.ParagraphStream;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// A sentence detector which uses a maxent model to predict the sentences.
	/// </summary>
	public sealed class SentenceDetectorTool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "learnable sentence detector";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " model < sentences";
		  }
	  }

	  /// <summary>
	  /// Perform sentence detection the input stream.
	  /// 
	  /// A newline will be treated as a paragraph boundary.
	  /// </summary>
	  public override void run(string[] args)
	  {

		if (args.Length != 1)
		{
		  Console.WriteLine(Help);
		}
		else
		{

		  SentenceModel model = (new SentenceModelLoader()).load(new File(args[0]));

		  SentenceDetectorME sdetector = new SentenceDetectorME(model);

		  ObjectStream<string> paraStream = new ParagraphStream(new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput)));

		  PerformanceMonitor perfMon = new PerformanceMonitor(System.err, "sent");
		  perfMon.start();

		  try
		  {
			string para;
			while ((para = paraStream.read()) != null)
			{

			  string[] sents = sdetector.sentDetect(para);
			  foreach (string sentence in sents)
			  {
				Console.WriteLine(sentence);
			  }

			  perfMon.incrementCounter(sents.Length);

			  Console.WriteLine();
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

}