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
using j4n.IO.OutputStream;
using j4n.IO.Reader;
using j4n.Serialization;
using opennlp.model;

namespace opennlp.tools.postag
{


	using DataStream = opennlp.maxent.DataStream;
	using GISModel = opennlp.maxent.GISModel;
	using SuffixSensitiveGISModelWriter = opennlp.maxent.io.SuffixSensitiveGISModelWriter;
	using AbstractModel = opennlp.model.AbstractModel;
	using EventStream = opennlp.model.EventStream;
	using TwoPassDataIndexer = opennlp.model.TwoPassDataIndexer;
	using SimplePerceptronSequenceTrainer = opennlp.perceptron.SimplePerceptronSequenceTrainer;
	using SuffixSensitivePerceptronModelWriter = opennlp.perceptron.SuffixSensitivePerceptronModelWriter;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using NGramModel = opennlp.tools.ngram.NGramModel;
	using opennlp.tools.util;
	using StringList = opennlp.tools.util.StringList;

	/// @deprecated Use <seealso cref="POSTaggerME#train(String, ObjectStream, opennlp.tools.util.model.ModelType, POSDictionary, Dictionary, int, int)"/> instead. 
	[Obsolete("Use <seealso cref=\"POSTaggerME#train(String, ObjectStream, opennlp.tools.util.model.ModelType, POSDictionary, Dictionary, int, int)\"/> instead.")]
	public class POSTaggerTrainer
	{
		[Obsolete]
		private static void usage()
		{
		Console.Error.WriteLine("Usage: POSTaggerTrainer [-encoding encoding] [-dict dict_file] -model [perceptron,maxnet] training_data model_file_name [cutoff] [iterations]");
		Console.Error.WriteLine("This trains a new model on the specified training file and writes the trained model to the model file.");
		Console.Error.WriteLine("-encoding Specifies the encoding of the training file");
		Console.Error.WriteLine("-dict Specifies that a dictionary file should be created for use in distinguising between rare and non-rare words");
		Console.Error.WriteLine("-model [perceptron|maxent] Specifies what type of model should be used.");
		Environment.Exit(1);
		}

	  /// 
	  /// <param name="samples"> </param>
	  /// <param name="tagDictionary"> </param>
	  /// <param name="ngramDictionary"> </param>
	  /// <param name="cutoff">
	  /// </param>
	  /// <exception cref="IOException">  its throws if an <seealso cref="IOException"/> is thrown
	  /// during IO operations on a temp file which is created during training occur. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static POSModel train(String languageCode, opennlp.tools.util.ObjectStream<POSSample> samples, POSDictionary tagDictionary, opennlp.tools.dictionary.Dictionary ngramDictionary, int cutoff, int iterations) throws java.io.IOException
	  public static POSModel train(string languageCode, ObjectStream<POSSample> samples, POSDictionary tagDictionary, Dictionary ngramDictionary, int cutoff, int iterations)
	  {

		GISModel posModel = opennlp.maxent.GIS.trainModel(iterations, new TwoPassDataIndexer(new POSSampleEventStream(samples, new DefaultPOSContextGenerator(ngramDictionary)), cutoff));

		return new POSModel(languageCode, posModel, tagDictionary, ngramDictionary);
	  }

	  /// <summary>
	  /// Trains a new model.
	  /// </summary>
	  /// <param name="evc"> </param>
	  /// <param name="modelFile"> </param>
	  /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static void trainMaxentModel(opennlp.model.EventStream evc, java.io.File modelFile) throws java.io.IOException
	  [Obsolete]
	  public static void trainMaxentModel(EventStream evc, Jfile modelFile)
	  {
		AbstractModel model = trainMaxentModel(evc, 100,5);
		(new SuffixSensitiveGISModelWriter(model, modelFile)).persist();
	  }

	  /// <summary>
	  /// Trains a new model
	  /// </summary>
	  /// <param name="es"> </param>
	  /// <param name="iterations"> </param>
	  /// <param name="cut"> </param>
	  /// <returns> the new model </returns>
	  /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static opennlp.model.AbstractModel trainMaxentModel(opennlp.model.EventStream es, int iterations, int cut) throws java.io.IOException
	  [Obsolete]
	  public static AbstractModel trainMaxentModel(EventStream es, int iterations, int cut)
	  {
		return opennlp.maxent.GIS.trainModel(iterations, new TwoPassDataIndexer(es, cut));
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.model.AbstractModel trainPerceptronModel(opennlp.model.EventStream es, int iterations, int cut, boolean useAverage) throws java.io.IOException
	  public static AbstractModel trainPerceptronModel(EventStream es, int iterations, int cut, bool useAverage)
	  {
		return (new opennlp.perceptron.PerceptronTrainer()).trainModel(iterations, new TwoPassDataIndexer(es, cut, false), cut, useAverage);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.model.AbstractModel trainPerceptronModel(opennlp.model.EventStream es, int iterations, int cut) throws java.io.IOException
	  public static AbstractModel trainPerceptronModel(EventStream es, int iterations, int cut)
	  {
		return trainPerceptronModel(es,iterations,cut,true);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.model.AbstractModel trainPerceptronSequenceModel(opennlp.model.SequenceStream ss, int iterations, int cut, boolean useAverage) throws java.io.IOException
	  public static AbstractModel trainPerceptronSequenceModel(SequenceStream<Event> ss, int iterations, int cut, bool useAverage)
	  {
		return (new SimplePerceptronSequenceTrainer()).trainModel(iterations, ss, cut,useAverage);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static void test(opennlp.model.AbstractModel model) throws java.io.IOException
	  [Obsolete]
	  public static void test(AbstractModel model)
	  {
		POSTaggerME tagger = new POSTaggerME(model, (TagDictionary) null);

		BufferedReader @in = new BufferedReader(new InputStreamReader(Console.OpenStandardOutput()));

		for (string line = @in.readLine(); line != null; line = @in.readLine())
		{
		  Console.WriteLine(tagger.tag(line));
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static void main(String[] args) throws java.io.IOException
	  [Obsolete]
	  public static void Main(string[] args)
	  {
		if (args.Length == 0)
		{
		  usage();
		}
		int ai = 0;
		try
		{
		  string encoding = null;
		  string dict = null;
		  bool perceptron = false;
		  bool sequence = false;
		  while (args[ai].StartsWith("-", StringComparison.Ordinal))
		  {
			if (args[ai].Equals("-encoding"))
			{
			  ai++;
			  if (ai < args.Length)
			  {
				encoding = args[ai++];
			  }
			  else
			  {
				usage();
			  }
			}
			else if (args[ai].Equals("-dict"))
			{
			  ai++;
			  if (ai < args.Length)
			  {
				dict = args[ai++];
			  }
			  else
			  {
				usage();
			  }
			}
			else if (args[ai].Equals("-sequence"))
			{
			  ai++;
			  sequence = true;
			}
			else if (args[ai].Equals("-model"))
			{
			  ai++;
			  if (ai < args.Length)
			  {
				string type = args[ai++];
				if (type.Equals("perceptron"))
				{
				  perceptron = true;
				}
				else if (type.Equals("maxent"))
				{

				}
				else
				{
				  usage();
				}
			  }
			  else
			  {
				usage();
			  }
			}
			else
			{
			  Console.Error.WriteLine("Unknown option " + args[ai]);
			  usage();
			}
		  }
          Jfile inFile = new Jfile(args[ai++]);
          Jfile outFile = new Jfile(args[ai++]);
		  int cutoff = 5;
		  int iterations = 100;
		  if (args.Length > ai)
		  {
			cutoff = Convert.ToInt32(args[ai++]);
			iterations = Convert.ToInt32(args[ai++]);
		  }
		  AbstractModel mod;
		  if (dict != null)
		  {
			buildDictionary(dict, inFile, cutoff);
		  }
		  if (sequence)
		  {
			POSSampleSequenceStream ss;
			if (encoding == null)
			{
			  if (dict == null)
			  {
				ss = new POSSampleSequenceStream(new WordTagSampleStream(new InputStreamReader(new FileInputStream(inFile))));
			  }
			  else
			  {
				POSContextGenerator cg = new DefaultPOSContextGenerator(new Dictionary(new FileInputStream(dict)));

				ss = new POSSampleSequenceStream(new WordTagSampleStream((new InputStreamReader(new FileInputStream(inFile)))), cg);
			  }
			}
			else
			{
			  if (dict == null)
			  {

				ss = new POSSampleSequenceStream(new WordTagSampleStream((new InputStreamReader(new FileInputStream(inFile), encoding))));
			  }
			  else
			  {
				POSContextGenerator cg = new DefaultPOSContextGenerator(new Dictionary(new FileInputStream(dict)));

				ss = new POSSampleSequenceStream(new WordTagSampleStream((new InputStreamReader(new FileInputStream(inFile), encoding))), cg);
			  }
			}
			mod = (new SimplePerceptronSequenceTrainer()).trainModel(iterations, ss, cutoff, true);
			Console.WriteLine("Saving the model as: " + outFile);
			(new SuffixSensitivePerceptronModelWriter(mod, outFile)).persist();
		  }
		  else
		  {
			POSSampleEventStream es;
			if (encoding == null)
			{
			  if (dict == null)
			  {
				es = new POSSampleEventStream(new WordTagSampleStream((new InputStreamReader(new FileInputStream(inFile)))));
			  }
			  else
			  {
				POSContextGenerator cg = new DefaultPOSContextGenerator(new Dictionary(new FileInputStream(dict)));

				es = new POSSampleEventStream(new WordTagSampleStream((new InputStreamReader(new FileInputStream(inFile)))), cg);
			  }
			}
			else
			{
			  if (dict == null)
			  {

				es = new POSSampleEventStream(new WordTagSampleStream((new InputStreamReader(new FileInputStream(inFile), encoding))));
			  }
			  else
			  {
				POSContextGenerator cg = new DefaultPOSContextGenerator(new Dictionary(new FileInputStream(dict)));

				es = new POSSampleEventStream(new WordTagSampleStream((new InputStreamReader(new FileInputStream(inFile), encoding))), cg);
			  }
			}
			if (perceptron)
			{
			  mod = trainPerceptronModel(es,iterations, cutoff);
			  Console.WriteLine("Saving the model as: " + outFile);
			  (new SuffixSensitivePerceptronModelWriter(mod, outFile)).persist();
			}
			else
			{
			  mod = trainMaxentModel(es, iterations, cutoff);

			  Console.WriteLine("Saving the model as: " + outFile);

			  (new SuffixSensitiveGISModelWriter(mod, outFile)).persist();
			}
		  }
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void buildDictionary(String dict, java.io.File inFile, int cutoff) throws java.io.FileNotFoundException, java.io.IOException
	  private static void buildDictionary(string dict, Jfile inFile, int cutoff)
	  {
		Console.Error.WriteLine("Building dictionary");

		NGramModel ngramModel = new NGramModel();

		DataStream data = new opennlp.maxent.PlainTextByLineDataStream(new FileReader(inFile));
		while (data.hasNext())
		{
		  string tagStr = (string) data.nextToken();
		  string[] tt = tagStr.Split(' ');
		  string[] words = new string[tt.Length];
		  for (int wi = 0;wi < words.Length;wi++)
		  {
			words[wi] = tt[wi].Substring(0,tt[wi].LastIndexOf('_'));
		  }

		  ngramModel.add(new StringList(words), 1, 1);
		}

		Console.WriteLine("Saving the dictionary");

		ngramModel.cutoff(cutoff, int.MaxValue);
		Dictionary dictionary = ngramModel.toDictionary(true);

		dictionary.serialize(new FileOutputStream(dict));
	  }

	}

}