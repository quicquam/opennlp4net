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
using j4n.IO.File;
using j4n.IO.OutputStream;
using opennlp.tools.cmdline.@params;
using opennlp.tools.postag;
using opennlp.tools.util.model;

namespace opennlp.tools.cmdline.postag
{
    public sealed class POSTaggerCrossValidatorTool : AbstractCrossValidatorTool<POSSample, POSTaggerCrossValidatorTool.CVToolParams>
	{
	    public interface CVToolParams : CVParams, TrainingParams
	  {
		Jfile ReportOutputFile {get;}
	  }

	  public POSTaggerCrossValidatorTool() : base(typeof(POSSample), typeof(CVToolParams))
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "K-fold cross validator for the learnable POS tagger";
		  }
	  }

	  public override void run(string format, string[] args)
	  {
		base.run(format, args);

		mlParams = CmdLineUtil.loadTrainingParameters(@params.Params, false);
		if (mlParams == null)
		{
		  mlParams = ModelUtil.createTrainingParameters(@params.Iterations.Value, @params.Cutoff.Value);
		}

		POSTaggerEvaluationMonitor missclassifiedListener = null;
		if (@params.Misclassified.Value)
		{
		  missclassifiedListener = new POSEvaluationErrorListener();
		}

		POSTaggerFineGrainedReportListener reportListener = null;
		Jfile reportFile = @params.ReportOutputFile;
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

		POSTaggerCrossValidator validator;
		try
		{
		  validator = new POSTaggerCrossValidator(@params.Lang, mlParams, @params.Dict, @params.Ngram, @params.TagDictCutoff, @params.Factory, missclassifiedListener, reportListener);

		  validator.evaluate(sampleStream, @params.Folds.Value);
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while reading training data or indexing data: " + e.Message, e);
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

		Console.WriteLine("Accuracy: " + validator.WordAccuracy);
	  }
	}

}