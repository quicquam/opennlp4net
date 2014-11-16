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
using j4n.IO.File;

namespace opennlp.tools.cmdline.postag
{


	using opennlp.tools.cmdline;
	using OptionalParameter = opennlp.tools.cmdline.ArgumentParser.OptionalParameter;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using EvaluatorParams = opennlp.tools.cmdline.@params.EvaluatorParams;
	using EvalToolParams = opennlp.tools.cmdline.postag.POSTaggerEvaluatorTool.EvalToolParams;
	using POSEvaluator = opennlp.tools.postag.POSEvaluator;
	using POSModel = opennlp.tools.postag.POSModel;
	using POSSample = opennlp.tools.postag.POSSample;
	using POSTaggerEvaluationMonitor = opennlp.tools.postag.POSTaggerEvaluationMonitor;

	public sealed class POSTaggerEvaluatorTool : AbstractEvaluatorTool<POSSample, EvalToolParams>
	{
	    public interface EvalToolParams : EvaluatorParams
	  {
		Jfile ReportOutputFile {get;}
	  }

	  public POSTaggerEvaluatorTool() : base(typeof(POSSample), typeof(EvalToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "Measures the performance of the POS tagger model with the reference data";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		POSModel model = (new POSModelLoader()).load(@params.Model);

		POSTaggerEvaluationMonitor missclassifiedListener = null;
		if (@params.Misclassified.Value)
		{
		  missclassifiedListener = new POSEvaluationErrorListener();
		}

		POSTaggerFineGrainedReportListener reportListener = null;
		File reportFile = @params.ReportOutputFile;
		OutputStream reportOutputStream = null;
		if (reportFile != null)
		{
		  CmdLineUtil.checkOutputFile("Report Output File", reportFile);
		  try
		  {
			reportOutputStream = new FileOutputStream(reportFile);
			reportListener = new POSTaggerFineGrainedReportListener(reportOutputStream);
		  }
		  catch (FileNotFoundException e)
		  {
			throw new TerminateToolException(-1, "IO error while creating POS Tagger fine-grained report file: " + e.Message);
		  }
		}

		POSEvaluator evaluator = new POSEvaluator(new opennlp.tools.postag.POSTaggerME(model), missclassifiedListener, reportListener);

		Console.Write("Evaluating ... ");
		try
		{
		  evaluator.evaluate(sampleStream);
		}
		catch (IOException e)
		{
		  Console.Error.WriteLine("failed");
		  throw new TerminateToolException(-1, "IO error while reading test data: " + e.Message, e);
		}
		finally
		{
		  try
		  {
			sampleStream.close();
		  }
		  catch (IOException)
		  {
			// sorry that this can fail
		  }
		}

		Console.WriteLine("done");

		if (reportListener != null)
		{
		  Console.WriteLine("Writing fine-grained report to " + @params.ReportOutputFile.AbsolutePath);
		  reportListener.writeReport();

		  try
		  {
			// TODO: is it a problem to close the stream now?
			reportOutputStream.close();
		  }
		  catch (IOException)
		  {
			// nothing to do
		  }
		}

		Console.WriteLine();

		Console.WriteLine("Accuracy: " + evaluator.WordAccuracy);
	  }
	}

}