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
using j4n.IO.Reader;
using j4n.Serialization;

namespace opennlp.tools.cmdline.doccat
{


	using DoccatModel = opennlp.tools.doccat.DoccatModel;
	using DocumentCategorizerME = opennlp.tools.doccat.DocumentCategorizerME;
	using DocumentSample = opennlp.tools.doccat.DocumentSample;
	using opennlp.tools.util;
	using ParagraphStream = opennlp.tools.util.ParagraphStream;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
	using WhitespaceTokenizer = opennlp.tools.tokenize.WhitespaceTokenizer;

	public class DoccatTool : BasicCmdLineTool
	{

	  public override string ShortDescription
	  {
		  get
		  {
			return "learnable document categorizer";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return "Usage: " + CLI.CMD + " " + Name + " model < documents";
		  }
	  }

	  public override void run(string[] args)
	  {

		if (0 == args.Length)
		{
		  Console.WriteLine(Help);
		}
		else
		{

		  DoccatModel model = (new DoccatModelLoader()).load(new Jfile(args[0]));

		  DocumentCategorizerME doccat = new DocumentCategorizerME(model);

		  ObjectStream<string> documentStream = new ParagraphStream(new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput())));

		  PerformanceMonitor perfMon = new PerformanceMonitor(Console.Error, "doc");
		  perfMon.start();

		  try
		  {
			string document;
			while ((document = documentStream.read()) != null)
			{
			  double[] prob = doccat.categorize(WhitespaceTokenizer.INSTANCE.tokenize(document));
			  string category = doccat.getBestCategory(prob);

			  DocumentSample sample = new DocumentSample(category, document);
			  Console.WriteLine(sample.ToString());

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

}