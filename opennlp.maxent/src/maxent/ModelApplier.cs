using System;
/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using j4n.IO.File;
using j4n.IO.Reader;
using j4n.Utils;

namespace opennlp.maxent
{


	using Event = opennlp.model.Event;
	using EventStream = opennlp.model.EventStream;
	using GenericModelReader = opennlp.model.GenericModelReader;
	using MaxentModel = opennlp.model.MaxentModel;
	using RealValueFileEventStream = opennlp.model.RealValueFileEventStream;

	/// <summary>
	/// Test the model on some input.
	/// </summary>
	public class ModelApplier
	{
	  internal MaxentModel _model;
	  internal ContextGenerator _cg = new BasicContextGenerator(",");
	  internal int counter = 1;

	  // The format for printing percentages
	  public static readonly DecimalFormat ROUNDED_FORMAT = new DecimalFormat("0.000");

	  public ModelApplier(MaxentModel m)
	  {
		_model = m;
	  }

	  private void eval(Event @event)
	  {
		eval(@event, false);
	  }

	  private void eval(Event @event, bool real)
	  {

		string outcome = @event.Outcome; // Is ignored
		string[] context = @event.Context;

		double[] ocs;
		if (!real)
		{
		  ocs = _model.eval(context);
		}
		else
		{
		  float[] values = RealValueFileEventStream.parseContexts(context);
		  ocs = _model.eval(context, values);
		}

		int numOutcomes = ocs.Length;
		DoubleStringPair[] result = new DoubleStringPair[numOutcomes];
		for (int i = 0; i < numOutcomes; i++)
		{
		  result[i] = new DoubleStringPair(ocs[i], _model.getOutcome(i));
		}

		Array.Sort(result);

		// Print the most likely outcome first, down to the least likely.
		for (int i = numOutcomes - 1; i >= 0; i--)
		{
		  Console.Write(result[i].stringValue + " " + result[i].doubleValue + " ");
		}
		Console.WriteLine();

	  }

	  private static void usage()
	  {
		Console.Error.WriteLine("java ModelApplier [-real] modelFile dataFile");
		Environment.Exit(1);
	  }

	  /// <summary>
	  /// Main method. Call as follows:
	  /// <para>
	  /// java ModelApplier modelFile dataFile
	  /// </para>
	  /// </summary>
	  public static void Main(string[] args)
	  {

		string dataFileName, modelFileName;
		bool real = false;
		string type = "maxent";
		int ai = 0;

		if (args.Length == 0)
		{
		  usage();
		}

		if (args.Length > 0)
		{
		  while (args[ai].StartsWith("-", StringComparison.Ordinal))
		  {
			if (args[ai].Equals("-real"))
			{
			  real = true;
			}
			else if (args[ai].Equals("-perceptron"))
			{
			  type = "perceptron";
			}
			else
			{
			  usage();
			}
			ai++;
		  }

		  modelFileName = args[ai++];
		  dataFileName = args[ai++];

		  ModelApplier predictor = null;
		  try
		  {
			MaxentModel m = (new GenericModelReader(new Jfile(modelFileName))).Model;
			predictor = new ModelApplier(m);
		  }
		  catch (Exception e)
		  {
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			Environment.Exit(0);
		  }

		  try
		  {
			EventStream es = new BasicEventStream(new PlainTextByLineDataStream(new FileReader(new Jfile(dataFileName))), ",");

			while (es.hasNext())
			{
			  predictor.eval(es.next(), real);
			}

			return;
		  }
		  catch (Exception e)
		  {
			Console.WriteLine("Unable to read from specified file: " + modelFileName);
			Console.WriteLine();
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		  }
		}
	  }
	}

}